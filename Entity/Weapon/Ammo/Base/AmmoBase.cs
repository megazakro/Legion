using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Legion.Environment;
using Legion.Util;
using Legion.Entity.Unit;
using Legion.Entity.View;

namespace Legion.Entity.Weapon.Ammo.Base
{
	public class AmmoBase : IAmmo
	{

		#region フィールド

		protected Queue<IUnit> collidedUnitQueue = new Queue<IUnit>();

		#endregion // フィールド

		#region プロパティ

		public double Power { get; set; }

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

		public virtual ViewType ViewType
		{
			get { throw new NotImplementedException(); }
		}

		#endregion // プロパティ

		#region public メソッド

		public virtual void Activate(Point origin, UserControl view)
		{
			this.Status = EntityStatus.Active;
			AmmoManager.Instance.RegisterAmmo(this, this.Status);
			this.Origin = origin;
		}

		public virtual void Deactivate()
		{
			this.Status = EntityStatus.Deactivated;
			AmmoManager.Instance.ChangeAmmoStatus(this);
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

		public virtual void DetectedCollision()
		{
			throw new NotImplementedException();
		}

		public virtual void UpdateFrame()
		{
		}

		public virtual void RegisterCollidedEntity(IUnit unit)
		{
			if (!this.collidedUnitQueue.Contains(unit))
			{
				this.collidedUnitQueue.Enqueue(unit);
			}
		}

		public virtual void RegisterCollidedEntity(IAmmo unit)
		{

			// 何もしない
		}

		public virtual void LevelUp(DamageSourceType damageSourceType, int level)
		{
			throw new NotImplementedException();
		}

		public virtual void RefreshView()
		{
			throw new NotImplementedException();
		}

		#endregion // public メソッド

	}
}
