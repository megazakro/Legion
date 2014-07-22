using System;
using System.Linq;
using System.Net;
using System.Windows;
using Legion.Entity.Weapon;
using Legion.Entity.Weapon.Ammo;
using Legion.Environment;
using Legion.Entity.Unit;
using System.Collections.Generic;
using Legion.Entity.Unit.Enemy;
using Legion.Util;

namespace Legion.Entity.Weapon
{
	public class MissileLauncher : IWeapon
	{

		#region 定数

		private const int MissilePositionCenterOffset = -32;
		private const int MissilePositionBackOffset = -48;
		private const int MissilePositionTopOffset = -28;
		private const int MissilePositionBottomOffset = -20;

		#endregion // 定数

		#region フィールド

		private Point firePosition;

		/// <summary>
		/// 発射間隔（フレーム数）
		/// </summary>
		private int firingSpan = 180;

		/// <summary>
		/// 最発射までのリチャージ
		/// </summary>
		private int recharge = 180;

		private Action firingAction;

		#endregion // フィールド

		#region コンストラクタ

		public MissileLauncher()
		{
			this.firePosition = new Point(0, 0);

			this.LevelUp(1);
			this.IsActive = false;
		}

		#endregion // コンストラクタ

		#region プロパティ

		public bool IsActive { get; set; }

		#endregion // プロパティ

		public void Fire(Point point)
		{
		}

		public void CeaseFire()
		{
		}

		public void UpdateFirePosition(Point point)
		{
			this.firePosition = point;
		}

		public void UpdateFrame()
		{
			if (this.recharge < this.firingSpan)
			{
				this.recharge++;
			}

			if (this.firingSpan <= this.recharge)
			{
				this.firingAction();
			}
		}

		private void FiringLv1()
		{
			List<IEnemy> enemies = this.findEnemy(1);
			if (enemies.Count < 1)
			{
				return;
			}

			// 下部に発射
			{
				var vector = new Vector2D(-1, 4);
				vector.Normalize();

				var position = new Point(this.firePosition.X + MissilePositionCenterOffset,
					this.firePosition.Y + MissilePositionBottomOffset);

				this.LaunchMissile(enemies[0], vector, 100, position);
			}

			this.recharge = 0;
		}

		private void FiringLv2()
		{
			List<IEnemy> enemies = this.findEnemy(2);
			if (enemies.Count < 1)
			{
				return;
			}

			// 下部に発射
			{
				var vector = new Vector2D(-1, 4);
				vector.Normalize();

				var position = new Point(this.firePosition.X + MissilePositionCenterOffset,
					this.firePosition.Y + MissilePositionBottomOffset);

				this.LaunchMissile(enemies[0], vector, 100, position);
			}

			// 上部に発射
			{
				var vector = new Vector2D(-1, -4);
				vector.Normalize();

				var position = new Point(this.firePosition.X + MissilePositionCenterOffset,
					this.firePosition.Y + MissilePositionTopOffset);

				this.LaunchMissile(enemies[1], vector, 100, position);
			}

			this.recharge = 0;
		}

		private void FiringLv3()
		{
			List<IEnemy> enemies = this.findEnemy(4);
			if (enemies.Count < 1)
			{
				return;
			}

			// 下部に発射
			{
				var vector = new Vector2D(-1, 4);
				vector.Normalize();

				var position = new Point(this.firePosition.X + MissilePositionCenterOffset,
					this.firePosition.Y + MissilePositionBottomOffset);

				this.LaunchMissile(enemies[0], vector, 100, position);
			}

			// 上部に発射
			{
				var vector = new Vector2D(-1, -4);
				vector.Normalize();

				var position = new Point(this.firePosition.X + MissilePositionCenterOffset,
					this.firePosition.Y + MissilePositionTopOffset);

				this.LaunchMissile(enemies[1], vector, 100, position);
			}

			// 下部後方に発射
			{
				var vector = new Vector2D(-2, 8);
				vector.Normalize();
				vector *= 0.5d;

				var position = new Point(this.firePosition.X + MissilePositionBackOffset,
					this.firePosition.Y + MissilePositionBottomOffset);

				this.LaunchMissile(enemies[2], vector, 150, position);
			}

			// 上部後方に発射
			{
				var vector = new Vector2D(-2, -8);
				vector.Normalize();
				vector *= 0.5d;

				var position = new Point(this.firePosition.X + MissilePositionBackOffset,
					this.firePosition.Y + MissilePositionTopOffset);

				this.LaunchMissile(enemies[3], vector, 150, position);
			}

			this.recharge = 0;
		}

		public void LevelUp(int level)
		{
			switch (level)
			{
				case 1:
					this.firingSpan = 180;
					this.firingAction = this.FiringLv1;

					break;
				case 3:
					this.firingSpan = 120;
					this.firingAction = this.FiringLv2;

					break;
				case 7:
					this.firingSpan = 60;
					this.firingAction = this.FiringLv3;

					break;
				default:
					break;
			}
		}

		#region private メソッド

		private void LaunchMissile(IUnit enemy, Vector2D heading, double firstVelocity, Point firePosition)
		{
			if (0 < EntityViewManager.Instance.GetAvailableViewCount(View.ViewType.Missile))
			{
				var ammo = new Missile();
				var view = EntityViewManager.Instance.GetView(View.ViewType.Missile);

				ammo.SetTarget(enemy);
				ammo.Velocity = firstVelocity;

				ammo.Heading = heading;
				ammo.CurrentVector = heading * ammo.Velocity * GameSettings.ElapsedTime;

				ammo.Activate(firePosition, view);
			}
		}

		private List<IEnemy> findEnemy(int requestCount)
		{
			List<IEnemy> enemies = new List<IEnemy>();

			// 命中判定領域から、対象として妥当かチェック
			Func<FatalArea, bool> isTargetable = (fatalArea) =>
			{
				if (fatalArea.rightBottom.X < 0 || fatalArea.rightBottom.Y < 0)
				{
					return false;
				}

				if (GameSettings.MainAreaWidth < fatalArea.leftTop.X ||
					GameSettings.MainAreaHeight < fatalArea.leftTop.Y)
				{
					return false;
				}

				return true;
			};

			var node = EnemyManager.Instance.EnemyList.First;

			int index = 0;
			IEnemy lastEnemy = null;
			while (index < requestCount)
			{
				if (null == node)
				{
					if (null == lastEnemy)
					{

						// 有効なターゲットなし
						return enemies;
					}
					else
					{
						while (index < requestCount)
						{
							enemies.Add(lastEnemy);
							index++;
						}

						break;
					}
				}
				else
				{
					if (!isTargetable(node.Value.GetFatalArea()))
					{
						node = node.Next;
						continue;
					}
					else
					{
						enemies.Add(node.Value);
						lastEnemy = node.Value;

						node = node.Next;
					}
				}

				index++;
			}

			return enemies;
		}

		#endregion // private メソッド

	}
}
