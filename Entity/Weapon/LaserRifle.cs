using System;
using System.Net;
using System.Windows;

using Legion.Entity.Weapon;
using Legion.Entity.Weapon.Ammo;
using Legion.Environment;
using System.Collections.Generic;

namespace Legion.Entity.Weapon
{
	public class LaserRifle : IWeapon
	{

		#region フィールド

		/// <summary>
		/// レーザー発射位置の補正用マップ
		/// </summary>
		private static Point[] adjustPointMap =
		{
			new Point(0, 0),
			new Point(-16, 24),
			new Point(-16, -24),
			new Point(-24, 48),
			new Point(-24, -48),
		};

		private List<IAmmo> lasers;

		private Action firingAction;

		private Point currentFirePosition;

		#endregion // フィールド

		#region コンストラクタ

		public LaserRifle()
		{
			this.IsActive = true;
			this.lasers = new List<IAmmo>();

			this.LevelUp(1);
		}

		#endregion // コンストラクタ

		#region プロパティ

		public bool IsActive { get; set; }

		#endregion // プロパティ

		#region public メソッド

		public void Fire(Point point)
		{
			this.currentFirePosition = point;
			this.firingAction();
		}

		public void CeaseFire()
		{
			foreach (var laser in lasers)
			{
				laser.Deactivate();
			}

			lasers.Clear();
		}

		public void UpdateFirePosition(Point point)
		{
			this.currentFirePosition = point;

			if (0 < this.lasers.Count)
			{
				this.lasers[0].AdjustPosition(point);

				int index = 1;
				while (index < this.lasers.Count)
				{
					var laser = this.lasers[index];

					var adjustPoint = adjustPointMap[index];
					var newPoint = new Point(point.X + adjustPoint.X, 
						point.Y + adjustPoint.Y);

					laser.AdjustPosition(newPoint);

					index++;
				}
			}
		}

		public void UpdateFrame()
		{
		}

		public void LevelUp(int level)
		{
			switch (level)
			{
				case 1:
					this.firingAction = this.FiringLv1;

					break;
				case 3:
					this.firingAction = this.FiringLv2;

					break;
				case 7:
					this.firingAction = this.FiringLv3;

					break;
				default:
					break;
			}

			if (0 < this.lasers.Count)
			{
				this.CeaseFire();
				this.Fire(this.currentFirePosition);
			}
		}

		#endregion // public メソッド

		#region private メソッド

		private void FiringLv1()
		{
			this.ActivateLaser(1);
		}

		private void FiringLv2()
		{
			this.ActivateLaser(3);
		}

		private void FiringLv3()
		{
			this.ActivateLaser(5);
		}

		private void ActivateLaser(int laserCount)
		{
			if (this.lasers.Count < laserCount)
			{
				if (laserCount <= EntityViewManager.Instance.GetAvailableViewCount(View.ViewType.Laser))
				{
					int index = 0;
					while (index < laserCount)
					{
						var adjustPoint = adjustPointMap[index];
						var position = new Point(this.currentFirePosition.X + adjustPoint.X,
							this.currentFirePosition.Y + adjustPoint.Y);

						var laser = new Laser();
						var view = EntityViewManager.Instance.GetView(View.ViewType.Laser);
						laser.Activate(position, view);

						this.lasers.Add(laser);

						index++;
					}
				}
			}
		}

		#endregion // private メソッド


	}
}
