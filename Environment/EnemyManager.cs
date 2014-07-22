using System;
using System.Collections.Generic;
using System.Windows;
using Legion.Entity;
using Legion.Entity.Unit;
using Legion.Entity.Unit.Enemy;
using Legion.Entity.Weapon.Ammo;
using Legion.Util;

namespace Legion.Environment
{

	/// <summary>
	/// 敵の状態を管理します。
	/// </summary>
	public class EnemyManager
	{

		#region フィールド

		private static readonly EnemyManager instance = new EnemyManager();

		// private DateTime lastGenerateTime = DateTime.MinValue;
		private int randomSeed;
		private Random randomX;
		private Random randomY;

		/// <summary>
		/// 敵の出現間隔の最小値
		/// </summary>
		private double chargeTimeMin;

		/// <summary>
		/// 敵の出現間隔の最大値
		/// </summary>
		private double chargeTimeMax;

		private double elapsedFromLastGenerate;

		/// <summary>
		/// レベル毎の設定のマップ
		/// </summary>
		private Dictionary<int, LevelRelatedContainer> enemyLevelMap;

		/// <summary>
		/// 削除対象にマークした敵のキュー。
		/// </summary>
		private Queue<IEnemy> removeEnemyQueue = new Queue<IEnemy>();

		#endregion // フィールド

		#region コンストラクタ

		private EnemyManager()
		{
			this.randomSeed = DateTime.Now.Minute * DateTime.Now.Second;
			this.randomX = new Random(randomSeed);
			this.randomY = new Random(randomSeed + DateTime.Now.Millisecond);

			this.EnemyList = new LinkedList<IEnemy>();
			this.ShowEnemyQueue = new Queue<IEnemy>();
			this.HideEnemyQueue = new Queue<IEnemy>();

			// レベル毎の設定マップの作成
			this.CreateEnemyLevelMap();

			var container = this.enemyLevelMap[1];
			this.chargeTimeMin = container.ChargeTimeMin;
			this.chargeTimeMax = container.ChargeTimeMax;
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static EnemyManager Instance
		{
			get { return instance; }
		}

		/// <summary>
		/// 表示する敵のキューを取得します。
		/// </summary>
		public Queue<IEnemy> ShowEnemyQueue { get; private set; }

		/// <summary>
		/// 非表示にする敵のキューを取得します。
		/// </summary>
		public Queue<IEnemy> HideEnemyQueue { get; private set; }

		/// <summary>
		/// 敵のマップを取得します。
		/// </summary>
		public LinkedList<IEnemy> EnemyList { get; private set; }

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.EnemyList.Clear();
			this.ShowEnemyQueue.Clear();
			this.HideEnemyQueue.Clear();

			var container = this.enemyLevelMap[1];
			this.chargeTimeMin = container.ChargeTimeMin;
			this.chargeTimeMax = container.ChargeTimeMax;
			this.elapsedFromLastGenerate = 0;
		}

		public void LevelUp(int level)
		{
			var container = this.enemyLevelMap[level];
			this.chargeTimeMin = container.ChargeTimeMin;
			this.chargeTimeMax = container.ChargeTimeMax;

			MessageManager.Instance.SystemMessageQueue.Enqueue("Enemy legion has trained more.");
			MessageManager.Instance.SystemMessageQueue.Enqueue(String.Format("Now, their level is [{0}].", level));
		}

		public void GenerateEnemy()
		{
			// lock (this)
			{
				bool generate = false;

				if (0 < this.EnemyList.Count)
				{
					if (this.chargeTimeMax < this.elapsedFromLastGenerate)
					{
						generate = true;
					}
				}
				else
				{
					if (this.chargeTimeMin < this.elapsedFromLastGenerate)
					{
						generate = true;
					}
				}

				if (generate)
				{
					this.enemyLevelMap[GrowthManager.Instance.EnemyGrowth.Level].GenerateEnemy();
					this.elapsedFromLastGenerate = 0;
				}
			}
		}

		public void RegisterEnemy(IEnemy enemy)
		{
			if (!this.EnemyList.Contains(enemy))
			{
				this.EnemyList.AddLast(enemy);
				this.ShowEnemyQueue.Enqueue(enemy);
			}
		}

		public void ChangeEnemyStatus(IEnemy enemy)
		{
			if (this.EnemyList.Contains(enemy))
			{
				switch (enemy.Status)
				{
					case EntityStatus.Standby:
						break;
					case EntityStatus.Active:
						break;
					case EntityStatus.Deactivated:
						break;
					case EntityStatus.Deletable:
						this.HideEnemyQueue.Enqueue(enemy);

						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// 対象の敵を削除対象としてマークします。
		/// </summary>
		/// <param name="enemy"></param>
		public void RemoveEnemy(IEnemy enemy)
		{
			if (this.EnemyList.Contains(enemy))
			{
				this.removeEnemyQueue.Enqueue(enemy);
			}
		}

		/// <summary>
		/// 削除対象にマークした敵の削除を実行します。
		/// </summary>
		public void CommitRemove()
		{
			int removeEnemyCount = this.removeEnemyQueue.Count;
			while (0 < this.removeEnemyQueue.Count)
			{
				var item = this.removeEnemyQueue.Dequeue();
				this.EnemyList.Remove(item);
			}

			// 敵が存在せず、かつ、敵の削除が行われた場合のみ
			if (this.EnemyList.Count <= 0 && 0 < removeEnemyCount)
			{

				// 再出現の早回し用に生成時刻をリセット
				// this.lastGenerateTime = DateTime.Now;
			}
		}

		public void UpdateFrame()
		{
			this.elapsedFromLastGenerate += GameSettings.ElapsedTime;
		}

		#endregion // public メソッド

		#region private メソッド

		private Point GetRandomPoint()
		{
			double x = randomX.Next(800, 900);
			double y = randomY.Next(0, 400);

			return new Point(x, y);
		}

		/// <summary>
		/// レベル毎の設定マップの作成を行います。
		/// </summary>
		private void CreateEnemyLevelMap()
		{
			this.enemyLevelMap = new Dictionary<int, LevelRelatedContainer>()
			{
				{1, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 2.0,
						ChargeTimeMax = 10.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(2, 1);
						}
					}
				},

				{2, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 2.0,
						ChargeTimeMax = 9.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(3, 1);
						}
					}
				},

				{3, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 2.0,
						ChargeTimeMax = 8.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(2, 2);
						}
					}
				},

				{4, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 2.0,
						ChargeTimeMax = 7.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(3, 2);
						}
					}
				},

				{5, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 2.0,
						ChargeTimeMax = 6.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(3, 3);
						}
					}
				},

				{6, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 1.0,
						ChargeTimeMax = 5.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(3, 4);
						}
					}
				},

				{7, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 1.0,
						ChargeTimeMax = 4.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(4, 5);
						}
					}
				},

				{8, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 1.0,
						ChargeTimeMax = 3.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(4, 6);
						}
					}
				},

				{9, new	LevelRelatedContainer()
					{
						ChargeTimeMin = 0,
						ChargeTimeMax = 2.0,
						GenerateEnemy = () =>
						{
							this.GenerateVelites(6, 6);
						}
					}
				},
			};
		}

		private void GenerateVelites(int groupCount, int unitCount)
		{
			int availableCount = EntityViewManager.Instance.GetAvailableViewCount(Entity.View.ViewType.Velites);

			int groupIndex = 0;
			while (groupIndex < groupCount)
			{
				availableCount -= unitCount;
				if (availableCount < 0)
				{
					return;
				}

				var point = this.GetRandomPoint();

				int unitIndex = 0;
				while (unitIndex < unitCount)
				{
					var enemy = new Velites();
					var position = new Point(point.X, (point.Y + (unitIndex * 40)));
					var view = EntityViewManager.Instance.GetView(Entity.View.ViewType.Velites);

					enemy.Activate(position, view);

					unitIndex++;
				}

				groupIndex++;
			}
		}

		#endregion // private メソッド

		/// <summary>
		/// レベルに関連する事項を格納するコンテナです。
		/// </summary>
		private class LevelRelatedContainer
		{

			/// <summary>
			/// 敵の出現間隔の最小値
			/// </summary>
			public double ChargeTimeMin { get; set; }

			/// <summary>
			/// 敵の出現間隔の最大値
			/// </summary>
			public double ChargeTimeMax { get; set; }

			/// <summary>
			/// 敵の出現処理
			/// </summary>
			public Action GenerateEnemy { get; set; }
		}

	}
}
