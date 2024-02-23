using Server.Mobiles;

namespace Server.Gumps
{
	public abstract class BaseConfirmGump : BaseGump
	{
		public bool DisplayChoice { get; set; }

		public TextDefinition Title { get; set; }
		public TextDefinition Label { get; set; }

		// 1074975: Are you sure you wish to select this?
		public BaseConfirmGump(PlayerMobile user)
			: this(user, 1074975)
		{ }

		// 1075083: <center>Warning!</center>
		public BaseConfirmGump(PlayerMobile user, TextDefinition label) : this(user, 1075083, label)
		{ }

		public BaseConfirmGump(PlayerMobile user, TextDefinition title, TextDefinition label) : base(user, 120, 50)
		{
			Title = title;
			Label = label;

			Closable = false;
			Disposable = true;
			Dragable = true;
			Resizable = false;
		}

		public override void AddGumpLayout()
		{
			AddPage(0);

			AddImageTiled(0, 0, 348, 262, 0xA8E);
			AddAlphaRegion(0, 0, 348, 262);
			AddImage(0, 15, 0x27A8);
			AddImageTiled(0, 30, 17, 200, 0x27A7);
			AddImage(0, 230, 0x27AA);
			AddImage(15, 230, 0x280C);
			AddImageTiled(30, 0, 300, 17, 0x280A);
			AddImage(315, 0, 0x280E);
			AddImage(15, 244, 0x280C);
			AddImageTiled(30, 244, 300, 17, 0x280A);
			AddImage(315, 244, 0x280E);
			AddImage(330, 15, 0x27A8);
			AddImageTiled(330, 30, 17, 200, 0x27A7);
			AddImage(330, 230, 0x27AA);
			AddImage(333, 2, 0x2716);
			AddImage(315, 248, 0x2716);
			AddImage(2, 248, 0x2716);
			AddImage(2, 2, 0x2716);
			TextDefinition.AddHtmlText(this, 25, 25, 200, 20, Title, false, false, 0x7D00, 0xFF4200);
			AddImage(25, 40, 0xBBF);
			TextDefinition.AddHtmlText(this, 25, 55, 300, 120, Label, false, false, 0x7FFF, 0xFFFFFF);

			if (DisplayChoice)
			{
				AddRadio(25, 175, 0x25F8, 0x25FB, true, (int)Buttons.Break);
				AddRadio(25, 210, 0x25F8, 0x25FB, false, (int)Buttons.Close);
			}

			AddHtmlLocalized(60, 180, 280, 20, 1074976, 0x7FFF, false, false);
			AddHtmlLocalized(60, 215, 280, 20, 1074977, 0x7FFF, false, false);

			AddButton(265, 220, 0xF7, 0xF8, (int)Buttons.Confirm, GumpButtonType.Reply, 0);
		}

		private enum Buttons
		{
			Close,
			Break,
			Confirm
		}

		public override void OnResponse(RelayInfo info)
		{
			base.OnResponse(info);

			if (info.ButtonID == (int)Buttons.Confirm)
			{
				if (!DisplayChoice)
				{
					Close();
					return;
				}

				if (info.IsSwitched((int)Buttons.Break))
				{
					Confirm();
				}
				else
				{
					Refuse();
				}
			}
		}

		public virtual void Confirm()
		{
		}

		public virtual void Refuse()
		{
		}
	}
}