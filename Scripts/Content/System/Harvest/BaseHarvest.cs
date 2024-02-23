using Server.Engine.Facet.Module.LumberHarvest;
using Server.Items;
using Server.Targeting;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.Harvest
{
	public abstract class HarvestSystem : IHarvestSystem
	{
		public List<HarvestDefinition> Definitions { get; }

		public HarvestSystem()
		{
			Definitions = new List<HarvestDefinition>();
		}

		public virtual bool CheckTool(Mobile from, IHarvestTool tool)
		{
			var wornOut = tool == null || tool.Deleted || (tool is IUsesRemaining && ((IUsesRemaining)tool).UsesRemaining <= 0);

			if (wornOut)
			{
				from.SendLocalizedMessage(1044038); // You have worn out your tool!
			}

			return !wornOut;
		}

		public virtual bool CheckHarvest(Mobile from, IHarvestTool tool)
		{
			return CheckTool(from, tool);
		}

		public virtual bool CheckHarvest(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest)
		{
			return CheckTool(from, tool);
		}

		public virtual bool CheckRange(Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
		{
			var inRange = from.Map == map && from.InRange(loc, def.MaxRange);

			if (!inRange)
			{
				def.SendMessageTo(from, timed ? def.TimedOutOfRangeMessage : def.OutOfRangeMessage);
			}

			return inRange;
		}

		public virtual bool CheckResources(Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc, bool timed)
		{
			var bank = def.GetBank(map, loc.X, loc.Y);
			var available = bank != null && bank.Current >= def.ConsumedPerHarvest;

			if (!available)
			{
				def.SendMessageTo(from, timed ? def.DoubleHarvestMessage : def.NoResourcesMessage);
			}

			return available;
		}

		public virtual void OnBadHarvestTarget(Mobile from, IHarvestTool tool, object toHarvest)
		{
		}

		public virtual object GetLock(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest)
		{
			/* Here we prevent multiple harvesting.
			 * 
			 * Some options:
			 *  - 'return tool;' : This will allow the player to harvest more than once concurrently, but only if they use multiple tools. This seems to be as OSI.
			 *  - 'return GetType();' : This will disallow multiple harvesting of the same type. That is, we couldn't mine more than once concurrently, but we could be both mining and lumberjacking.
			 *  - 'return typeof( HarvestSystem );' : This will completely restrict concurrent harvesting.
			 */

			return tool;
		}

		public virtual void OnConcurrentHarvest(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest)
		{
		}

		public virtual void OnHarvestStarted(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest)
		{
		}

		public virtual bool BeginHarvesting(Mobile from, IHarvestTool tool)
		{
			if (!CheckHarvest(from, tool))
			{
				return false;
			}

			from.Target = new HarvestTarget(tool, this);
			return true;
		}

		public virtual void FinishHarvesting(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest, object locked)
		{
			from.EndAction(locked);

			if (!CheckHarvest(from, tool))
			{
				return;
			}

			if (!GetHarvestDetails(from, tool, toHarvest, out var tileID, out var map, out var loc))
			{
				OnBadHarvestTarget(from, tool, toHarvest);
				return;
			}

			if (!def.Validate(tileID))
			{
				OnBadHarvestTarget(from, tool, toHarvest);
				return;
			}

			if (!CheckRange(from, tool, def, map, loc, true))
			{
				return;
			}

			if (!CheckResources(from, tool, def, map, loc, true))
			{
				return;
			}

			if (!CheckHarvest(from, tool, def, toHarvest))
			{
				return;
			}

			if (SpecialHarvest(from, tool, def, toHarvest, tileID, map, loc))
			{
				return;
			}

			var bank = def.GetBank(map, loc.X, loc.Y);

			if (bank == null)
			{
				return;
			}

			var vein = bank.Vein;

			if (vein != null)
			{
				vein = MutateVein(from, tool, def, bank, toHarvest, vein);
			}

			if (vein == null)
			{
				return;
			}

			var primary = vein.PrimaryResource;
			var fallback = vein.FallbackResource;
			var resource = MutateResource(from, tool, def, map, loc, vein, primary, fallback);

			var skillBase = from.Skills[def.Skill].Base;

			Type type = null;

			if (skillBase >= resource.ReqSkill && from.CheckSkill(def.Skill, resource.MinSkill, resource.MaxSkill))
			{
				type = GetResourceType(from, tool, def, map, loc, resource);
				type = MutateType(type, from, tool, def, map, loc, resource);
				
				if (type != null)
				{
					var item = Construct(type, from);

					if (item == null)
					{
						type = null;
					}
					else
					{
						//The whole harvest system is kludgy and I'm sure this is just adding to it.
						if (item.Stackable)
						{
							var amount = def.ConsumedPerHarvest;
							var feluccaAmount = def.ConsumedPerFeluccaHarvest;

							var racialAmount = (int)Math.Ceiling(amount * 1.1);
							var feluccaRacialAmount = (int)Math.Ceiling(feluccaAmount * 1.1);

							var eligableForRacialBonus = def.RaceBonus && from.Race == Race.Human;
							var inFelucca = map == Map.Felucca;

							if (eligableForRacialBonus && inFelucca && bank.Current >= feluccaRacialAmount && 0.1 > Utility.RandomDouble())
							{
								item.Amount = feluccaRacialAmount;
							}
							else if (inFelucca && bank.Current >= feluccaAmount)
							{
								item.Amount = feluccaAmount;
							}
							else if (eligableForRacialBonus && bank.Current >= racialAmount && 0.1 > Utility.RandomDouble())
							{
								item.Amount = racialAmount;
							}
							else
							{
								item.Amount = amount;
							}
						}

						var itemAmount = item.Amount;

						bank.Consume(itemAmount, from);

						if (Give(from, item, def.PlaceAtFeetIfFull))
						{
							EventSink.InvokeHarvestedItem(new HarvestedItemEventArgs(from, item, itemAmount, this, tool));

							SendSuccessTo(from, item, resource);
						}
						else
						{
							SendPackFullTo(from, item, def, resource);
							item.Delete();
						}

						var bonus = def.GetBonusResource();

						if (bonus != null && bonus.Type != null && skillBase >= bonus.ReqSkill)
						{
							var bonusItem = Construct(bonus.Type, from);

							var bonusAmount = bonusItem.Amount;

							if (Give(from, bonusItem, true)) // Bonuses always allow placing at feet, even if pack is full regrdless of def
							{
								bonus.SendSuccessTo(from);

								EventSink.InvokeHarvestedItem(new HarvestedItemEventArgs(from, bonusItem, bonusAmount, this, tool));
							}
							else
							{
								item.Delete();
							}
						}

						if (tool is IUsesRemaining toolWithUses)
						{
							toolWithUses.ShowUsesRemaining = true;

							if (toolWithUses.UsesRemaining > 0)
							{
								--toolWithUses.UsesRemaining;
							}

							if (toolWithUses.UsesRemaining < 1)
							{
								tool.Delete();
								def.SendMessageTo(from, def.ToolBrokeMessage);
							}
						}
					}
				}
			}

			if (type == null)
			{
				def.SendMessageTo(from, def.FailMessage);
			}

			OnHarvestFinished(from, tool, def, vein, bank, resource, toHarvest);
		}

		public virtual void OnHarvestFinished(Mobile from, IHarvestTool tool, HarvestDefinition def, HarvestVein vein, HarvestBank bank, HarvestResource resource, object harvested)
		{
		}

		public virtual bool SpecialHarvest(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest, int tileID, Map map, Point3D loc)
		{
			return false;
		}

		public virtual Item Construct(Type type, Mobile from)
		{
			try
			{
				return Activator.CreateInstance(type) as Item;
			}
			catch
			{
				return null;
			}
		}

		public virtual HarvestVein MutateVein(Mobile from, IHarvestTool tool, HarvestDefinition def, HarvestBank bank, object toHarvest, HarvestVein vein)
		{
			return vein;
		}

		public virtual void SendSuccessTo(Mobile from, Item item, HarvestResource resource)
		{
			resource.SendSuccessTo(from);
		}

		public virtual void SendPackFullTo(Mobile from, Item item, HarvestDefinition def, HarvestResource resource)
		{
			def.SendMessageTo(from, def.PackFullMessage);
		}

		public virtual bool Give(Mobile m, Item item, bool placeAtFeet)
		{
			if (m.PlaceInBackpack(item))
			{
				return true;
			}

			if (!placeAtFeet)
			{
				return false;
			}

			var map = m.Map;

			if (map == null)
			{
				return false;
			}

			var atFeet = new List<Item>();

			foreach (var obj in m.GetItemsInRange(0))
			{
				atFeet.Add(obj);
			}

			for (var i = 0; i < atFeet.Count; ++i)
			{
				var check = atFeet[i];

				if (check.StackWith(m, item, false))
				{
					return true;
				}
			}

			item.MoveToWorld(m.Location, map);
			return true;
		}

		public virtual Type MutateType(Type type, Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
		{
			var region = Region.Find(loc, map) ?? from?.Region;

			return region?.GetResource(from, tool, map, loc, this, type) ?? type;
		}

		public virtual Type GetResourceType(Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc, HarvestResource resource)
		{
			if (resource.Types.Length > 0)
			{
				return resource.Types[Utility.Random(resource.Types.Length)];
			}

			return null;
		}

		public virtual HarvestResource MutateResource(Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc, HarvestVein vein, HarvestResource primary, HarvestResource fallback)
		{
			var racialBonus = def.RaceBonus && from.Race == Race.Elf;

			if (vein.ChanceToFallback > (Utility.RandomDouble() + (racialBonus ? .20 : 0)))
			{
				return fallback;
			}

			var skillValue = from.Skills[def.Skill].Value;

			if (fallback != null && (skillValue < primary.ReqSkill || skillValue < primary.MinSkill))
			{
				return fallback;
			}

			return primary;
		}

		public virtual bool OnHarvesting(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest, object locked, bool last)
		{
			if (!CheckHarvest(from, tool))
			{
				from.EndAction(locked);
				return false;
			}

			int tileID;
			Map map;
			Point3D loc;

			if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
			{
				from.EndAction(locked);
				OnBadHarvestTarget(from, tool, toHarvest);
				return false;
			}
			else if (!def.Validate(tileID))
			{
				from.EndAction(locked);
				OnBadHarvestTarget(from, tool, toHarvest);
				return false;
			}
			else if (!CheckRange(from, tool, def, map, loc, true))
			{
				from.EndAction(locked);
				return false;
			}
			else if (!CheckResources(from, tool, def, map, loc, true))
			{
				from.EndAction(locked);
				return false;
			}
			else if (!CheckHarvest(from, tool, def, toHarvest))
			{
				from.EndAction(locked);
				return false;
			}

			DoHarvestingEffect(from, tool, def, map, loc);

			new HarvestSoundTimer(from, tool, this, def, toHarvest, locked, last).Start();

			return !last;
		}

		public virtual void DoHarvestingSound(Mobile from, IHarvestTool tool, HarvestDefinition def, object toHarvest)
		{
			if (def.EffectSounds.Length > 0)
			{
				from.PlaySound(Utility.RandomList(def.EffectSounds));
			}
		}

		public virtual void DoHarvestingEffect(Mobile from, IHarvestTool tool, HarvestDefinition def, Map map, Point3D loc)
		{
			from.Direction = from.GetDirectionTo(loc);

			if (!from.Mounted)
			{
				from.Animate(Utility.RandomList(def.EffectActions), 5, 1, true, false, 0);
			}
		}

		public virtual HarvestDefinition GetDefinition(int tileID)
		{
			HarvestDefinition def = null;

			for (var i = 0; def == null && i < Definitions.Count; ++i)
			{
				var check = Definitions[i];

				if (check.Validate(tileID))
				{
					def = check;
				}
			}

			return def;
		}

		public virtual void StartHarvesting(Mobile from, IHarvestTool tool, object toHarvest)
		{
			if (!CheckHarvest(from, tool))
			{
				return;
			}

			int tileID;
			Map map;
			Point3D loc;

			if (!GetHarvestDetails(from, tool, toHarvest, out tileID, out map, out loc))
			{
				OnBadHarvestTarget(from, tool, toHarvest);
				return;
			}

			var def = GetDefinition(tileID);

			if (def == null)
			{
				OnBadHarvestTarget(from, tool, toHarvest);
				return;
			}

			if (!CheckRange(from, tool, def, map, loc, false))
			{
				return;
			}
			else if (!CheckResources(from, tool, def, map, loc, false))
			{
				return;
			}
			else if (!CheckHarvest(from, tool, def, toHarvest))
			{
				return;
			}

			var toLock = GetLock(from, tool, def, toHarvest);

			if (!from.BeginAction(toLock))
			{
				OnConcurrentHarvest(from, tool, def, toHarvest);
				return;
			}

			new HarvestTimer(from, tool, this, def, toHarvest, toLock).Start();
			OnHarvestStarted(from, tool, def, toHarvest);
		}

		public virtual bool GetHarvestDetails(Mobile from, IHarvestTool tool, object toHarvest, out int tileID, out Map map, out Point3D loc)
		{
			if (toHarvest is Static && !((Static)toHarvest).Movable)
			{
				var obj = (Static)toHarvest;

				tileID = (obj.ItemID & 0x3FFF) | 0x4000;
				map = obj.Map;
				loc = obj.GetWorldLocation();
			}
			else if (toHarvest is StaticTarget)
			{
				var obj = (StaticTarget)toHarvest;

				tileID = (obj.ItemID & 0x3FFF) | 0x4000;
				map = from.Map;
				loc = obj.Location;
			}
			else if (toHarvest is LandTarget obj)
			{
				tileID = obj.TileID;
				map = from.Map;
				loc = obj.Location;
			}
			else
			{
				tileID = 0;
				map = null;
				loc = Point3D.Zero;
				return false;
			}

			return map != null && map != Map.Internal;
		}
	}

	public class HarvestDefinition
	{
		public int BankWidth { get; set; }
		public int BankHeight { get; set; }
		public int MinTotal { get; set; }
		public int MaxTotal { get; set; }
		public int[] Tiles { get; set; }
		public bool RangedTiles { get; set; }
		public TimeSpan MinRespawn { get; set; }
		public TimeSpan MaxRespawn { get; set; }
		public int MaxRange { get; set; }
		public int ConsumedPerHarvest { get; set; }
		public int ConsumedPerFeluccaHarvest { get; set; }
		public bool PlaceAtFeetIfFull { get; set; }
		public SkillName Skill { get; set; }
		public int[] EffectActions { get; set; }
		public int[] EffectCounts { get; set; }
		public int[] EffectSounds { get; set; }
		public TimeSpan EffectSoundDelay { get; set; }
		public TimeSpan EffectDelay { get; set; }
		public object NoResourcesMessage { get; set; }
		public object OutOfRangeMessage { get; set; }
		public object TimedOutOfRangeMessage { get; set; }
		public object DoubleHarvestMessage { get; set; }
		public object FailMessage { get; set; }
		public object PackFullMessage { get; set; }
		public object ToolBrokeMessage { get; set; }
		public HarvestResource[] Resources { get; set; }
		public HarvestVein[] Veins { get; set; }
		public BonusHarvestResource[] BonusResources { get; set; }
		public bool RaceBonus { get; set; }
		public bool RandomizeVeins { get; set; }

		public Dictionary<Map, Dictionary<Point2D, HarvestBank>> Banks { get; set; }

		public void SendMessageTo(Mobile from, object message)
		{
			if (message is int)
			{
				from.SendLocalizedMessage((int)message);
			}
			else if (message is string)
			{
				from.SendMessage((string)message);
			}
		}

		public HarvestBank GetBank(Map map, int x, int y)
		{
			if (map == null || map == Map.Internal)
			{
				return null;
			}

			x /= BankWidth;
			y /= BankHeight;

			Dictionary<Point2D, HarvestBank> banks;
			_ = Banks.TryGetValue(map, out banks);

			if (banks == null)
			{
				Banks[map] = banks = new Dictionary<Point2D, HarvestBank>();
			}

			var key = new Point2D(x, y);
			HarvestBank bank;
			_ = banks.TryGetValue(key, out bank);

			if (bank == null)
			{
				banks[key] = bank = new HarvestBank(this, GetVeinAt(map, x, y));
			}

			return bank;
		}

		public HarvestVein GetVeinAt(Map map, int x, int y)
		{
			if (Veins.Length == 1)
			{
				return Veins[0];
			}

			double randomValue;

			if (RandomizeVeins)
			{
				randomValue = Utility.RandomDouble();
			}
			else
			{
				var random = new Random((x * 17) + (y * 11) + (map.MapID * 3));
				randomValue = random.NextDouble();
			}

			return GetVeinFrom(randomValue);
		}

		public HarvestVein GetVeinFrom(double randomValue)
		{
			if (Veins.Length == 1)
			{
				return Veins[0];
			}

			randomValue *= 100;

			for (var i = 0; i < Veins.Length; ++i)
			{
				if (randomValue <= Veins[i].VeinChance)
				{
					return Veins[i];
				}

				randomValue -= Veins[i].VeinChance;
			}

			return null;
		}

		public BonusHarvestResource GetBonusResource()
		{
			if (BonusResources == null)
			{
				return null;
			}

			var randomValue = Utility.RandomDouble() * 100;

			for (var i = 0; i < BonusResources.Length; ++i)
			{
				if (randomValue <= BonusResources[i].Chance)
				{
					return BonusResources[i];
				}

				randomValue -= BonusResources[i].Chance;
			}

			return null;
		}

		public HarvestDefinition()
		{
			Banks = new Dictionary<Map, Dictionary<Point2D, HarvestBank>>();
		}

		public bool Validate(int tileID)
		{
			if (RangedTiles)
			{
				var contains = false;

				for (var i = 0; !contains && i < Tiles.Length; i += 2)
				{
					contains = tileID >= Tiles[i] && tileID <= Tiles[i + 1];
				}

				return contains;
			}
			else
			{
				var dist = -1;

				for (var i = 0; dist < 0 && i < Tiles.Length; ++i)
				{
					dist = Tiles[i] - tileID;
				}

				return dist == 0;
			}
		}
	}

	public class HarvestBank
	{
		private int m_Current;
		private readonly int m_Maximum;
		private DateTime m_NextRespawn;
		private HarvestVein m_Vein, m_DefaultVein;

		public HarvestDefinition Definition { get; }

		public int Current
		{
			get
			{
				CheckRespawn();
				return m_Current;
			}
		}

		public HarvestVein Vein
		{
			get
			{
				CheckRespawn();
				return m_Vein;
			}
			set => m_Vein = value;
		}

		public HarvestVein DefaultVein
		{
			get
			{
				CheckRespawn();
				return m_DefaultVein;
			}
		}

		public void CheckRespawn()
		{
			if (m_Current == m_Maximum || m_NextRespawn > DateTime.UtcNow)
			{
				return;
			}

			m_Current = m_Maximum;

			if (Definition.RandomizeVeins)
			{
				m_DefaultVein = Definition.GetVeinFrom(Utility.RandomDouble());
			}

			m_Vein = m_DefaultVein;
		}

		public void Consume(int amount, Mobile from)
		{
			CheckRespawn();

			if (m_Current == m_Maximum)
			{
				var min = Definition.MinRespawn.TotalMinutes;
				var max = Definition.MaxRespawn.TotalMinutes;
				var rnd = Utility.RandomDouble();

				m_Current = m_Maximum - amount;

				var minutes = min + (rnd * (max - min));
				if (Definition.RaceBonus && from.Race == Race.Elf)    //def.RaceBonus = Core.ML
				{
					minutes *= .75; //25% off the time.  
				}

				m_NextRespawn = DateTime.UtcNow + TimeSpan.FromMinutes(minutes);
			}
			else
			{
				m_Current -= amount;
			}

			if (m_Current < 0)
			{
				m_Current = 0;
			}
		}

		public HarvestBank(HarvestDefinition def, HarvestVein defaultVein)
		{
			m_Maximum = Utility.RandomMinMax(def.MinTotal, def.MaxTotal);
			m_Current = m_Maximum;
			m_DefaultVein = defaultVein;
			m_Vein = m_DefaultVein;

			Definition = def;
		}
	}

	public class HarvestVein
	{
		public double VeinChance { get; set; }
		public double ChanceToFallback { get; set; }
		public HarvestResource PrimaryResource { get; set; }
		public HarvestResource FallbackResource { get; set; }

		public HarvestVein(double veinChance, double chanceToFallback, HarvestResource primaryResource, HarvestResource fallbackResource)
		{
			VeinChance = veinChance;
			ChanceToFallback = chanceToFallback;
			PrimaryResource = primaryResource;
			FallbackResource = fallbackResource;
		}
	}

	public class HarvestTarget : Target
	{
		private readonly IHarvestTool m_Tool;
		private readonly HarvestSystem m_System;

		public HarvestTarget(IHarvestTool tool, HarvestSystem system) : base(-1, true, TargetFlags.None)
		{
			m_Tool = tool;
			m_System = system;

			DisallowMultis = true;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_System is Mining)
			{
				if (targeted is TreasureMap tmap)
				{
					tmap.OnBeginDig(from);
					return;
				}
			}
			else if (m_System is Lumberjacking)
			{
				if (targeted is IChopable chop)
				{
					chop.OnChop(from);
					return;
				}

				if (targeted is ICarvable carvable)
				{
					carvable.Carve(from, m_Tool as Item);
					return;
				}

				if (targeted is Item item)
				{
					if (targeted is IAxe axable && m_Tool is BaseAxe axe)
					{
						if (!item.IsChildOf(from.Backpack))
						{
							from.SendLocalizedMessage(1062334); // This item must be in your backpack to be used.
						}
						else if (axable.Axe(from, axe))
						{
							from.PlaySound(0x13E);
						}

						return;
					}

					if (FurnitureAttribute.Check(item))
					{
						DestroyFurniture(from, item);
						return;
					}
				}
			}

			m_System.StartHarvesting(from, m_Tool, targeted);
		}

		private static void DestroyFurniture(Mobile from, Item item)
		{
			if (!from.InRange(item.GetWorldLocation(), 3))
			{
				from.SendLocalizedMessage(500446); // That is too far away.
				return;
			}

			if (!item.IsChildOf(from.Backpack) && !item.Movable)
			{
				from.SendLocalizedMessage(500462); // You can't destroy that while it is here.
				return;
			}

			from.SendLocalizedMessage(500461); // You destroy the item.

			Effects.PlaySound(item.GetWorldLocation(), item.Map, 0x3B3);

			if (item is Container c)
			{
				if (c is TrapableContainer tc)
				{
					_ = tc.ExecuteTrap(from);
				}

				c.Destroy();
			}
			else
			{
				item.Delete();
			}
		}
	}

	public class HarvestSoundTimer : Timer
	{
		private readonly Mobile m_From;
		private readonly IHarvestTool m_Tool;
		private readonly HarvestSystem m_System;
		private readonly HarvestDefinition m_Definition;
		private readonly object m_ToHarvest, m_Locked;
		private readonly bool m_Last;

		public HarvestSoundTimer(Mobile from, IHarvestTool tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked, bool last) : base(def.EffectSoundDelay)
		{
			m_From = from;
			m_Tool = tool;
			m_System = system;
			m_Definition = def;
			m_ToHarvest = toHarvest;
			m_Locked = locked;
			m_Last = last;
		}

		protected override void OnTick()
		{
			m_System.DoHarvestingSound(m_From, m_Tool, m_Definition, m_ToHarvest);

			if (m_Last)
			{
				m_System.FinishHarvesting(m_From, m_Tool, m_Definition, m_ToHarvest, m_Locked);
			}
		}
	}

	public class HarvestResource
	{
		public Type[] Types { get; set; }
		public double ReqSkill { get; set; }
		public double MinSkill { get; set; }
		public double MaxSkill { get; set; }
		public object SuccessMessage { get; }

		public void SendSuccessTo(Mobile m)
		{
			if (SuccessMessage is int)
			{
				m.SendLocalizedMessage((int)SuccessMessage);
			}
			else if (SuccessMessage is string)
			{
				m.SendMessage((string)SuccessMessage);
			}
		}

		public HarvestResource(double reqSkill, double minSkill, double maxSkill, object message, params Type[] types)
		{
			ReqSkill = reqSkill;
			MinSkill = minSkill;
			MaxSkill = maxSkill;
			Types = types;
			SuccessMessage = message;
		}
	}

	public class BonusHarvestResource
	{
		public Type Type { get; set; }
		public double ReqSkill { get; set; }
		public double Chance { get; set; }

		public TextDefinition SuccessMessage { get; }

		public void SendSuccessTo(Mobile m)
		{
			TextDefinition.SendMessageTo(m, SuccessMessage);
		}

		public BonusHarvestResource(double reqSkill, double chance, TextDefinition message, Type type)
		{
			ReqSkill = reqSkill;

			Chance = chance;
			Type = type;
			SuccessMessage = message;
		}
	}

	public class HarvestTimer : Timer
	{
		private readonly Mobile m_From;
		private readonly IHarvestTool m_Tool;
		private readonly HarvestSystem m_System;
		private readonly HarvestDefinition m_Definition;
		private readonly object m_ToHarvest, m_Locked;
		private int m_Index;
		private readonly int m_Count;

		public HarvestTimer(Mobile from, IHarvestTool tool, HarvestSystem system, HarvestDefinition def, object toHarvest, object locked) : base(TimeSpan.Zero, def.EffectDelay)
		{
			m_From = from;
			m_Tool = tool;
			m_System = system;
			m_Definition = def;
			m_ToHarvest = toHarvest;
			m_Locked = locked;
			m_Count = Utility.RandomList(def.EffectCounts);
		}

		protected override void OnTick()
		{
			if (!m_System.OnHarvesting(m_From, m_Tool, m_Definition, m_ToHarvest, m_Locked, ++m_Index == m_Count))
			{
				Stop();
			}
		}
	}

	public sealed class HarvestNodes : IEnumerable<HarvestNode>
	{
		private static readonly Type _FishingSystem = typeof(Fishing);
		private static readonly Type _MiningSystem = typeof(Mining);
		private static readonly Type _LumberjackingSystem = typeof(Lumberjacking);

		private Dictionary<Type, HarvestNode> _Nodes;

		public HarvestNode this[Type type]
		{
			get
			{
				_Nodes ??= new();

				if (!_Nodes.TryGetValue(type, out var types))
				{
					_Nodes[type] = types = new(type);
				}

				return types;
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public HarvestNode FishingNodes => this[_FishingSystem];

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public HarvestNode MiningNodes => this[_MiningSystem];

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public HarvestNode LumberjackingNodes => this[_LumberjackingSystem];

		public HarvestNodes()
		{
		}

		public HarvestNodes(GenericReader reader)
		{
			Deserialize(reader);
		}

		public IEnumerator<HarvestNode> GetEnumerator()
		{
			if (_Nodes?.Count > 0)
			{
				foreach (var node in _Nodes.Values)
				{
					yield return node;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteEncodedInt(0);

			if (_Nodes?.Count > 0)
			{
				writer.WriteEncodedInt(_Nodes.Count);

				foreach (var (harvest, types) in _Nodes)
				{
					if (types?.Count > 0)
					{
						writer.Write(true);

						writer.WriteObjectType(harvest);

						types.Serialize(writer);
					}
					else
					{
						writer.Write(false);
					}
				}
			}
			else
			{
				writer.WriteEncodedInt(0);
			}
		}

		public void Deserialize(GenericReader reader)
		{
			_ = reader.ReadEncodedInt();

			var count = reader.ReadEncodedInt();

			while (--count >= 0)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				var harvest = reader.ReadObjectType();

				if (harvest != null)
				{
					_Nodes ??= new();

					if (!_Nodes.TryGetValue(harvest, out var types))
					{
						_Nodes[harvest] = new(harvest, reader);
					}
					else
					{
						types.Deserialize(reader);
					}
				}
				else
				{
					_ = new HarvestNode(null, reader);
				}
			}
		}
	}

	public sealed class HarvestNode : TypeAmounts
	{
		public override int DefaultAmountMin => 0;

		public Type Harvest { get; }

		public HarvestNode(Type harvest)
		{
			Harvest = harvest;
		}

		public HarvestNode(Type harvest, GenericReader reader)
			: base(reader)
		{
			Harvest = harvest;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();
		}
	}
}