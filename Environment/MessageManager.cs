using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace Legion.Environment
{

	/// <summary>
	/// メッセージを管理します。
	/// </summary>
	public class MessageManager
	{

		#region フィールド

		private static readonly MessageManager instance = new MessageManager();

		#endregion // フィールド

		#region コンストラクタ

		private MessageManager()
		{
			this.SystemMessageQueue = new Queue<string>();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static MessageManager Instance
		{
			get { return instance; }
		}

		public Queue<String> SystemMessageQueue { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.SystemMessageQueue.Clear();
		}

		#endregion // public メソッド

	}
}
