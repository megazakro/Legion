using System;
using System.Net;
using System.Collections.Generic;

namespace Legion.Util
{
	public class Debug
	{
		private static Queue<String> messageQueue = new Queue<string>();

		public static Queue<String> GetMessageQueue()
		{
			return messageQueue;
		}

		public static void AddMessage(String message)
		{
			messageQueue.Enqueue(message);
		}

		public static void AddMessageLine(String message)
		{
			messageQueue.Enqueue(message + System.Environment.NewLine);
		}
	}
}
