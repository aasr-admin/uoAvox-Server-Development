using Server.Accounting;
using Server.ContextMenus;
using Server.Mobiles;
using Server.Network;
using Server.Regions;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Server.Quests
{
	public sealed class FindBankQuest : Quest
	{
		#region Banker Locations

		private static QuestArea[] _BankBounds;

		private static void InitBounds()
		{
			_BankBounds = null;

			var bounds = new HashSet<QuestArea>();

			var points = new Point2D[8];

			foreach (var banker in BaseVendor.AllVendors.OfType<Banker>())
			{
				if (banker.Map != Map.Felucca && banker.Map != Map.Trammel)
				{
					continue;
				}

				if (banker.Spawner == null || !banker.Region.IsPartOf<TownRegion>())
				{
					continue;
				}

				var center = banker.Home != Point3D.Zero ? banker.Home : banker.Location;
				var radius = Math.Max(5, banker.RangeHome);
				var slice = 360 / points.Length;

				for (var i = 0; i < points.Length; i++)
				{
					points[i] = Angle.GetPoint2D(center.X, center.Y, i * slice, radius);
				}

				bounds.Add(new QuestArea(banker.Map, center, new Poly3D(Region.MinZ, Region.MaxZ, points)));
			}

			if (bounds.Count > 0)
			{
				_BankBounds = bounds.ToArray();
			}
		}

		#endregion

		public override TextDefinition Name => "Secure Banking";
		public override TextDefinition Lore => "In this day and age of vegabonds and monsters, your loot is never truly safe from being pilfered!\n"
											 + "So long as you carry your stash, you are at risk of a quick and violent end.\n\n"
											 + "You can find many trustworthy banking establishments throughout the lands...";

		public override TextDefinition MessageOffered => "Are you ready to find a safe place to off-load your loot?";
		public override TextDefinition MessageAccepted => "Finding a trustworthy establishment can be hard in these trying times...";
		public override TextDefinition MessageDeclined => "Finding a trustworthy establishment can be hard in these trying times...";
		public override TextDefinition MessageCompleted => "Now you can safely store your precious loot!";
		public override TextDefinition MessageRedeemed => "Now you can safely store your precious loot!";
		public override TextDefinition MessageAbandoned => "Your pockets will forever remain overloaded.";

		public FindBankQuest(PlayerMobile owner, IQuestLauncher launcher)
			: base(owner, launcher)
		{
		}

		public FindBankQuest(UID uid)
			: base(uid)
		{
		}

		private static bool CheckInitBounds()
		{
			if (_BankBounds == null || _BankBounds.Length == 0)
			{
				InitBounds();
			}

			return _BankBounds?.Length > 0;
		}

		protected override void OnGenerate()
		{
			if (CheckInitBounds())
			{
				AddObjective(new FindBankObjective());
				AddObjective(new FindBankerObjective());
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.WriteEncodedInt(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			_ = reader.ReadEncodedInt();
		}

		public sealed class FindBankObjective : QuestObjective
		{
			public override TextDefinition Title => "Find A Bank";
			public override TextDefinition Summary => "Find a trustworthy banking establishment.\nBritain is probably the best choice... if you're welcome there.";

			public override double ProgressRequired => 1;

			public override QuestArea[] Locations => _BankBounds;

			public FindBankObjective()
			{
			}

			public FindBankObjective(Quest quest)
				: base(quest)
			{
			}

			public override int HandleMoving(Direction dir)
			{
				return InBounds(Owner) ? 1 : 0;
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.WriteEncodedInt(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadEncodedInt();
			}
		}

		public sealed class FindBankerObjective : QuestObjective
		{
			public override TextDefinition Title => "Find A Banker";
			public override TextDefinition Summary => "Find a trustworthy banker.\nBritain is probably the best choice... if you're welcome there.";

			public override double ProgressRequired => 1;

			public override QuestArea[] Locations => _BankBounds;

			public FindBankerObjective()
			{
			}

			public FindBankerObjective(Quest quest)
				: base(quest)
			{
			}

			public override int HandleContextMenuRequest(IEntity owner, List<ContextMenuEntry> entries)
			{
				return owner is Banker ? 1 : 0;
			}

			public override int HandleSaidSpeech(MessageType type, int hue, string said, int[] keywords)
			{
				#region Banker Speech Handling

				int range;

				switch (type)
				{
					case MessageType.Regular: range = 12; break;
					case MessageType.Whisper: range = 1; break;
					case MessageType.Yell: range = 18; break;
					default: return 0;
				}

				var handle = false;

				for (var i = 0; !handle && i < keywords.Length; ++i)
				{
					switch (keywords[i])
					{
						case 0x0000: // *withdraw*
							{
								if (Owner.Criminal)
								{
									handle = true;
									break;
								}

								var split = said.Split(' ');

								if (split.Length >= 2)
								{
									int amount;

									if (!Int32.TryParse(split[1], out amount))
									{
										break;
									}

									if (amount > (Core.ML ? 60000 : 5000))
									{
										handle = true;
										break;
									}
									
									var pack = Owner.Backpack;

									if (pack == null || pack.Deleted || !(pack.TotalWeight < pack.MaxWeight) || !(pack.TotalItems < pack.MaxItems))
									{
										handle = true;
										break;
									}
									
									if (amount > 0)
									{
										handle = true;
										break;
									}
								}

								break;
							}
						case 0x0001: // *balance*
							{
								handle = true;
								break;
							}
						case 0x0002: // *bank*
							{
								handle = true;
								break;
							}
						case 0x0003: // *check*
							{
								if (AccountGold.Enabled)
								{
									break;
								}

								if (Quest.Owner.Criminal)
								{
									handle = true;
									break;
								}

								var split = said.Split(' ');

								if (split.Length >= 2)
								{
									if (!Int32.TryParse(split[1], out var _))
									{
										break;
									}

									handle = true;
									break;
								}

								break;
							}
					}
				}

				if (handle)
				{
					handle = false;

					var eable = Owner.GetMobilesInRange(range);

					foreach (var mob in eable)
					{
						if (mob is Banker banker && banker.HandlesOnSpeech(Owner))
						{
							handle = true;
							break;
						}
					}

					eable.Free();
				}

				return handle ? 1 : 0;

				#endregion
			}

			public override int HandleGiveGold(Item gold, int amount, Mobile receiver)
			{
				return amount > 0 && receiver is Banker ? 1 : 0;
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.WriteEncodedInt(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadEncodedInt();
			}
		}

		public sealed class FindBankLauncher : QuestLauncherItem
		{
			private static readonly Type[] _Quests =
			{
				typeof(FindBankQuest),
			};

			public override Type[] Quests => _Quests;

			public override string DefaultName => "Unidentified Coins";

			public override string DefaultDescription => "";

			[Constructable]
			public FindBankLauncher()
				: base(3820)
			{
			}

			public FindBankLauncher(Serial serial)
				: base(serial)
			{
			}

			public override void Serialize(GenericWriter writer)
			{
				base.Serialize(writer);

				writer.WriteEncodedInt(0);
			}

			public override void Deserialize(GenericReader reader)
			{
				base.Deserialize(reader);

				_ = reader.ReadEncodedInt();
			}
		}
	}
}
