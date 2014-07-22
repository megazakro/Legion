using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Entity.Unit.Enemy.Base;
using Legion.Environment;
using Legion.Util;
using Legion.AI.SteeringBehaviors;
using Legion.Entity.View.Unit.Enemy;
using Legion.Entity.View;
using Legion.Entity.Weapon.Ammo;

namespace Legion.Entity.Unit.Enemy
{

	/// <summary>
	/// ウェリテス。軽装歩兵です。
	/// </summary>
	public class Velites : EnemyBase
	{

		#region フィールド

		private VelitesView view;

		private static Dictionary<int, LevelRelatedContainer> levelMap = new Dictionary<int, LevelRelatedContainer>()
		{
			{1, new	LevelRelatedContainer(){ Armor = 10, Score = 300, Exp=2, Velocity=80, AmmoCount=0, }},
			{2, new	LevelRelatedContainer(){ Armor = 12, Score = 350, Exp=3, Velocity=90, AmmoCount=0, }},
			{3, new	LevelRelatedContainer(){ Armor = 14, Score = 400, Exp=4, Velocity=100, AmmoCount=1, }},
			{4, new	LevelRelatedContainer(){ Armor = 16, Score = 450, Exp=5, Velocity=110, AmmoCount=1, }},
			{5, new	LevelRelatedContainer(){ Armor = 20, Score = 500, Exp=6, Velocity=150, AmmoCount=1, }},
			{6, new	LevelRelatedContainer(){ Armor = 24, Score = 550, Exp=8, Velocity=160, AmmoCount=1, }},
			{7, new	LevelRelatedContainer(){ Armor = 28, Score = 600, Exp=10, Velocity=170, AmmoCount=2, }},
			{8, new	LevelRelatedContainer(){ Armor = 32, Score = 650, Exp=12, Velocity=180, AmmoCount=2, }},
			{9, new	LevelRelatedContainer(){ Armor = 40, Score = 700, Exp=15, Velocity=200, AmmoCount=3, }},
		};

		private double shotSpan = 0.5;
		private double shotCharge = 0;

		private int ammoCount;

		#endregion // フィールド

		#region コンストラクタ

		public Velites()
		{
		}

		#endregion // コンストラクタ

		#region プロパティ

		public override UserControl View
		{
			get
			{
				return this.view;
			}
			set
			{
				this.view = value as VelitesView;
			}
		}

		public override ViewType ViewType
		{
			get
			{
				return ViewType.Velites;
			}
		}

		#endregion // プロパティ

		#region public メソッド

		public override void Activate(Point position, UserControl view)
		{
			base.Activate(position, view);

			this.view = view as VelitesView;

			var container = levelMap[GrowthManager.Instance.EnemyGrowth.Level];
			{
				this.Armor = container.Armor;
				this.Score = container.Score;
				this.Exp = container.Exp;
				this.Velocity = container.Velocity;
				this.ammoCount = container.AmmoCount;
			}

			this.AdjustPosition(position);

			// 自機を狙う
			this.MoveToPlayer();
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.Status = EntityStatus.Deletable;
			EnemyManager.Instance.ChangeEnemyStatus(this);
		}

		public override void AdjustPosition(Point position)
		{
			this.Origin = position;

			var fatalArea = this.GetFatalArea();
			if (fatalArea.leftTop.X < 0 || fatalArea.leftTop.Y < 0 ||
				GameSettings.MainAreaWidth < fatalArea.rightBottom.X || GameSettings.MainAreaHeight < fatalArea.rightBottom.Y)
			{
				this.Retarget();
			}

			CollisionMonitor.Instance.RegisterEnemy(this);
		}

		public override FatalArea GetFatalArea()
		{
			return new FatalArea(new Point(this.Origin.X + 4, this.Origin.Y + 4),
				new Point(this.Origin.X + 28, this.Origin.Y + 28));
		}

		public override Point GetFatalAreaCenter()
		{
			return new Point(this.Origin.X + 16, this.Origin.Y + 16);
		}

		public override void DetectedCollision()
		{
			{
				double damage = 0;
				while (0 < this.collidedUnitQueue.Count)
				{
					var item = this.collidedUnitQueue.Dequeue();
					damage += item.Armor;
				}

				if (0 < damage)
				{
					this.SetDamage(damage, DamageSourceType.Armor);
				}
			}

			{
				DamageSourceType damageSourceType = DamageSourceType.None;
				double damage = 0;
				while (0 < this.collidedAmmoQueue.Count)
				{
					var item = this.collidedAmmoQueue.Dequeue();

					damage += item.Power;
					damageSourceType = item.DamageSourceType;
				}

				if (0 < damage)
				{
					this.SetDamage(damage, damageSourceType);
				}
			}
		}

		public override void SetDamage(double damage, DamageSourceType damageSourceType)
		{
			if (this.Armor <= 0)
			{
				return;
			}

			this.totalDamage += damage;

			if (this.Armor - 0.5 <= this.totalDamage)
			{
				if (DamageSourceType.None == this.causeOfDeath)
				{
					this.causeOfDeath = damageSourceType;
				}
			}
		}

		public override void UpdateArmor()
		{
			base.UpdateArmor();

			if (this.Armor < 0.5)
			{
				this.Armor = 0;

				// スコアの加算
				ScoreManager.Instance.AddScore(this.Score);
				GrowthManager.Instance.EnemyGrowth.AddExp(this.Score);

				// 経験値の加算
				switch (causeOfDeath)
				{
					case DamageSourceType.Armor:
						GrowthManager.Instance.ArmorGrowth.AddExp(this.Exp);
						break;
					case DamageSourceType.Laser:
						GrowthManager.Instance.LaserGrowth.AddExp(this.Exp);
						break;
					case DamageSourceType.Gun:
						GrowthManager.Instance.GunGrowth.AddExp(this.Exp);
						break;
					case DamageSourceType.Missile:
						GrowthManager.Instance.MissileGrowth.AddExp(this.Exp);
						break;
					case DamageSourceType.None:
					default:
						break;
				}
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

			if (this.Armor < 0)
			{
				this.view.LblArmor.Text = "000";
			}
			else
			{
				if (this.Armor < 1)
				{
					this.view.LblArmor.Text = "001";
				}
				else
				{
					this.view.LblArmor.Text = this.Armor.ToString("000");
				}
			}
		}

		public override void UpdateFrame()
		{
			base.UpdateFrame();

			if (0 < this.ammoCount)
			{
				var center = this.GetFatalAreaCenter();

				// 画面外からは撃たない
				if (0 < center.X && center.X < GameSettings.MainAreaWidth &&
					0 < center.Y && center.Y < GameSettings.MainAreaHeight)
				{
					if (this.shotSpan <= this.shotCharge)
					{
						if (0 < EntityViewManager.Instance.GetAvailableViewCount(ViewType.EnemyNormalShot))
						{
							var shot = new EnemyNormalShot();
							var shotView = EntityViewManager.Instance.GetView(Entity.View.ViewType.EnemyNormalShot);
							shot.Velocity = 100;
							shot.Power = 10;

							shot.Activate(this.GetFatalAreaCenter(), shotView);
						}

						this.ammoCount--;
						this.shotCharge = 0;
					}
					else
					{
						this.shotCharge += GameSettings.ElapsedTime;
					}
				}
			}
		}

		#endregion // public メソッド

		#region イベント

		#endregion // イベント

		/// <summary>
		/// レベルに関連する事項を格納するコンテナです。
		/// </summary>
		private class LevelRelatedContainer
		{

			/// <summary>
			/// 装甲強度
			/// </summary>
			public double Armor { get; set; }

			/// <summary>
			/// 撃破時の獲得スコア
			/// </summary>
			public long Score { get; set; }

			/// <summary>
			/// 撃破時の獲得経験値
			/// </summary>
			public int Exp { get; set; }

			/// <summary>
			/// 速度
			/// </summary>
			public int Velocity { get; set; }

			/// <summary>
			/// 所持している弾の数量
			/// </summary>
			public int AmmoCount { get; set; }
		}

		/// <summary>
		/// 自機に向かって移動します。
		/// </summary>
		private void MoveToPlayer()
		{
			var player = PlayerManager.Instance.Player;
			var targetPoint = player.GetFatalAreaCenter();

			Straight.Execute(this, targetPoint);
		}

		/// <summary>
		/// 攻撃対象の再設定を行います。
		/// </summary>
		private void Retarget()
		{
			this.CurrentVector *= 0;

			// 自機を狙う
			this.MoveToPlayer();
		}
	}
}
