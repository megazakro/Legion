using System;
using System.Collections.Generic;
using Legion.Entity.Weapon.Ammo;
using Legion.Entity.Unit;
using Legion.Entity.Unit.Enemy;
using Legion.Util;
using Legion.Entity;

namespace Legion.Environment
{

	/// <summary>
	/// 自機の状態を管理します。
	/// </summary>
	public class PlayerManager
	{

		#region フィールド

		private static readonly PlayerManager instance = new PlayerManager();

		#endregion // フィールド

		#region コンストラクタ

		private PlayerManager()
		{
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static PlayerManager Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// 自機のレベルを取得または設定します。
		/// </summary>
		public IUnit Player { get; set; }

		#endregion // プロパティ

		#region public メソッド


		#endregion // public メソッド

	}
}
