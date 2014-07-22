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
using Legion.Entity.Weapon.Ammo.Base;
using Legion.Environment;
using Legion.Util;
using Legion.AI.SteeringBehaviors;

namespace Legion.Entity.View.Weapon.Ammo
{
	public partial class MissileView : UserControl
	{

		#region フィールド

		private Storyboard beginExplodeAnimation;
		private Storyboard endExplodeAnimation;

		#endregion // フィールド

		#region コンストラクタ

		public MissileView()
		{
			InitializeComponent();
			this.beginExplodeAnimation = new Storyboard();
			{
				var animation = new DoubleAnimation()
				{
					From = 0.5d,
					To = 1.0d,
					Duration = new Duration(new TimeSpan(0, 0, 0, 0, 200)),
					
					AutoReverse = false,
				};
				Storyboard.SetTarget(animation, shapeExplode);
				Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));

				this.beginExplodeAnimation.Children.Add(animation);
			}

			this.endExplodeAnimation = new Storyboard();
			{
				var animation = new DoubleAnimation()
				{
					From = 1.0d,
					To = 0.0d,
					Duration = new Duration(new TimeSpan(0, 0, 0, 0, 500)),

					AutoReverse = false,
				};
				Storyboard.SetTarget(animation, shapeExplode);
				Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));

				this.endExplodeAnimation.Children.Add(animation);
			}
		}

		#endregion // コンストラクタ

		#region プロパティ

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.shapeBody.Visibility = System.Windows.Visibility.Visible;
			this.shapeExplode.Visibility = System.Windows.Visibility.Collapsed;
		}

		public void Rotate(double angle)
		{
			var transform = new RotateTransform();
			transform.Angle = angle;
			transform.CenterX = 24;
			transform.CenterY = 24;

			this.RenderTransform = transform;
		}

		public void BeginExplode()
		{
			this.shapeBody.Visibility = System.Windows.Visibility.Collapsed;
			this.shapeExplode.Visibility = System.Windows.Visibility.Visible;

			beginExplodeAnimation.Begin();
		}

		public void EndExplode()
		{
			endExplodeAnimation.Begin();
		}

		#endregion // public メソッド

	}
}
