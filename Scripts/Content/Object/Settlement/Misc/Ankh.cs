using Server.ContextMenus;

using System.Collections.Generic;

namespace Server.Items
{
	/// Facing: East
	[TypeAlias("Server.Items.AnkhEast")]
	public class AnkhNorth : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public AnkhNorth() : this(false)
		{
		}

		[Constructable]
		public AnkhNorth(bool bloodied) : base(bloodied ? 0x1E5D : 0x4)
		{
			Movable = false;

			m_Item = new InternalItem(bloodied, this);
		}

		public AnkhNorth(Serial serial)
			: base(serial)
		{
		}

		public override bool HandlesOnMovement => true;  // Tell the core that we implement OnMovement

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
			{
				Ankhs.Resurrect(m, this);
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			Ankhs.GetContextMenuEntries(from, this, list);
		}

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get => base.Hue;
			set
			{
				base.Hue = value; if (m_Item.Hue != value)
				{
					m_Item.Hue = value;
				}
			}
		}

		public override void OnDoubleClickDead(Mobile m)
		{
			Ankhs.Resurrect(m, this);
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			if (m_Item != null)
			{
				m_Item.Location = new Point3D(X + 1, Y, Z);
			}
		}

		public override void OnMapChange(Map oldMap)
		{
			if (m_Item != null)
			{
				m_Item.Map = Map;
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Item != null)
			{
				m_Item.Delete();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Item);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}

		[TypeAlias("Server.Items.AnkhEast+InternalItem")]
		private class InternalItem : Item
		{
			private AnkhNorth m_Item;

			public InternalItem(bool bloodied, AnkhNorth item)
				: base(bloodied ? 0x1E5C : 0x5)
			{
				Movable = false;

				m_Item = item;
			}

			public InternalItem(Serial serial) : base(serial)
			{
			}

			public override void OnLocationChange(Point3D oldLocation)
			{
				if (m_Item != null)
				{
					m_Item.Location = new Point3D(X - 1, Y, Z);
				}
			}

			public override void OnMapChange(Map oldMap)
			{
				if (m_Item != null)
				{
					m_Item.Map = Map;
				}
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Item != null)
				{
					m_Item.Delete();
				}
			}

			public override bool HandlesOnMovement => true;  // Tell the core that we implement OnMovement

			public override void OnMovement(Mobile m, Point3D oldLocation)
			{
				if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
				{
					Ankhs.Resurrect(m, this);
				}
			}

			public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
			{
				base.GetContextMenuEntries(from, list);
				Ankhs.GetContextMenuEntries(from, this, list);
			}

			[Hue, CommandProperty(AccessLevel.GameMaster)]
			public override int Hue
			{
				get => base.Hue;
				set
				{
					base.Hue = value; if (m_Item.Hue != value)
					{
						m_Item.Hue = value;
					}
				}
			}

			public override void OnDoubleClickDead(Mobile m)
			{
				Ankhs.Resurrect(m, this);
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_Item);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				m_Item = reader.ReadItem() as AnkhNorth;
			}
		}
	}

	/// Facing: West
	public class AnkhWest : Item
	{
		private InternalItem m_Item;

		[Constructable]
		public AnkhWest() : this(false)
		{
		}

		[Constructable]
		public AnkhWest(bool bloodied) : base(bloodied ? 0x1D98 : 0x3)
		{
			Movable = false;

			m_Item = new InternalItem(bloodied, this);
		}

		public AnkhWest(Serial serial) : base(serial)
		{
		}

		public override bool HandlesOnMovement => true;  // Tell the core that we implement OnMovement

		public override void OnMovement(Mobile m, Point3D oldLocation)
		{
			if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
			{
				Ankhs.Resurrect(m, this);
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			Ankhs.GetContextMenuEntries(from, this, list);
		}

		[Hue, CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get => base.Hue;
			set
			{
				base.Hue = value; if (m_Item.Hue != value)
				{
					m_Item.Hue = value;
				}
			}
		}

		public override void OnDoubleClickDead(Mobile m)
		{
			Ankhs.Resurrect(m, this);
		}

		public override void OnLocationChange(Point3D oldLocation)
		{
			if (m_Item != null)
			{
				m_Item.Location = new Point3D(X, Y + 1, Z);
			}
		}

		public override void OnMapChange(Map oldMap)
		{
			if (m_Item != null)
			{
				m_Item.Map = Map;
			}
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			if (m_Item != null)
			{
				m_Item.Delete();
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.Write(m_Item);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_Item = reader.ReadItem() as InternalItem;
		}

		private class InternalItem : Item
		{
			private AnkhWest m_Item;

			public InternalItem(bool bloodied, AnkhWest item) : base(bloodied ? 0x1D97 : 0x2)
			{
				Movable = false;

				m_Item = item;
			}

			public InternalItem(Serial serial) : base(serial)
			{
			}

			public override void OnLocationChange(Point3D oldLocation)
			{
				if (m_Item != null)
				{
					m_Item.Location = new Point3D(X, Y - 1, Z);
				}
			}

			public override void OnMapChange(Map oldMap)
			{
				if (m_Item != null)
				{
					m_Item.Map = Map;
				}
			}

			public override void OnAfterDelete()
			{
				base.OnAfterDelete();

				if (m_Item != null)
				{
					m_Item.Delete();
				}
			}

			public override bool HandlesOnMovement => true;  // Tell the core that we implement OnMovement

			public override void OnMovement(Mobile m, Point3D oldLocation)
			{
				if (Parent == null && Utility.InRange(Location, m.Location, 1) && !Utility.InRange(Location, oldLocation, 1))
				{
					Ankhs.Resurrect(m, this);
				}
			}

			public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
			{
				base.GetContextMenuEntries(from, list);
				Ankhs.GetContextMenuEntries(from, this, list);
			}

			[Hue, CommandProperty(AccessLevel.GameMaster)]
			public override int Hue
			{
				get => base.Hue;
				set
				{
					base.Hue = value; if (m_Item.Hue != value)
					{
						m_Item.Hue = value;
					}
				}
			}

			public override void OnDoubleClickDead(Mobile m)
			{
				Ankhs.Resurrect(m, this);
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.Write(0); // version

				writer.Write(m_Item);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				var version = reader.ReadInt();

				m_Item = reader.ReadItem() as AnkhWest;
			}
		}
	}
}