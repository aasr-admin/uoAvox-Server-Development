using Server.ContextMenus;
using Server.Multis;
using Server.Network;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Server.Engines.Mahjong
{
	public enum MahjongPieceDirection
	{
		Up,
		Left,
		Down,
		Right
	}

	public enum MahjongWind
	{
		North,
		East,
		South,
		West
	}

	public enum MahjongTileType
	{
		Dagger1 = 1,
		Dagger2,
		Dagger3,
		Dagger4,
		Dagger5,
		Dagger6,
		Dagger7,
		Dagger8,
		Dagger9,
		Gem1,
		Gem2,
		Gem3,
		Gem4,
		Gem5,
		Gem6,
		Gem7,
		Gem8,
		Gem9,
		Number1,
		Number2,
		Number3,
		Number4,
		Number5,
		Number6,
		Number7,
		Number8,
		Number9,
		North,
		East,
		South,
		West,
		Green,
		Red,
		White
	}

	public class MahjongGame : Item, ISecurable
	{
		public const int MaxPlayers = 4;
		public const int BaseScore = 30000;

		private MahjongTile[] m_Tiles;
		private MahjongDealerIndicator m_DealerIndicator;
		private MahjongWallBreakIndicator m_WallBreakIndicator;
		private MahjongDice m_Dice;
		private MahjongPlayers m_Players;
		private bool m_ShowScores;
		private bool m_SpectatorVision;
		private DateTime m_LastReset;

		public MahjongTile[] Tiles => m_Tiles;
		public MahjongDealerIndicator DealerIndicator => m_DealerIndicator;
		public MahjongWallBreakIndicator WallBreakIndicator => m_WallBreakIndicator;
		public MahjongDice Dice => m_Dice;
		public MahjongPlayers Players => m_Players;

		private SecureLevel m_Level;

		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level
		{
			get => m_Level;
			set => m_Level = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool ShowScores
		{
			get => m_ShowScores;
			set
			{
				if (m_ShowScores == value)
				{
					return;
				}

				m_ShowScores = value;

				if (value)
				{
					m_Players.SendPlayersPacket(true, true);
				}

				m_Players.SendGeneralPacket(true, true);

				m_Players.SendLocalizedMessage(value ? 1062777 : 1062778); // The dealer has enabled/disabled score display.
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool SpectatorVision
		{
			get => m_SpectatorVision;
			set
			{
				if (m_SpectatorVision == value)
				{
					return;
				}

				m_SpectatorVision = value;

				if (m_Players.IsInGamePlayer(m_Players.DealerPosition))
				{
					m_Players.Dealer.Send(new MahjongGeneralInfo(this));
				}

				m_Players.SendTilesPacket(false, true);

				m_Players.SendLocalizedMessage(value ? 1062715 : 1062716); // The dealer has enabled/disabled Spectator Vision.

				InvalidateProperties();
			}
		}

		[Constructable]
		public MahjongGame() : base(0xFAA)
		{
			Weight = 5.0;

			BuildWalls();
			m_DealerIndicator = new MahjongDealerIndicator(this, new Point2D(300, 300), MahjongPieceDirection.Up, MahjongWind.North);
			m_WallBreakIndicator = new MahjongWallBreakIndicator(this, new Point2D(335, 335));
			m_Dice = new MahjongDice(this);
			m_Players = new MahjongPlayers(this, MaxPlayers, BaseScore);
			m_LastReset = DateTime.UtcNow;
			m_Level = SecureLevel.CoOwners;
		}

		public MahjongGame(Serial serial) : base(serial)
		{
		}

		private void BuildHorizontalWall(ref int index, int x, int y, int stackLevel, MahjongPieceDirection direction, MahjongTileTypeGenerator typeGenerator)
		{
			for (var i = 0; i < 17; i++)
			{
				var position = new Point2D(x + i * 20, y);
				m_Tiles[index + i] = new MahjongTile(this, index + i, typeGenerator.Next(), position, stackLevel, direction, false);
			}

			index += 17;
		}

		private void BuildVerticalWall(ref int index, int x, int y, int stackLevel, MahjongPieceDirection direction, MahjongTileTypeGenerator typeGenerator)
		{
			for (var i = 0; i < 17; i++)
			{
				var position = new Point2D(x, y + i * 20);
				m_Tiles[index + i] = new MahjongTile(this, index + i, typeGenerator.Next(), position, stackLevel, direction, false);
			}

			index += 17;
		}

		private void BuildWalls()
		{
			m_Tiles = new MahjongTile[17 * 8];

			var typeGenerator = new MahjongTileTypeGenerator(4);

			var i = 0;

			BuildHorizontalWall(ref i, 165, 110, 0, MahjongPieceDirection.Up, typeGenerator);
			BuildHorizontalWall(ref i, 165, 115, 1, MahjongPieceDirection.Up, typeGenerator);

			BuildVerticalWall(ref i, 530, 165, 0, MahjongPieceDirection.Left, typeGenerator);
			BuildVerticalWall(ref i, 525, 165, 1, MahjongPieceDirection.Left, typeGenerator);

			BuildHorizontalWall(ref i, 165, 530, 0, MahjongPieceDirection.Down, typeGenerator);
			BuildHorizontalWall(ref i, 165, 525, 1, MahjongPieceDirection.Down, typeGenerator);

			BuildVerticalWall(ref i, 110, 165, 0, MahjongPieceDirection.Right, typeGenerator);
			BuildVerticalWall(ref i, 115, 165, 1, MahjongPieceDirection.Right, typeGenerator);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (m_SpectatorVision)
			{
				list.Add(1062717); // Spectator Vision Enabled
			}
			else
			{
				list.Add(1062718); // Spectator Vision Disabled
			}
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);

			m_Players.CheckPlayers();

			if (from.Alive && IsAccessibleTo(from) && m_Players.GetInGameMobiles(true, false).Count == 0)
			{
				list.Add(new ResetGameEntry(this));
			}

			SetSecureLevelEntry.AddTo(from, this, list);
		}

		private class ResetGameEntry : ContextMenuEntry
		{
			private readonly MahjongGame m_Game;

			public ResetGameEntry(MahjongGame game) : base(6162)
			{
				m_Game = game;
			}

			public override void OnClick()
			{
				var from = Owner.From;

				if (from.CheckAlive() && !m_Game.Deleted && m_Game.IsAccessibleTo(from) && m_Game.Players.GetInGameMobiles(true, false).Count == 0)
				{
					m_Game.ResetGame(from);
				}
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			m_Players.CheckPlayers();

			m_Players.Join(from);
		}

		public void ResetGame(Mobile from)
		{
			if (DateTime.UtcNow - m_LastReset < TimeSpan.FromSeconds(5.0))
			{
				return;
			}

			m_LastReset = DateTime.UtcNow;

			if (from != null)
			{
				m_Players.SendLocalizedMessage(1062771, from.Name); // ~1_name~ has reset the game.
			}

			m_Players.SendRelievePacket(true, true);

			BuildWalls();
			m_DealerIndicator = new MahjongDealerIndicator(this, new Point2D(300, 300), MahjongPieceDirection.Up, MahjongWind.North);
			m_WallBreakIndicator = new MahjongWallBreakIndicator(this, new Point2D(335, 335));
			m_Players = new MahjongPlayers(this, MaxPlayers, BaseScore);
		}

		public void ResetWalls(Mobile from)
		{
			if (DateTime.UtcNow - m_LastReset < TimeSpan.FromSeconds(5.0))
			{
				return;
			}

			m_LastReset = DateTime.UtcNow;

			BuildWalls();

			m_Players.SendTilesPacket(true, true);

			if (from != null)
			{
				m_Players.SendLocalizedMessage(1062696); // The dealer rebuilds the wall.
			}
		}

		public int GetStackLevel(MahjongPieceDim dim)
		{
			var level = -1;
			foreach (var tile in m_Tiles)
			{
				if (tile.StackLevel > level && dim.IsOverlapping(tile.Dimensions))
				{
					level = tile.StackLevel;
				}
			}
			return level;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(1); // version

			writer.Write((int)m_Level);

			writer.Write(m_Tiles.Length);

			for (var i = 0; i < m_Tiles.Length; i++)
			{
				m_Tiles[i].Save(writer);
			}

			m_DealerIndicator.Save(writer);

			m_WallBreakIndicator.Save(writer);

			m_Dice.Save(writer);

			m_Players.Save(writer);

			writer.Write(m_ShowScores);
			writer.Write(m_SpectatorVision);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt();

			switch (version)
			{
				case 1:
					{
						m_Level = (SecureLevel)reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version < 1)
						{
							m_Level = SecureLevel.CoOwners;
						}

						var length = reader.ReadInt();
						m_Tiles = new MahjongTile[length];

						for (var i = 0; i < length; i++)
						{
							m_Tiles[i] = new MahjongTile(this, reader);
						}

						m_DealerIndicator = new MahjongDealerIndicator(this, reader);

						m_WallBreakIndicator = new MahjongWallBreakIndicator(this, reader);

						m_Dice = new MahjongDice(this, reader);

						m_Players = new MahjongPlayers(this, reader);

						m_ShowScores = reader.ReadBool();
						m_SpectatorVision = reader.ReadBool();

						m_LastReset = DateTime.UtcNow;

						break;
					}
			}
		}
	}

	public class MahjongDealerIndicator
	{
		public static MahjongPieceDim GetDimensions(Point2D position, MahjongPieceDirection direction)
		{
			if (direction == MahjongPieceDirection.Up || direction == MahjongPieceDirection.Down)
			{
				return new MahjongPieceDim(position, 40, 20);
			}
			else
			{
				return new MahjongPieceDim(position, 20, 40);
			}
		}

		private readonly MahjongGame m_Game;
		private Point2D m_Position;
		private MahjongPieceDirection m_Direction;
		private MahjongWind m_Wind;

		public MahjongGame Game => m_Game;
		public Point2D Position => m_Position;
		public MahjongPieceDirection Direction => m_Direction;
		public MahjongWind Wind => m_Wind;

		public MahjongDealerIndicator(MahjongGame game, Point2D position, MahjongPieceDirection direction, MahjongWind wind)
		{
			m_Game = game;
			m_Position = position;
			m_Direction = direction;
			m_Wind = wind;
		}

		public MahjongPieceDim Dimensions => GetDimensions(m_Position, m_Direction);

		public void Move(Point2D position, MahjongPieceDirection direction, MahjongWind wind)
		{
			var dim = GetDimensions(position, direction);

			if (!dim.IsValid())
			{
				return;
			}

			m_Position = position;
			m_Direction = direction;
			m_Wind = wind;

			m_Game.Players.SendGeneralPacket(true, true);
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Position);
			writer.Write((int)m_Direction);
			writer.Write((int)m_Wind);
		}

		public MahjongDealerIndicator(MahjongGame game, GenericReader reader)
		{
			m_Game = game;

			var version = reader.ReadInt();

			m_Position = reader.ReadPoint2D();
			m_Direction = (MahjongPieceDirection)reader.ReadInt();
			m_Wind = (MahjongWind)reader.ReadInt();
		}
	}

	public class MahjongPlayers
	{
		private readonly MahjongGame m_Game;
		private readonly Mobile[] m_Players;
		private readonly bool[] m_InGame;
		private readonly bool[] m_PublicHand;
		private readonly int[] m_Scores;
		private int m_DealerPosition;
		private readonly ArrayList m_Spectators;

		public MahjongGame Game => m_Game;
		public int Seats => m_Players.Length;
		public Mobile Dealer => m_Players[m_DealerPosition];
		public int DealerPosition => m_DealerPosition;

		public MahjongPlayers(MahjongGame game, int maxPlayers, int baseScore)
		{
			m_Game = game;
			m_Spectators = new ArrayList();

			m_Players = new Mobile[maxPlayers];
			m_InGame = new bool[maxPlayers];
			m_PublicHand = new bool[maxPlayers];
			m_Scores = new int[maxPlayers];

			for (var i = 0; i < m_Scores.Length; i++)
			{
				m_Scores[i] = baseScore;
			}
		}

		public Mobile GetPlayer(int index)
		{
			if (index < 0 || index >= m_Players.Length)
			{
				return null;
			}
			else
			{
				return m_Players[index];
			}
		}

		public int GetPlayerIndex(Mobile mobile)
		{
			for (var i = 0; i < m_Players.Length; i++)
			{
				if (m_Players[i] == mobile)
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsInGameDealer(Mobile mobile)
		{
			if (Dealer != mobile)
			{
				return false;
			}
			else
			{
				return m_InGame[m_DealerPosition];
			}
		}

		public bool IsInGamePlayer(int index)
		{
			if (index < 0 || index >= m_Players.Length || m_Players[index] == null)
			{
				return false;
			}
			else
			{
				return m_InGame[index];
			}
		}

		public bool IsInGamePlayer(Mobile mobile)
		{
			var index = GetPlayerIndex(mobile);

			return IsInGamePlayer(index);
		}

		public bool IsSpectator(Mobile mobile)
		{
			return m_Spectators.Contains(mobile);
		}

		public int GetScore(int index)
		{
			if (index < 0 || index >= m_Scores.Length)
			{
				return 0;
			}
			else
			{
				return m_Scores[index];
			}
		}

		public bool IsPublic(int index)
		{
			if (index < 0 || index >= m_PublicHand.Length)
			{
				return false;
			}
			else
			{
				return m_PublicHand[index];
			}
		}

		public void SetPublic(int index, bool value)
		{
			if (index < 0 || index >= m_PublicHand.Length || m_PublicHand[index] == value)
			{
				return;
			}

			m_PublicHand[index] = value;

			SendTilesPacket(true, !m_Game.SpectatorVision);

			if (IsInGamePlayer(index))
			{
				m_Players[index].SendLocalizedMessage(value ? 1062775 : 1062776); // Your hand is [not] publicly viewable.
			}
		}

		public ArrayList GetInGameMobiles(bool players, bool spectators)
		{
			var list = new ArrayList();

			if (players)
			{
				for (var i = 0; i < m_Players.Length; i++)
				{
					if (IsInGamePlayer(i))
					{
						list.Add(m_Players[i]);
					}
				}
			}

			if (spectators)
			{
				list.AddRange(m_Spectators);
			}

			return list;
		}

		public void CheckPlayers()
		{
			var removed = false;

			for (var i = 0; i < m_Players.Length; i++)
			{
				var player = m_Players[i];

				if (player != null)
				{
					if (player.Deleted)
					{
						m_Players[i] = null;

						SendPlayerExitMessage(player);
						UpdateDealer(true);

						removed = true;
					}
					else if (m_InGame[i])
					{
						if (player.NetState == null)
						{
							m_InGame[i] = false;

							SendPlayerExitMessage(player);
							UpdateDealer(true);

							removed = true;
						}
						else if (!m_Game.IsAccessibleTo(player) || player.Map != m_Game.Map || !player.InRange(m_Game.GetWorldLocation(), 5))
						{
							m_InGame[i] = false;

							player.Send(new MahjongRelieve(m_Game));

							SendPlayerExitMessage(player);
							UpdateDealer(true);

							removed = true;
						}
					}
				}
			}

			for (var i = 0; i < m_Spectators.Count;)
			{
				var mobile = (Mobile)m_Spectators[i];

				if (mobile.NetState == null || mobile.Deleted)
				{
					m_Spectators.RemoveAt(i);
				}
				else if (!m_Game.IsAccessibleTo(mobile) || mobile.Map != m_Game.Map || !mobile.InRange(m_Game.GetWorldLocation(), 5))
				{
					m_Spectators.RemoveAt(i);

					mobile.Send(new MahjongRelieve(m_Game));
				}
				else
				{
					i++;
				}
			}

			if (removed && !UpdateSpectators())
			{
				SendPlayersPacket(true, true);
			}
		}

		private void UpdateDealer(bool message)
		{
			if (IsInGamePlayer(m_DealerPosition))
			{
				return;
			}

			for (var i = m_DealerPosition + 1; i < m_Players.Length; i++)
			{
				if (IsInGamePlayer(i))
				{
					m_DealerPosition = i;

					if (message)
					{
						SendDealerChangedMessage();
					}

					return;
				}
			}

			for (var i = 0; i < m_DealerPosition; i++)
			{
				if (IsInGamePlayer(i))
				{
					m_DealerPosition = i;

					if (message)
					{
						SendDealerChangedMessage();
					}

					return;
				}
			}
		}

		private int GetNextSeat()
		{
			for (var i = m_DealerPosition; i < m_Players.Length; i++)
			{
				if (m_Players[i] == null)
				{
					return i;
				}
			}

			for (var i = 0; i < m_DealerPosition; i++)
			{
				if (m_Players[i] == null)
				{
					return i;
				}
			}

			return -1;
		}

		private bool UpdateSpectators()
		{
			if (m_Spectators.Count == 0)
			{
				return false;
			}

			var nextSeat = GetNextSeat();

			if (nextSeat >= 0)
			{
				var newPlayer = (Mobile)m_Spectators[0];

				m_Spectators.RemoveAt(0);

				AddPlayer(newPlayer, nextSeat, false);

				UpdateSpectators();

				return true;
			}
			else
			{
				return false;
			}
		}

		private void AddPlayer(Mobile player, int index, bool sendJoinGame)
		{
			m_Players[index] = player;
			m_InGame[index] = true;

			UpdateDealer(false);

			if (sendJoinGame)
			{
				player.Send(new MahjongJoinGame(m_Game));
			}

			SendPlayersPacket(true, true);

			player.Send(new MahjongGeneralInfo(m_Game));
			player.Send(new MahjongTilesInfo(m_Game, player));

			if (m_DealerPosition == index)
			{
				SendLocalizedMessage(1062773, player.Name); // ~1_name~ has entered the game as the dealer.
			}
			else
			{
				SendLocalizedMessage(1062772, player.Name); // ~1_name~ has entered the game as a player.
			}
		}

		private void AddSpectator(Mobile mobile)
		{
			if (!IsSpectator(mobile))
			{
				m_Spectators.Add(mobile);
			}

			mobile.Send(new MahjongJoinGame(m_Game));
			mobile.Send(new MahjongPlayersInfo(m_Game, mobile));
			mobile.Send(new MahjongGeneralInfo(m_Game));
			mobile.Send(new MahjongTilesInfo(m_Game, mobile));
		}

		public void Join(Mobile mobile)
		{
			var index = GetPlayerIndex(mobile);

			if (index >= 0)
			{
				AddPlayer(mobile, index, true);
			}
			else
			{
				var nextSeat = GetNextSeat();

				if (nextSeat >= 0)
				{
					AddPlayer(mobile, nextSeat, true);
				}
				else
				{
					AddSpectator(mobile);
				}
			}
		}

		public void LeaveGame(Mobile player)
		{
			var index = GetPlayerIndex(player);
			if (index >= 0)
			{
				m_InGame[index] = false;

				SendPlayerExitMessage(player);
				UpdateDealer(true);

				SendPlayersPacket(true, true);
			}
			else
			{
				m_Spectators.Remove(player);
			}
		}

		public void ResetScores(int value)
		{
			for (var i = 0; i < m_Scores.Length; i++)
			{
				m_Scores[i] = value;
			}

			SendPlayersPacket(true, m_Game.ShowScores);

			SendLocalizedMessage(1062697); // The dealer redistributes the score sticks evenly.
		}

		public void TransferScore(Mobile from, int toPosition, int amount)
		{
			var fromPosition = GetPlayerIndex(from);
			var to = GetPlayer(toPosition);

			if (fromPosition < 0 || to == null || m_Scores[fromPosition] < amount)
			{
				return;
			}

			m_Scores[fromPosition] -= amount;
			m_Scores[toPosition] += amount;

			if (m_Game.ShowScores)
			{
				SendPlayersPacket(true, true);
			}
			else
			{
				from.Send(new MahjongPlayersInfo(m_Game, from));
				to.Send(new MahjongPlayersInfo(m_Game, to));
			}

			SendLocalizedMessage(1062774, String.Format("{0}\t{1}\t{2}", from.Name, to.Name, amount)); // ~1_giver~ gives ~2_receiver~ ~3_number~ points.
		}

		public void OpenSeat(int index)
		{
			var player = GetPlayer(index);
			if (player == null)
			{
				return;
			}

			if (m_InGame[index])
			{
				player.Send(new MahjongRelieve(m_Game));
			}

			m_Players[index] = null;

			SendLocalizedMessage(1062699, player.Name); // ~1_name~ is relieved from the game by the dealer.

			UpdateDealer(true);

			if (!UpdateSpectators())
			{
				SendPlayersPacket(true, true);
			}
		}

		public void AssignDealer(int index)
		{
			var to = GetPlayer(index);

			if (to == null || !m_InGame[index])
			{
				return;
			}

			var oldDealer = m_DealerPosition;

			m_DealerPosition = index;

			if (IsInGamePlayer(oldDealer))
			{
				m_Players[oldDealer].Send(new MahjongPlayersInfo(m_Game, m_Players[oldDealer]));
			}

			to.Send(new MahjongPlayersInfo(m_Game, to));

			SendDealerChangedMessage();
		}

		private void SendDealerChangedMessage()
		{
			if (Dealer != null)
			{
				SendLocalizedMessage(1062698, Dealer.Name); // ~1_name~ is assigned the dealer.
			}
		}

		private void SendPlayerExitMessage(Mobile who)
		{
			SendLocalizedMessage(1062762, who.Name); // ~1_name~ has left the game.
		}

		public void SendPlayersPacket(bool players, bool spectators)
		{
			foreach (Mobile mobile in GetInGameMobiles(players, spectators))
			{
				mobile.Send(new MahjongPlayersInfo(m_Game, mobile));
			}
		}

		public void SendGeneralPacket(bool players, bool spectators)
		{
			var mobiles = GetInGameMobiles(players, spectators);

			if (mobiles.Count == 0)
			{
				return;
			}

			var generalInfo = new MahjongGeneralInfo(m_Game);

			generalInfo.Acquire();

			foreach (Mobile mobile in mobiles)
			{
				mobile.Send(generalInfo);
			}

			generalInfo.Release();
		}

		public void SendTilesPacket(bool players, bool spectators)
		{
			foreach (Mobile mobile in GetInGameMobiles(players, spectators))
			{
				mobile.Send(new MahjongTilesInfo(m_Game, mobile));
			}
		}

		public void SendTilePacket(MahjongTile tile, bool players, bool spectators)
		{
			foreach (Mobile mobile in GetInGameMobiles(players, spectators))
			{
				mobile.Send(new MahjongTileInfo(tile, mobile));
			}
		}

		public void SendRelievePacket(bool players, bool spectators)
		{
			var mobiles = GetInGameMobiles(players, spectators);

			if (mobiles.Count == 0)
			{
				return;
			}

			var relieve = new MahjongRelieve(m_Game);

			relieve.Acquire();

			foreach (Mobile mobile in mobiles)
			{
				mobile.Send(relieve);
			}

			relieve.Release();
		}

		public void SendLocalizedMessage(int number)
		{
			foreach (Mobile mobile in GetInGameMobiles(true, true))
			{
				mobile.SendLocalizedMessage(number);
			}
		}

		public void SendLocalizedMessage(int number, string args)
		{
			foreach (Mobile mobile in GetInGameMobiles(true, true))
			{
				mobile.SendLocalizedMessage(number, args);
			}
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(Seats);

			for (var i = 0; i < Seats; i++)
			{
				writer.Write(m_Players[i]);
				writer.Write(m_PublicHand[i]);
				writer.Write(m_Scores[i]);
			}

			writer.Write(m_DealerPosition);
		}

		public MahjongPlayers(MahjongGame game, GenericReader reader)
		{
			m_Game = game;
			m_Spectators = new ArrayList();

			var version = reader.ReadInt();

			var seats = reader.ReadInt();
			m_Players = new Mobile[seats];
			m_InGame = new bool[seats];
			m_PublicHand = new bool[seats];
			m_Scores = new int[seats];

			for (var i = 0; i < seats; i++)
			{
				m_Players[i] = reader.ReadMobile();
				m_PublicHand[i] = reader.ReadBool();
				m_Scores[i] = reader.ReadInt();
			}

			m_DealerPosition = reader.ReadInt();
		}
	}

	#region Game Packets

	public delegate void OnMahjongPacketReceive(MahjongGame game, NetState state, PacketReader pvSrc);

	public sealed class MahjongPacketHandlers
	{
		private static readonly OnMahjongPacketReceive[] m_SubCommandDelegates = new OnMahjongPacketReceive[0x100];

		public static void RegisterSubCommand(int subCmd, OnMahjongPacketReceive onReceive)
		{
			m_SubCommandDelegates[subCmd] = onReceive;
		}

		public static OnMahjongPacketReceive GetSubCommandDelegate(int cmd)
		{
			if (cmd >= 0 && cmd < 0x100)
			{
				return m_SubCommandDelegates[cmd];
			}
			else
			{
				return null;
			}
		}

		public static void Initialize()
		{
			PacketHandlers.Register(0xDA, 0, true, new OnPacketReceive(OnPacket));

			RegisterSubCommand(0x6, new OnMahjongPacketReceive(ExitGame));
			RegisterSubCommand(0xA, new OnMahjongPacketReceive(GivePoints));
			RegisterSubCommand(0xB, new OnMahjongPacketReceive(RollDice));
			RegisterSubCommand(0xC, new OnMahjongPacketReceive(BuildWalls));
			RegisterSubCommand(0xD, new OnMahjongPacketReceive(ResetScores));
			RegisterSubCommand(0xF, new OnMahjongPacketReceive(AssignDealer));
			RegisterSubCommand(0x10, new OnMahjongPacketReceive(OpenSeat));
			RegisterSubCommand(0x11, new OnMahjongPacketReceive(ChangeOption));
			RegisterSubCommand(0x15, new OnMahjongPacketReceive(MoveWallBreakIndicator));
			RegisterSubCommand(0x16, new OnMahjongPacketReceive(TogglePublicHand));
			RegisterSubCommand(0x17, new OnMahjongPacketReceive(MoveTile));
			RegisterSubCommand(0x18, new OnMahjongPacketReceive(MoveDealerIndicator));
		}

		public static void OnPacket(NetState state, PacketReader pvSrc)
		{
			var game = pvSrc.ReadItem() as MahjongGame;

			if (game != null)
			{
				game.Players.CheckPlayers();
			}

			pvSrc.ReadByte();

			int cmd = pvSrc.ReadByte();

			var onReceive = GetSubCommandDelegate(cmd);

			if (onReceive != null)
			{
				onReceive(game, state, pvSrc);
			}
			else
			{
				pvSrc.Trace(state);
			}
		}

		private static MahjongPieceDirection GetDirection(int value)
		{
			switch (value)
			{
				case 0: return MahjongPieceDirection.Up;
				case 1: return MahjongPieceDirection.Left;
				case 2: return MahjongPieceDirection.Down;
				default: return MahjongPieceDirection.Right;
			}
		}

		private static MahjongWind GetWind(int value)
		{
			switch (value)
			{
				case 0: return MahjongWind.North;
				case 1: return MahjongWind.East;
				case 2: return MahjongWind.South;
				default: return MahjongWind.West;
			}
		}

		public static void ExitGame(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null)
			{
				return;
			}

			var from = state.Mobile;

			game.Players.LeaveGame(from);
		}

		public static void GivePoints(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			{
				return;
			}

			int to = pvSrc.ReadByte();
			var amount = pvSrc.ReadInt32();

			game.Players.TransferScore(state.Mobile, to, amount);
		}

		public static void RollDice(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			{
				return;
			}

			game.Dice.RollDice(state.Mobile);
		}

		public static void BuildWalls(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			game.ResetWalls(state.Mobile);
		}

		public static void ResetScores(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			game.Players.ResetScores(MahjongGame.BaseScore);
		}

		public static void AssignDealer(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			int position = pvSrc.ReadByte();

			game.Players.AssignDealer(position);
		}

		public static void OpenSeat(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			int position = pvSrc.ReadByte();

			if (game.Players.GetPlayer(position) == state.Mobile)
			{
				return;
			}

			game.Players.OpenSeat(position);
		}

		public static void ChangeOption(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			pvSrc.ReadInt16();
			pvSrc.ReadByte();

			int options = pvSrc.ReadByte();

			game.ShowScores = (options & 0x1) != 0;
			game.SpectatorVision = (options & 0x2) != 0;
		}

		public static void MoveWallBreakIndicator(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			int y = pvSrc.ReadInt16();
			int x = pvSrc.ReadInt16();

			game.WallBreakIndicator.Move(new Point2D(x, y));
		}

		public static void TogglePublicHand(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			{
				return;
			}

			pvSrc.ReadInt16();
			pvSrc.ReadByte();

			var publicHand = pvSrc.ReadBoolean();

			game.Players.SetPublic(game.Players.GetPlayerIndex(state.Mobile), publicHand);
		}

		public static void MoveTile(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGamePlayer(state.Mobile))
			{
				return;
			}

			int number = pvSrc.ReadByte();

			if (number < 0 || number >= game.Tiles.Length)
			{
				return;
			}

			pvSrc.ReadByte(); // Current direction

			var direction = GetDirection(pvSrc.ReadByte());

			pvSrc.ReadByte();

			var flip = pvSrc.ReadBoolean();

			pvSrc.ReadInt16(); // Current Y
			pvSrc.ReadInt16(); // Current X

			pvSrc.ReadByte();

			int y = pvSrc.ReadInt16();
			int x = pvSrc.ReadInt16();

			pvSrc.ReadByte();

			game.Tiles[number].Move(new Point2D(x, y), direction, flip, game.Players.GetPlayerIndex(state.Mobile));
		}

		public static void MoveDealerIndicator(MahjongGame game, NetState state, PacketReader pvSrc)
		{
			if (game == null || !game.Players.IsInGameDealer(state.Mobile))
			{
				return;
			}

			var direction = GetDirection(pvSrc.ReadByte());

			var wind = GetWind(pvSrc.ReadByte());

			int y = pvSrc.ReadInt16();
			int x = pvSrc.ReadInt16();

			game.DealerIndicator.Move(new Point2D(x, y), direction, wind);
		}
	}

	public sealed class MahjongJoinGame : Packet
	{
		public MahjongJoinGame(MahjongGame game) : base(0xDA)
		{
			EnsureCapacity(9);

			m_Stream.Write(game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x19);
		}
	}

	public sealed class MahjongPlayersInfo : Packet
	{
		public MahjongPlayersInfo(MahjongGame game, Mobile to) : base(0xDA)
		{
			var players = game.Players;

			EnsureCapacity(11 + 45 * players.Seats);

			m_Stream.Write(game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x2);

			m_Stream.Write((byte)0);
			m_Stream.Write((byte)players.Seats);

			var n = 0;
			for (var i = 0; i < players.Seats; i++)
			{
				var mobile = players.GetPlayer(i);

				if (mobile != null)
				{
					m_Stream.Write(mobile.Serial);
					m_Stream.Write(players.DealerPosition == i ? (byte)0x1 : (byte)0x2);
					m_Stream.Write((byte)i);

					if (game.ShowScores || mobile == to)
					{
						m_Stream.Write(players.GetScore(i));
					}
					else
					{
						m_Stream.Write(0);
					}

					m_Stream.Write((short)0);
					m_Stream.Write((byte)0);

					m_Stream.Write(players.IsPublic(i));

					m_Stream.WriteAsciiFixed(mobile.Name, 30);
					m_Stream.Write(!players.IsInGamePlayer(i));

					n++;
				}
				else if (game.ShowScores)
				{
					m_Stream.Write(0);
					m_Stream.Write((byte)0x2);
					m_Stream.Write((byte)i);

					m_Stream.Write(players.GetScore(i));

					m_Stream.Write((short)0);
					m_Stream.Write((byte)0);

					m_Stream.Write(players.IsPublic(i));

					m_Stream.WriteAsciiFixed("", 30);
					m_Stream.Write(true);

					n++;
				}
			}

			if (n != players.Seats)
			{
				m_Stream.Seek(10, System.IO.SeekOrigin.Begin);
				m_Stream.Write((byte)n);
			}
		}
	}

	public sealed class MahjongGeneralInfo : Packet
	{
		public MahjongGeneralInfo(MahjongGame game) : base(0xDA)
		{
			EnsureCapacity(13);

			m_Stream.Write(game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x5);

			m_Stream.Write((short)0);
			m_Stream.Write((byte)0);

			m_Stream.Write((byte)((game.ShowScores ? 0x1 : 0x0) | (game.SpectatorVision ? 0x2 : 0x0)));

			m_Stream.Write((byte)game.Dice.First);
			m_Stream.Write((byte)game.Dice.Second);

			m_Stream.Write((byte)game.DealerIndicator.Wind);
			m_Stream.Write((short)game.DealerIndicator.Position.Y);
			m_Stream.Write((short)game.DealerIndicator.Position.X);
			m_Stream.Write((byte)game.DealerIndicator.Direction);

			m_Stream.Write((short)game.WallBreakIndicator.Position.Y);
			m_Stream.Write((short)game.WallBreakIndicator.Position.X);
		}
	}

	public sealed class MahjongTilesInfo : Packet
	{
		public MahjongTilesInfo(MahjongGame game, Mobile to) : base(0xDA)
		{
			var tiles = game.Tiles;
			var players = game.Players;

			EnsureCapacity(11 + 9 * tiles.Length);

			m_Stream.Write(game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x4);

			m_Stream.Write((short)tiles.Length);

			foreach (var tile in tiles)
			{
				m_Stream.Write((byte)tile.Number);

				if (tile.Flipped)
				{
					var hand = tile.Dimensions.GetHandArea();

					if (hand < 0 || players.IsPublic(hand) || players.GetPlayer(hand) == to || (game.SpectatorVision && players.IsSpectator(to)))
					{
						m_Stream.Write((byte)tile.Value);
					}
					else
					{
						m_Stream.Write((byte)0);
					}
				}
				else
				{
					m_Stream.Write((byte)0);
				}

				m_Stream.Write((short)tile.Position.Y);
				m_Stream.Write((short)tile.Position.X);
				m_Stream.Write((byte)tile.StackLevel);
				m_Stream.Write((byte)tile.Direction);

				m_Stream.Write(tile.Flipped ? (byte)0x10 : (byte)0x0);
			}
		}
	}

	public sealed class MahjongTileInfo : Packet
	{
		public MahjongTileInfo(MahjongTile tile, Mobile to) : base(0xDA)
		{
			var game = tile.Game;
			var players = game.Players;

			EnsureCapacity(18);

			m_Stream.Write(tile.Game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x3);

			m_Stream.Write((byte)tile.Number);

			if (tile.Flipped)
			{
				var hand = tile.Dimensions.GetHandArea();

				if (hand < 0 || players.IsPublic(hand) || players.GetPlayer(hand) == to || (game.SpectatorVision && players.IsSpectator(to)))
				{
					m_Stream.Write((byte)tile.Value);
				}
				else
				{
					m_Stream.Write((byte)0);
				}
			}
			else
			{
				m_Stream.Write((byte)0);
			}

			m_Stream.Write((short)tile.Position.Y);
			m_Stream.Write((short)tile.Position.X);
			m_Stream.Write((byte)tile.StackLevel);
			m_Stream.Write((byte)tile.Direction);

			m_Stream.Write(tile.Flipped ? (byte)0x10 : (byte)0x0);
		}
	}

	public sealed class MahjongRelieve : Packet
	{
		public MahjongRelieve(MahjongGame game) : base(0xDA)
		{
			EnsureCapacity(9);

			m_Stream.Write(game.Serial);
			m_Stream.Write((byte)0);
			m_Stream.Write((byte)0x1A);
		}
	}

	#endregion

	public struct MahjongPieceDim
	{
		private Point2D m_Position;
		private readonly int m_Width;
		private readonly int m_Height;

		public Point2D Position => m_Position;
		public int Width => m_Width;
		public int Height => m_Height;

		public MahjongPieceDim(Point2D position, int width, int height)
		{
			m_Position = position;
			m_Width = width;
			m_Height = height;
		}

		public bool IsValid()
		{
			return m_Position.X >= 0 && m_Position.Y >= 0 && m_Position.X + m_Width <= 670 && m_Position.Y + m_Height <= 670;
		}

		public bool IsOverlapping(MahjongPieceDim dim)
		{
			return m_Position.X < dim.m_Position.X + dim.m_Width && m_Position.Y < dim.m_Position.Y + dim.m_Height && m_Position.X + m_Width > dim.m_Position.X && m_Position.Y + m_Height > dim.m_Position.Y;
		}

		public int GetHandArea()
		{
			if (m_Position.X + m_Width > 150 && m_Position.X < 520 && m_Position.Y < 35)
			{
				return 0;
			}

			if (m_Position.X + m_Width > 635 && m_Position.Y + m_Height > 150 && m_Position.Y < 520)
			{
				return 1;
			}

			if (m_Position.X + m_Width > 150 && m_Position.X < 520 && m_Position.Y + m_Height > 635)
			{
				return 2;
			}

			if (m_Position.X < 35 && m_Position.Y + m_Height > 150 && m_Position.Y < 520)
			{
				return 3;
			}

			return -1;
		}
	}

	public class MahjongTile
	{
		public static MahjongPieceDim GetDimensions(Point2D position, MahjongPieceDirection direction)
		{
			if (direction == MahjongPieceDirection.Up || direction == MahjongPieceDirection.Down)
			{
				return new MahjongPieceDim(position, 20, 30);
			}
			else
			{
				return new MahjongPieceDim(position, 30, 20);
			}
		}

		private readonly MahjongGame m_Game;
		private readonly int m_Number;
		private readonly MahjongTileType m_Value;
		protected Point2D m_Position;
		private int m_StackLevel;
		private MahjongPieceDirection m_Direction;
		private bool m_Flipped;

		public MahjongGame Game => m_Game;
		public int Number => m_Number;
		public MahjongTileType Value => m_Value;
		public Point2D Position => m_Position;
		public int StackLevel => m_StackLevel;
		public MahjongPieceDirection Direction => m_Direction;
		public bool Flipped => m_Flipped;

		public MahjongTile(MahjongGame game, int number, MahjongTileType value, Point2D position, int stackLevel, MahjongPieceDirection direction, bool flipped)
		{
			m_Game = game;
			m_Number = number;
			m_Value = value;
			m_Position = position;
			m_StackLevel = stackLevel;
			m_Direction = direction;
			m_Flipped = flipped;
		}

		public MahjongPieceDim Dimensions => GetDimensions(m_Position, m_Direction);

		public bool IsMovable => m_Game.GetStackLevel(Dimensions) <= m_StackLevel;

		public void Move(Point2D position, MahjongPieceDirection direction, bool flip, int validHandArea)
		{
			var dim = GetDimensions(position, direction);
			var curHandArea = Dimensions.GetHandArea();
			var newHandArea = dim.GetHandArea();

			if (!IsMovable || !dim.IsValid() || (validHandArea >= 0 && ((curHandArea >= 0 && curHandArea != validHandArea) || (newHandArea >= 0 && newHandArea != validHandArea))))
			{
				return;
			}

			m_Position = position;
			m_Direction = direction;
			m_StackLevel = -1; // Avoid self interference
			m_StackLevel = m_Game.GetStackLevel(dim) + 1;
			m_Flipped = flip;

			m_Game.Players.SendTilePacket(this, true, true);
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Number);
			writer.Write((int)m_Value);
			writer.Write(m_Position);
			writer.Write(m_StackLevel);
			writer.Write((int)m_Direction);
			writer.Write(m_Flipped);
		}

		public MahjongTile(MahjongGame game, GenericReader reader)
		{
			m_Game = game;

			var version = reader.ReadInt();

			m_Number = reader.ReadInt();
			m_Value = (MahjongTileType)reader.ReadInt();
			m_Position = reader.ReadPoint2D();
			m_StackLevel = reader.ReadInt();
			m_Direction = (MahjongPieceDirection)reader.ReadInt();
			m_Flipped = reader.ReadBool();
		}
	}

	public class MahjongTileTypeGenerator
	{
		private readonly ArrayList m_LeftTileTypes;

		public ArrayList LeftTileTypes => m_LeftTileTypes;

		public MahjongTileTypeGenerator(int count)
		{
			m_LeftTileTypes = new ArrayList(34 * count);

			for (var i = 1; i <= 34; i++)
			{
				for (var j = 0; j < count; j++)
				{
					m_LeftTileTypes.Add((MahjongTileType)i);
				}
			}
		}

		public MahjongTileType Next()
		{
			var random = Utility.Random(m_LeftTileTypes.Count);
			var next = (MahjongTileType)m_LeftTileTypes[random];
			m_LeftTileTypes.RemoveAt(random);

			return next;
		}
	}

	public class MahjongWallBreakIndicator
	{
		public static MahjongPieceDim GetDimensions(Point2D position)
		{
			return new MahjongPieceDim(position, 20, 20);
		}

		private readonly MahjongGame m_Game;
		private Point2D m_Position;

		public MahjongGame Game => m_Game;
		public Point2D Position => m_Position;

		public MahjongWallBreakIndicator(MahjongGame game, Point2D position)
		{
			m_Game = game;
			m_Position = position;
		}

		public MahjongPieceDim Dimensions => GetDimensions(m_Position);

		public void Move(Point2D position)
		{
			var dim = GetDimensions(position);

			if (!dim.IsValid())
			{
				return;
			}

			m_Position = position;

			m_Game.Players.SendGeneralPacket(true, true);
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_Position);
		}

		public MahjongWallBreakIndicator(MahjongGame game, GenericReader reader)
		{
			m_Game = game;

			var version = reader.ReadInt();

			m_Position = reader.ReadPoint2D();
		}
	}

	public class MahjongDice
	{
		private readonly MahjongGame m_Game;
		private int m_First;
		private int m_Second;

		public MahjongGame Game => m_Game;
		public int First => m_First;
		public int Second => m_Second;

		public MahjongDice(MahjongGame game)
		{
			m_Game = game;
			m_First = Utility.Random(1, 6);
			m_Second = Utility.Random(1, 6);
		}

		public void RollDice(Mobile from)
		{
			m_First = Utility.Random(1, 6);
			m_Second = Utility.Random(1, 6);

			m_Game.Players.SendGeneralPacket(true, true);

			if (from != null)
			{
				m_Game.Players.SendLocalizedMessage(1062695, String.Format("{0}\t{1}\t{2}", from.Name, m_First, m_Second)); // ~1_name~ rolls the dice and gets a ~2_number~ and a ~3_number~!
			}
		}

		public void Save(GenericWriter writer)
		{
			writer.Write(0); // version

			writer.Write(m_First);
			writer.Write(m_Second);
		}

		public MahjongDice(MahjongGame game, GenericReader reader)
		{
			m_Game = game;

			var version = reader.ReadInt();

			m_First = reader.ReadInt();
			m_Second = reader.ReadInt();
		}
	}
}