using System;

namespace Server.Quests
{
	[Flags, CustomEnum("Talk", "Whisper", "Yell", "Emote")]
	public enum QuestMessageType : byte
	{
		Talk = 0x1,
		Whisper = 0x2,
		Yell = 0x4,
		Emote = 0x8,

		Party = 0x20,
		Private = 0x40,
		System = 0x80,

		PartyTalk = Party | Talk,
		PartyWhisper = Party | Whisper,
		PartyYell = Party | Yell,
		PartyEmote = Party | Emote,

		PrivateTalk = Private | Talk,
		PrivateWhisper = Private | Whisper,
		PrivateYell = Private | Yell,
		PrivateEmote = Private | Emote,

		SystemTalk = System | Talk,
		SystemWhisper = System | Whisper,
		SystemYell = System | Yell,
		SystemEmote = System | Emote,
	}
}
