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
using Legion.Entity.View.Weapon.Ammo;

namespace Legion.Entity.Weapon.Ammo
{
	public partial class EnemyNormalShot : AmmoBase
	{

		#region フィールド

		private EnemyNormalShotView view;

		#endregion // フィールド

		#region コンストラクタ

		public EnemyNormalShot()
		{
		}

		#endregion // コンストラクタ

		#region プロパティ

		public override DamageSourceType DamageSourceType
		{
			get
			{
				return DamageSourceType.Gun;
			}
		}

		public override UserControl View
		{
			get
			{
				return this.view;
			}
			set
			{
				this.view = value as EnemyNormalShotView;
			}
		}

		public override View.ViewType ViewType
		{
			get
			{
				return Entity.View.ViewType.EnemyNormalShot;
			}
		}

		#endregion // プロパティ

		#region public メソッド

		public override void Activate(Point origin, UserControl view)
		{
			base.Activate(origin, view);

			this.view = view as EnemyNormalShotView;

			this.AdjustPosition(origin);

			// 自機を狙う
			{
				var player = PlayerManager.Instance.Player;
				var targetPoint = player.GetFatalAreaCenter();

				Straight.Execute(this, targetPoint);
			}
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.Power = 0;
			this.Status = EntityStatus.Deletable;
			AmmoManager.Instance.ChangeAmmoStatus(this);
		}

		public override void AdjustPosition(Point position)
		{
			this.Origin = position;

			CollisionMonitor.Instance.RegisterEnemyAmmo(this);
		}

		public override FatalArea GetFatalArea()
		{
			return new FatalArea(new Point(this.Origin.X + 2, this.Origin.Y + 2),
				new Point(this.Origin.X + 14, this.Origin.Y + 14));
		}

		/// <summary>
		/// 命中判定のある領域の中心を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の中心を表す座標。</returns>
		public override Point GetFatalAreaCenter()
		{
			return new Point(this.Origin.X + 8, this.Origin.Y  + 8);
		}

		public override void DetectedCollision()
		{
			if (0 < this.collidedUnitQueue.Count)
			{
				this.Deactivate();
			}
		}

		public override void RefreshView()
		{
			if (null == this.view)
			{
				return;
			}

			Canvas.SetLeft(this.view, this.Origin.X);
			Canvas.SetTop(this.view, this.Origin.Y);
		}

		#endregion // public メソッド

	}
}
