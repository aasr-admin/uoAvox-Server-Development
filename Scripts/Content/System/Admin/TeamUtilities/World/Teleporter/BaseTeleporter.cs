﻿using Server.Spells;

using System;

namespace Server.Items
{
	public class Teleporter : Item
	{
		private bool m_Active, m_Creatures, m_CombatCheck, m_CriminalCheck;
		private Point3D m_PointDest;
		private Map m_MapDest;
		private bool m_SourceEffect;
		private bool m_DestEffect;
		private int m_SoundID;
		private TimeSpan m_Delay;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SourceEffect
		{
			get => m_SourceEffect;
			set { m_SourceEffect = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DestEffect
		{
			get => m_DestEffect;
			set { m_DestEffect = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int SoundID
		{
			get => m_SoundID;
			set { m_SoundID = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan Delay
		{
			get => m_Delay;
			set { m_Delay = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Active
		{
			get => m_Active;
			set { m_Active = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Point3D PointDest
		{
			get => m_PointDest;
			set { m_PointDest = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public Map MapDest
		{
			get => m_MapDest;
			set { m_MapDest = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Creatures
		{
			get => m_Creatures;
			set { m_Creatures = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CombatCheck
		{
			get => m_CombatCheck;
			set { m_CombatCheck = value; InvalidateProperties(); }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CriminalCheck
		{
			get => m_CriminalCheck;
			set { m_CriminalCheck = value; InvalidateProperties(); }
		}

		public override int LabelNumber => 1026095;  // teleporter

		[Constructable]
		public Teleporter()
			: this(new Point3D(0, 0, 0), null, false)
		{
		}

		[Constructable]
		public Teleporter(Point3D pointDest, Map mapDest)
			: this(pointDest, mapDest, false)
		{
		}

		[Constructable]
		public Teleporter(Point3D pointDest, Map mapDest, bool creatures)
			: base(0x1BC3)
		{
			Movable = false;
			Visible = false;

			m_Active = true;
			m_PointDest = pointDest;
			m_MapDest = mapDest;
			m_Creatures = creatures;

			m_CombatCheck = false;
			m_CriminalCheck = false;
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_Active)
			{
				list.Add(1060742); // active
			}
			else
			{
				list.Add(1060743); // inactive
			}

			if (m_MapDest != null)
			{
				list.Add(1060658, "Map\t{0}", m_MapDest);
			}

			if (m_PointDest != Point3D.Zero)
			{
				list.Add(1060659, "Coords\t{0}", m_PointDest);
			}

			list.Add(1060660, "Creatures\t{0}", m_Creatures ? "Yes" : "No");
		}

		public override void OnSingleClick(Mobile from)
		{
			base.OnSingleClick(from);

			if (m_Active)
			{
				if (m_MapDest != null && m_PointDest != Point3D.Zero)
				{
					LabelTo(from, "{0} [{1}]", m_PointDest, m_MapDest);
				}
				else if (m_MapDest != null)
				{
					LabelTo(from, "[{0}]", m_MapDest);
				}
				else if (m_PointDest != Point3D.Zero)
				{
					LabelTo(from, m_PointDest.ToString());
				}
			}
			else
			{
				LabelTo(from, "(inactive)");
			}
		}

		public virtual bool CanTeleport(Mobile m)
		{
			if (!m_Creatures && !m.Player)
			{
				return false;
			}
			else if (m_CriminalCheck && m.Criminal)
			{
				m.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
				return false;
			}
			else if (m_CombatCheck && SpellHelper.CheckCombat(m))
			{
				m.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
				return false;
			}

			return true;
		}

		public virtual void StartTeleport(Mobile m)
		{
			if (m_Delay == TimeSpan.Zero)
			{
				DoTeleport(m);
			}
			else
			{
				Timer.DelayCall<Mobile>(m_Delay, DoTeleport, m);
			}
		}

		public virtual void DoTeleport(Mobile m)
		{
			var map = m_MapDest;

			if (map == null || map == Map.Internal)
			{
				map = m.Map;
			}

			var p = m_PointDest;

			if (p == Point3D.Zero)
			{
				p = m.Location;
			}

			Server.Mobiles.BaseCreature.TeleportPets(m, p, map);

			var sendEffect = (!m.Hidden || m.AccessLevel == AccessLevel.Player);

			if (m_SourceEffect && sendEffect)
			{
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
			}

			m.MoveToWorld(p, map);

			if (m_DestEffect && sendEffect)
			{
				Effects.SendLocationEffect(m.Location, m.Map, 0x3728, 10, 10);
			}

			if (m_SoundID > 0 && sendEffect)
			{
				Effects.PlaySound(m.Location, m.Map, m_SoundID);
			}
		}

		public override bool OnMoveOver(Mobile m)
		{
			if (m_Active && CanTeleport(m))
			{
				StartTeleport(m);
				return false;
			}

			return true;
		}

		public Teleporter(Serial serial)
			: base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(4); // version

			writer.Write(m_CriminalCheck);
			writer.Write(m_CombatCheck);

			writer.Write(m_SourceEffect);
			writer.Write(m_DestEffect);
			writer.Write(m_Delay);
			writer.WriteEncodedInt(m_SoundID);

			writer.Write(m_Creatures);

			writer.Write(m_Active);
			writer.Write(m_PointDest);
			writer.Write(m_MapDest);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 4:
					{
						m_CriminalCheck = reader.ReadBool();
						goto case 3;
					}
				case 3:
					{
						m_CombatCheck = reader.ReadBool();
						goto case 2;
					}
				case 2:
					{
						m_SourceEffect = reader.ReadBool();
						m_DestEffect = reader.ReadBool();
						m_Delay = reader.ReadTimeSpan();
						m_SoundID = reader.ReadEncodedInt();

						goto case 1;
					}
				case 1:
					{
						m_Creatures = reader.ReadBool();

						goto case 0;
					}
				case 0:
					{
						m_Active = reader.ReadBool();
						m_PointDest = reader.ReadPoint3D();
						m_MapDest = reader.ReadMap();

						break;
					}
			}
		}
	}
}