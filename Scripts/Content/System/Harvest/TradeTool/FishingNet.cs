using Server.Mobiles;
using Server.Multis;
using Server.Spells;
using Server.Targeting;

using System;

namespace Server.Items
{
	public class SpecialFishingNet : Item
	{
		public override int LabelNumber => 1041079;  // a special fishing net

		[CommandProperty(AccessLevel.GameMaster)]
		public bool InUse { get; set; }

		[Constructable]
		public SpecialFishingNet() : base(0x0DCA)
		{
			Weight = 1.0;

			if (0.01 > Utility.RandomDouble())
			{
				Hue = Utility.RandomList(m_Hues);
			}
			else
			{
				Hue = 0x8A0;
			}
		}

		private static readonly int[] m_Hues = new int[]
			{
				0x09B,
				0x0CD,
				0x0D3,
				0x14D,
				0x1DD,
				0x1E9,
				0x1F4,
				0x373,
				0x451,
				0x47F,
				0x489,
				0x492,
				0x4B5,
				0x8AA
			};

		public SpecialFishingNet(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			AddNetProperties(list);
		}

		protected virtual void AddNetProperties(ObjectPropertyList list)
		{
			// as if the name wasn't enough..
			list.Add(1017410); // Special Fishing Net
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write(InUse);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						InUse = reader.ReadBool();

						if (InUse)
						{
							Delete();
						}

						break;
					}
			}

			Stackable = false;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (InUse)
			{
				from.SendLocalizedMessage(1010483); // Someone is already using that net!
			}
			else if (IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1010484); // Where do you wish to use the net?
				_ = from.BeginTarget(-1, true, TargetFlags.None, new TargetCallback(OnTarget));
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public virtual bool RequireDeepWater => true;

		public void OnTarget(Mobile from, object obj)
		{
			if (Deleted || InUse)
			{
				return;
			}

			var map = from.Map;

			if (map == null || map == Map.Internal)
			{
				return;
			}

			if (obj is not IPoint3D i3d)
			{
				return;
			}

			var p3D = new Point3D(i3d);

			if (BaseHouse.FindHouseAt(p3D, map, 0) != null)
			{
				return;
			}
			
			if (!from.InRange(p3D, 6))
			{
				from.SendLocalizedMessage(500976); // You need to be closer to the water to fish!
			}
			else if (!from.InLOS(obj))
			{
				from.SendLocalizedMessage(500979); // You cannot see that location.
			}
			else
			{
				var valid = false;
				var fresh = false;
				var z = p3D.Z;

				if (RequireDeepWater)
				{
					valid = WaterUtility.FullDeepValidation(map, p3D, ref z, out fresh);
				}
				else
				{
					valid = WaterUtility.ValidateWater(map, obj, ref z, out fresh);
				}

				if (valid)
				{
					p3D.Z = z;

					if (GetType() == typeof(SpecialFishingNet))
					{
						for (var i = 1; i < Amount; ++i) // these were stackable before, doh
						{
							_ = from.AddToBackpack(new SpecialFishingNet());
						}
					}

					InUse = true;
					Movable = false;

					MoveToWorld(p3D, map);

					SpellHelper.Turn(from, p3D);

					from.Animate(12, 5, 1, true, false, 0);

					Effects.SendLocationEffect(p3D, map, 0x352D, 16, 4);
					Effects.PlaySound(p3D, map, 0x364);

					_EffectIndex = 0;

					_ = Timer.DelayCall(TimeSpan.FromSeconds(1.00), TimeSpan.FromSeconds(1.25), 14, DoEffect, p3D, from);

					from.SendLocalizedMessage(RequireDeepWater ? 1010487 : 1074492); // You plunge the net into the sea... / You plunge the net into the water...
				}
				else
				{
					from.SendLocalizedMessage(RequireDeepWater ? 1010485 : 1074491); // You can only use this net in deep water! / You can only use this net in water!
				}
			}
		}

		private int _EffectIndex;

		private void DoEffect(Point3D p, Mobile from)
		{
			if (Deleted)
			{
				return;
			}

			if (++_EffectIndex == 1)
			{
				Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
				Effects.PlaySound(p, Map, 0x364);
			}
			else if (_EffectIndex is <= 7 or 14)
			{
				if (RequireDeepWater)
				{
					for (var i = 0; i < 3; ++i)
					{
						int x, y;

						switch (Utility.Random(8))
						{
							default:
							case 0: x = -1; y = -1; break;
							case 1: x = -1; y = 0; break;
							case 2: x = -1; y = +1; break;
							case 3: x = 0; y = -1; break;
							case 4: x = 0; y = +1; break;
							case 5: x = +1; y = -1; break;
							case 6: x = +1; y = 0; break;
							case 7: x = +1; y = +1; break;
						}

						Effects.SendLocationEffect(new Point3D(p.X + x, p.Y + y, p.Z), Map, 0x352D, 16, 4);
					}
				}
				else
				{
					Effects.SendLocationEffect(p, Map, 0x352D, 16, 4);
				}

				if (Utility.RandomBool())
				{
					Effects.PlaySound(p, Map, 0x364);
				}

				if (_EffectIndex >= 14)
				{
					FinishEffect(p, Map, from);
				}
				else
				{
					--Z;
				}
			}
		}

		protected virtual int GetSpawnCount()
		{
			var count = Utility.RandomMinMax(1, 3);

			if (Hue != 0x8A0)
			{
				count += Utility.RandomMinMax(1, 2);
			}

			return count;
		}

		protected static void Spawn(Point3D p, Map map, BaseCreature spawn)
		{
			if (map == null)
			{
				spawn.Delete();
				return;
			}

			int x = p.X, y = p.Y;

			for (var j = 0; j < 20; ++j)
			{
				var tx = p.X - 2 + Utility.Random(5);
				var ty = p.Y - 2 + Utility.Random(5);

				var t = map.Tiles.GetLandTile(tx, ty);

				if (t.Z == p.Z && ((t.ID >= 0xA8 && t.ID <= 0xAB) || (t.ID >= 0x136 && t.ID <= 0x137)) && !SpellHelper.CheckMulti(new Point3D(tx, ty, p.Z), map))
				{
					x = tx;
					y = ty;
					break;
				}
			}

			spawn.MoveToWorld(new Point3D(x, y, p.Z), map);

			if (spawn is Kraken && 0.2 > Utility.RandomDouble())
			{
				spawn.PackItem(new MessageInABottle(map == Map.Felucca ? Map.Felucca : Map.Trammel));
			}
		}

		protected virtual void FinishEffect(Point3D p, Map map, Mobile from)
		{
			from.RevealingAction();

			var count = GetSpawnCount();

			for (var i = 0; map != null && i < count; ++i)
			{
				BaseCreature spawn = Utility.Random(4) switch
				{
					1 => new DeepSeaSerpent(),
					2 => new WaterElemental(),
					3 => new Kraken(),
					_ => new SeaSerpent(),
				};
				Spawn(p, map, spawn);

				spawn.Combatant = from;
			}

			Delete();
		}
	}

	public class FabledFishingNet : SpecialFishingNet
	{
		public override int LabelNumber => 1063451;  // a fabled fishing net

		[Constructable]
		public FabledFishingNet()
		{
			Hue = 0x481;
		}

		protected override void AddNetProperties(ObjectPropertyList list)
		{
		}

		protected override int GetSpawnCount()
		{
			return base.GetSpawnCount() + 4;
		}

		protected override void FinishEffect(Point3D p, Map map, Mobile from)
		{
			switch (Utility.Random(1))
			{
				default:
				case 0: Spawn(p, map, new Leviathan(from)); break;
			}

			base.FinishEffect(p, map, from);
		}

		public FabledFishingNet(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	#region Aquarium Fish Net

	public partial class AquariumFishNet : SpecialFishingNet
	{
		public override int LabelNumber => 1074463;  // An aquarium fishing net

		[Constructable]
		public AquariumFishNet()
		{
			ItemID = 0xDC8;

			if (Hue == 0x8A0)
			{
				Hue = 0x240;
			}
		}

		protected override void AddNetProperties(ObjectPropertyList list)
		{
		}

		public override bool RequireDeepWater => false;

		protected override void FinishEffect(Point3D p, Map map, Mobile from)
		{
			if (from.Skills.Fishing.Value < 10)
			{
				from.SendLocalizedMessage(1074487); // The creatures are too quick for you!
			}
			else
			{
				var fish = GiveFish(from);
				var bowl = Aquarium.GetEmptyBowl(from);

				if (bowl != null)
				{
					fish.StopTimer();
					bowl.AddItem(fish);
					from.SendLocalizedMessage(1074489); // A live creature jumps into the fish bowl in your pack!
					Delete();
					return;
				}
				else
				{
					if (from.PlaceInBackpack(fish))
					{
						from.PlaySound(0x5A2);
						from.SendLocalizedMessage(1074490); // A live creature flops around in your pack before running out of air.

						fish.Kill();
						Delete();
						return;
					}
					else
					{
						fish.Delete();

						from.SendLocalizedMessage(1074488); // You could not hold the creature.
					}
				}
			}

			InUse = false;
			Movable = true;

			if (!from.PlaceInBackpack(this))
			{
				if (from.Map == null || from.Map == Map.Internal)
				{
					Delete();
				}
				else
				{
					MoveToWorld(from.Location, from.Map);
				}
			}
		}

		public AquariumFishNet(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	// Legacy code
	public class AquariumFishingNet : Item
	{
		public override int LabelNumber => 1074463;  // An aquarium fishing net

		public AquariumFishingNet()
		{
		}

		public AquariumFishingNet(Serial serial) : base(serial)
		{
		}

		private Item CreateReplacement()
		{
			var result = new AquariumFishNet();

			Commands.Dupe.CopyProperties(result, this);

			return result;
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (!IsChildOf(from.Backpack))
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
				return;
			}

			var replacement = CreateReplacement();

			if (!from.PlaceInBackpack(replacement))
			{
				replacement.Delete();
				from.SendLocalizedMessage(500720); // You don't have enough room in your backpack!
			}
			else
			{
				Delete();
				from.Use(replacement);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);
			_ = reader.ReadInt();
		}
	}

	#endregion
}