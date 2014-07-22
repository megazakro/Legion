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
	public class Gun : IWeapon
	{

		#region 定数

		private const int GunPositionFrontOffset = -6;
		private const int GunPositionBackOffset = -60;
		private const int GunPositionTopOffset = -12;
		private const int GunPositionBottomOffset = 6;

		#endregion // 定数

		#region フィールド

		private Point firePosition;

		private double firingSpan;
		private double recharge;

		private int level;

		private Action firingAction;

		#endregion // フィールド

		#region コンストラクタ

		public Gun()
		{
			this.firePosition = new Point(0, 0);
			this.level = 0;

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
				this.recharge += GameSettings.ElapsedTime;
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

			// 下部機関砲
			{
				var position = new Point(this.firePosition.X + GunPositionFrontOffset,
					this.firePosition.Y + GunPositionBottomOffset);

				this.ActivateBullet(position, enemies[0], FireDirections.Bottom);
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

			// 下部機関砲
			{
				var position = new Point(this.firePosition.X + GunPositionFrontOffset,
					this.firePosition.Y + GunPositionBottomOffset);
				this.ActivateBullet(position, enemies[0], FireDirections.Bottom);
			}

			// 上部機関砲
			{
				var position = new Point(this.firePosition.X + GunPositionFrontOffset,
					this.firePosition.Y + GunPositionTopOffset);
				this.ActivateBullet(position, enemies[1], FireDirections.Top);
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

			// 下部機関砲(前)
			{
				var position = new Point(this.firePosition.X + GunPositionFrontOffset,
					this.firePosition.Y + GunPositionBottomOffset);
				this.ActivateBullet(position, enemies[0], FireDirections.Bottom);
			}

			// 上部機関砲(前)
			{
				var position = new Point(this.firePosition.X + GunPositionFrontOffset,
					this.firePosition.Y + GunPositionTopOffset);
				this.ActivateBullet(position, enemies[1], FireDirections.Top);
			}

			// 下部機関砲(後)
			{
				var position = new Point(this.firePosition.X + GunPositionBackOffset,
					this.firePosition.Y + GunPositionBottomOffset);
				this.ActivateBullet(position, enemies[2], FireDirections.Bottom);
			}

			// 上部機関砲(後)
			{
				var position = new Point(this.firePosition.X + GunPositionBackOffset,
					this.firePosition.Y + GunPositionTopOffset);
				this.ActivateBullet(position, enemies[3], FireDirections.Top);
			}

			this.recharge = 0;
		}

		public void LevelUp(int level)
		{
			this.level = level;
			switch (this.level)
			{
				case 1:
					this.firingSpan = 0.125;
					this.firingAction = this.FiringLv1;

					break;
				case 3:
					this.firingSpan = 0.05;
					this.firingAction = this.FiringLv2;

					break;
				case 7:
					this.firingSpan = 0.025;
					this.firingAction = this.FiringLv3;

					break;
				default:
					break;
			}
		}

		#region private メソッド

		private void ActivateBullet(Point position, IUnit enemy, FireDirections direction)
		{
			if (0 < EntityViewManager.Instance.GetAvailableViewCount(View.ViewType.Bullet))
			{
				var ammo = new Bullet();
				var view = EntityViewManager.Instance.GetView(View.ViewType.Bullet);

				ammo.SetTarget(enemy);
				ammo.Direction = direction;

				ammo.Activate(position, view);
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
