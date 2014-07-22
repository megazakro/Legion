using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Entity.Unit.Enemy.Base;
using Legion.Environment;
using Legion.Util;
using Legion.AI.SteeringBehaviors;

namespace Legion.Entity.View.Unit.Enemy
{

	/// <summary>
	/// Velitesのビュークラスです。
	/// </summary>
	public partial class VelitesView : UserControl
	{

		#region コンストラクタ

		public VelitesView()
		{
			InitializeComponent();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public TextBlock LblArmor { get { return this.lblArmor; } }

		#endregion // プロパティ

		#region public メソッド

		#endregion // public メソッド

		#region イベント

		#endregion // イベント

	}
}
