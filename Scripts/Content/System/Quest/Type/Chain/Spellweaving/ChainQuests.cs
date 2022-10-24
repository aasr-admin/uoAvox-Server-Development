﻿using Server.Engines.ChainQuests.Items;
using Server.Engines.ChainQuests.Mobiles;
using Server.Items;
using Server.Mobiles;

using System;

namespace Server.Engines.ChainQuests.Definitions
{
	public static class Spellweaving
	{
		public static void AwardTo(PlayerMobile pm)
		{
			if (pm == null)
			{
				return;
			}

			var context = ChainQuestSystem.GetOrCreateContext(pm);

			if (!context.Spellweaving)
			{
				context.Spellweaving = true;

				Effects.SendLocationParticles(EffectItem.Create(pm.Location, pm.Map, EffectItem.DefaultDuration), 0, 0, 0, 0, 0, 5060, 0);
				Effects.PlaySound(pm.Location, pm.Map, 0x243);

				Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 6, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
				Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 4, pm.Y - 6, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);
				Effects.SendMovingParticles(new Entity(Server.Serial.Zero, new Point3D(pm.X - 6, pm.Y - 4, pm.Z + 15), pm.Map), pm, 0x36D4, 7, 0, false, true, 0x497, 0, 9502, 1, 0, (EffectLayer)255, 0x100);

				Effects.SendTargetParticles(pm, 0x375A, 35, 90, 0x00, 0x00, 9502, (EffectLayer)255, 0x100);
			}
		}
	}


	/// Patience
	public class Patience : ChainQuest
	{
		public override Type NextQuest => typeof(NeedsOfTheManyHeartwood1);

		public Patience()
		{
			Activated = true;
			Title = 1072753; // Patience
			Description = 1072762; // Learning to weave spells and control the forces of nature requires sacrifice, discipline, focus, and an unwavering dedication to Sosaria herself.  We do not teach the unworthy.  They do not comprehend the lessons nor the dedication required.  If you would walk the path of the Arcanist, then you must do as I require without hesitation or question.  Your first task is to gather miniature mushrooms ... 20 of them from the branches of our mighty home.  I give you one hour to complete the task.
			RefusalMessage = 1072767; // *nods* Not everyone has the temperment to undertake the way of the Arcanist.
			InProgressMessage = 1072774; // The mushrooms I seek can be found growing here in The Heartwood. Seek them out and gather them.  You are running out of time.
			CompletionMessage = 1074166; // Have you gathered the mushrooms?

			Objectives.Add(new TimedCollectObjective(TimeSpan.FromHours(1), 20, typeof(MiniatureMushroom), "miniature mushrooms"));

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 3, "Aeluva"), new Point3D(7064, 349, 0), Map.Felucca);
			PutSpawner(new Spawner(1, 5, 10, 0, 3, "Aeluva"), new Point3D(7064, 349, 0), Map.Trammel);

			// Split up to prevent stacking on the spawner
			PutSpawner(new Spawner(20, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 30, "MiniatureMushroom"), new Point3D(7015, 366, 0), Map.Felucca);
			PutSpawner(new Spawner(20, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 30, "MiniatureMushroom"), new Point3D(7015, 366, 0), Map.Trammel);

			PutSpawner(new Spawner(5, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7081, 373, 0), Map.Felucca);
			PutSpawner(new Spawner(5, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7081, 373, 0), Map.Trammel);

			PutSpawner(new Spawner(15, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7052, 414, 0), Map.Felucca);
			PutSpawner(new Spawner(15, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30), 0, 20, "MiniatureMushroom"), new Point3D(7052, 414, 0), Map.Trammel);
		}
	}

	public class NeedsOfTheManyHeartwood1 : ChainQuest
	{
		public override Type NextQuest => typeof(NeedsOfTheManyHeartwood2);
		public override bool IsChainTriggered => true;

		public NeedsOfTheManyHeartwood1()
		{
			Activated = true;
			Title = 1072797; // Needs of the Many - The Heartwood
			Description = 1072763; // The way of the Arcanist involves cooperation with others and a strong committment to the community of your people.  We have run low on the cotton we use to pack wounds and our people have need.  Bring 10 bales of cotton to me.
			RefusalMessage = 1072768; // You endanger your progress along the path with your unwillingness.
			InProgressMessage = 1072775; // I care not where you acquire the cotton, merely that you provide it.
			CompletionMessage = 1074110; // Well, where are the cotton bales?

			Objectives.Add(new CollectObjective(10, typeof(Cotton), 1023577)); // bale of cotton

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class NeedsOfTheManyHeartwood2 : ChainQuest
	{
		public override Type NextQuest => typeof(MakingAContributionHeartwood);
		public override bool IsChainTriggered => true;

		public NeedsOfTheManyHeartwood2()
		{
			Activated = true;
			Title = 1072797; // Needs of the Many - The Heartwood
			Description = 1072764; // We must look to the defense of our people!  Bring boards for new arrows.
			RefusalMessage = 1072769; // The people have need of these items.  You are proving yourself inadequate to the demands of a member of this community.
			InProgressMessage = 1072776; // The requirements are simple -- 250 boards.
			CompletionMessage = 1074152; // Well, where are the boards?

			Objectives.Add(new CollectObjective(250, typeof(Board), 1027127)); // board

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class MakingAContributionHeartwood : ChainQuest
	{
		public override Type NextQuest => typeof(UnnaturalCreations);
		public override bool IsChainTriggered => true;

		public MakingAContributionHeartwood()
		{
			Activated = true;
			Title = 1072798; // Making a Contribution - The Heartwood
			Description = 1072765; // With health and defense assured, we need look to the need of the community for food and drink.  We will feast on fish steaks, sweets, and wine.  You will supply the ingredients, the cooks will prepare the meal.  As a Arcanist relies upon others to build focus and lend their power to her workings, the community needs the effort of all to survive.
			RefusalMessage = 1072770; // Do not falter now.  You have begun to show promise.
			InProgressMessage = 1072777; // Where are the items you've been tasked to supply for the feast?
			CompletionMessage = 1074158; // Ah good, you're back.  We're eager for the feast.

			Objectives.Add(new CollectObjective(1, typeof(SackFlour), 1024153)); // sack of flour
			Objectives.Add(new CollectObjective(10, typeof(JarHoney), 1022540)); // jar of honey
			Objectives.Add(new CollectObjective(20, typeof(SaltwaterFishSteak), 1022427)); // fish steak

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class UnnaturalCreations : ChainQuest
	{
		public override bool IsChainTriggered => true;

		public UnnaturalCreations()
		{
			Activated = true;
			Title = 1072758; // Unnatural Creations
			Description = 1072780; // You have proven your desire to contribute to the community and serve the people.  Now you must demonstrate your willingness to defend Sosaria from the greatest blight that plagues her.  Unnatural creatures, brought to a sort of perverted life, despoil our fair world.  Destroy them -- 5 Exodus Overseers and 2 Exodus Minions.
			RefusalMessage = 1072771; // You must serve Sosaria with all your heart and strength.  Your unwillingness does not reflect favorably upon you.
			InProgressMessage = 1072779; // Every moment you procrastinate, these unnatural creatures damage Sosaria.
			CompletionMessage = 1074167; // Well done!  Well done, indeed.  You are worthy to become an arcanist!

			Objectives.Add(new KillObjective(5, new Type[] { typeof(ExodusOverseer) }, "Exodus Overseers"));
			Objectives.Add(new KillObjective(2, new Type[] { typeof(ExodusMinion) }, "Exodus Minions"));

			Rewards.Add(new ItemReward(1031601, typeof(ArcaneCircleScroll))); // Arcane Circle
			Rewards.Add(new ItemReward(1031600, typeof(BookOfSpellweaving))); // Spellweaving Spellbook
			Rewards.Add(new ItemReward(1031602, typeof(GiftOfRenewalScroll))); // Gift of Renewal
		}

		public override void GetRewards(ChainQuestInstance instance)
		{
			Spellweaving.AwardTo(instance.Player);
			base.GetRewards(instance);
		}
	}


	/// Discipline
	public class Discipline : ChainQuest
	{
		public override Type NextQuest => typeof(NeedsOfTheManySanctuary);

		public Discipline()
		{
			Activated = true;
			Title = 1072752; // Discipline
			Description = 1072761; // Learning to weave spells and control the forces of nature requires sacrifice, discipline, focus, and an unwavering dedication to Sosaria herself.  We do not teach the unworthy.  They do not comprehend the lessons nor the dedication required.  If you would walk the path of the Arcanist, then you must do as I require without hesitation or question.  Your first task is to rid our home of rats ... 50 of them in the next hour.
			RefusalMessage = 1072767; // *nods* Not everyone has the temperment to undertake the way of the Arcanist.
			InProgressMessage = 1072773; // You waste my time.  The task is simple. Kill 50 rats in an hour.
										 // No completion message

			Objectives.Add(new TimedKillObjective(TimeSpan.FromHours(1), 50, new Type[] { typeof(Rat) }, "rats", new QuestArea(1074807, "Sanctuary"))); // Sanctuary

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Koole"), new Point3D(6257, 110, -10), Map.Felucca);
			PutSpawner(new Spawner(1, 5, 10, 0, 0, "Koole"), new Point3D(6257, 110, -10), Map.Trammel);
		}
	}

	public class NeedsOfTheManySanctuary : ChainQuest
	{
		public override Type NextQuest => typeof(MakingAContributionSanctuary);
		public override bool IsChainTriggered => true;

		public NeedsOfTheManySanctuary()
		{
			Activated = true;
			Title = 1072754; // Needs of the Many - Sanctuary
			Description = 1072763; // The way of the Arcanist involves cooperation with others and a strong committment to the community of your people.  We have run low on the cotton we use to pack wounds and our people have need.  Bring 10 bales of cotton to me.
			RefusalMessage = 1072768; // You endanger your progress along the path with your unwillingness.
			InProgressMessage = 1072775; // I care not where you acquire the cotton, merely that you provide it.
			CompletionMessage = 1074110; // Well, where are the cotton bales?

			Objectives.Add(new CollectObjective(10, typeof(Cotton), 1023577)); // bale of cotton

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class MakingAContributionSanctuary : ChainQuest
	{
		public override Type NextQuest => typeof(SuppliesForSanctuary);
		public override bool IsChainTriggered => true;

		public MakingAContributionSanctuary()
		{
			Activated = true;
			Title = 1072755; // Making a Contribution - Sanctuary
			Description = 1072764; // We must look to the defense of our people!  Bring boards for new arrows.
			RefusalMessage = 1072769; // The people have need of these items.  You are proving yourself inadequate to the demands of a member of this community.
			InProgressMessage = 1072776; // The requirements are simple -- 250 boards.
			CompletionMessage = 1074152; // Well, where are the boards?

			Objectives.Add(new CollectObjective(250, typeof(Board), 1027127)); // board

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class SuppliesForSanctuary : ChainQuest
	{
		public override Type NextQuest => typeof(TheHumanBlight);
		public override bool IsChainTriggered => true;

		public SuppliesForSanctuary()
		{
			Activated = true;
			Title = 1072756; // Supplies for Sanctuary
			Description = 1072765; // With health and defense assured, we need look to the need of the community for food and drink.  We will feast on fish steaks, sweets, and wine.  You will supply the ingredients, the cooks will prepare the meal.  As a Arcanist relies upon others to build focus and lend their power to her workings, the community needs the effort of all to survive.
			RefusalMessage = 1072770; // Do not falter now.  You have begun to show promise.
			InProgressMessage = 1072777; // Where are the items you've been tasked to supply for the feast?
			CompletionMessage = 1074158; // Ah good, you're back.  We're eager for the feast.

			Objectives.Add(new CollectObjective(1, typeof(SackFlour), 1024153)); // sack of flour
			Objectives.Add(new CollectObjective(10, typeof(JarHoney), 1022540)); // jar of honey
			Objectives.Add(new CollectObjective(20, typeof(SaltwaterFishSteak), 1022427)); // fish steak

			Rewards.Add(new DummyReward(1074872)); // The opportunity to learn the ways of the Arcanist.
		}
	}

	public class TheHumanBlight : ChainQuest
	{
		public override bool IsChainTriggered => true;

		public TheHumanBlight()
		{
			Activated = true;
			Title = 1072757; // The Human Blight
			Description = 1072766; // You have proven your desire to contribute to the community and serve the people.  Now you must demonstrate your willingness to defend Sosaria from the greatest blight that plagues her.  The human vermin that have spread as a disease, despoiling the land are the greatest blight we face.  Kill humans and return to me the proof of your actions. Bring me 30 human ears.
			RefusalMessage = 1072771; // You must serve Sosaria with all your heart and strength.  Your unwillingness does not reflect favorably upon you.
			InProgressMessage = 1072778; // Why do you delay?  The human blight must be averted.
			CompletionMessage = 1074160; // I will take the ears you have collected now.  Hand them here.

			Objectives.Add(new CollectObjective(30, typeof(SeveredHumanEars), 1032591)); // severed human ears

			Rewards.Add(new ItemReward(1031601, typeof(ArcaneCircleScroll))); // Arcane Circle
			Rewards.Add(new ItemReward(1031600, typeof(BookOfSpellweaving))); // Spellweaving Spellbook
			Rewards.Add(new ItemReward(1031602, typeof(GiftOfRenewalScroll))); // Gift of Renewal
		}

		public override void GetRewards(ChainQuestInstance instance)
		{
			Spellweaving.AwardTo(instance.Player);
			base.GetRewards(instance);
		}
	}


	/// Friend Of The Fey
	public class FriendOfTheFey : ChainQuest
	{
		public override Type NextQuest => typeof(TokenOfFriendship);

		public FriendOfTheFey()
		{
			Activated = true;
			Title = 1074284; // Friend of the Fey
			Description = 1074286; // The children of Sosaria understand the dedication and committment of an arcanist -- and will, from time to time offer their friendship.  If you would forge such a bond, first seek out a goodwill offering to present.  Pixies enjoy sweets and pretty things.
			RefusalMessage = 1074288; // There's always time to make new friends.
			InProgressMessage = 1074290; // I think honey and some sparkly beads would please a pixie.
			CompletionMessage = 1074292; // What have we here? Oh yes, gifts for a pixie.

			Objectives.Add(new CollectObjective(1, typeof(Beads), 1024235)); // beads
			Objectives.Add(new CollectObjective(1, typeof(JarHoney), 1022540)); // jar of honey

			Rewards.Add(new DummyReward(1074874)); // The opportunity to prove yourself worthy of learning to Summon Fey. (Sufficient spellweaving skill is required to cast the spell)
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 3, "Synaeva"), new Point3D(7064, 350, 0), Map.Felucca);
			PutSpawner(new Spawner(1, 5, 10, 0, 3, "Synaeva"), new Point3D(7064, 350, 0), Map.Trammel);
		}
	}

	public class TokenOfFriendship : ChainQuest
	{
		public override Type NextQuest => typeof(Alliance);
		public override bool IsChainTriggered => true;

		public TokenOfFriendship()
		{
			Activated = true;
			Title = 1074293; // Token of Friendship
			Description = 1074297; // I've wrapped your gift suitably to present to a pixie of discriminating taste.  Seek out Arielle and give her your offering.
			RefusalMessage = 1074310; // I'll hold onto this gift in case you change your mind.
			InProgressMessage = 1074315; // Arielle wanders quite a bit, so I'm not sure exactly where to find her.  I'm sure she's going to love your gift.
			CompletionMessage = 1074319; // *giggle*  Oooh!  For me?

			Objectives.Add(new DeliverObjective(typeof(GiftForArielle), 1, "gift for Arielle", typeof(Arielle)));

			Rewards.Add(new DummyReward(1074874)); // The opportunity to prove yourself worthy of learning to Summon Fey. (Sufficient spellweaving skill is required to cast the spell)
		}
	}

	public class Alliance : ChainQuest
	{
		public override bool IsChainTriggered => true;

		public Alliance()
		{
			Activated = true;
			Title = 1074294; // Alliance
			Description = 1074298; // *giggle* Mean reapers make pixies unhappy.  *light-hearted giggle*  You could fix them!
			RefusalMessage = 1074311; // *giggle* Okies!
			InProgressMessage = 1074316; // Mean reapers are all around trees!  *giggle*  You fix them up, please.
			CompletionNotice = CompletionNoticeShortReturn;

			Objectives.Add(new KillObjective(20, new Type[] { typeof(Reaper) }, "reapers"));

			Rewards.Add(new ItemReward(1031607, typeof(SummonFeyScroll))); // Summon Fey
		}

		public override void GetRewards(ChainQuestInstance instance)
		{
			instance.PlayerContext.SummonFey = true;
			instance.Player.SendLocalizedMessage(1074320, "", 0x2A); // *giggle* Mean reapers got fixed!  Pixie friend now! *giggle* When mean thingies bother you, a brave pixie will help.

			base.GetRewards(instance);
		}
	}


	/// Fiendish Friends
	public class FiendishFriends : ChainQuest
	{
		public override Type NextQuest => typeof(CrackingTheWhipI);

		public FiendishFriends()
		{
			Activated = true;
			Title = 1074283; // Fiendish Friends
			Description = 1074285; // It is true that a skilled arcanist can summon and dominate an imp to serve at their pleasure.  To do such at thing though, you must master the miserable little fiends utterly by demonstrating your superiority.  Rough them up some -- kill a few.  That will do the trick.
			RefusalMessage = 1074287; // You're probably right.  They're not worth the effort.
			InProgressMessage = 1074289; // Surely you're not having difficulties swatting down those annoying pests?
										 // TODO: Verify
			CompletionMessage = 1074291; // Hah!  You showed them!

			Objectives.Add(new KillObjective(50, new Type[] { typeof(Imp) }, "imps"));

			Rewards.Add(new DummyReward(1074873)); // The opportunity to prove yourself worthy of learning to Summon Fiends. (Sufficient spellweaving skill is required to cast the spell)
		}

		public override void Generate()
		{
			base.Generate();

			PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderBrae"), new Point3D(6266, 124, 0), Map.Felucca);
			PutSpawner(new Spawner(1, 5, 10, 0, 0, "ElderBrae"), new Point3D(6266, 124, 0), Map.Trammel);
		}
	}

	// TODO: Verify
	public class CrackingTheWhipI : ChainQuest
	{
		public override Type NextQuest => typeof(CrackingTheWhipII);
		public override bool IsChainTriggered => true;

		public CrackingTheWhipI()
		{
			Activated = true;
			Title = 1074295; // Cracking the Whip
			Description = 1074300; // Now that you've shown those mini pests your might, you should collect suitable implements to use to train your summoned pet.  I suggest a stout whip.
			RefusalMessage = 1074313; // Heh. Changed your mind, eh?
			InProgressMessage = 1074317; // Well, hurry up.  If you don't get a whip how do you expect to control the little devil?
			CompletionMessage = 1074321; // That's a well-made whip.  No imp will ignore the sting of that lash.

			Objectives.Add(new CollectObjective(1, typeof(StoutWhip), "Stout Whip"));

			Rewards.Add(new DummyReward(1074873)); // The opportunity to prove yourself worthy of learning to Summon Fiends. (Sufficient spellweaving skill is required to cast the spell)
		}
	}

	// TODO: Verify
	public class CrackingTheWhipII : ChainQuest
	{
		public override bool IsChainTriggered => true;

		public CrackingTheWhipII()
		{
			Activated = true;
			Title = 1074295; // Cracking the Whip
			Description = 1074302; // Now you just need to make the little buggers fear you -- if you can slay an arcane daemon, you'll earn their subservience.
			RefusalMessage = 1074314; // If you're not up for it, so be it.
			InProgressMessage = 1074318; // You need to vanquish an arcane daemon before the imps will fear you properly.

			Objectives.Add(new KillObjective(1, new Type[] { typeof(ArcaneDaemon) }, 1029733)); // arcane demon

			Rewards.Add(new ItemReward(1031608, typeof(SummonFiendScroll))); // Summon Fiend
		}

		public override void GetRewards(ChainQuestInstance instance)
		{
			instance.PlayerContext.SummonFiend = true;
			instance.Player.SendLocalizedMessage(1074322, "", 0x2A); // You've demonstrated your strength, got a means of control, and taught the imps to fear you.  You're ready now to summon them.

			base.GetRewards(instance);
		}
	}
}