using Server.Network;

using System;

namespace Server.Gumps
{
	public delegate void WarningGumpCallback(Mobile from, bool okay, object state);

	public delegate void WarningGumpCallback<T>(Mobile from, bool okay, T state);

	public class WarningGump : WarningGump<object>
	{
		public WarningGump(int header, short headerColor, object content, int contentColor, int width, int height, WarningGumpCallback callback, object state)
			: base(header, headerColor, content, contentColor, width, height, (m, o, s) => callback?.Invoke(m, o, s), state)
		{
		}

		public WarningGump(int header, short headerColor, object content, int contentColor, int width, int height, WarningGumpCallback callback, object state, bool cancelButton) 
			: base(header, headerColor, content, contentColor, width, height, (m, o, s) => callback?.Invoke(m, o, s), state, cancelButton)
		{
		}
	}

	public class WarningGump<T> : Gump
	{
		private readonly WarningGumpCallback<T> m_Callback;
		private readonly T m_State;
		private readonly bool m_CancelButton;

		public WarningGump(int header, short headerColor, object content, int contentColor, int width, int height, WarningGumpCallback<T> callback, T state)
			: this(header, headerColor, content, contentColor, width, height, callback, state, true)
		{
		}

		public WarningGump(int header, short headerColor, object content, int contentColor, int width, int height, WarningGumpCallback<T> callback, T state, bool cancelButton) 
			: base((640 - width) / 2, (480 - height) / 2)
		{
			m_Callback = callback;
			m_State = state;
			m_CancelButton = cancelButton;

			Closable = false;

			AddPage(0);

			AddBackground(0, 0, width, height, 5054);

			AddImageTiled(10, 10, width - 20, 20, 2624);
			AddAlphaRegion(10, 10, width - 20, 20);

			AddHtmlLocalized(10, 10, width - 20, 20, header, headerColor, false, false);

			AddImageTiled(10, 40, width - 20, height - 80, 2624);
			AddAlphaRegion(10, 40, width - 20, height - 80);

			if (content is int)
			{
				Utility.ConvertColor(contentColor, out var contentColor16);

				AddHtmlLocalized(10, 40, width - 20, height - 80, (int)content, contentColor16, false, true);
			}
			else if (content is string)
			{
				AddHtml(10, 40, width - 20, height - 80, String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", contentColor, content), false, true);
			}

			AddImageTiled(10, height - 30, width - 20, 20, 2624);
			AddAlphaRegion(10, height - 30, width - 20, 20);

			AddButton(10, height - 30, 4005, 4007, 1, GumpButtonType.Reply, 0);
			AddHtmlLocalized(40, height - 30, 170, 20, 1011036, 32767, false, false); // OKAY

			if (m_CancelButton)
			{
				AddButton(10 + ((width - 20) / 2), height - 30, 4005, 4007, 0, GumpButtonType.Reply, 0);
				AddHtmlLocalized(40 + ((width - 20) / 2), height - 30, 170, 20, 1011012, 32767, false, false); // CANCEL
			}
		}

		public override void OnResponse(NetState sender, RelayInfo info)
		{
			if (info.ButtonID == 1 && m_Callback != null)
			{
				m_Callback(sender.Mobile, true, m_State);
			}
			else if (m_Callback != null)
			{
				m_Callback(sender.Mobile, false, m_State);
			}
		}
	}
}