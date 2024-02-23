using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Engines.BulkOrders
{
	public enum BulkMaterialType
	{
		None,
		DullCopper,
		ShadowIron,
		Copper,
		Bronze,
		Gold,
		Agapite,
		Verite,
		Valorite,
		Spined,
		Horned,
		Barbed
	}

	public enum BulkGenericType
	{
		Iron,
		Cloth,
		Leather
	}

	public class BGTClassifier
	{
		public static BulkGenericType Classify(BODType deedType, Type itemType)
		{
			if (deedType == BODType.Tailor)
			{
				if (itemType == null || itemType.IsSubclassOf(typeof(BaseArmor)) || itemType.IsSubclassOf(typeof(BaseShoes)))
				{
					return BulkGenericType.Leather;
				}

				return BulkGenericType.Cloth;
			}

			return BulkGenericType.Iron;
		}
	}

	#region Small Bulk Order Deeds

	[TypeAlias("Scripts.Engines.BulkOrders.SmallBOD")]
	public abstract class SmallBOD : Item
	{
		private int m_AmountCur, m_AmountMax;
		private Type m_Type;
		private int m_Number;
		private int m_Graphic;
		private bool m_RequireExceptional;
		private BulkMaterialType m_Material;

		[CommandProperty(AccessLevel.GameMaster)]
		public int AmountCur { get => m_AmountCur; set { m_AmountCur = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int AmountMax { get => m_AmountMax; set { m_AmountMax = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Type Type { get => m_Type; set => m_Type = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Number { get => m_Number; set { m_Number = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Graphic { get => m_Graphic; set => m_Graphic = value; }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RequireExceptional { get => m_RequireExceptional; set { m_RequireExceptional = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public BulkMaterialType Material { get => m_Material; set { m_Material = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Complete => (m_AmountCur == m_AmountMax);

		public override int LabelNumber => 1045151;  // a bulk order deed

		[Constructable]
		public SmallBOD(int hue, int amountMax, Type type, int number, int graphic, bool requireExeptional, BulkMaterialType material) : base(Core.AOS ? 0x2258 : 0x14EF)
		{
			Weight = 1.0;
			Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
			LootType = LootType.Blessed;

			m_AmountMax = amountMax;
			m_Type = type;
			m_Number = number;
			m_Graphic = graphic;
			m_RequireExceptional = requireExeptional;
			m_Material = material;
		}

		public SmallBOD() : base(Core.AOS ? 0x2258 : 0x14EF)
		{
			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public static BulkMaterialType GetRandomMaterial(BulkMaterialType start, double[] chances)
		{
			var random = Utility.RandomDouble();

			for (var i = 0; i < chances.Length; ++i)
			{
				if (random < chances[i])
				{
					return (i == 0 ? BulkMaterialType.None : start + (i - 1));
				}

				random -= chances[i];
			}

			return BulkMaterialType.None;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1060654); // small bulk order

			if (m_RequireExceptional)
			{
				list.Add(1045141); // All items must be exceptional.
			}

			if (m_Material != BulkMaterialType.None)
			{
				list.Add(SmallBODGump.GetMaterialNumberFor(m_Material)); // All items must be made with x material.
			}

			list.Add(1060656, m_AmountMax.ToString()); // amount to make: ~1_val~
			list.Add(1060658, "#{0}\t{1}", m_Number, m_AmountCur); // ~1_val~: ~2_val~
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || InSecureTrade || RootParent is PlayerVendor)
			{
				from.SendGump(new SmallBODGump(from, this));
			}
			else
			{
				from.SendLocalizedMessage(1045156); // You must have the deed in your backpack to use it.
			}
		}

		public override void OnDoubleClickNotAccessible(Mobile from)
		{
			OnDoubleClick(from);
		}

		public override void OnDoubleClickSecureTrade(Mobile from)
		{
			OnDoubleClick(from);
		}

		public void BeginCombine(Mobile from)
		{
			if (m_AmountCur < m_AmountMax)
			{
				from.Target = new SmallBODTarget(this);
			}
			else
			{
				from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
			}
		}

		public abstract List<Item> ComputeRewards(bool full);
		public abstract int ComputeGold();
		public abstract int ComputeFame();

		public virtual void GetRewards(out Item reward, out int gold, out int fame)
		{
			reward = null;
			gold = ComputeGold();
			fame = ComputeFame();

			var rewards = ComputeRewards(false);

			if (rewards.Count > 0)
			{
				reward = rewards[Utility.Random(rewards.Count)];

				for (var i = 0; i < rewards.Count; ++i)
				{
					if (rewards[i] != reward)
					{
						rewards[i].Delete();
					}
				}
			}
		}

		public static BulkMaterialType GetMaterial(CraftResource resource)
		{
			switch (resource)
			{
				case CraftResource.DullCopper: return BulkMaterialType.DullCopper;
				case CraftResource.ShadowIron: return BulkMaterialType.ShadowIron;
				case CraftResource.Copper: return BulkMaterialType.Copper;
				case CraftResource.Bronze: return BulkMaterialType.Bronze;
				case CraftResource.Gold: return BulkMaterialType.Gold;
				case CraftResource.Agapite: return BulkMaterialType.Agapite;
				case CraftResource.Verite: return BulkMaterialType.Verite;
				case CraftResource.Valorite: return BulkMaterialType.Valorite;
				case CraftResource.SpinedLeather: return BulkMaterialType.Spined;
				case CraftResource.HornedLeather: return BulkMaterialType.Horned;
				case CraftResource.BarbedLeather: return BulkMaterialType.Barbed;
			}

			return BulkMaterialType.None;
		}

		public void EndCombine(Mobile from, object o)
		{
			if (o is Item item && item.IsChildOf(from.Backpack))
			{
				var objectType = item.GetType();

				if (m_AmountCur >= m_AmountMax)
				{
					from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
				}
				else if (m_Type == null || (objectType != m_Type && !objectType.IsSubclassOf(m_Type)) || (!(item is BaseWeapon) && !(item is BaseArmor) && !(item is BaseClothing)))
				{
					from.SendLocalizedMessage(1045169); // The item is not in the request.
				}
				else
				{
					var material = BulkMaterialType.None;

					if (item is BaseArmor a)
					{
						material = GetMaterial(a.Resource);
					}
					else if (item is BaseClothing c)
					{
						material = GetMaterial(c.Resource);
					}

					if (m_Material >= BulkMaterialType.DullCopper && m_Material <= BulkMaterialType.Valorite && material != m_Material)
					{
						from.SendLocalizedMessage(1045168); // The item is not made from the requested ore.
					}
					else if (m_Material >= BulkMaterialType.Spined && m_Material <= BulkMaterialType.Barbed && material != m_Material)
					{
						from.SendLocalizedMessage(1049352); // The item is not made from the requested leather type.
					}
					else
					{
						var isExceptional = false;

						if (item is BaseWeapon bw)
						{
							isExceptional = bw.Quality == ItemQuality.Exceptional;
						}
						else if (item is BaseArmor ba)
						{
							isExceptional = ba.Quality == ItemQuality.Exceptional;
						}
						else if (item is BaseClothing bc)
						{
							isExceptional = bc.Quality == ItemQuality.Exceptional;
						}

						if (m_RequireExceptional && !isExceptional)
						{
							from.SendLocalizedMessage(1045167); // The item must be exceptional.
						}
						else
						{
							item.Delete();

							++AmountCur;

							from.SendLocalizedMessage(1045170); // The item has been combined with the deed.

							from.SendGump(new SmallBODGump(from, this));

							if (m_AmountCur < m_AmountMax)
							{
								BeginCombine(from);
							}
						}
					}
				}
			}
			else
			{
				from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
			}
		}

		public SmallBOD(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_AmountCur);
			writer.Write(m_AmountMax);
			writer.Write(m_Type?.FullName);
			writer.Write(m_Number);
			writer.Write(m_Graphic);
			writer.Write(m_RequireExceptional);
			writer.Write((int)m_Material);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_AmountCur = reader.ReadInt();
						m_AmountMax = reader.ReadInt();

						var type = reader.ReadString();

						if (type != null)
						{
							m_Type = ScriptCompiler.FindTypeByFullName(type);
						}

						m_Number = reader.ReadInt();
						m_Graphic = reader.ReadInt();
						m_RequireExceptional = reader.ReadBool();
						m_Material = (BulkMaterialType)reader.ReadInt();

						break;
					}
			}

			if (Weight == 0.0)
			{
				Weight = 1.0;
			}

			if (Core.AOS && ItemID == 0x14EF)
			{
				ItemID = 0x2258;
			}

			if (Parent == null && Map == Map.Internal && Location == Point3D.Zero)
			{
				Delete();
			}
		}
	}

	public class SmallBODGump : Gump
	{
		private readonly SmallBOD m_Deed;
		private readonly Mobile m_From;

		public SmallBODGump(Mobile from, SmallBOD deed) : base(25, 25)
		{
			m_From = from;
			m_Deed = deed;

			m_From.CloseGump(typeof(LargeBODGump));
			m_From.CloseGump(typeof(SmallBODGump));

			AddPage(0);

			AddBackground(50, 10, 455, 260, 5054);
			AddImageTiled(58, 20, 438, 241, 2624);
			AddAlphaRegion(58, 20, 438, 241);

			AddImage(45, 5, 10460);
			AddImage(480, 5, 10460);
			AddImage(45, 245, 10460);
			AddImage(480, 245, 10460);

			AddHtmlLocalized(225, 25, 120, 20, 1045133, 0x7FFF, false, false); // A bulk order

			AddHtmlLocalized(75, 48, 250, 20, 1045138, 0x7FFF, false, false); // Amount to make:
			AddLabel(275, 48, 1152, deed.AmountMax.ToString());

			AddHtmlLocalized(275, 76, 200, 20, 1045153, 0x7FFF, false, false); // Amount finished:
			AddHtmlLocalized(75, 72, 120, 20, 1045136, 0x7FFF, false, false); // Item requested:

			AddItem(410, 72, deed.Graphic);

			AddHtmlLocalized(75, 96, 210, 20, deed.Number, 0x7FFF, false, false);
			AddLabel(275, 96, 0x480, deed.AmountCur.ToString());

			if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(75, 120, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:
			}

			if (deed.RequireExceptional)
			{
				AddHtmlLocalized(75, 144, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
			}

			if (deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(75, deed.RequireExceptional ? 168 : 144, 300, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
			}

			AddButton(125, 192, 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(160, 192, 300, 20, 1045154, 0x7FFF, false, false); // Combine this deed with the item requested.

			AddButton(125, 216, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(160, 216, 120, 20, 1011441, 0x7FFF, false, false); // EXIT
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Deed.Deleted || !m_Deed.IsChildOf(m_From.Backpack))
			{
				return;
			}

			if (info.ButtonID == 2) // Combine
			{
				m_From.SendGump(new SmallBODGump(m_From, m_Deed));
				m_Deed.BeginCombine(m_From);
			}
		}

		public static int GetMaterialNumberFor(BulkMaterialType material)
		{
			if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
			{
				return 1045142 + (material - BulkMaterialType.DullCopper);
			}
			else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
			{
				return 1049348 + (material - BulkMaterialType.Spined);
			}

			return 0;
		}
	}

	public class SmallBODAcceptGump : Gump
	{
		private readonly SmallBOD m_Deed;
		private readonly Mobile m_From;

		public SmallBODAcceptGump(Mobile from, SmallBOD deed) : base(50, 50)
		{
			m_From = from;
			m_Deed = deed;

			m_From.CloseGump(typeof(LargeBODAcceptGump));
			m_From.CloseGump(typeof(SmallBODAcceptGump));

			AddPage(0);

			AddBackground(25, 10, 430, 264, 5054);

			AddImageTiled(33, 20, 413, 245, 2624);
			AddAlphaRegion(33, 20, 413, 245);

			AddImage(20, 5, 10460);
			AddImage(430, 5, 10460);
			AddImage(20, 249, 10460);
			AddImage(430, 249, 10460);

			AddHtmlLocalized(190, 25, 120, 20, 1045133, 0x7FFF, false, false); // A bulk order
			AddHtmlLocalized(40, 48, 350, 20, 1045135, 0x7FFF, false, false); // Ah!  Thanks for the goods!  Would you help me out?

			AddHtmlLocalized(40, 72, 210, 20, 1045138, 0x7FFF, false, false); // Amount to make:
			AddLabel(250, 72, 1152, deed.AmountMax.ToString());

			AddHtmlLocalized(40, 96, 120, 20, 1045136, 0x7FFF, false, false); // Item requested:
			AddItem(385, 96, deed.Graphic);
			AddHtmlLocalized(40, 120, 210, 20, deed.Number, 0x7FFF, false, false);

			if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(40, 144, 210, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:

				if (deed.RequireExceptional)
				{
					AddHtmlLocalized(40, 168, 350, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
				}

				if (deed.Material != BulkMaterialType.None)
				{
					AddHtmlLocalized(40, deed.RequireExceptional ? 192 : 168, 350, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
				}
			}

			AddHtmlLocalized(40, 216, 350, 20, 1045139, 0x7FFF, false, false); // Do you want to accept this order?

			AddButton(100, 240, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(135, 240, 120, 20, 1006044, 0x7FFF, false, false); // Ok

			AddButton(275, 240, 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(310, 240, 120, 20, 1011012, 0x7FFF, false, false); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1) // Ok
			{
				if (m_From.PlaceInBackpack(m_Deed))
				{
					m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.
				}
				else
				{
					m_From.SendLocalizedMessage(1045150); // There is not enough room in your backpack for the deed.
					m_Deed.Delete();
				}
			}
			else
			{
				m_Deed.Delete();
			}
		}

		public static int GetMaterialNumberFor(BulkMaterialType material)
		{
			if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
			{
				return 1045142 + (material - BulkMaterialType.DullCopper);
			}
			else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
			{
				return 1049348 + (material - BulkMaterialType.Spined);
			}

			return 0;
		}
	}

	public class SmallBulkEntry
	{
		private static readonly Dictionary<BODType, Dictionary<string, SmallBulkEntry[]>> m_Entries = new()
		{
			#region Blacksmith

			[BODType.Smith] = new()
			{
				#region Weapons

				["Weapons"] = new SmallBulkEntry[]
				{
					new(typeof(Axe), 0x0F49),
					new(typeof(BattleAxe), 0x0F47),
					new(typeof(Dagger), 0x0F52),
					new(typeof(DoubleAxe), 0x0F4B),
					new(typeof(ExecutionersAxe), 0x0F45),
					new(typeof(LargeBattleAxe), 0x13FB),
					new(typeof(TwoHandedAxe), 0x1443),
					new(typeof(WarAxe), 0x13B0),
					new(typeof(HammerPick), 0x143D),
					new(typeof(Mace), 0x0F5C),
					new(typeof(Maul), 0x143B),
					new(typeof(WarHammer), 0x1439),
					new(typeof(WarMace), 0x1407),
					new(typeof(Bardiche), 0x0F4D),
					new(typeof(Halberd), 0x143E),
					new(typeof(ShortSpear), 0x1403),
					new(typeof(Spear), 0x0F62),
					new(typeof(WarFork), 0x1405),
					new(typeof(Broadsword), 0x0F5E),
					new(typeof(Cutlass), 0x1441),
					new(typeof(Katana), 0x13FF),
					new(typeof(Kryss), 0x1401),
					new(typeof(Longsword), 0x0F61),
					new(typeof(Scimitar), 0x13B6),
					new(typeof(VikingSword), 0x13B9),
				},

				#endregion

				#region Armor

				["Armor"] = new SmallBulkEntry[]
				{
					// Armor
					new(typeof(ChainChest), 0x13BF),
					new(typeof(ChainLegs), 0x13BE),
					new(typeof(Bascinet), 0x140C),
					new(typeof(ChainCoif), 0x13BB),
					new(typeof(CloseHelm), 0x1408),
					new(typeof(Helmet), 0x140A),
					new(typeof(NorseHelm), 0x140E),
					new(typeof(PlateHelm), 0x1412),
					new(typeof(FemalePlateChest), 0x1C04),
					new(typeof(PlateArms), 0x1410),
					new(typeof(PlateChest), 0x1415),
					new(typeof(PlateGloves), 0x1414),
					new(typeof(PlateGorget), 0x1413),
					new(typeof(PlateLegs), 0x1411),
					new(typeof(RingmailArms), 0x13EE),
					new(typeof(RingmailChest), 0x13EC),
					new(typeof(RingmailGloves), 0x13EB),
					new(typeof(RingmailLegs), 0x13F0),

					// Shields
					new(typeof(BronzeShield), 0x1B72),
					new(typeof(Buckler), 0x1B73),
					new(typeof(HeaterShield), 0x1B76),
					new(typeof(MetalKiteShield), 0x1B74),
					new(typeof(MetalShield), 0x1B7B),
					new(typeof(WoodenKiteShield), 0x1B78),
				},

				#endregion
			},

			#endregion

			#region Tailor

			[BODType.Tailor] = new()
			{
				#region Cloth

				["Cloth"] = new SmallBulkEntry[]
				{
					#region Misc

					new(typeof(SkullCap), 0x1544),
					new(typeof(Bandana), 0x1540),
					new(typeof(FloppyHat), 0x1713),
					new(typeof(Cap), 0x1715),
					new(typeof(WideBrimHat), 0x1714),
					new(typeof(StrawHat), 0x1717),
					new(typeof(TallStrawHat), 0x1716),
					new(typeof(WizardsHat), 0x1718),
					new(typeof(Bonnet), 0x1719),
					new(typeof(FeatheredHat), 0x171A),
					new(typeof(TricorneHat), 0x171B),
					new(typeof(JesterHat), 0x171C),
					new(typeof(Doublet), 0x1F7B),
					new(typeof(Shirt), 0x1517),
					new(typeof(FancyShirt), 0x1EFD),
					new(typeof(Tunic), 0x1FA1),
					new(typeof(Surcoat), 0x1FFD),
					new(typeof(PlainDress), 0x1F01),
					new(typeof(FancyDress), 0x1EFF),
					new(typeof(Cloak), 0x1515),
					new(typeof(Robe), 0x1F03),
					new(typeof(JesterSuit), 0x1F9F),
					new(typeof(ShortPants), 0x152E),
					new(typeof(LongPants), 0x1539),
					new(typeof(Kilt), 0x1537),
					new(typeof(Skirt), 0x1516),
					new(typeof(BodySash), 0x1541),
					new(typeof(HalfApron), 0x153B),
					new(typeof(FullApron), 0x153D),

					#endregion
				},

				#endregion

				#region Leather

				["Leather"] = new SmallBulkEntry[]
				{
					#region Normal Armor

					new(typeof(LeatherGorget), 0x13C7),
					new(typeof(LeatherCap), 0x1DB9),
					new(typeof(LeatherGloves), 0x13C6),
					new(typeof(LeatherArms), 0x13CD),
					new(typeof(LeatherLegs), 0x13CB),
					new(typeof(LeatherChest), 0x13CC),

					#endregion

					#region Studded Armor

					new(typeof(StuddedGorget), 0x13D6),
					new(typeof(StuddedGloves), 0x13D5),
					new(typeof(StuddedArms), 0x13DC),
					new(typeof(StuddedLegs), 0x13DA),
					new(typeof(StuddedChest), 0x13DB),

					#endregion

					#region Bone Armor

					new(typeof(BoneHelm), 0x1451),
					new(typeof(BoneGloves), 0x1450),
					new(typeof(BoneArms), 0x144E),
					new(typeof(BoneLegs), 0x1452),
					new(typeof(BoneChest), 0x144F),

					#endregion

					#region Female Armor

					new(typeof(LeatherShorts), 0x1C00),
					new(typeof(LeatherSkirt), 0x1C08),
					new(typeof(LeatherBustierArms), 0x1C0A),
					new(typeof(StuddedBustierArms), 0x1C0C),
					new(typeof(FemaleLeatherChest), 0x1C06),
					new(typeof(FemaleStuddedChest), 0x1C02),

					#endregion

					#region Shoes

					new(typeof(Boots), 0x170B),
					new(typeof(ThighBoots), 0x1711),
					new(typeof(Shoes), 0x170F),
					new(typeof(Sandals), 0x170D),

					#endregion
				},

				#endregion
			}

			#endregion
		};

		public static SmallBulkEntry[] BlacksmithWeapons => GetEntries(BODType.Smith, "Weapons");
		public static SmallBulkEntry[] BlacksmithArmor => GetEntries(BODType.Smith, "Armor");

		public static SmallBulkEntry[] TailorCloth => GetEntries(BODType.Tailor, "Cloth");
		public static SmallBulkEntry[] TailorLeather => GetEntries(BODType.Tailor, "Leather");

		public static SmallBulkEntry[] GetEntries(BODType type, string name)
		{
			if (!m_Entries.TryGetValue(type, out var table))
			{
				return Array.Empty<SmallBulkEntry>();
			}

			if (!table.TryGetValue(name, out var entries))
			{
				return Array.Empty<SmallBulkEntry>();
			}

			return entries;
		}

		public Type Type { get; }
		public int Number { get; }
		public int Graphic { get; }

		public SmallBulkEntry(Type type, int graphic)
			: this(type, graphic < 0x4000 ? 1020000 + graphic : 1078872 + graphic, graphic)
		{ }

		public SmallBulkEntry(Type type, int number, int graphic)
		{
			Type = type;
			Number = number;
			Graphic = graphic;
		}
	}

	public class SmallBODTarget : Target
	{
		private readonly SmallBOD m_Deed;

		public SmallBODTarget(SmallBOD deed) : base(18, false, TargetFlags.None)
		{
			m_Deed = deed;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Deed.Deleted || !m_Deed.IsChildOf(from.Backpack))
			{
				return;
			}

			m_Deed.EndCombine(from, targeted);
		}
	}

	#endregion

	#region Large Bulk Order Deeds

	[TypeAlias("Scripts.Engines.BulkOrders.LargeBOD")]
	public abstract class LargeBOD : Item
	{
		private int m_AmountMax;
		private bool m_RequireExceptional;
		private BulkMaterialType m_Material;
		private LargeBulkEntry[] m_Entries;

		[CommandProperty(AccessLevel.GameMaster)]
		public int AmountMax { get => m_AmountMax; set { m_AmountMax = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool RequireExceptional { get => m_RequireExceptional; set { m_RequireExceptional = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public BulkMaterialType Material { get => m_Material; set { m_Material = value; InvalidateProperties(); } }

		public LargeBulkEntry[] Entries { get => m_Entries; set { m_Entries = value; InvalidateProperties(); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Complete
		{
			get
			{
				for (var i = 0; i < m_Entries.Length; ++i)
				{
					if (m_Entries[i].Amount < m_AmountMax)
					{
						return false;
					}
				}

				return true;
			}
		}

		public abstract List<Item> ComputeRewards(bool full);
		public abstract int ComputeGold();
		public abstract int ComputeFame();

		public virtual void GetRewards(out Item reward, out int gold, out int fame)
		{
			reward = null;
			gold = ComputeGold();
			fame = ComputeFame();

			var rewards = ComputeRewards(false);

			if (rewards.Count > 0)
			{
				reward = rewards[Utility.Random(rewards.Count)];

				for (var i = 0; i < rewards.Count; ++i)
				{
					if (rewards[i] != reward)
					{
						rewards[i].Delete();
					}
				}
			}
		}

		public static BulkMaterialType GetRandomMaterial(BulkMaterialType start, double[] chances)
		{
			var random = Utility.RandomDouble();

			for (var i = 0; i < chances.Length; ++i)
			{
				if (random < chances[i])
				{
					return (i == 0 ? BulkMaterialType.None : start + (i - 1));
				}

				random -= chances[i];
			}

			return BulkMaterialType.None;
		}

		public override int LabelNumber => 1045151;  // a bulk order deed

		public LargeBOD(int hue, int amountMax, bool requireExeptional, BulkMaterialType material, LargeBulkEntry[] entries) : base(Core.AOS ? 0x2258 : 0x14EF)
		{
			Weight = 1.0;
			Hue = hue; // Blacksmith: 0x44E; Tailoring: 0x483
			LootType = LootType.Blessed;

			m_AmountMax = amountMax;
			m_RequireExceptional = requireExeptional;
			m_Material = material;
			m_Entries = entries;
		}

		public LargeBOD() : base(Core.AOS ? 0x2258 : 0x14EF)
		{
			Weight = 1.0;
			LootType = LootType.Blessed;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(1060655); // large bulk order

			if (m_RequireExceptional)
			{
				list.Add(1045141); // All items must be exceptional.
			}

			if (m_Material != BulkMaterialType.None)
			{
				list.Add(LargeBODGump.GetMaterialNumberFor(m_Material)); // All items must be made with x material.
			}

			list.Add(1060656, m_AmountMax.ToString()); // amount to make: ~1_val~

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				list.Add(1060658 + i, "#{0}\t{1}", m_Entries[i].Details.Number, m_Entries[i].Amount); // ~1_val~: ~2_val~
			}
		}

		public override void OnDoubleClickNotAccessible(Mobile from)
		{
			OnDoubleClick(from);
		}

		public override void OnDoubleClickSecureTrade(Mobile from)
		{
			OnDoubleClick(from);
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (IsChildOf(from.Backpack) || InSecureTrade || RootParent is PlayerVendor)
			{
				from.SendGump(new LargeBODGump(from, this));
			}
			else
			{
				from.SendLocalizedMessage(1045156); // You must have the deed in your backpack to use it.
			}
		}

		public void BeginCombine(Mobile from)
		{
			if (!Complete)
			{
				from.Target = new LargeBODTarget(this);
			}
			else
			{
				from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
			}
		}

		public void EndCombine(Mobile from, object o)
		{
			if (o is Item item && item.IsChildOf(from.Backpack))
			{
				if (o is SmallBOD small)
				{
					LargeBulkEntry entry = null;

					for (var i = 0; entry == null && i < m_Entries.Length; ++i)
					{
						if (m_Entries[i].Details.Type == small.Type)
						{
							entry = m_Entries[i];
						}
					}

					if (entry == null)
					{
						from.SendLocalizedMessage(1045160); // That is not a bulk order for this large request.
					}
					else if (m_RequireExceptional && !small.RequireExceptional)
					{
						from.SendLocalizedMessage(1045161); // Both orders must be of exceptional quality.
					}
					else if (m_Material >= BulkMaterialType.DullCopper && m_Material <= BulkMaterialType.Valorite && small.Material != m_Material)
					{
						from.SendLocalizedMessage(1045162); // Both orders must use the same ore type.
					}
					else if (m_Material >= BulkMaterialType.Spined && m_Material <= BulkMaterialType.Barbed && small.Material != m_Material)
					{
						from.SendLocalizedMessage(1049351); // Both orders must use the same leather type.
					}
					else if (m_AmountMax != small.AmountMax)
					{
						from.SendLocalizedMessage(1045163); // The two orders have different requested amounts and cannot be combined.
					}
					else if (small.AmountCur < small.AmountMax)
					{
						from.SendLocalizedMessage(1045164); // The order to combine with is not completed.
					}
					else if (entry.Amount >= m_AmountMax)
					{
						from.SendLocalizedMessage(1045166); // The maximum amount of requested items have already been combined to this deed.
					}
					else
					{
						entry.Amount += small.AmountCur;
						small.Delete();

						from.SendLocalizedMessage(1045165); // The orders have been combined.

						from.SendGump(new LargeBODGump(from, this));

						if (!Complete)
						{
							BeginCombine(from);
						}
					}
				}
				else
				{
					from.SendLocalizedMessage(1045159); // That is not a bulk order.
				}
			}
			else
			{
				from.SendLocalizedMessage(1045158); // You must have the item in your backpack to target it.
			}
		}

		public LargeBOD(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_AmountMax);
			writer.Write(m_RequireExceptional);
			writer.Write((int)m_Material);

			writer.Write(m_Entries.Length);

			for (var i = 0; i < m_Entries.Length; ++i)
			{
				m_Entries[i].Serialize(writer);
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						m_AmountMax = reader.ReadInt();
						m_RequireExceptional = reader.ReadBool();
						m_Material = (BulkMaterialType)reader.ReadInt();

						m_Entries = new LargeBulkEntry[reader.ReadInt()];

						for (var i = 0; i < m_Entries.Length; ++i)
						{
							m_Entries[i] = new LargeBulkEntry(this, reader);
						}

						break;
					}
			}

			if (Weight == 0.0)
			{
				Weight = 1.0;
			}

			if (Core.AOS && ItemID == 0x14EF)
			{
				ItemID = 0x2258;
			}

			if (Parent == null && Map == Map.Internal && Location == Point3D.Zero)
			{
				Delete();
			}
		}
	}

	public class LargeBODGump : Gump
	{
		private readonly LargeBOD m_Deed;
		private readonly Mobile m_From;

		public LargeBODGump(Mobile from, LargeBOD deed) : base(25, 25)
		{
			m_From = from;
			m_Deed = deed;

			m_From.CloseGump(typeof(LargeBODGump));
			m_From.CloseGump(typeof(SmallBODGump));

			var entries = deed.Entries;

			AddPage(0);

			AddBackground(50, 10, 455, 236 + (entries.Length * 24), 5054);

			AddImageTiled(58, 20, 438, 217 + (entries.Length * 24), 2624);
			AddAlphaRegion(58, 20, 438, 217 + (entries.Length * 24));

			AddImage(45, 5, 10460);
			AddImage(480, 5, 10460);
			AddImage(45, 221 + (entries.Length * 24), 10460);
			AddImage(480, 221 + (entries.Length * 24), 10460);

			AddHtmlLocalized(225, 25, 120, 20, 1045134, 0x7FFF, false, false); // A large bulk order

			AddHtmlLocalized(75, 48, 250, 20, 1045138, 0x7FFF, false, false); // Amount to make:
			AddLabel(275, 48, 1152, deed.AmountMax.ToString());

			AddHtmlLocalized(75, 72, 120, 20, 1045137, 0x7FFF, false, false); // Items requested:
			AddHtmlLocalized(275, 76, 200, 20, 1045153, 0x7FFF, false, false); // Amount finished:

			var y = 96;

			for (var i = 0; i < entries.Length; ++i)
			{
				var entry = entries[i];
				var details = entry.Details;

				AddHtmlLocalized(75, y, 210, 20, details.Number, 0x7FFF, false, false);
				AddLabel(275, y, 0x480, entry.Amount.ToString());

				y += 24;
			}

			if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(75, y, 200, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:
				y += 24;
			}

			if (deed.RequireExceptional)
			{
				AddHtmlLocalized(75, y, 300, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
				y += 24;
			}

			if (deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(75, y, 300, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
			}

			AddButton(125, 168 + (entries.Length * 24), 4005, 4007, 2, GumpButtonType.Reply, 0);
			AddHtmlLocalized(160, 168 + (entries.Length * 24), 300, 20, 1045155, 0x7FFF, false, false); // Combine this deed with another deed.

			AddButton(125, 192 + (entries.Length * 24), 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(160, 192 + (entries.Length * 24), 120, 20, 1011441, 0x7FFF, false, false); // EXIT
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (m_Deed.Deleted || !m_Deed.IsChildOf(m_From.Backpack))
			{
				return;
			}

			if (info.ButtonID == 2) // Combine
			{
				m_From.SendGump(new LargeBODGump(m_From, m_Deed));
				m_Deed.BeginCombine(m_From);
			}
		}

		public static int GetMaterialNumberFor(BulkMaterialType material)
		{
			if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
			{
				return 1045142 + (material - BulkMaterialType.DullCopper);
			}
			else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
			{
				return 1049348 + (material - BulkMaterialType.Spined);
			}

			return 0;
		}
	}

	public class LargeBODAcceptGump : Gump
	{
		private readonly LargeBOD m_Deed;
		private readonly Mobile m_From;

		public LargeBODAcceptGump(Mobile from, LargeBOD deed) : base(50, 50)
		{
			m_From = from;
			m_Deed = deed;

			m_From.CloseGump(typeof(LargeBODAcceptGump));
			m_From.CloseGump(typeof(SmallBODAcceptGump));

			var entries = deed.Entries;

			AddPage(0);

			AddBackground(25, 10, 430, 240 + (entries.Length * 24), 5054);

			AddImageTiled(33, 20, 413, 221 + (entries.Length * 24), 2624);
			AddAlphaRegion(33, 20, 413, 221 + (entries.Length * 24));

			AddImage(20, 5, 10460);
			AddImage(430, 5, 10460);
			AddImage(20, 225 + (entries.Length * 24), 10460);
			AddImage(430, 225 + (entries.Length * 24), 10460);

			AddHtmlLocalized(180, 25, 120, 20, 1045134, 0x7FFF, false, false); // A large bulk order

			AddHtmlLocalized(40, 48, 350, 20, 1045135, 0x7FFF, false, false); // Ah!  Thanks for the goods!  Would you help me out?

			AddHtmlLocalized(40, 72, 210, 20, 1045138, 0x7FFF, false, false); // Amount to make:
			AddLabel(250, 72, 1152, deed.AmountMax.ToString());

			AddHtmlLocalized(40, 96, 120, 20, 1045137, 0x7FFF, false, false); // Items requested:

			var y = 120;

			for (var i = 0; i < entries.Length; ++i, y += 24)
			{
				AddHtmlLocalized(40, y, 210, 20, entries[i].Details.Number, 0x7FFF, false, false);
			}

			if (deed.RequireExceptional || deed.Material != BulkMaterialType.None)
			{
				AddHtmlLocalized(40, y, 210, 20, 1045140, 0x7FFF, false, false); // Special requirements to meet:
				y += 24;

				if (deed.RequireExceptional)
				{
					AddHtmlLocalized(40, y, 350, 20, 1045141, 0x7FFF, false, false); // All items must be exceptional.
					y += 24;
				}

				if (deed.Material != BulkMaterialType.None)
				{
					AddHtmlLocalized(40, y, 350, 20, GetMaterialNumberFor(deed.Material), 0x7FFF, false, false); // All items must be made with x material.
					y += 24;
				}
			}

			AddHtmlLocalized(40, 192 + (entries.Length * 24), 350, 20, 1045139, 0x7FFF, false, false); // Do you want to accept this order?

			AddButton(100, 216 + (entries.Length * 24), 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(135, 216 + (entries.Length * 24), 120, 20, 1006044, 0x7FFF, false, false); // Ok

			AddButton(275, 216 + (entries.Length * 24), 4005, 4007, 0, GumpButtonType.Reply, 0);
			AddHtmlLocalized(310, 216 + (entries.Length * 24), 120, 20, 1011012, 0x7FFF, false, false); // CANCEL
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1) // Ok
			{
				if (m_From.PlaceInBackpack(m_Deed))
				{
					m_From.SendLocalizedMessage(1045152); // The bulk order deed has been placed in your backpack.
				}
				else
				{
					m_From.SendLocalizedMessage(1045150); // There is not enough room in your backpack for the deed.
					m_Deed.Delete();
				}
			}
			else
			{
				m_Deed.Delete();
			}
		}

		public static int GetMaterialNumberFor(BulkMaterialType material)
		{
			if (material >= BulkMaterialType.DullCopper && material <= BulkMaterialType.Valorite)
			{
				return 1045142 + (material - BulkMaterialType.DullCopper);
			}
			else if (material >= BulkMaterialType.Spined && material <= BulkMaterialType.Barbed)
			{
				return 1049348 + (material - BulkMaterialType.Spined);
			}

			return 0;
		}
	}

	public class LargeBulkEntry
	{
		private static readonly Dictionary<BODType, Dictionary<string, SmallBulkEntry[]>> m_Entries = new()
		{
			#region Blacksmith

			[BODType.Smith] = new()
			{
				#region Weapons

				#region Axes

				["Axes"] = new SmallBulkEntry[]
				{
					new(typeof(Axe), 0x0F49),
					new(typeof(BattleAxe), 0x0F47),
					new(typeof(DoubleAxe), 0x0F4B),
					new(typeof(ExecutionersAxe), 0x0F45),
					new(typeof(LargeBattleAxe), 0x13FB),
					new(typeof(TwoHandedAxe), 0x1443),
				},

				#endregion

				#region Fencing

				["Fencing"] = new SmallBulkEntry[]
				{
					new(typeof(Dagger), 0x0F52),
					new(typeof(ShortSpear), 0x1403),
					new(typeof(Spear), 0x0F62),
					new(typeof(WarFork), 0x1405),
					new(typeof(Kryss), 0x1401),
				},

				#endregion

				#region Maces

				["Maces"] = new SmallBulkEntry[]
				{
					new(typeof(WarAxe), 0x13B0),
					new(typeof(HammerPick), 0x143D),
					new(typeof(Mace), 0x0F5C),
					new(typeof(Maul), 0x143B),
					new(typeof(WarHammer), 0x1439),
					new(typeof(WarMace), 0x1407),
				},

				#endregion

				#region Polearms

				["Polearms"] = new SmallBulkEntry[]
				{
					new(typeof(Bardiche), 0x0F4D),
					new(typeof(Halberd), 0x143E),
				},

				#endregion

				#region Swords

				["Swords"] = new SmallBulkEntry[]
				{
					new(typeof(Broadsword), 0x0F5E),
					new(typeof(Cutlass), 0x1441),
					new(typeof(Katana), 0x13FF),
					new(typeof(Longsword), 0x0F61),
					new(typeof(Scimitar), 0x13B6),
					new(typeof(VikingSword), 0x13B9),
				},

				#endregion

				#endregion

				#region Armor

				#region Chainmail

				["Chainmail"] = new SmallBulkEntry[]
				{
					new(typeof(ChainCoif), 0x13BB),
					new(typeof(ChainLegs), 0x13BE),
					new(typeof(ChainChest), 0x13BF),
				},

				#endregion

				#region Plate

				["Platemail"] = new SmallBulkEntry[]
				{
					new(typeof(PlateArms), 0x1410),
					new(typeof(PlateLegs), 0x1411),
					new(typeof(PlateHelm), 0x1412),
					new(typeof(PlateGorget), 0x1413),
					new(typeof(PlateGloves), 0x1414),
					new(typeof(PlateChest), 0x1415),
				},

				#endregion

				#region Ringmail

				["Ringmail"] = new SmallBulkEntry[]
				{
					new(typeof(RingmailGloves), 0x13EB),
					new(typeof(RingmailChest), 0x13EC),
					new(typeof(RingmailArms), 0x13EE),
					new(typeof(RingmailLegs), 0x13F0),
				},

				#endregion

				#endregion
			},

			#endregion

			#region Tailor

			[BODType.Tailor] = new()
			{
				#region Armor

				#region Bone Set

				["Bone Set"] = new SmallBulkEntry[]
				{
					new(typeof(BoneHelm), 0x1451),
					new(typeof(BoneGloves), 0x1450),
					new(typeof(BoneArms), 0x144E),
					new(typeof(BoneLegs), 0x1452),
					new(typeof(BoneChest), 0x144F),
				},

				#endregion

				#region Female Leather Set

				["Female Leather Set"] = new SmallBulkEntry[]
				{
					new(typeof(LeatherSkirt), 0x1C08),
					new(typeof(LeatherBustierArms), 0x1C0A),
					new(typeof(LeatherShorts), 0x1C00),
					new(typeof(FemaleLeatherChest), 0x1C06),
					new(typeof(FemaleStuddedChest), 0x1C02),
					new(typeof(StuddedBustierArms), 0x1C0C),
				},

				#endregion

				#region Male Leather Set

				["Male Leather Set"] = new SmallBulkEntry[]
				{
					new(typeof(LeatherGorget), 0x13C7),
					new(typeof(LeatherCap), 0x1DB9),
					new(typeof(LeatherGloves), 0x13C6),
					new(typeof(LeatherArms), 0x13CD),
					new(typeof(LeatherLegs), 0x13CB),
					new(typeof(LeatherChest), 0x13CC),
				},

				#endregion

				#region Studded Set

				["Studded Set"] = new SmallBulkEntry[]
				{
					new(typeof(StuddedGorget), 0x13D6),
					new(typeof(StuddedGloves), 0x13D5),
					new(typeof(StuddedArms), 0x13DC),
					new(typeof(StuddedLegs), 0x13DA),
					new(typeof(StuddedChest), 0x13DB),
				},

				#endregion

				#endregion

				#region Professions

				#region Farmer

				["Farmer"] = new SmallBulkEntry[]
				{
					new(typeof(StrawHat), 0x1717),
					new(typeof(Tunic), 0x1FA1),
					new(typeof(LongPants), 0x1539),
					new(typeof(Boots), 0x170B),
				},

				#endregion

				#region Fisher Girl

				["Fisher Girl"] = new SmallBulkEntry[]
				{
					new(typeof(FloppyHat), 0x1713),
					new(typeof(FullApron), 0x153D),
					new(typeof(PlainDress), 0x1F01),
					new(typeof(Sandals), 0x170D),
				},

				#endregion

				#region Gypsy

				["Gypsy"] = new SmallBulkEntry[]
				{
					new(typeof(Bandana), 0x1540),
					new(typeof(Shirt), 0x1517),
					new(typeof(Skirt), 0x1516),
					new(typeof(ThighBoots), 0x1711),
				},

				#endregion

				#region Jester

				["Jester"] = new SmallBulkEntry[]
				{
					new(typeof(JesterHat), 0x171C),
					new(typeof(JesterSuit), 0x1F9F),
					new(typeof(Cloak), 0x1515),
					new(typeof(Shoes), 0x170F),
				},

				#endregion

				#region Lady

				["Lady"] = new SmallBulkEntry[]
				{
					new(typeof(Bonnet), 0x1719),
					new(typeof(HalfApron), 0x153B),
					new(typeof(FancyDress), 0x1EFF),
					new(typeof(Sandals), 0x170D),
				},

				#endregion

				#region Pirate

				["Pirate"] = new SmallBulkEntry[]
				{
					new(typeof(SkullCap), 0x1544),
					new(typeof(Doublet), 0x1F7B),
					new(typeof(Kilt), 0x1537),
					new(typeof(Shoes), 0x170F),
				},

				#endregion

				#region Town Crier

				["Town Crier"] = new SmallBulkEntry[]
				{
					new(typeof(FeatheredHat), 0x171A),
					new(typeof(Surcoat), 0x1FFD),
					new(typeof(FancyShirt), 0x1EFD),
					new(typeof(ShortPants), 0x152E),
					new(typeof(ThighBoots), 0x1711),
				},

				#endregion

				#region Wizard

				["Wizard"] = new SmallBulkEntry[]
				{
					new(typeof(WizardsHat), 0x1718),
					new(typeof(BodySash), 0x1541),
					new(typeof(Robe), 0x1F03),
					new(typeof(Boots), 0x170B),
				},

				#endregion

				#endregion

				#region Misc

				#region Hat Set

				["Hat Set"] = new SmallBulkEntry[]
				{
					new(typeof(TricorneHat), 0x171B),
					new(typeof(Cap), 0x1715),
					new(typeof(WideBrimHat), 0x1714),
					new(typeof(TallStrawHat), 0x1716),
				},

				#endregion

				#region Shoe Set

				["Shoe Set"] = new SmallBulkEntry[]
				{
					new(typeof(Sandals), 0x170D),
					new(typeof(Shoes), 0x170F),
					new(typeof(Boots), 0x170B),
					new(typeof(ThighBoots), 0x1711),
				},

				#endregion

				#endregion
			},

			#endregion
		};

		public static SmallBulkEntry[] LargeRing => GetEntries(BODType.Smith, "Ringmail");
		public static SmallBulkEntry[] LargePlate => GetEntries(BODType.Smith, "Platemail");
		public static SmallBulkEntry[] LargeChain => GetEntries(BODType.Smith, "Chainmail");
		public static SmallBulkEntry[] LargeAxes => GetEntries(BODType.Smith, "Axes");
		public static SmallBulkEntry[] LargeFencing => GetEntries(BODType.Smith, "Fencing");
		public static SmallBulkEntry[] LargeMaces => GetEntries(BODType.Smith, "Maces");
		public static SmallBulkEntry[] LargePolearms => GetEntries(BODType.Smith, "Polearms");
		public static SmallBulkEntry[] LargeSwords => GetEntries(BODType.Smith, "Swords");

		public static SmallBulkEntry[] BoneSet => GetEntries(BODType.Tailor, "Bone Set");
		public static SmallBulkEntry[] Farmer => GetEntries(BODType.Tailor, "Farmer");
		public static SmallBulkEntry[] FemaleLeatherSet => GetEntries(BODType.Tailor, "Female Leather Set");
		public static SmallBulkEntry[] FisherGirl => GetEntries(BODType.Tailor, "Fisher Girl");
		public static SmallBulkEntry[] Gypsy => GetEntries(BODType.Tailor, "Gypsy");
		public static SmallBulkEntry[] HatSet => GetEntries(BODType.Tailor, "Hat Set");
		public static SmallBulkEntry[] Jester => GetEntries(BODType.Tailor, "Jester");
		public static SmallBulkEntry[] Lady => GetEntries(BODType.Tailor, "Lady");
		public static SmallBulkEntry[] MaleLeatherSet => GetEntries(BODType.Tailor, "Male Leather Set");
		public static SmallBulkEntry[] Pirate => GetEntries(BODType.Tailor, "Pirate");
		public static SmallBulkEntry[] ShoeSet => GetEntries(BODType.Tailor, "Shoe Set");
		public static SmallBulkEntry[] StuddedSet => GetEntries(BODType.Tailor, "Studded Set");
		public static SmallBulkEntry[] TownCrier => GetEntries(BODType.Tailor, "Town Crier");
		public static SmallBulkEntry[] Wizard => GetEntries(BODType.Tailor, "Wizard");

		public static SmallBulkEntry[] GetEntries(BODType type, string name)
		{
			if (!m_Entries.TryGetValue(type, out var table))
			{
				return Array.Empty<SmallBulkEntry>();
			}

			if (!table.TryGetValue(name, out var entries))
			{
				return Array.Empty<SmallBulkEntry>();
			}

			return entries;
		}

		public static LargeBulkEntry[] ConvertEntries(LargeBOD owner, SmallBulkEntry[] small)
		{
			var large = new LargeBulkEntry[small.Length];

			for (var i = 0; i < small.Length; ++i)
			{
				large[i] = new LargeBulkEntry(owner, small[i]);
			}

			return large;
		}

		private int m_Amount;

		public int Amount
		{
			get => m_Amount;
			set
			{
				m_Amount = value;
				
				Owner?.InvalidateProperties();
			}
		}

		public SmallBulkEntry Details { get; }

		public LargeBOD Owner { get; set; }

		public LargeBulkEntry(LargeBOD owner, SmallBulkEntry details)
		{
			Owner = owner;
			Details = details;
		}

		public LargeBulkEntry(LargeBOD owner, GenericReader reader)
		{
			Owner = owner;

			m_Amount = reader.ReadInt();

			Type realType = null;

			var type = reader.ReadString();

			if (type != null)
			{
				realType = ScriptCompiler.FindTypeByFullName(type);
			}

			Details = new SmallBulkEntry(realType, reader.ReadInt(), reader.ReadInt());
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(m_Amount);
			writer.Write(Details.Type?.FullName);
			writer.Write(Details.Number);
			writer.Write(Details.Graphic);
		}
	}

	public class LargeBODTarget : Target
	{
		private readonly LargeBOD m_Deed;

		public LargeBODTarget(LargeBOD deed) : base(18, false, TargetFlags.None)
		{
			m_Deed = deed;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			if (m_Deed.Deleted || !m_Deed.IsChildOf(from.Backpack))
			{
				return;
			}

			m_Deed.EndCombine(from, targeted);
		}
	}

	#endregion
}