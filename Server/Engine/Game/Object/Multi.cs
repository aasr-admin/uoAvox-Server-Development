
using System;

namespace Server.Items
{
	public class BaseMulti : Item
	{
		[Constructable]
		public BaseMulti(int itemID) : base(itemID)
		{
			Movable = false;
		}

		public BaseMulti(Serial serial) : base(serial)
		{
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public override int ItemID
		{
			get => base.ItemID;
			set
			{
				if (base.ItemID != value)
				{
					var facet = (Parent == null ? Map : null);

					if (facet != null)
					{
						facet.OnLeave(this);
					}

					base.ItemID = value;

					if (facet != null)
					{
						facet.OnEnter(this);
					}
				}
			}
		}

		[Obsolete("Replace with calls to OnLeave and OnEnter surrounding component invalidation.", true)]
		public virtual void RefreshComponents()
		{
			if (Parent == null)
			{
				var facet = Map;

				if (facet != null)
				{
					facet.OnLeave(this);
					facet.OnEnter(this);
				}
			}
		}

		public override int LabelNumber
		{
			get
			{
				var mcl = Components;

				if (mcl.List.Length > 0)
				{
					int id = mcl.List[0].ItemID;

					if (id < 0x4000)
					{
						return 1020000 + id;
					}

					return 1078872 + id;
				}

				return base.LabelNumber;
			}
		}

		public virtual bool AllowsRelativeDrop => false;

		public override int GetMaxUpdateRange()
		{
			return base.GetMaxUpdateRange() + 4;
		}

		public override int GetUpdateRange(Mobile m)
		{
			return base.GetUpdateRange(m) + 4;
		}

		public virtual MultiComponentList Components => MultiData.GetComponents(ItemID);

		public bool Contains(IEntity e)
		{
			if (e.Map == Map)
			{
				if (e is Item item)
				{
					return Contains(item.WorldLocation);
				}

				return Contains(e.Location);
			}
			
			return false;
		}

		public bool Contains(IPoint2D p)
		{
			if (p is IEntity e)
			{
				return Contains(e);
			}

			return Contains(p.X, p.Y);
		}

		public bool Contains(IPoint3D p)
		{
			if (p is IEntity e)
			{
				return Contains(e);
			}

			return Contains(p.X, p.Y, p.Z);
		}

		public virtual bool Contains(int x, int y)
		{
			var mcl = Components;

			x -= X + mcl.Min.m_X;
			y -= Y + mcl.Min.m_Y;

			return x >= 0
				&& x < mcl.Width
				&& y >= 0
				&& y < mcl.Height
				&& mcl.Tiles[x][y].Length > 0;
		}

		public virtual bool Contains(int x, int y, int z)
		{
			var mcl = Components;

			x -= X + mcl.Min.m_X;
			y -= Y + mcl.Min.m_Y;
			z -= Z + mcl.Min.m_Z;

			return x >= 0
				&& x < mcl.Width
				&& y >= 0
				&& y < mcl.Height
				&& z >= 0
				&& z < mcl.Depth
				&& mcl.Tiles[x][y].Length > 0;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			if (version == 0)
			{
				if (ItemID >= 0x4000)
				{
					ItemID -= 0x4000;
				}
			}
		}
	}
}