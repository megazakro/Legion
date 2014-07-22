using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using Legion.Entity.Weapon.Ammo.Base;
using Legion.Environment;
using Legion.Util;
using Legion.Entity.View;
using Legion.Entity.View.Weapon.Ammo;
using Legion.AI.SteeringBehaviors;
using Legion.Entity.Unit;

namespace Legion.Entity.Weapon.Ammo
{
	public partial class Bullet : AmmoBase
	{

		#region フィールド

		private BulletView view;

		private IUnit target;

		private static Dictionary<int, LevelRelatedContainer> levelMap = new Dictionary<int, LevelRelatedContainer>()
		{
			{1, new	LevelRelatedContainer(){ Power = 1.0, Velocity = 600, }},
			{2, new	LevelRelatedContainer(){ Power = 1.5, Velocity = 650, }},
			{3, new	LevelRelatedContainer(){ Power = 2.0, Velocity = 700, }},
			{4, new	LevelRelatedContainer(){ Power = 2.5, Velocity = 750, }},
			{5, new	LevelRelatedContainer(){ Power = 4.0, Velocity = 800, }},
			{6, new	LevelRelatedContainer(){ Power = 4.5, Velocity = 850, }},
			{7, new	LevelRelatedContainer(){ Power = 5.0, Velocity = 900, }},
			{8, new	LevelRelatedContainer(){ Power = 5.5, Velocity = 950, }},
			{9, new	LevelRelatedContainer(){ Power = 7.0, Velocity = 1000, }},
		};

		#endregion // フィールド

		#region コンストラクタ

		public Bullet()
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

		public override ViewType ViewType
		{
			get
			{
				return ViewType.Bullet;
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
				this.view = value as BulletView;
			}
		}

		public FireDirections Direction { get; set; }

		#endregion // プロパティ

		#region public メソッド

		public override void Activate(Point primaryPoint, UserControl view)
		{
			base.Activate(primaryPoint, view);

			this.view = view as BulletView;

			var container = levelMap[GrowthManager.Instance.GunGrowth.Level];
			this.Power = container.Power;
			this.Velocity = container.Velocity;

			this.AdjustPosition(primaryPoint);

			// 敵を狙う
			{
				if (null != target)
				{
					var thisCenter = this.GetFatalAreaCenter();

					var enemyCenter = this.target.GetFatalAreaCenter();

					// 自分と敵とのベクトルを取得
					Vector2D vector = new Vector2D(enemyCenter.X - thisCenter.X, enemyCenter.Y - thisCenter.Y);
					vector.Normalize();
					vector = vector * this.Velocity;

					// 相手の進路ベクトルと加算
					Vector2D targetVector = vector + this.target.Heading;
					targetVector.Normalize();

					switch (this.Direction)
					{
						case FireDirections.Top:

							// 下に向いている場合、水平になるよう補正
							if (0 < targetVector.Y)
							{
								targetVector.Y = 0;

								if (0 < targetVector.X)
								{
									targetVector.X = 1;
								}
								else
								{
									targetVector.X = -1;
								}
							}

							break;
						case FireDirections.Bottom:

							// 上に向いている場合、水平になるよう補正
							if (targetVector.Y < 0)
							{
								targetVector.Y = 0;

								if (0 < targetVector.X)
								{
									targetVector.X = 1;
								}
								else
								{
									targetVector.X = -1;
								}
							}

							break;
						default:
							break;
					}

					this.Heading = targetVector;
					this.CurrentVector = targetVector * this.Velocity * GameSettings.ElapsedTime;
				}
				else
				{
					Straight.Execute(this, new Point(800, 200));
				}
			}
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.Status = EntityStatus.Deletable;
			AmmoManager.Instance.ChangeAmmoStatus(this);
		}

		public override void AdjustPosition(Point position)
		{
			this.Origin = position;

			CollisionMonitor.Instance.RegisterPlayerAmmo(this);
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
			return new Point(this.Origin.X + 8, this.Origin.Y + 8);
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

		public void SetTarget(IUnit target)
		{
			this.target = target;
		}

		#endregion // public メソッド

		/// <summary>
		/// レベルに関連する事項を格納するコンテナです。
		/// </summary>
		private class LevelRelatedContainer
		{

			/// <summary>
			/// 威力
			/// </summary>
			public double Power { get; set; }

			/// <summary>
			/// 速度
			/// </summary>
			public double Velocity { get; set; }
		}

	}
}
