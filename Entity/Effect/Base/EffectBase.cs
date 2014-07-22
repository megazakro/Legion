using System;
using System.Windows;
using System.Windows.Controls;
using Legion.Environment;
using System.Collections.Generic;
using Legion.Util;

namespace Legion.Entity.Effect.Base
{
	public class EffectBase : IEffect
	{

		#region フィールド

		#endregion // フィールド

		#region プロパティ

		public double Velocity { get; set; }

		public Vector2D Heading { get; set; }

		public Vector2D CurrentVector { get; set; }

		public Point Origin { get; protected set; }

		public DamageSourceType DamageSourceType
		{
			get
			{
				return Entity.DamageSourceType.None;
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
			EffectManager.Instance.RegisterEffect(this);
			this.Origin = origin;
		}

		public virtual void Deactivate()
		{
			EffectManager.Instance.ChangeEffectStatus(this);
		}

		public virtual void AdjustPosition(Point position)
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

		#region 実装されないメソッド

		/// <summary>
		/// 実装されません。
		/// </summary>
		/// <returns></returns>
		public FatalArea GetFatalArea()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 実装されません。
		/// </summary>
		/// <returns></returns>
		public Point GetFatalAreaCenter()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 実装されません。
		/// </summary>
		public void DetectedCollision()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 実装されません。
		/// </summary>
		/// <param name="unit"></param>
		public void RegisterCollidedEntity(Unit.IUnit unit)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 実装されません。
		/// </summary>
		/// <param name="unit"></param>
		public void RegisterCollidedEntity(Weapon.Ammo.IAmmo unit)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// 実装されません。
		/// </summary>
		/// <param name="damageSourceType"></param>
		/// <param name="level"></param>
		public void LevelUp(DamageSourceType damageSourceType, int level)
		{
			throw new NotImplementedException();
		}

		#endregion // 実装されないメソッド

		#endregion // public メソッド

	}
}
