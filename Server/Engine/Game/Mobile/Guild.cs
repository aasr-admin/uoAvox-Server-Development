
using System;
using System.Collections.Generic;

namespace Server.Guilds
{
	public enum GuildType
	{
		Regular,
		Chaos,
		Order
	}

	public abstract class BaseGuild : ISerializable
	{
		private static int m_NextID = 1;

		public static Dictionary<int, BaseGuild> List { get; } = new();

		public static BaseGuild Find(int id)
		{
			BaseGuild g;

			List.TryGetValue(id, out g);

			return g;
		}

		public static BaseGuild FindByName(string name)
		{
			foreach (var g in List.Values)
			{
				if (g.Name == name)
				{
					return g;
				}
			}

			return null;
		}

		public static BaseGuild FindByAbbrev(string abbr)
		{
			foreach (var g in List.Values)
			{
				if (g.Abbreviation == abbr)
				{
					return g;
				}
			}

			return null;
		}

		public static List<BaseGuild> Search(string find)
		{
			var words = find.ToLower().Split(' ');
			var results = new List<BaseGuild>();

			foreach (var g in List.Values)
			{
				var match = true;
				var name = g.Name.ToLower();
				for (var i = 0; i < words.Length; i++)
				{
					if (!name.Contains(words[i], StringComparison.InvariantCulture))
					{
						match = false;
						break;
					}
				}

				if (match)
				{
					results.Add(g);
				}
			}

			return results;
		}

		internal Serial m_Serial;

		int ISerializable.TypeReference => 0;
		int ISerializable.SerialIdentity => m_Serial;

		[CommandProperty(AccessLevel.Counselor, true)]
		public int Id { get => m_Serial; private set => m_Serial = new Serial(value); }

		protected BaseGuild()
		{
			List.Add(Id = m_NextID++, this);
		}

		protected BaseGuild(int id)
		{
			List.Add(Id = id, this);

			if (++id > m_NextID)
			{
				m_NextID = id;
			}
		}

		public abstract void Deserialize(GenericReader reader);
		public abstract void Serialize(GenericWriter writer);

		public abstract string Abbreviation { get; set; }
		public abstract string Name { get; set; }
		public abstract GuildType Type { get; set; }
		public abstract bool Disbanded { get; }
		public abstract void OnDelete(Mobile mob);

		public override string ToString()
		{
			return $"0x{Id:X} \"{Name} [{Abbreviation}]\"";
		}
	}
}