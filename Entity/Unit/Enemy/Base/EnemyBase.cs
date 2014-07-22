using System;
using System.Windows;
using System.Windows.Controls;
using Legion.Environment;
using System.Collections.Generic;
using Legion.Entity.Weapon.Ammo;
using Legion.Util;

namespace Legion.Entity.Unit.Enemy.Base
{
	public class EnemyBase : IEnemy
	{

		#region フィールド

		protected Queue<IUnit> collidedUnitQueue = new Queue<IUnit>();

		protected Queue<IAmmo> collidedAmmoQueue = new Queue<IAmmo>();

		protected double totalDamage;

		protected DamageSourceType causeOfDeath = DamageSourceType.None;

		#endregion // フィールド

		#region プロパティ

		public double Armor { get; protected set; }

		public long Score { get; protected set; }

		public int Exp { get; protected set; }

		public double Velocity { get; set; }

		public Vector2D Heading { get; set; }

		public Vector2D CurrentVector { get; set; }

		public Point Origin { get; protected set; }

		public virtual DamageSourceType DamageSourceType
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public EntityStatus Status { get; protected set; }

		public virtual UserControl View
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public virtual View.ViewType ViewType
		{
			get { throw new NotImplementedException(); }
		}

		#endregion // プロパティ

		#region public メソッド

		public virtual void Activate(Point origin, UserControl view)
		{
			this.Status = EntityStatus.Active;
			EnemyManager.Instance.RegisterEnemy(this);
			this.Origin = origin;
		}

		public virtual void Deactivate()
		{
			EnemyManager.Instance.ChangeEnemyStatus(this);
		}

		public virtual void AdjustPosition(Point position)
		{
			throw new NotImplementedException();
		}

		public virtual FatalArea GetFatalArea()
		{
			throw new NotImplementedException();
		}

		public virtual Point GetFatalAreaCenter()
		{
			throw new NotImplementedException();
		}

		public virtual void SetDamage(double damage, DamageSourceType damageSourceType)
		{
			throw new NotImplementedException();
		}

		public virtual void DetectedCollision()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 衝突が発生したユニットエンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		public virtual void RegisterCollidedEntity(IUnit unit)
		{
			if (!this.collidedUnitQueue.Contains(unit))
			{
				this.collidedUnitQueue.Enqueue(unit);
			}
		}

		/// <summary>
		/// 衝突が発生した弾エンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		public virtual void RegisterCollidedEntity(IAmmo ammo)
		{
			if (!this.collidedAmmoQueue.Contains(ammo))
			{
				this.collidedAmmoQueue.Enqueue(ammo);
			}
		}

		public virtual void UpdateArmor()
		{
			this.Armor -= this.totalDamage;
			this.totalDamage = 0;
		}

		public virtual void LevelUp(DamageSourceType damageSourceType, int level)
		{
			throw new NotImplementedException();
		}

		public virtual void RefreshView()
		{
			throw new NotImplementedException();
		}

		public virtual void UpdateFrame()
		{
		}

		#endregion // public メソッド

	}
}
