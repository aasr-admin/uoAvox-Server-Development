using System;

namespace Server.Items
{
	public interface ITicketTaker
	{
	}

	public interface ITicket
	{
		void OnTicketUsed(ITicketTaker teleporter, Mobile from);
	}

	public class TicketTeleporter : Teleporter, ITicketTaker
	{
		private Type m_TicketType;
		private TextDefinition m_Message;

		[CommandProperty(AccessLevel.GameMaster)]
		public Type TicketType
		{
			get => m_TicketType;
			set { m_TicketType = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TextDefinition Message
		{
			get => m_Message;
			set => m_Message = value;
		}

		[Constructable]
		public TicketTeleporter()
			: this(Point3D.Zero, null, null, null)
		{
		}

		[Constructable]
		public TicketTeleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, null, null)
		{
		}

		[Constructable]
		public TicketTeleporter(Point3D pointDest, Map mapDest, Type ticketType, TextDefinition message)
			: base(pointDest, mapDest)
		{
			m_TicketType = ticketType;
			m_Message = message;
		}

		public override bool CanTeleport(Mobile m)
		{
			if (!base.CanTeleport(m))
			{
				return false;
			}

			if (m_TicketType != null)
			{
				var ticket = m.Backpack?.FindItemByType(m_TicketType, false); // Check (top level) backpack
				
				if (ticket == null)
				{
					foreach (var item in m.Items) // Check paperdoll
					{
						if (m_TicketType.IsAssignableFrom(item.GetType()))
						{
							ticket = item;
							break;
						}
					}
				}

				if (ticket == null)
				{
					TextDefinition.SendMessageTo(m, m_Message);
					return false;
				}

				if (ticket is ITicket t)
				{
					t.OnTicketUsed(this, m);
				}
			}

			return true;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_TicketType != null)
			{
				list.Add(String.Format("Required ticket: {0}", m_TicketType.Name));
			}
		}

		public TicketTeleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0); // version

			writer.WriteObjectType(m_TicketType);

			TextDefinition.Serialize(writer, m_Message);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			m_TicketType = reader.ReadObjectType();

			m_Message = TextDefinition.Deserialize(reader);
		}
	}
}