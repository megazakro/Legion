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

namespace Legion.Entity
{
	public enum EntityStatus
	{

		/// <summary>
		/// 未アクティブ状態
		/// </summary>
		Standby,

		/// <summary>
		/// アクティブ
		/// </summary>
		Active,

		/// <summary>
		/// アクティブ状態終了
		/// </summary>
		Deactivated,

		/// <summary>
		/// 削除可能
		/// </summary>
		Deletable,
	}
}
