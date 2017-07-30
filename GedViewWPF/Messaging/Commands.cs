using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GedViewWPF.Messaging
{
	public class MessageArgs : EventArgs
	{
		public object Sender { get; set; }
		public object Data { get; set; }

		public MessageArgs(object sender)
		{
			Sender = sender;
			Data = null;
		}

		public MessageArgs(object sender, object data)
		{
			Sender = sender;
			Data = data;
		}
	}

	public class Messages
	{
		// Command requests
		public static string DoFileNew = "DoFileNew";
		public static string DoFileOpen = "DoFileOpen";
		public static string DoFileClose = "DoFileClose";
		public static string DoFileExit = "DoFileExit";

		public static string DoViewNavigate = "DoViewNavigate";

		// Notifications
	}
}
