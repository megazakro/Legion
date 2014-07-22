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
	/// 得点の状態を管理します。
	/// </summary>
	public class ScoreManager
	{

		#region フィールド

		private static readonly ScoreManager instance = new ScoreManager();

		#endregion // フィールド

		#region コンストラクタ

		private ScoreManager()
		{
			this.Score = 0;
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static ScoreManager Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// 現在のスコアを取得します。
		/// </summary>
		public long Score { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.Score = 0;
		}

		/// <summary>
		/// スコアを加算します。
		/// </summary>
		/// <param name="exp">スコア</param>
		public void AddScore(long score)
		{
			this.Score += score;
		}

		#endregion // public メソッド

		#region private メソッド

		#endregion // private メソッド

	}
}
