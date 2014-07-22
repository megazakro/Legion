using System;
using System.Collections.Generic;
using Legion.Entity;
using Legion.Entity.Weapon.Ammo;

namespace Legion.Environment
{

	/// <summary>
	/// 画面上の弾の状態を管理します。
	/// </summary>
	public class AmmoManager
	{

		#region フィールド

		private static readonly AmmoManager instance = new AmmoManager();

		/// <summary>
		/// 削除対象にマークした弾のキュー。
		/// </summary>
		private Queue<IAmmo> removeAmmoQueue = new Queue<IAmmo>();

		#endregion // フィールド

		#region コンストラクタ

		private AmmoManager()
		{
			this.AmmoList = new LinkedList<IAmmo>();
			this.ShowAmmoQueue = new Queue<IAmmo>();
			this.HideAmmoQueue = new Queue<IAmmo>();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static AmmoManager Instance
		{
			get { return instance; }
		}

		public Queue<IAmmo> ShowAmmoQueue { get; private set; }

		public Queue<IAmmo> HideAmmoQueue { get; private set; }

		public LinkedList<IAmmo> AmmoList { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.removeAmmoQueue.Clear();
			this.ShowAmmoQueue.Clear();
			this.HideAmmoQueue.Clear();
			this.AmmoList.Clear();
		}

		public void RegisterAmmo(IAmmo ammo, EntityStatus status)
		{
			if (!this.AmmoList.Contains(ammo))
			{
				this.AmmoList.AddLast(ammo);
				this.ShowAmmoQueue.Enqueue(ammo);
			}
		}

		public void ChangeAmmoStatus(IAmmo ammo)
		{
			if (this.AmmoList.Contains(ammo))
			{
				switch (ammo.Status)
				{
					case EntityStatus.Standby:
						break;
					case EntityStatus.Active:
						break;
					case EntityStatus.Deactivated:
						break;
					case EntityStatus.Deletable:
						if (!this.HideAmmoQueue.Contains(ammo))
						{
							this.HideAmmoQueue.Enqueue(ammo);
						}

						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// 対象の弾を削除対象としてマークします。
		/// </summary>
		/// <param name="ammo">削除対象の弾</param>
		public void RemoveAmmo(IAmmo ammo)
		{
			if (this.AmmoList.Contains(ammo))
			{
				this.removeAmmoQueue.Enqueue(ammo);
			}
		}

		/// <summary>
		/// 削除対象にマークした弾の削除を実行します。
		/// </summary>
		public void CommitRemove()
		{
			while (0 < this.removeAmmoQueue.Count)
			{
				var item = this.removeAmmoQueue.Dequeue();
				this.AmmoList.Remove(item);
			}
		}

		#endregion // public メソッド

	}
}
