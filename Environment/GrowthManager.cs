using System;
using System.Collections.Generic;
using Legion.Util;
using Legion.Entity;

namespace Legion.Environment
{

	/// <summary>
	/// 成長要素を管理します。
	/// </summary>
	public class GrowthManager
	{

		#region フィールド

		private static readonly GrowthManager instance = new GrowthManager();

		#endregion // フィールド

		#region コンストラクタ

		private GrowthManager()
		{
			this.Initialize();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static GrowthManager Instance
		{
			get { return instance; }
		}

		public Growth ArmorGrowth { get; private set; }

		public Growth LaserGrowth { get; private set; }

		public Growth GunGrowth { get; private set; }

		public Growth MissileGrowth { get; private set; }

		public Growth EnemyGrowth { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			{
				this.ArmorGrowth = new Growth(0, 0, 9999);
				this.LaserGrowth = new Growth(0, 0, 9999);
				this.GunGrowth = new Growth(0, 0, 9999);
				this.MissileGrowth = new Growth(0, 0, 9999);

				var nextExpMap = new Dictionary<int, long>()
				{
					{1, 50},
					{2, 200},
					{3, 700},
					{4, 1500},
					{5, 2500},
					{6, 4500},
					{7, 7000},
					{8, 9999},
					{9, 9999},
				};

				Action<Growth> initGrowth = (growth) =>
				{
					growth.NextExpMap = nextExpMap;
					growth.LevelUp();
				};

				initGrowth(this.ArmorGrowth);
				initGrowth(this.LaserGrowth);
				initGrowth(this.GunGrowth);
				initGrowth(this.MissileGrowth);
			}

			{

				// スコアにより敵のレベルを決定
				var nextExpMap = new Dictionary<int, long>()
				{
					{1, 3000},
					{2, 10000},
					{3, 20000},
					{4, 35000},
					{5, 50000},
					{6, 75000},
					{7, 100000},
					{8, 150000},
					{9, 999999},
				};

				this.EnemyGrowth = new Growth(0, 0, 999999) { NextExpMap = nextExpMap };
				this.EnemyGrowth.LevelUp();
			}
		}

		/// <summary>
		/// 各成長要素のレベルアップ時の挙動を登録します。
		/// </summary>
		public void RegisterOnLevelUp()
		{
			this.ArmorGrowth.OnLevelUp = () =>
			{
				PlayerManager.Instance.Player.LevelUp(DamageSourceType.Armor, this.ArmorGrowth.Level);
			};

			this.LaserGrowth.OnLevelUp = () =>
			{
				PlayerManager.Instance.Player.LevelUp(DamageSourceType.Laser, this.LaserGrowth.Level);
			};

			this.GunGrowth.OnLevelUp = () =>
			{
				PlayerManager.Instance.Player.LevelUp(DamageSourceType.Gun, this.GunGrowth.Level);
			};

			this.MissileGrowth.OnLevelUp = () =>
			{
				PlayerManager.Instance.Player.LevelUp(DamageSourceType.Missile, this.MissileGrowth.Level);
			};

			this.EnemyGrowth.OnLevelUp = () =>
			{
				EnemyManager.Instance.LevelUp(this.EnemyGrowth.Level);
			};
		}

		#endregion // public メソッド

	}
}
