using Server.Accounting;
using Server.ContextMenus;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Network;
using Server.Spells;
using Server.Targeting;

using System;
using System.Collections.Generic;

namespace Server.Gumps
{
	public class CharacterStatueGump : Gump
	{
		private readonly Item m_Maker;
		private readonly CharacterStatue m_Statue;
		private readonly Mobile m_Owner;

		private enum Buttons
		{
			Close,
			Sculpt,
			PosePrev,
			PoseNext,
			DirPrev,
			DirNext,
			MatPrev,
			MatNext,
			Restore
		}

		public CharacterStatueGump(Item maker, CharacterStatue statue, Mobile owner) : base(60, 36)
		{
			m_Maker = maker;
			m_Statue = statue;
			m_Owner = owner;

			if (m_Statue == null)
			{
				return;
			}

			Closable = true;
			Disposable = true;
			Dragable = true;
			Resizable = false;

			AddPage(0);

			AddBackground(0, 0, 327, 324, 0x13BE);
			AddImageTiled(10, 10, 307, 20, 0xA40);
			AddImageTiled(10, 40, 307, 244, 0xA40);
			AddImageTiled(10, 294, 307, 20, 0xA40);
			AddAlphaRegion(10, 10, 307, 304);
			AddHtmlLocalized(14, 12, 327, 20, 1076156, 0x7FFF, false, false); // Character Statue Maker

			// pose
			AddHtmlLocalized(133, 41, 120, 20, 1076168, 0x7FFF, false, false); // Choose Pose
			AddHtmlLocalized(133, 61, 120, 20, 1076208 + (int)m_Statue.Pose, 0x77E, false, false);
			AddButton(163, 81, 0xFA5, 0xFA7, (int)Buttons.PoseNext, GumpButtonType.Reply, 0);
			AddButton(133, 81, 0xFAE, 0xFB0, (int)Buttons.PosePrev, GumpButtonType.Reply, 0);

			// direction
			AddHtmlLocalized(133, 126, 120, 20, 1076170, 0x7FFF, false, false); // Choose Direction
			AddHtmlLocalized(133, 146, 120, 20, GetDirectionNumber(m_Statue.Direction), 0x77E, false, false);
			AddButton(163, 167, 0xFA5, 0xFA7, (int)Buttons.DirNext, GumpButtonType.Reply, 0);
			AddButton(133, 167, 0xFAE, 0xFB0, (int)Buttons.DirPrev, GumpButtonType.Reply, 0);

			// material
			AddHtmlLocalized(133, 211, 120, 20, 1076171, 0x7FFF, false, false); // Choose Material
			AddHtmlLocalized(133, 231, 120, 20, GetMaterialNumber(m_Statue.StatueType, m_Statue.Material), 0x77E, false, false);
			AddButton(163, 253, 0xFA5, 0xFA7, (int)Buttons.MatNext, GumpButtonType.Reply, 0);
			AddButton(133, 253, 0xFAE, 0xFB0, (int)Buttons.MatPrev, GumpButtonType.Reply, 0);

			// cancel
			AddButton(10, 294, 0xFB1, 0xFB2, (int)Buttons.Close, GumpButtonType.Reply, 0);
			AddHtmlLocalized(45, 294, 80, 20, 1006045, 0x7FFF, false, false);   // Cancel

			// sculpt
			AddButton(234, 294, 0xFB7, 0xFB9, (int)Buttons.Sculpt, GumpButtonType.Reply, 0);
			AddHtmlLocalized(269, 294, 80, 20, 1076174, 0x7FFF, false, false); // Sculpt

			// restore
			if (m_Maker is CharacterStatueDeed)
			{
				AddButton(107, 294, 0xFAB, 0xFAD, (int)Buttons.Restore, GumpButtonType.Reply, 0);
				AddHtmlLocalized(142, 294, 80, 20, 1076193, 0x7FFF, false, false); // Restore
			}
		}

		private int GetMaterialNumber(StatueType type, StatueMaterial material)
		{
			switch (material)
			{
				case StatueMaterial.Antique:

					switch (type)
					{
						case StatueType.Bronze: return 1076187;
						case StatueType.Jade: return 1076186;
						case StatueType.Marble: return 1076182;
					}

					return 1076187;
				case StatueMaterial.Dark:

					if (type == StatueType.Marble)
					{
						return 1076183;
					}

					return 1076182;
				case StatueMaterial.Medium: return 1076184;
				case StatueMaterial.Light: return 1076185;
				default: return 1076187;
			}
		}

		private int GetDirectionNumber(Direction direction)
		{
			switch (direction)
			{
				case Direction.North: return 1075389;
				case Direction.Right: return 1075388;
				case Direction.East: return 1075387;
				case Direction.Down: return 1076204;
				case Direction.South: return 1075386;
				case Direction.Left: return 1075391;
				case Direction.West: return 1075390;
				case Direction.Up: return 1076205;
				default: return 1075386;
			}
		}

		public override void OnResponse(NetState state, RelayInfo info)
		{
			if (m_Statue == null || m_Statue.Deleted)
			{
				return;
			}

			var sendGump = false;

			if (info.ButtonID == (int)Buttons.Sculpt)
			{
				if (m_Maker is CharacterStatueDeed)
				{
					var backup = ((CharacterStatueDeed)m_Maker).Statue;

					if (backup != null)
					{
						backup.Delete();
					}
				}

				if (m_Maker != null)
				{
					m_Maker.Delete();
				}

				m_Statue.Sculpt(state.Mobile);
			}
			else if (info.ButtonID == (int)Buttons.PosePrev)
			{
				m_Statue.Pose = (StatuePose)(((int)m_Statue.Pose + 5) % 6);
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.PoseNext)
			{
				m_Statue.Pose = (StatuePose)(((int)m_Statue.Pose + 1) % 6);
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.DirPrev)
			{
				m_Statue.Direction = (Direction)(((int)m_Statue.Direction + 7) % 8);
				m_Statue.InvalidatePose();
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.DirNext)
			{
				m_Statue.Direction = (Direction)(((int)m_Statue.Direction + 1) % 8);
				m_Statue.InvalidatePose();
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.MatPrev)
			{
				m_Statue.Material = (StatueMaterial)(((int)m_Statue.Material + 3) % 4);
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.MatNext)
			{
				m_Statue.Material = (StatueMaterial)(((int)m_Statue.Material + 1) % 4);
				sendGump = true;
			}
			else if (info.ButtonID == (int)Buttons.Restore)
			{
				if (m_Maker is CharacterStatueDeed)
				{
					var backup = ((CharacterStatueDeed)m_Maker).Statue;

					if (backup != null)
					{
						m_Statue.Restore(backup);
					}
				}

				sendGump = true;
			}
			else // Close
			{
				sendGump = !m_Statue.Demolish(state.Mobile);
			}

			if (sendGump)
			{
				state.Mobile.SendGump(new CharacterStatueGump(m_Maker, m_Statue, m_Owner));
			}
		}
	}
}

namespace Server.Items
{
	public class CharacterStatueMaker : Item, IRewardItem
	{
		public override int LabelNumber => 1076173;  // Character Statue Maker

		private bool m_IsRewardItem;
		private StatueType m_Type;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem
		{
			get => m_IsRewardItem;
			set { m_IsRewardItem = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public StatueType StatueType
		{
			get => m_Type;
			set { m_Type = value; InvalidateHue(); }
		}

		public CharacterStatueMaker(StatueType type) : base(0x32F0)
		{
			m_Type = type;

			InvalidateHue();

			LootType = LootType.Blessed;
			Weight = 5.0;
		}

		public CharacterStatueMaker(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_IsRewardItem && !RewardSystem.CheckIsUsableBy(from, this, new object[] { m_Type }))
			{
				return;
			}

			if (IsChildOf(from.Backpack))
			{
				if (!from.IsBodyMod)
				{
					from.SendLocalizedMessage(1076194); // Select a place where you would like to put your statue.
					from.Target = new CharacterStatueTarget(this, m_Type);
				}
				else
				{
					from.SendLocalizedMessage(1073648); // You may only proceed while in your original state...
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_IsRewardItem)
			{
				list.Add(1076222); // 6th Year Veteran Reward
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(m_IsRewardItem);
			writer.Write((int)m_Type);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			m_IsRewardItem = reader.ReadBool();
			m_Type = (StatueType)reader.ReadInt();
		}

		public void InvalidateHue()
		{
			Hue = 0xB8F + (int)m_Type * 4;
		}
	}

	public class MarbleStatueMaker : CharacterStatueMaker
	{
		[Constructable]
		public MarbleStatueMaker() : base(StatueType.Marble)
		{
		}

		public MarbleStatueMaker(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class JadeStatueMaker : CharacterStatueMaker
	{
		[Constructable]
		public JadeStatueMaker() : base(StatueType.Jade)
		{
		}

		public JadeStatueMaker(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	public class BronzeStatueMaker : CharacterStatueMaker
	{
		[Constructable]
		public BronzeStatueMaker() : base(StatueType.Bronze)
		{
		}

		public BronzeStatueMaker(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();
		}
	}

	/// Stone Statue Platform
	public class CharacterStatuePlinth : Static, IAddon
	{
		public Item Deed => new CharacterStatueDeed(m_Statue);
		public override int LabelNumber => 1076201;  // Character Statue

		private CharacterStatue m_Statue;

		public CharacterStatuePlinth(CharacterStatue statue) : base(0x32F2)
		{
			m_Statue = statue;

			InvalidateHue();
		}

		public CharacterStatuePlinth(Serial serial) : base(serial)
		{
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Statue != null && !m_Statue.Deleted)
			{
				m_Statue.Delete();
			}
		}

		public override void OnMapChange(Map oldMap)
		{
			if (m_Statue != null)
			{
				m_Statue.Map = Map;
			}
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			if (m_Statue != null)
			{
				m_Statue.Location = new Point3D(X, Y, Z + 5);
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			if (m_Statue != null)
			{
				from.SendGump(new CharacterPlinthGump(m_Statue));
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write(m_Statue);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			m_Statue = reader.ReadMobile() as CharacterStatue;

			if (m_Statue == null || m_Statue.SculptedBy == null || Map == Map.Internal)
			{
				Timer.DelayCall(TimeSpan.Zero, Delete);
			}
		}

		public void InvalidateHue()
		{
			if (m_Statue != null)
			{
				Hue = 0xB8F + (int)m_Statue.StatueType * 4 + (int)m_Statue.Material;
			}
		}

		public virtual bool CouldFit(IPoint3D p, Map map)
		{
			var point = new Point3D(p.X, p.Y, p.Z);

			if (map == null || !map.CanFit(point, 20))
			{
				return false;
			}

			var house = BaseHouse.FindHouseAt(point, map, 20);

			if (house == null)
			{
				return false;
			}

			var result = CharacterStatueTarget.CheckDoors(point, 20, house);

			if (result == AddonFitResult.Valid)
			{
				return true;
			}

			return false;
		}

		private class CharacterPlinthGump : Gump
		{
			public CharacterPlinthGump(CharacterStatue statue) : base(60, 30)
			{
				Closable = true;
				Disposable = true;
				Dragable = true;
				Resizable = false;

				AddPage(0);
				AddImage(0, 0, 0x24F4);
				AddHtml(55, 50, 150, 20, statue.Name, false, false);
				AddHtml(55, 75, 150, 20, statue.SculptedOn.ToString(), false, false);
				AddHtmlLocalized(55, 100, 150, 20, GetTypeNumber(statue.StatueType), 0, false, false);
			}

			public int GetTypeNumber(StatueType type)
			{
				switch (type)
				{
					case StatueType.Marble: return 1076181;
					case StatueType.Jade: return 1076180;
					case StatueType.Bronze: return 1076230;
					default: return 1076181;
				}
			}
		}
	}
}

namespace Server.Mobiles
{
	public enum StatueType
	{
		Marble,
		Jade,
		Bronze
	}

	public enum StatuePose
	{
		Ready,
		Casting,
		Salute,
		AllPraiseMe,
		Fighting,
		HandsOnHips
	}

	public enum StatueMaterial
	{
		Antique,
		Dark,
		Medium,
		Light
	}

	public class CharacterStatue : Mobile, IRewardItem
	{
		private StatueType m_Type;
		private StatuePose m_Pose;
		private StatueMaterial m_Material;

		[CommandProperty(AccessLevel.GameMaster)]
		public StatueType StatueType
		{
			get => m_Type;
			set { m_Type = value; InvalidateHues(); InvalidatePose(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public StatuePose Pose
		{
			get => m_Pose;
			set { m_Pose = value; InvalidatePose(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public StatueMaterial Material
		{
			get => m_Material;
			set { m_Material = value; InvalidateHues(); InvalidatePose(); }
		}

		private Mobile m_SculptedBy;
		private DateTime m_SculptedOn;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile SculptedBy
		{
			get => m_SculptedBy;
			set { m_SculptedBy = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime SculptedOn
		{
			get => m_SculptedOn;
			set => m_SculptedOn = value;
		}

		private CharacterStatuePlinth m_Plinth;

		public CharacterStatuePlinth Plinth
		{
			get => m_Plinth;
			set => m_Plinth = value;
		}

		private bool m_IsRewardItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem
		{
			get => m_IsRewardItem;
			set => m_IsRewardItem = value;
		}

		public CharacterStatue(Mobile from, StatueType type) : base()
		{
			m_Type = type;
			m_Pose = StatuePose.Ready;
			m_Material = StatueMaterial.Antique;

			Direction = Direction.South;
			AccessLevel = AccessLevel.Counselor;
			Hits = HitsMax;
			Blessed = true;
			Frozen = true;

			CloneBody(from);
			CloneClothes(from);
			InvalidateHues();
		}

		public CharacterStatue(Serial serial) : base(serial)
		{
		}

		public override void OnDoubleClick(Mobile from)
		{
			DisplayPaperdollTo(from);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_SculptedBy != null)
			{
				if (m_SculptedBy.ShowFameTitle && (m_SculptedBy.Player || m_SculptedBy.Body.IsHuman) && m_SculptedBy.Fame >= 10000)
				{
					list.Add(1076202, String.Format("{0} {1}", m_SculptedBy.Female ? "Lady" : "Lord", m_SculptedBy.Name)); // Sculpted by ~1_Name~
				}
				else
				{
					list.Add(1076202, m_SculptedBy.Name); // Sculpted by ~1_Name~
				}
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			if (from.Alive && m_SculptedBy != null)
			{
				var house = BaseHouse.FindHouseAt(this);

				if ((house != null && house.IsCoOwner(from)) || (int)from.AccessLevel > (int)AccessLevel.Counselor)
				{
					list.Add(new DemolishEntry(this));
				}
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Plinth != null && !m_Plinth.Deleted)
			{
				m_Plinth.Delete();
			}
		}

		protected override void OnMapChange(Map oldMap)
		{
			InvalidatePose();

			if (m_Plinth != null)
			{
				m_Plinth.Map = Map;
			}
		}

		protected override void OnLocationChange(Point3D oldLocation)
		{
			InvalidatePose();

			if (m_Plinth != null)
			{
				m_Plinth.Location = new Point3D(X, Y, Z - 5);
			}
		}

		public override bool CanBeRenamedBy(Mobile from)
		{
			return false;
		}

		public override bool CanBeDamaged()
		{
			return false;
		}

		public void OnRequestedAnimation(Mobile from)
		{
			from.Send(new UpdateStatueAnimation(this, 1, m_Animation, m_Frames));
		}

		public override void OnAosSingleClick(Mobile from)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0); // version

			writer.Write((int)m_Type);
			writer.Write((int)m_Pose);
			writer.Write((int)m_Material);

			writer.Write(m_SculptedBy);
			writer.Write(m_SculptedOn);

			writer.Write(m_Plinth);
			writer.Write(m_IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			m_Type = (StatueType)reader.ReadInt();
			m_Pose = (StatuePose)reader.ReadInt();
			m_Material = (StatueMaterial)reader.ReadInt();

			m_SculptedBy = reader.ReadMobile();
			m_SculptedOn = reader.ReadDateTime();

			m_Plinth = reader.ReadItem() as CharacterStatuePlinth;
			m_IsRewardItem = reader.ReadBool();

			InvalidatePose();

			Frozen = true;

			if (m_SculptedBy == null || Map == Map.Internal) // Remove preview statues
			{
				Timer.DelayCall(TimeSpan.Zero, Delete);
			}
		}

		public void Sculpt(Mobile by)
		{
			m_SculptedBy = by;
			m_SculptedOn = DateTime.UtcNow;

			InvalidateProperties();
		}

		public bool Demolish(Mobile by)
		{
			var deed = new CharacterStatueDeed(null);

			if (by.PlaceInBackpack(deed))
			{
				Delete();

				deed.Statue = this;
				deed.StatueType = m_Type;
				deed.IsRewardItem = m_IsRewardItem;

				if (m_Plinth != null)
				{
					m_Plinth.Delete();
				}

				return true;
			}
			else
			{
				by.SendLocalizedMessage(500720); // You don't have enough room in your backpack!
				deed.Delete();

				return false;
			}
		}

		public void Restore(CharacterStatue from)
		{
			m_Material = from.Material;
			m_Pose = from.Pose;

			Direction = from.Direction;

			CloneBody(from);
			CloneClothes(from);

			InvalidateHues();
			InvalidatePose();
		}

		public void CloneBody(Mobile from)
		{
			Name = from.Name;
			BodyValue = from.BodyValue;
			Female = from.Female;
			HairItemID = from.HairItemID;
			FacialHairItemID = from.FacialHairItemID;
		}

		public void CloneClothes(Mobile from)
		{
			for (var i = Items.Count - 1; i >= 0; i--)
			{
				Items[i].Delete();
			}

			for (var i = from.Items.Count - 1; i >= 0; i--)
			{
				var item = from.Items[i];

				if (item.Layer != Layer.Backpack && item.Layer != Layer.Mount && item.Layer != Layer.Bank)
				{
					AddItem(CloneItem(item));
				}
			}
		}

		public Item CloneItem(Item item)
		{
			var cloned = new Item(item.ItemID) {
				Layer = item.Layer,
				Name = item.Name,
				Hue = item.Hue,
				Weight = item.Weight,
				Movable = false
			};

			return cloned;
		}

		public void InvalidateHues()
		{
			Hue = 0xB8F + (int)m_Type * 4 + (int)m_Material;

			HairHue = Hue;

			if (FacialHairItemID > 0)
			{
				FacialHairHue = Hue;
			}

			for (var i = Items.Count - 1; i >= 0; i--)
			{
				Items[i].Hue = Hue;
			}

			if (m_Plinth != null)
			{
				m_Plinth.InvalidateHue();
			}
		}

		private int m_Animation;
		private int m_Frames;

		public void InvalidatePose()
		{
			switch (m_Pose)
			{
				case StatuePose.Ready:
					m_Animation = 4;
					m_Frames = 0;
					break;
				case StatuePose.Casting:
					m_Animation = 16;
					m_Frames = 2;
					break;
				case StatuePose.Salute:
					m_Animation = 33;
					m_Frames = 1;
					break;
				case StatuePose.AllPraiseMe:
					m_Animation = 17;
					m_Frames = 4;
					break;
				case StatuePose.Fighting:
					m_Animation = 31;
					m_Frames = 5;
					break;
				case StatuePose.HandsOnHips:
					m_Animation = 6;
					m_Frames = 1;
					break;
			}

			if (Map != null)
			{
				ProcessDelta();

				Packet p = null;

				IPooledEnumerable eable = Map.GetClientsInRange(Location);

				foreach (NetState state in eable)
				{
					state.Mobile.ProcessDelta();

					if (p == null)
					{
						p = Packet.Acquire(new UpdateStatueAnimation(this, 1, m_Animation, m_Frames));
					}

					state.Send(p);
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		private class DemolishEntry : ContextMenuEntry
		{
			private readonly CharacterStatue m_Statue;

			public DemolishEntry(CharacterStatue statue) : base(6275, 2)
			{
				m_Statue = statue;
			}

			public override void OnClick()
			{
				if (m_Statue.Deleted)
				{
					return;
				}

				m_Statue.Demolish(Owner.From);
			}
		}
	}

	public class CharacterStatueDeed : Item, IRewardItem
	{
		public override int LabelNumber
		{
			get
			{
				var t = m_Type;

				if (m_Statue != null)
				{
					t = m_Statue.StatueType;
				}

				switch (t)
				{
					case StatueType.Marble: return 1076189;
					case StatueType.Jade: return 1076188;
					case StatueType.Bronze: return 1076190;
					default: return 1076173;
				}
			}
		}

		private CharacterStatue m_Statue;
		private StatueType m_Type;
		private bool m_IsRewardItem;

		[CommandProperty(AccessLevel.GameMaster)]
		public CharacterStatue Statue
		{
			get => m_Statue;
			set => m_Statue = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public StatueType StatueType
		{
			get
			{
				if (m_Statue != null)
				{
					return m_Statue.StatueType;
				}

				return m_Type;
			}
			set => m_Type = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsRewardItem
		{
			get => m_IsRewardItem;
			set { m_IsRewardItem = value; InvalidateProperties(); }
		}

		public CharacterStatueDeed(CharacterStatue statue) : base(0x14F0)
		{
			m_Statue = statue;

			if (statue != null)
			{
				m_Type = statue.StatueType;
				m_IsRewardItem = statue.IsRewardItem;
			}

			LootType = LootType.Blessed;
			Weight = 1.0;
		}

		public CharacterStatueDeed(Serial serial) : base(serial)
		{
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_IsRewardItem)
			{
				list.Add(1076222); // 6th Year Veteran Reward
			}

			if (m_Statue != null)
			{
				list.Add(1076231, m_Statue.Name); // Statue of ~1_Name~
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			var acct = from.Account as Account;

			if (acct != null && from.AccessLevel == AccessLevel.Player)
			{
				var time = TimeSpan.FromDays(RewardSystem.RewardInterval.TotalDays * 6) - (DateTime.UtcNow - acct.Created);

				if (time > TimeSpan.Zero)
				{
					from.SendLocalizedMessage(1008126, true, Math.Ceiling(time.TotalDays / RewardSystem.RewardInterval.TotalDays).ToString()); // Your account is not old enough to use this item. Months until you can use this item :
					return;
				}
			}

			if (IsChildOf(from.Backpack))
			{
				if (!from.IsBodyMod)
				{
					from.SendLocalizedMessage(1076194); // Select a place where you would like to put your statue.
					from.Target = new CharacterStatueTarget(this, StatueType);
				}
				else
				{
					from.SendLocalizedMessage(1073648); // You may only proceed while in your original state...
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public override void OnDelete()
		{
			base.OnDelete();

			if (m_Statue != null)
			{
				m_Statue.Delete();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(1); // version

			writer.Write((int)m_Type);

			writer.Write(m_Statue);
			writer.Write(m_IsRewardItem);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadEncodedInt();

			if (version >= 1)
			{
				m_Type = (StatueType)reader.ReadInt();
			}

			m_Statue = reader.ReadMobile() as CharacterStatue;
			m_IsRewardItem = reader.ReadBool();
		}
	}

	public class CharacterStatueTarget : Target
	{
		private readonly Item m_Maker;
		private readonly StatueType m_Type;

		public CharacterStatueTarget(Item maker, StatueType type) : base(-1, true, TargetFlags.None)
		{
			m_Maker = maker;
			m_Type = type;
		}

		protected override void OnTarget(Mobile from, object targeted)
		{
			var p = targeted as IPoint3D;
			var map = from.Map;

			if (p == null || map == null || m_Maker == null || m_Maker.Deleted)
			{
				return;
			}

			if (m_Maker.IsChildOf(from.Backpack))
			{
				SpellHelper.GetSurfaceTop(ref p);
				BaseHouse house = null;
				var loc = new Point3D(p);

				if (targeted is Item && !((Item)targeted).IsLockedDown && !((Item)targeted).IsSecure && !(targeted is AddonComponent))
				{
					from.SendLocalizedMessage(1076191); // Statues can only be placed in houses.
					return;
				}
				else if (from.IsBodyMod)
				{
					from.SendLocalizedMessage(1073648); // You may only proceed while in your original state...
					return;
				}

				var result = CouldFit(loc, map, from, ref house);

				if (result == AddonFitResult.Valid)
				{
					var statue = new CharacterStatue(from, m_Type);
					var plinth = new CharacterStatuePlinth(statue);

					house.Addons.Add(plinth);

					if (m_Maker is IRewardItem)
					{
						statue.IsRewardItem = ((IRewardItem)m_Maker).IsRewardItem;
					}

					statue.Plinth = plinth;
					plinth.MoveToWorld(loc, map);
					statue.InvalidatePose();

					/*
					 * TODO: Previously the maker wasn't deleted until after statue
					 * customization, leading to redeeding issues. Exact OSI behavior
					 * needs looking into.
					 */
					m_Maker.Delete();
					statue.Sculpt(from);

					from.CloseGump(typeof(CharacterStatueGump));
					from.SendGump(new CharacterStatueGump(m_Maker, statue, from));
				}
				else if (result == AddonFitResult.Blocked)
				{
					from.SendLocalizedMessage(500269); // You cannot build that there.
				}
				else if (result == AddonFitResult.NotInHouse)
				{
					from.SendLocalizedMessage(1076192); // Statues can only be placed in houses where you are the owner or co-owner.
				}
				else if (result == AddonFitResult.DoorTooClose)
				{
					from.SendLocalizedMessage(500271); // You cannot build near the door.
				}
			}
			else
			{
				from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
			}
		}

		public static AddonFitResult CouldFit(Point3D p, Map map, Mobile from, ref BaseHouse house)
		{
			if (!map.CanFit(p.X, p.Y, p.Z, 20, true, true, true))
			{
				return AddonFitResult.Blocked;
			}
			else if (!BaseAddon.CheckHouse(from, p, map, 20, ref house))
			{
				return AddonFitResult.NotInHouse;
			}
			else
			{
				return CheckDoors(p, 20, house);
			}
		}

		public static AddonFitResult CheckDoors(Point3D p, int height, BaseHouse house)
		{
			foreach (var door in house.Doors)
			{
				var doorLoc = door.GetWorldLocation();
				var doorHeight = door.ItemData.CalcHeight;

				if (Utility.InRange(doorLoc, p, 1) && (p.Z == doorLoc.Z || ((p.Z + height) > doorLoc.Z && (doorLoc.Z + doorHeight) > p.Z)))
				{
					return AddonFitResult.DoorTooClose;
				}
			}

			return AddonFitResult.Valid;
		}
	}
}

namespace Server.Network
{
	public class UpdateStatueAnimation : Packet
	{
		public UpdateStatueAnimation(Mobile m, int status, int animation, int frame) : base(0xBF, 17)
		{
			m_Stream.Write((short)0x11);
			m_Stream.Write((short)0x19);
			m_Stream.Write((byte)0x5);
			m_Stream.Write(m.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0xFF);
			m_Stream.Write((byte)status);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)animation);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)frame);
		}
	}
}