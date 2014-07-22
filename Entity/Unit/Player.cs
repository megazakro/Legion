using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using Legion.Entity.Weapon;
using Legion.Entity.Weapon.Ammo;
using Legion.Environment;
using Legion.Util;
using System.Threading;
using Legion.Entity.View.Unit;

namespace Legion.Entity.Unit
{
	public partial class Player : IUnit
	{

		#region 定数

		/// <summary>
		/// 原点から、機首までのX座標のオフセット
		/// </summary>
		private const int NoseOffsetX = 40;

		/// <summary>
		/// 原点から、機尾までのX座標のオフセット
		/// </summary>
		private const int TailOffsetX = 8;

		/// <summary>
		/// 原点から、機体上部までのY座標のオフセット
		/// </summary>
		private const int TopOffsetY = 16;

		/// <summary>
		/// 原点から、機体下部までのY座標のオフセット
		/// </summary>
		private const int BottomOffsetY = 32;

		/// <summary>
		/// 装甲の取り得る最大値
		/// </summary>
		private const int ArmorMax = 999;

		private double regenerateRatio = 0.01;
		private Dictionary<int, double> regenerateRatioMap = new Dictionary<int, double>()
		{
			{1, 0.01},
			{2, 0.02},
			{3, 0.03},
			{4, 0.04},
			{5, 0.06},
			{6, 0.08},
			{7, 0.10},
			{8, 0.12},
			{9, 0.15},
		};

		private TimeSpan regenerateSpan = new TimeSpan(0, 0, 0, 1, 0);

		private DateTime lastRegenerateTime = DateTime.MinValue;

		#endregion // 定数

		public enum MovingState
		{
			Stop,
			Moving,
		}

		#region フィールド

		private MovingState movingState = MovingState.Stop;
		private double deltaX = 0;
		private double deltaY = 0;

		private IWeapon mainWeapon;
		private IWeapon subWeapon;

		private Queue<IUnit> collidedEnemyQueue = new Queue<IUnit>();
		private Queue<IAmmo> collidedAmmoQueue = new Queue<IAmmo>();

		private double damage;

		private PlayerView view;

		#endregion // フィールド

		#region コンストラクタ

		public Player()
		{
			CollisionMonitor.Instance.RegisterPlayer(this);

			this.Armor = 100;
			this.ArmorFull = 100;
			this.Speed = 2;
			this.Velocity = 50;
			this.TotalRegeneratedArmor = 0;
			this.damage = 0;

			this.view = EntityViewManager.Instance.GetView(Entity.View.ViewType.Player) as PlayerView;

			this.Laser = new LaserRifle();
			this.Gun = new Gun();
			this.Missile = new MissileLauncher();

			this.mainWeapon = this.Laser;
			this.subWeapon = null;
		}

		#endregion // コンストラクタ

		#region プロパティ

		public double DeltaX
		{
			get
			{
				return this.deltaX;
			}
			set
			{
				this.deltaX = value * this.Speed;
			}
		}

		public double DeltaY
		{
			get
			{
				return this.deltaY;
			}
			set
			{
				this.deltaY = value * this.Speed;
			}
		}

		/// <summary>
		/// 装甲強度を取得します。
		/// </summary>
		public double Armor { get; private set; }

		/// <summary>
		/// 全快時の装甲強度を取得します。
		/// </summary>
		public double ArmorFull { get; private set; }

		/// <summary>
		/// 自機のスピードを取得します。
		/// </summary>
		public int Speed { get; private set; }

		/// <summary>
		/// 回復したArmorの総量
		/// </summary>
		public double TotalRegeneratedArmor { get; private set; }

		public DamageSourceType DamageSourceType
		{
			get
			{
				return DamageSourceType.Armor;
			}
		}

		public double Velocity { get; set; }

		public Vector2D Heading { get; set; }

		public Vector2D CurrentVector { get; set; }

		public Point Origin { get; private set; }

		public EntityStatus Status { get; private set; }

		public View.ViewType ViewType
		{
			get { throw new NotImplementedException(); }
		}

		public IWeapon Laser { get; private set; }
		public IWeapon Gun { get; private set; }
		public IWeapon Missile { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void SpeedUp()
		{
			int lastSpeed = this.Speed;
			this.Speed++;

			if (5 < this.Speed)
			{
				this.Speed = 5;
			}

			if (1 == this.Speed)
			{
				if (this.deltaX < 0)
				{
					this.deltaX = -1;
				}
				else if (0 < this.deltaX)
				{
					this.deltaX = 1;
				}

				if (this.deltaY < 0)
				{
					this.deltaY = -1;
				}
				else if (0 < this.deltaY)
				{
					this.deltaY = 1;
				}
			}
			else
			{
				this.deltaX = this.deltaX / lastSpeed * this.Speed;
				this.deltaY = this.deltaY / lastSpeed * this.Speed;
			}
		}

		public void SpeedDown()
		{
			int lastSpeed = this.Speed;
			this.Speed--;

			if (this.Speed < 1)
			{
				this.Speed = 1;
			}

			if (1 == this.Speed)
			{
				if (this.deltaX < 0)
				{
					this.deltaX = -1;
				}
				else if (0 < this.deltaX)
				{
					this.deltaX = 1;
				}

				if (this.deltaY < 0)
				{
					this.deltaY = -1;
				}
				else if (0 < this.deltaY)
				{
					this.deltaY = 1;
				}
			}
			else
			{
				this.deltaX = this.deltaX / lastSpeed * this.Speed;
				this.deltaY = this.deltaY / lastSpeed * this.Speed;
			}
		}

		public void Fire()
		{
			Point firePosition = this.GetFirePosition();

			this.mainWeapon.Fire(firePosition);

			if (null != this.subWeapon)
			{
				this.subWeapon.Fire(firePosition);
			}
		}

		public void CeaseFire()
		{
			this.mainWeapon.CeaseFire();

			if (null != this.subWeapon)
			{
				this.subWeapon.CeaseFire();
			}
		}

		/// <summary>
		/// 命中判定のある領域を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の原点、終点の座標を表す配列。</returns>
		public FatalArea GetFatalArea()
		{
			return new FatalArea(new Point(this.Origin.X + 22, this.Origin.Y + 22),
				new Point(this.Origin.X + 26, this.Origin.Y + 26));
		}

		/// <summary>
		/// 命中判定のある領域の中心を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の中心を表す座標。</returns>
		public Point GetFatalAreaCenter()
		{
			return new Point(this.Origin.X + 24, this.Origin.Y + 24);
		}

		/// <summary>
		/// 自機にダメージを設定します。
		/// </summary>
		/// <param name="damage"></param>
		public void SetDamage(double damage, DamageSourceType damageSourceType)
		{
			if (0 == damage)
			{
				return;
			}

			this.damage += damage;
		}

		/// <summary>
		/// 衝突が発生したユニットエンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		public void RegisterCollidedEntity(IUnit unit)
		{
			if (!this.collidedEnemyQueue.Contains(unit))
			{
				this.collidedEnemyQueue.Enqueue(unit);
			}
		}

		/// <summary>
		/// 衝突が発生した弾エンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		public void RegisterCollidedEntity(IAmmo ammo)
		{
			if (!this.collidedAmmoQueue.Contains(ammo))
			{
				this.collidedAmmoQueue.Enqueue(ammo);
			}
		}

		/// <summary>
		/// 自機のArmorを回復します。
		/// </summary>
		public void RegenerateArmor()
		{
			if (this.ArmorFull == this.Armor ||
				this.Armor <= 0)
			{
				return;
			}

			if (this.regenerateSpan < (DateTime.Now - this.lastRegenerateTime))
			{
				double regenerateValue = this.ArmorFull * this.regenerateRatio;

				// 回復量がダメージを上回る場合は補正する
				double currentDamage = this.ArmorFull - this.Armor;
				if (currentDamage < regenerateValue)
				{
					regenerateValue = currentDamage;
				}

				this.Armor += regenerateValue;

				this.TotalRegeneratedArmor += regenerateValue;

				this.ArmorFull = 100 + (this.TotalRegeneratedArmor / 10);
				if (ArmorMax < this.ArmorFull)
				{
					this.ArmorFull = ArmorMax;
				}

				this.lastRegenerateTime = DateTime.Now;
			}
		}

		public void Activate(Point origin, UserControl view)
		{
			throw new NotImplementedException();
		}

		public void Deactivate()
		{
			throw new NotImplementedException();
		}

		public void AdjustPosition(Point position)
		{
			double x = this.Origin.X;
			if ((0 - TailOffsetX) <= position.X && position.X < (GameSettings.MainAreaWidth - NoseOffsetX))
			{
				x = position.X;
			}

			double y = this.Origin.Y;
			if ((0 - TopOffsetY) <= position.Y && position.Y < (GameSettings.MainAreaHeight - BottomOffsetY))
			{
				y = position.Y;
			}

			this.Origin = new Point(x, y);

			// 自機の座標変更の反映
			{
				CollisionMonitor.Instance.RegisterPlayer(this);

				Point firePosition = this.GetFirePosition();
				this.mainWeapon.UpdateFirePosition(firePosition);

				if (null != this.subWeapon)
				{
					this.subWeapon.UpdateFirePosition(firePosition);
				}
			};
		}

		public void DetectedCollision()
		{
			{
				double damage = 0;

				while (0 < this.collidedEnemyQueue.Count)
				{
					var enemy = this.collidedEnemyQueue.Dequeue();

					if (0 < enemy.Armor)
					{
						damage += enemy.Armor;
					}
				}

				this.SetDamage(damage, DamageSourceType.Armor);
			}

			{
				DamageSourceType damageSourceType = DamageSourceType.None;
				double damage = 0;
				while (0 < this.collidedAmmoQueue.Count)
				{
					var ammo = this.collidedAmmoQueue.Dequeue();
					if (0 < ammo.Power)
					{
						damage += ammo.Power;
						damageSourceType = ammo.DamageSourceType;
					}
				}

				this.SetDamage(damage, damageSourceType);
			}
		}

		/// <summary>
		/// 装甲強度を更新します。
		/// </summary>
		public void UpdateArmor()
		{
			this.Armor -= this.damage;
			this.damage = 0;
		}

		/// <summary>
		/// 装備のレベルアップを行います。
		/// </summary>
		/// <param name="damageSourceType">装備種別</param>
		/// <param name="level">レベル</param>
		public void LevelUp(DamageSourceType damageSourceType, int level)
		{
			switch (damageSourceType)
			{
				case DamageSourceType.Armor:
					this.regenerateRatio = regenerateRatioMap[level];

					break;
				case DamageSourceType.Laser:
					this.Laser.LevelUp(level);

					break;
				case DamageSourceType.Gun:
					this.Gun.LevelUp(level);

					break;
				case DamageSourceType.Missile:
					this.Missile.LevelUp(level);

					break;
				case DamageSourceType.None:
				default:
					return;
			}
		}

		public void UpdateFrame()
		{
			if (0 == this.deltaX &&
				0 == this.deltaY)
			{
				this.movingState = MovingState.Stop;
			}
			else
			{
				this.movingState = MovingState.Moving;
			}

			switch (this.movingState)
			{
				case MovingState.Stop:
					break;
				case MovingState.Moving:
					Point newPosition = new Point(
						this.Origin.X + (this.deltaX * this.Velocity * GameSettings.ElapsedTime),
						this.Origin.Y + (this.deltaY * this.Velocity * GameSettings.ElapsedTime));

					this.AdjustPosition(newPosition);

					break;
				default:
					break;
			}

			this.mainWeapon.UpdateFrame();
			if (null != this.subWeapon)
			{
				this.subWeapon.UpdateFrame();
			}
		}

		public UserControl View
		{
			get
			{
				return this.view;
			}
			set
			{
				this.view = value as PlayerView;
			}
		}

		public void RefreshView()
		{
			PlayerView.State state = PlayerView.State.Normal;
			if (this.deltaY < 0)
			{
				state = PlayerView.State.MoveUp;
			}
			else if (0 < this.deltaY)
			{
				state = PlayerView.State.MoveDown;
			}

			this.view.RefreshView(state);
			Canvas.SetLeft(this.view, this.Origin.X);
			Canvas.SetTop(this.view, this.Origin.Y);
		}

		/// <summary>
		/// サブウェポンの切り替えを行います。
		/// </summary>
		public void ToggleSubWeapon()
		{
			if (null == this.subWeapon)
			{
				this.subWeapon = this.Gun;
				this.Gun.IsActive = true;
				this.Gun.UpdateFirePosition(this.GetFirePosition());
			}
			else if (this.Gun == this.subWeapon)
			{
				this.Gun.IsActive = false;

				this.subWeapon = this.Missile;
				this.Missile.IsActive = true;
				this.Missile.UpdateFirePosition(this.GetFirePosition());
			}
			else if (this.Missile == this.subWeapon)
			{
				this.Missile.IsActive = false;

				this.subWeapon = null;
			}
		}

		#endregion // public メソッド

		#region private メソッド

		private Point GetFirePosition()
		{
			return new Point(this.Origin.X + 40, this.Origin.Y + 24);
		}

		#endregion // private メソッド

	}
}
