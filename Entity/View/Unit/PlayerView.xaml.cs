using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Legion.Entity.Weapon;
using Legion.Entity.Weapon.Ammo;
using Legion.Environment;
using Legion.Util;
using System.Threading;

namespace Legion.Entity.View.Unit
{
	public partial class PlayerView : UserControl
	{

		#region 定数

		#endregion // 定数

		public enum State
		{
			Normal,
			MoveUp,
			MoveDown,
		}

		#region フィールド

		private State lastState;

		#endregion // フィールド

		#region コンストラクタ

		public PlayerView()
		{
			InitializeComponent();
			this.lastState = State.Normal;
		}

		#endregion // コンストラクタ

		#region プロパティ

		#endregion // プロパティ

		#region public メソッド

		public void RefreshView(State state)
		{
			if (state == lastState)
			{
				return;
			}

			switch (state)
			{
				case State.Normal:
					this.shapeNormal.Visibility = Visibility.Visible;
					this.shapeUp.Visibility = Visibility.Collapsed;
					this.shapeDown.Visibility = Visibility.Collapsed;

					break;
				case State.MoveUp:
					this.shapeNormal.Visibility = Visibility.Collapsed;
					this.shapeUp.Visibility = Visibility.Visible;
					this.shapeDown.Visibility = Visibility.Collapsed;

					break;
				case State.MoveDown:
					this.shapeNormal.Visibility = Visibility.Collapsed;
					this.shapeUp.Visibility = Visibility.Collapsed;
					this.shapeDown.Visibility = Visibility.Visible;

					break;
				default:
					break;
			}

			this.lastState = state;
		}

		#endregion // public メソッド

		#region イベント

		#endregion // イベント

		#region private メソッド

		#endregion // private メソッド

	}
}
