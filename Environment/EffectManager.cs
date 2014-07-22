using System;
using System.Collections.Generic;
using Legion.Entity;
using Legion.Entity.Weapon.Ammo;
using Legion.Entity.Effect;
using Legion.Util;

namespace Legion.Environment
{

	/// <summary>
	/// 画面上のエフェクトの状態を管理します。
	/// </summary>
	public class EffectManager
	{

		#region フィールド

		private static readonly EffectManager instance = new EffectManager();

		/// <summary>
		/// 削除対象にマークしたエフェクトのキュー。
		/// </summary>
		private Queue<IEffect> removeEffectQueue = new Queue<IEffect>();

		#endregion // フィールド

		#region コンストラクタ

		private EffectManager()
		{
			this.EffectList = new LinkedList<IEffect>();
			this.ShowEffectQueue = new Queue<IEffect>();
			this.HideEffectQueue = new Queue<IEffect>();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static EffectManager Instance
		{
			get { return instance; }
		}

		public Queue<IEffect> ShowEffectQueue { get; private set; }

		public Queue<IEffect> HideEffectQueue { get; private set; }

		public LinkedList<IEffect> EffectList { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.removeEffectQueue.Clear();
			this.ShowEffectQueue.Clear();
			this.HideEffectQueue.Clear();
			this.EffectList.Clear();
		}

		public void RegisterEffect(IEffect effect)
		{
			if (!this.EffectList.Contains(effect))
			{
				this.EffectList.AddLast(effect);
				this.ShowEffectQueue.Enqueue(effect);
			}
		}

		public void ChangeEffectStatus(IEffect effect)
		{
			if (this.EffectList.Contains(effect))
			{
				switch (effect.Status)
				{
					case EntityStatus.Standby:
						break;
					case EntityStatus.Active:
						break;
					case EntityStatus.Deactivated:
						break;
					case EntityStatus.Deletable:
						if (!this.HideEffectQueue.Contains(effect))
						{
							this.HideEffectQueue.Enqueue(effect);
						}

						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// 対象のエフェクトを削除対象としてマークします。
		/// </summary>
		/// <param name="ammo">削除対象のエフェクト</param>
		public void RemoveEffect(IEffect effect)
		{
			if (this.EffectList.Contains(effect))
			{
				this.removeEffectQueue.Enqueue(effect);
			}
		}

		/// <summary>
		/// 削除対象にマークしたエフェクトの削除を実行します。
		/// </summary>
		public void CommitRemove()
		{
			while (0 < this.removeEffectQueue.Count)
			{
				var item = this.removeEffectQueue.Dequeue();
				this.EffectList.Remove(item);
			}
		}

		#endregion // public メソッド

	}
}
