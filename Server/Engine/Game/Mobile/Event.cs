using Server.Accounting;
using Server.Commands;
using Server.ContextMenus;
using Server.Guilds;
using Server.Items;
using Server.Network;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

namespace Server
{
	public delegate void CharacterCreatedEventHandler(CharacterCreatedEventArgs e);
	public delegate void OpenDoorMacroEventHandler(OpenDoorMacroEventArgs e);
	public delegate void SpeechEventHandler(SpeechEventArgs e);
	public delegate void HeardEventHandler(HeardEventArgs e);
	public delegate void LoginEventHandler(LoginEventArgs e);
	public delegate void ServerListEventHandler(ServerListEventArgs e);
	public delegate void MovementEventHandler(MovementEventArgs e);
	public delegate void HungerChangedEventHandler(HungerChangedEventArgs e);
	public delegate void CrashedEventHandler(CrashedEventArgs e);
	public delegate void ShutdownEventHandler(ShutdownEventArgs e);
	public delegate void HelpRequestEventHandler(HelpRequestEventArgs e);
	public delegate void DisarmRequestEventHandler(DisarmRequestEventArgs e);
	public delegate void StunRequestEventHandler(StunRequestEventArgs e);
	public delegate void OpenSpellbookRequestEventHandler(OpenSpellbookRequestEventArgs e);
	public delegate void CastSpellRequestEventHandler(CastSpellRequestEventArgs e);
	public delegate void CastSpellSuccessEventHandler(CastSpellSuccessEventArgs e);
	public delegate void BandageTargetRequestEventHandler(BandageTargetRequestEventArgs e);
	public delegate void AnimateRequestEventHandler(AnimateRequestEventArgs e);
	public delegate void LogoutEventHandler(LogoutEventArgs e);
	public delegate void SocketConnectEventHandler(SocketConnectEventArgs e);
	public delegate void ConnectedEventHandler(ConnectedEventArgs e);
	public delegate void DisconnectedEventHandler(DisconnectedEventArgs e);
	public delegate void RenameRequestEventHandler(RenameRequestEventArgs e);
	public delegate void PlayerDeathEventHandler(PlayerDeathEventArgs e);
	public delegate void CreatureDeathEventHandler(CreatureDeathEventArgs e);
	public delegate void VirtueGumpRequestEventHandler(VirtueGumpRequestEventArgs e);
	public delegate void VirtueItemRequestEventHandler(VirtueItemRequestEventArgs e);
	public delegate void VirtueMacroRequestEventHandler(VirtueMacroRequestEventArgs e);
	public delegate void ChatRequestEventHandler(ChatRequestEventArgs e);
	public delegate void AccountLoginEventHandler(AccountLoginEventArgs e);
	public delegate void PaperdollRequestEventHandler(PaperdollRequestEventArgs e);
	public delegate void ProfileRequestEventHandler(ProfileRequestEventArgs e);
	public delegate void ChangeProfileRequestEventHandler(ChangeProfileRequestEventArgs e);
	public delegate void AggressiveActionEventHandler(AggressiveActionEventArgs e);
	public delegate void GameLoginEventHandler(GameLoginEventArgs e);
	public delegate void DeleteRequestEventHandler(DeleteRequestEventArgs e);
	public delegate void WorldLoadEventHandler();
	public delegate void WorldSaveEventHandler(WorldSaveEventArgs e);
	public delegate void SetAbilityEventHandler(SetAbilityEventArgs e);
	public delegate void FastWalkEventHandler(FastWalkEventArgs e);
	public delegate void ServerStartedEventHandler();
	public delegate void CreateGuildHandler(CreateGuildEventArgs e);
	public delegate void GuildGumpRequestHandler(GuildGumpRequestEventArgs e);
	public delegate void QuestGumpRequestHandler(QuestGumpRequestEventArgs e);
	public delegate void ClientVersionReceivedHandler(ClientVersionReceivedEventArgs e);
	public delegate void ParentChangedEventHandler(ParentChangedEventArgs e);
	public delegate void CraftedItemEventHandler(CraftedItemEventArgs e);
	public delegate void HarvestedItemEventHandler(HarvestedItemEventArgs e);
	public delegate void StolenItemEventHandler(StolenItemEventArgs e);
	public delegate void FeedCreatureEventHandler(FeedCreatureEventArgs e);
	public delegate void GiveGoldEventHandler(GiveGoldEventArgs e);
	public delegate void SellToVendorEventHandler(SellToVendorEventArgs e);
	public delegate void BuyFromVendorEventHandler(BuyFromVendorEventArgs e);
	public delegate void MobileDamagedEventHandler(MobileDamagedEventArgs e);
	public delegate void MobileHealedEventHandler(MobileHealedEventArgs e);
	public delegate void CreatureTamedEventHandler(CreatureTamedEventArgs e);
	public delegate void SkillChangedEventHandler(SkillChangedEventArgs e);
	public delegate void ContextMenuRequestEventHandler(ContextMenuRequestEventArgs e);
	public delegate void ContextMenuResponseEventHandler(ContextMenuResponseEventArgs e);

	public class ParentChangedEventArgs : EventArgs
	{
		public Item Item { get; }
		public IEntity OldParent { get; }
		public IEntity NewParent { get; }

		public ParentChangedEventArgs(Item item, IEntity oldParent, IEntity newParent)
		{
			Item = item;
			OldParent = oldParent;
			NewParent = newParent;
		}
	}

	public class CraftedItemEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Item { get; }
		public int Amount { get; }

		public ICraftSystem System { get; }
		public ICraftItem Craft { get; }
		public ICraftTool Tool { get; }

		public CraftedItemEventArgs(Mobile mobile, Item item, int amount, ICraftSystem system, ICraftItem craft, ICraftTool tool)
		{
			Mobile = mobile;
			Item = item;
			Amount = amount;
			System = system;
			Craft = craft;
			Tool = tool;
		}
	}

	public class HarvestedItemEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Item { get; }
		public int Amount { get; }

		public IHarvestSystem System { get; }
		public IHarvestTool Tool { get; }

		public HarvestedItemEventArgs(Mobile mobile, Item item, int amount, IHarvestSystem system, IHarvestTool tool)
		{
			Mobile = mobile;
			Item = item;
			Amount = amount;
			System = system;
			Tool = tool;
		}
	}

	public class StolenItemEventArgs : EventArgs
	{
		public Mobile Thief { get; }
		public Mobile Victim { get; }
		public Item Item { get; }
		public int Amount { get; }

		public StolenItemEventArgs(Mobile thief, Mobile victim, Item item, int amount)
		{
			Thief = thief;
			Victim = victim;
			Item = item;
			Amount = amount;
		}
	}

	public class FeedCreatureEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Creature { get; }
		public Item Food { get; }
		public int Amount { get; }

		public FeedCreatureEventArgs(Mobile mobile, Mobile creature, Item food, int amount)
		{
			Mobile = mobile;
			Creature = creature;
			Food = food;
			Amount = amount;
		}
	}

	public class GiveGoldEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Receiver { get; }
		public Item Gold { get; }
		public int Amount { get; }

		public GiveGoldEventArgs(Mobile mobile, Mobile receiver, Item gold, int amount)
		{
			Mobile = mobile;
			Receiver = receiver;
			Gold = gold;
			Amount = amount;
		}
	}

	public class SellToVendorEventArgs : EventArgs
	{
		public Mobile Seller { get; }
		public Mobile Vendor { get; }
		public IEntity Sold { get; }
		public int Amount { get; }

		public SellToVendorEventArgs(Mobile seller, Mobile vendor, IEntity sold, int amount)
		{
			Seller = seller;
			Vendor = vendor;
			Sold = sold;
			Amount = amount;
		}
	}

	public class BuyFromVendorEventArgs : EventArgs
	{
		public Mobile Buyer { get; }
		public Mobile Vendor { get; }
		public IEntity Sold { get; }
		public int Amount { get; }

		public BuyFromVendorEventArgs(Mobile buyer, Mobile vendor, IEntity sold, int amount)
		{
			Buyer = buyer;
			Vendor = vendor;
			Sold = sold;
			Amount = amount;
		}
	}

	public class MobileDamagedEventArgs : EventArgs
	{
		public Mobile Source { get; }
		public Mobile Target { get; }
		public int Amount { get; }

		public MobileDamagedEventArgs(Mobile source, Mobile target, int amount)
		{
			Source = source;
			Target = target;
			Amount = amount;
		}
	}

	public class MobileHealedEventArgs : EventArgs
	{
		public Mobile Source { get; }
		public Mobile Target { get; }
		public int Amount { get; }

		public MobileHealedEventArgs(Mobile source, Mobile target, int amount)
		{
			Source = source;
			Target = target;
			Amount = amount;
		}
	}

	public class CreatureTamedEventArgs : EventArgs
	{
		public Mobile Tamer { get; }
		public Mobile Creature { get; }

		public CreatureTamedEventArgs(Mobile tamer, Mobile creature)
		{
			Tamer = tamer;
			Creature = creature;
		}
	}

	public class SkillChangedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public SkillName Skill { get; }
		public double Offset { get; }

		public SkillChangedEventArgs(Mobile source, SkillName skill, double offset)
		{
			Mobile = source;
			Skill = skill;
			Offset = offset;
		}
	}

	public class ContextMenuRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public IEntity Owner { get; }

		public List<ContextMenuEntry> Entries { get; }

		public ContextMenuRequestEventArgs(Mobile source, IEntity owner, List<ContextMenuEntry> entries)
		{
			Mobile = source;
			Owner = owner;
			Entries = entries;
		}
	}

	public class ContextMenuResponseEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public IEntity Owner { get; }

		public ContextMenuEntry Entry { get; }

		public bool Blocked { get; set; }

		public ContextMenuResponseEventArgs(Mobile source, IEntity owner, ContextMenuEntry entry)
		{
			Mobile = source;
			Owner = owner;
			Entry = entry;
		}
	}

	public class ClientVersionReceivedEventArgs : EventArgs
	{
		public NetState State { get; }
		public ClientVersion Version { get; }

		public ClientVersionReceivedEventArgs(NetState state, ClientVersion cv)
		{
			State = state;
			Version = cv;
		}
	}

	public class CreateGuildEventArgs : EventArgs
	{
		public int Id { get; set; }
		public BaseGuild Guild { get; set; }

		public CreateGuildEventArgs(int id)
		{
			Id = id;
		}
	}

	public class GuildGumpRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public GuildGumpRequestEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class QuestGumpRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public QuestGumpRequestEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class SetAbilityEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int Index { get; }

		public SetAbilityEventArgs(Mobile mobile, int index)
		{
			Mobile = mobile;
			Index = index;
		}
	}

	public class DeleteRequestEventArgs : EventArgs
	{
		public NetState State { get; }
		public int Index { get; }

		public DeleteRequestEventArgs(NetState state, int index)
		{
			State = state;
			Index = index;
		}
	}

	public class GameLoginEventArgs : EventArgs
	{
		public NetState State { get; }
		public string Username { get; }
		public string Password { get; }
		public bool Accepted { get; set; }
		public CityInfo[] CityInfo { get; set; }

		public GameLoginEventArgs(NetState state, string un, string pw)
		{
			State = state;
			Username = un;
			Password = pw;
		}
	}

	public class AggressiveActionEventArgs : EventArgs
	{
		public Mobile Aggressed { get; private set; }
		public Mobile Aggressor { get; private set; }
		public bool Criminal { get; private set; }

		private static readonly Queue<AggressiveActionEventArgs> m_Pool = new();

		public static AggressiveActionEventArgs Create(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			AggressiveActionEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.Aggressed = aggressed;
				args.Aggressor = aggressor;
				args.Criminal = criminal;
			}
			else
			{
				args = new AggressiveActionEventArgs(aggressed, aggressor, criminal);
			}

			return args;
		}

		private AggressiveActionEventArgs(Mobile aggressed, Mobile aggressor, bool criminal)
		{
			Aggressed = aggressed;
			Aggressor = aggressor;
			Criminal = criminal;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ProfileRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public ProfileRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class ChangeProfileRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }
		public string Text { get; }

		public ChangeProfileRequestEventArgs(Mobile beholder, Mobile beheld, string text)
		{
			Beholder = beholder;
			Beheld = beheld;
			Text = text;
		}
	}

	public class PaperdollRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public PaperdollRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class AccountLoginEventArgs : EventArgs
	{
		public NetState State { get; }
		public string Username { get; }
		public string Password { get; }
		public bool Accepted { get; set; }
		public ALRReason RejectReason { get; set; }

		public AccountLoginEventArgs(NetState state, string username, string password)
		{
			State = state;
			Username = username;
			Password = password;
		}
	}

	public class VirtueItemRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }
		public int GumpID { get; }

		public VirtueItemRequestEventArgs(Mobile beholder, Mobile beheld, int gumpID)
		{
			Beholder = beholder;
			Beheld = beheld;
			GumpID = gumpID;
		}
	}

	public class VirtueGumpRequestEventArgs : EventArgs
	{
		public Mobile Beholder { get; }
		public Mobile Beheld { get; }

		public VirtueGumpRequestEventArgs(Mobile beholder, Mobile beheld)
		{
			Beholder = beholder;
			Beheld = beheld;
		}
	}

	public class VirtueMacroRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int VirtueID { get; }

		public VirtueMacroRequestEventArgs(Mobile mobile, int virtueID)
		{
			Mobile = mobile;
			VirtueID = virtueID;
		}
	}

	public class ChatRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public ChatRequestEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class PlayerDeathEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Killer { get; }
		public Container Corpse { get; }

		public PlayerDeathEventArgs(Mobile mobile, Mobile killer, Container corpse)
		{
			Mobile = mobile;
			Killer = killer;
			Corpse = corpse;
		}
	}

	public class CreatureDeathEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Killer { get; }
		public Container Corpse { get; }

		public CreatureDeathEventArgs(Mobile mobile, Mobile killer, Container corpse)
		{
			Mobile = mobile;
			Killer = killer;
			Corpse = corpse;
		}
	}

	public class RenameRequestEventArgs : EventArgs
	{
		public Mobile From { get; }
		public Mobile Target { get; }
		public string Name { get; }

		public RenameRequestEventArgs(Mobile from, Mobile target, string name)
		{
			From = from;
			Target = target;
			Name = name;
		}
	}

	public class LogoutEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public LogoutEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class SocketConnectEventArgs : EventArgs
	{
		public Socket Socket { get; }
		public bool AllowConnection { get; set; }

		public SocketConnectEventArgs(Socket s)
		{
			Socket = s;
			AllowConnection = true;
		}
	}

	public class ConnectedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public ConnectedEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class DisconnectedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public DisconnectedEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class AnimateRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public string Action { get; }

		public AnimateRequestEventArgs(Mobile m, string action)
		{
			Mobile = m;
			Action = action;
		}
	}

	public class CastSpellRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public ISpellbook Spellbook { get; }
		public SpellName SpellID { get; }

		public CastSpellRequestEventArgs(Mobile m, SpellName spellID, ISpellbook book)
		{
			Mobile = m;
			Spellbook = book;
			SpellID = spellID;
		}
	}

	public class CastSpellSuccessEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public ISpell Spell { get; }

		public CastSpellSuccessEventArgs(Mobile m, ISpell spell)
		{
			Mobile = m;
			Spell = spell;
		}
	}

	public class BandageTargetRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Item Bandage { get; }
		public Mobile Target { get; }

		public BandageTargetRequestEventArgs(Mobile m, Item bandage, Mobile target)
		{
			Mobile = m;
			Bandage = bandage;
			Target = target;
		}
	}

	public class OpenSpellbookRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int Type { get; }

		public OpenSpellbookRequestEventArgs(Mobile m, int type)
		{
			Mobile = m;
			Type = type;
		}
	}

	public class StunRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public StunRequestEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class DisarmRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public DisarmRequestEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class HelpRequestEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public HelpRequestEventArgs(Mobile m)
		{
			Mobile = m;
		}
	}

	public class ShutdownEventArgs : EventArgs
	{
		public ShutdownEventArgs()
		{
		}
	}

	public class CrashedEventArgs : EventArgs
	{
		public Exception Exception { get; }
		public bool Close { get; set; }

		public CrashedEventArgs(Exception e)
		{
			Exception = e;
		}
	}

	public class HungerChangedEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public int OldValue { get; }

		public HungerChangedEventArgs(Mobile mobile, int oldValue)
		{
			Mobile = mobile;
			OldValue = oldValue;
		}
	}

	public class MovementEventArgs : EventArgs
	{
		public Mobile Mobile { get; private set; }
		public Direction Direction { get; private set; }
		public bool Blocked { get; set; }

		private static readonly Queue<MovementEventArgs> m_Pool = new();

		public static MovementEventArgs Create(Mobile mobile, Direction dir)
		{
			MovementEventArgs args;

			if (m_Pool.Count > 0)
			{
				args = m_Pool.Dequeue();

				args.Mobile = mobile;
				args.Direction = dir;
				args.Blocked = false;
			}
			else
			{
				args = new MovementEventArgs(mobile, dir);
			}

			return args;
		}

		public MovementEventArgs(Mobile mobile, Direction dir)
		{
			Mobile = mobile;
			Direction = dir;
		}

		public void Free()
		{
			m_Pool.Enqueue(this);
		}
	}

	public class ServerListEventArgs : EventArgs
	{
		public NetState State { get; }
		public IAccount Account { get; }
		public bool Rejected { get; set; }
		public List<ServerInfo> Servers { get; }

		public void AddServer(string name, IPEndPoint address)
		{
			AddServer(name, 0, TimeZoneInfo.Local, address);
		}

		public void AddServer(string name, int fullPercent, TimeZoneInfo tz, IPEndPoint address)
		{
			Servers.Add(new ServerInfo(name, fullPercent, tz, address));
		}

		public ServerListEventArgs(NetState state, IAccount account)
		{
			State = state;
			Account = account;
			Servers = new List<ServerInfo>();
		}
	}

	public class CharacterCreatedEventArgs : EventArgs
	{
		public NetState State { get; }
		public IAccount Account { get; }
		public Mobile Mobile { get; set; }
		public string Name { get; }
		public bool Female { get; }
		public int Hue { get; }
		public int Str { get; }
		public int Dex { get; }
		public int Int { get; }
		public CityInfo City { get; }
		public SkillNameValue[] Skills { get; }
		public int ShirtHue { get; }
		public int PantsHue { get; }
		public int HairID { get; }
		public int HairHue { get; }
		public int BeardID { get; }
		public int BeardHue { get; }
		public int Profession { get; set; }
		public Race Race { get; }

		public CharacterCreatedEventArgs(NetState state, IAccount a, string name, bool female, int hue, int str, int dex, int intel, CityInfo city, SkillNameValue[] skills, int shirtHue, int pantsHue, int hairID, int hairHue, int beardID, int beardHue, int profession, Race race)
		{
			State = state;
			Account = a;
			Name = name;
			Female = female;
			Hue = hue;
			Str = str;
			Dex = dex;
			Int = intel;
			City = city;
			Skills = skills;
			ShirtHue = shirtHue;
			PantsHue = pantsHue;
			HairID = hairID;
			HairHue = hairHue;
			BeardID = beardID;
			BeardHue = beardHue;
			Profession = profession;
			Race = race;
		}
	}

	public class OpenDoorMacroEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public OpenDoorMacroEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class SpeechEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public string Speech { get; set; }
		public MessageType Type { get; }
		public int Hue { get; }
		public int[] Keywords { get; }
		public bool Handled { get; set; }
		public bool Blocked { get; set; }

		public bool HasKeyword(int keyword)
		{
			for (var i = 0; i < Keywords.Length; ++i)
			{
				if (Keywords[i] == keyword)
				{
					return true;
				}
			}

			return false;
		}

		public SpeechEventArgs(Mobile mobile, string speech, MessageType type, int hue, int[] keywords)
		{
			Mobile = mobile;
			Speech = speech;
			Type = type;
			Hue = hue;
			Keywords = keywords;
		}
	}

	public class HeardEventArgs : EventArgs
	{
		public Mobile Mobile { get; }
		public Mobile Speaker { get; }
		public string Speech { get; set; }
		public MessageType Type { get; }
		public int Hue { get; }
		public int[] Keywords { get; }
		public bool Handled { get; set; }
		public bool Blocked { get; set; }

		public bool HasKeyword(int keyword)
		{
			for (var i = 0; i < Keywords.Length; ++i)
			{
				if (Keywords[i] == keyword)
				{
					return true;
				}
			}

			return false;
		}

		public HeardEventArgs(Mobile mobile, Mobile speaker, string speech, MessageType type, int hue, int[] keywords)
		{
			Mobile = mobile;
			Speaker = speaker;
			Speech = speech;
			Type = type;
			Hue = hue;
			Keywords = keywords;
		}
	}

	public class LoginEventArgs : EventArgs
	{
		public Mobile Mobile { get; }

		public LoginEventArgs(Mobile mobile)
		{
			Mobile = mobile;
		}
	}

	public class WorldSaveEventArgs : EventArgs
	{
		public bool Message { get; }

		public WorldSaveEventArgs(bool msg)
		{
			Message = msg;
		}
	}

	public class FastWalkEventArgs : EventArgs
	{
		public FastWalkEventArgs(NetState state)
		{
			NetState = state;
			Blocked = false;
		}

		public NetState NetState { get; }
		public bool Blocked { get; set; }
	}

	public static class EventSink
	{
		public static event CharacterCreatedEventHandler CharacterCreated;
		public static event OpenDoorMacroEventHandler OpenDoorMacroUsed;
		public static event SpeechEventHandler Speech;
		public static event HeardEventHandler Heard;
		public static event LoginEventHandler Login;
		public static event ServerListEventHandler ServerList;
		public static event MovementEventHandler Movement;
		public static event HungerChangedEventHandler HungerChanged;
		public static event CrashedEventHandler Crashed;
		public static event ShutdownEventHandler Shutdown;
		public static event HelpRequestEventHandler HelpRequest;
		public static event DisarmRequestEventHandler DisarmRequest;
		public static event StunRequestEventHandler StunRequest;
		public static event OpenSpellbookRequestEventHandler OpenSpellbookRequest;
		public static event CastSpellRequestEventHandler CastSpellRequest;
		public static event CastSpellSuccessEventHandler CastSpellSuccess;
		public static event BandageTargetRequestEventHandler BandageTargetRequest;
		public static event AnimateRequestEventHandler AnimateRequest;
		public static event LogoutEventHandler Logout;
		public static event SocketConnectEventHandler SocketConnect;
		public static event ConnectedEventHandler Connected;
		public static event DisconnectedEventHandler Disconnected;
		public static event RenameRequestEventHandler RenameRequest;
		public static event PlayerDeathEventHandler PlayerDeath;
		public static event CreatureDeathEventHandler CreatureDeath;
		public static event VirtueGumpRequestEventHandler VirtueGumpRequest;
		public static event VirtueItemRequestEventHandler VirtueItemRequest;
		public static event VirtueMacroRequestEventHandler VirtueMacroRequest;
		public static event ChatRequestEventHandler ChatRequest;
		public static event AccountLoginEventHandler AccountLogin;
		public static event PaperdollRequestEventHandler PaperdollRequest;
		public static event ProfileRequestEventHandler ProfileRequest;
		public static event ChangeProfileRequestEventHandler ChangeProfileRequest;
		public static event AggressiveActionEventHandler AggressiveAction;
		public static event CommandEventHandler Command;
		public static event GameLoginEventHandler GameLogin;
		public static event DeleteRequestEventHandler DeleteRequest;
		public static event WorldLoadEventHandler WorldPreLoad;
		public static event WorldLoadEventHandler WorldLoad;
		public static event WorldLoadEventHandler WorldPostLoad;
		public static event WorldSaveEventHandler WorldPreSave;
		public static event WorldSaveEventHandler WorldSave;
		public static event WorldSaveEventHandler WorldPostSave;
		public static event SetAbilityEventHandler SetAbility;
		public static event FastWalkEventHandler FastWalk;
		public static event CreateGuildHandler CreateGuild;
		public static event ServerStartedEventHandler ServerStarted;
		public static event GuildGumpRequestHandler GuildGumpRequest;
		public static event QuestGumpRequestHandler QuestGumpRequest;
		public static event ClientVersionReceivedHandler ClientVersionReceived;
		public static event ParentChangedEventHandler ParentChanged;
		public static event CraftedItemEventHandler CraftedItem;
		public static event HarvestedItemEventHandler HarvestedItem;
		public static event StolenItemEventHandler StolenItem;
		public static event FeedCreatureEventHandler FeedCreature;
		public static event GiveGoldEventHandler GiveGold;
		public static event SellToVendorEventHandler SellToVendor;
		public static event BuyFromVendorEventHandler BuyFromVendor;
		public static event MobileDamagedEventHandler MobileDamaged;
		public static event MobileHealedEventHandler MobileHealed;
		public static event CreatureTamedEventHandler CreatureTamed;
		public static event SkillChangedEventHandler SkillChanged;
		public static event ContextMenuRequestEventHandler ContextMenuRequest;
		public static event ContextMenuResponseEventHandler ContextMenuResponse;

		public static void InvokeParentChanged(ParentChangedEventArgs e)
		{
			ParentChanged?.Invoke(e);
		}

		public static void InvokeCraftedItem(CraftedItemEventArgs e)
		{
			CraftedItem?.Invoke(e);
		}

		public static void InvokeHarvestedItem(HarvestedItemEventArgs e)
		{
			HarvestedItem?.Invoke(e);
		}

		public static void InvokeStolenItem(StolenItemEventArgs e)
		{
			StolenItem?.Invoke(e);
		}

		public static void InvokeFeedCreature(FeedCreatureEventArgs e)
		{
			FeedCreature?.Invoke(e);
		}

		public static void InvokeGiveGold(GiveGoldEventArgs e)
		{
			GiveGold?.Invoke(e);
		}

		public static void InvokeSellToVendor(SellToVendorEventArgs e)
		{
			SellToVendor?.Invoke(e);
		}

		public static void InvokeBuyFromVendor(BuyFromVendorEventArgs e)
		{
			BuyFromVendor?.Invoke(e);
		}

		public static void InvokeMobileDamaged(MobileDamagedEventArgs e)
		{
			MobileDamaged?.Invoke(e);
		}

		public static void InvokeMobileHealed(MobileHealedEventArgs e)
		{
			MobileHealed?.Invoke(e);
		}

		public static void InvokeCreatureTamed(CreatureTamedEventArgs e)
		{
			CreatureTamed?.Invoke(e);
		}

		public static void InvokeSkillChanged(SkillChangedEventArgs e)
		{
			SkillChanged?.Invoke(e);
		}

		public static void InvokeContextMenuRequest(ContextMenuRequestEventArgs e)
		{
			ContextMenuRequest?.Invoke(e);
		}

		public static void InvokeContextMenuResponse(ContextMenuResponseEventArgs e)
		{
			ContextMenuResponse?.Invoke(e);
		}

		public static void InvokeClientVersionReceived(ClientVersionReceivedEventArgs e)
		{
			ClientVersionReceived?.Invoke(e);
		}

		public static void InvokeServerStarted()
		{
			ServerStarted?.Invoke();
		}

		public static void InvokeCreateGuild(CreateGuildEventArgs e)
		{
			CreateGuild?.Invoke(e);
		}

		public static void InvokeSetAbility(SetAbilityEventArgs e)
		{
			SetAbility?.Invoke(e);
		}

		public static void InvokeGuildGumpRequest(GuildGumpRequestEventArgs e)
		{
			GuildGumpRequest?.Invoke(e);
		}

		public static void InvokeQuestGumpRequest(QuestGumpRequestEventArgs e)
		{
			QuestGumpRequest?.Invoke(e);
		}

		public static void InvokeFastWalk(FastWalkEventArgs e)
		{
			FastWalk?.Invoke(e);
		}

		public static void InvokeDeleteRequest(DeleteRequestEventArgs e)
		{
			DeleteRequest?.Invoke(e);
		}

		public static void InvokeGameLogin(GameLoginEventArgs e)
		{
			GameLogin?.Invoke(e);
		}

		public static void InvokeCommand(CommandEventArgs e)
		{
			Command?.Invoke(e);
		}

		public static void InvokeAggressiveAction(AggressiveActionEventArgs e)
		{
			AggressiveAction?.Invoke(e);
		}

		public static void InvokeProfileRequest(ProfileRequestEventArgs e)
		{
			ProfileRequest?.Invoke(e);
		}

		public static void InvokeChangeProfileRequest(ChangeProfileRequestEventArgs e)
		{
			ChangeProfileRequest?.Invoke(e);
		}

		public static void InvokePaperdollRequest(PaperdollRequestEventArgs e)
		{
			PaperdollRequest?.Invoke(e);
		}

		public static void InvokeAccountLogin(AccountLoginEventArgs e)
		{
			AccountLogin?.Invoke(e);
		}

		public static void InvokeChatRequest(ChatRequestEventArgs e)
		{
			ChatRequest?.Invoke(e);
		}

		public static void InvokeVirtueItemRequest(VirtueItemRequestEventArgs e)
		{
			VirtueItemRequest?.Invoke(e);
		}

		public static void InvokeVirtueGumpRequest(VirtueGumpRequestEventArgs e)
		{
			VirtueGumpRequest?.Invoke(e);
		}

		public static void InvokeVirtueMacroRequest(VirtueMacroRequestEventArgs e)
		{
			VirtueMacroRequest?.Invoke(e);
		}

		public static void InvokePlayerDeath(PlayerDeathEventArgs e)
		{
			PlayerDeath?.Invoke(e);
		}

		public static void InvokeCreatureDeath(CreatureDeathEventArgs e)
		{
			CreatureDeath?.Invoke(e);
		}

		public static void InvokeRenameRequest(RenameRequestEventArgs e)
		{
			RenameRequest?.Invoke(e);
		}

		public static void InvokeLogout(LogoutEventArgs e)
		{
			Logout?.Invoke(e);
		}

		public static void InvokeSocketConnect(SocketConnectEventArgs e)
		{
			SocketConnect?.Invoke(e);
		}

		public static void InvokeConnected(ConnectedEventArgs e)
		{
			Connected?.Invoke(e);
		}

		public static void InvokeDisconnected(DisconnectedEventArgs e)
		{
			Disconnected?.Invoke(e);
		}

		public static void InvokeAnimateRequest(AnimateRequestEventArgs e)
		{
			AnimateRequest?.Invoke(e);
		}

		public static void InvokeCastSpellRequest(CastSpellRequestEventArgs e)
		{
			CastSpellRequest?.Invoke(e);
		}

		public static void InvokeCastSpellSuccess(CastSpellSuccessEventArgs e)
		{
			CastSpellSuccess?.Invoke(e);
		}

		public static void InvokeBandageTargetRequest(BandageTargetRequestEventArgs e)
		{
			BandageTargetRequest?.Invoke(e);
		}

		public static void InvokeOpenSpellbookRequest(OpenSpellbookRequestEventArgs e)
		{
			OpenSpellbookRequest?.Invoke(e);
		}

		public static void InvokeDisarmRequest(DisarmRequestEventArgs e)
		{
			DisarmRequest?.Invoke(e);
		}

		public static void InvokeStunRequest(StunRequestEventArgs e)
		{
			StunRequest?.Invoke(e);
		}

		public static void InvokeHelpRequest(HelpRequestEventArgs e)
		{
			HelpRequest?.Invoke(e);
		}

		public static void InvokeShutdown(ShutdownEventArgs e)
		{
			Shutdown?.Invoke(e);
		}

		public static void InvokeCrashed(CrashedEventArgs e)
		{
			Crashed?.Invoke(e);
		}

		public static void InvokeHungerChanged(HungerChangedEventArgs e)
		{
			HungerChanged?.Invoke(e);
		}

		public static void InvokeMovement(MovementEventArgs e)
		{
			Movement?.Invoke(e);
		}

		public static void InvokeServerList(ServerListEventArgs e)
		{
			ServerList?.Invoke(e);
		}

		public static void InvokeLogin(LoginEventArgs e)
		{
			Login?.Invoke(e);
		}

		public static void InvokeSpeech(SpeechEventArgs e)
		{
			Speech?.Invoke(e);
		}

		public static void InvokeHeard(HeardEventArgs e)
		{
			Heard?.Invoke(e);
		}

		public static void InvokeCharacterCreated(CharacterCreatedEventArgs e)
		{
			CharacterCreated?.Invoke(e);
		}

		public static void InvokeOpenDoorMacroUsed(OpenDoorMacroEventArgs e)
		{
			OpenDoorMacroUsed?.Invoke(e);
        }

        public static void InvokeWorldPreLoad()
        {
            WorldPreLoad?.Invoke();
        }

        public static void InvokeWorldLoad()
		{
			WorldLoad?.Invoke();
		}

		public static void InvokeWorldPostLoad()
		{
			WorldPostLoad?.Invoke();
		}

		public static void InvokeWorldPreSave(WorldSaveEventArgs e)
		{
			WorldPreSave?.Invoke(e);
		}

		public static void InvokeWorldSave(WorldSaveEventArgs e)
		{
			WorldSave?.Invoke(e);
		}

		public static void InvokeWorldPostSave(WorldSaveEventArgs e)
		{
			WorldPostSave?.Invoke(e);
		}

		public static void Reset()
		{
			var owner = typeof(EventSink);

			foreach (var e in owner.GetEvents(BindingFlags.Public | BindingFlags.Static))
			{
				Utility.ClearEventDelegates(owner, e);
			}
		}
	}
}