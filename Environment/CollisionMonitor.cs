using System;
using System.Collections.Generic;
using Legion.Entity.Unit;
using Legion.Entity.Weapon.Ammo;
using Legion.Entity.Unit.Enemy;
using System.Windows;
using Legion.Util;

namespace Legion.Environment
{

	/// <summary>
	/// オブジェクトの衝突判定を監視します。
	/// </summary>
	public class CollisionMonitor
	{

		#region 定数

		/// <summary>
		/// エリアグリッドの行数
		/// </summary>
		private const int AreaGridRowCount = 2;

		/// <summary>
		/// エリアグリッドの列数
		/// </summary>
		private const int AreaGridColCount = 4;

		/// <summary>
		/// エリアの幅
		/// </summary>
		private const int AreaWidth = 200;

		/// <summary>
		/// エリアの高さ
		/// </summary>
		private const int AreaHeight = 200;

		#endregion // 定数

		#region フィールド

		private static readonly CollisionMonitor instance = new CollisionMonitor();

		/// <summary>
		/// エリアグリッド
		/// </summary>
		private Area[,] areaGrid;

		#endregion // フィールド

		#region コンストラクタ

		private CollisionMonitor()
		{
			this.InitializeAreaGrid();

			this.UpdateAroundArea();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static CollisionMonitor Instance
		{
			get
			{
				return instance;
			}
		}

		#endregion // プロパティ

		#region public メソッド

		public void Initialieze()
		{
			foreach (var area in this.areaGrid)
			{
				area.CollisionOccurred = false;
				area.EnemyAmmoList.Clear();
				area.EnemyList.Clear();
				area.Player = null;
				area.PlayerAmmoList.Clear();
			}
		}

		#region オブジェクト登録

		public void RegisterPlayerAmmo(IAmmo ammo)
		{
			this.RemovePlayerAmmo(ammo);

			Action<Area> registerAction = (area) =>
			{
				if (!area.PlayerAmmoList.Contains(ammo))
				{
					area.PlayerAmmoList.Add(ammo);
				}
			};

			this.RegisterObject(ammo.GetFatalArea(), registerAction);
		}

		public void RegisterEnemyAmmo(IAmmo ammo)
		{
			this.RemoveEnemyAmmo(ammo);

			Action<Area> registerAction = (area) =>
			{
				if (!area.EnemyAmmoList.Contains(ammo))
				{
					area.EnemyAmmoList.Add(ammo);
				}
			};

			this.RegisterObject(ammo.GetFatalArea(), registerAction);
		}

		public void RegisterEnemy(IEnemy enemy)
		{
			this.RemoveEnemy(enemy);

			Action<Area> registerAction = (area) =>
			{
				if (!area.EnemyList.Contains(enemy))
				{
					area.EnemyList.Add(enemy);
				}
			};

			this.RegisterObject(enemy.GetFatalArea(), registerAction);
		}

		public void RegisterPlayer(IUnit player)
		{
			foreach (var area in this.areaGrid)
			{
				area.Player = null;
			}

			Action<Area> registerAction = (area) =>
			{
				area.Player = player;
			};

			this.RegisterObject(player.GetFatalArea(), registerAction);
		}

		public void Clear()
		{
			this.InitializeAreaGrid();

			this.UpdateAroundArea();
		}

		#endregion // オブジェクト登録

		#region オブジェクト削除

		/// <summary>
		/// 登録された弾オブジェクトを削除します。
		/// </summary>
		/// <param name="ammo"></param>
		public void RemoveAmmo(IAmmo ammo)
		{
			foreach (var area in this.areaGrid)
			{
				if (area.PlayerAmmoList.Contains(ammo))
				{
					area.PlayerAmmoList.Remove(ammo);
				}

				if (area.EnemyAmmoList.Contains(ammo))
				{
					area.EnemyAmmoList.Remove(ammo);
				}
			}
		}

		public void RemovePlayerAmmo(IAmmo ammo)
		{
			foreach (var area in this.areaGrid)
			{
				if (area.PlayerAmmoList.Contains(ammo))
				{
					area.PlayerAmmoList.Remove(ammo);
				}
			}
		}

		public void RemoveEnemyAmmo(IAmmo ammo)
		{
			foreach (var area in this.areaGrid)
			{
				if (area.PlayerAmmoList.Contains(ammo))
				{
					area.PlayerAmmoList.Remove(ammo);
				}
			}
		}

		public void RemoveEnemy(IEnemy enemy)
		{
			foreach (var area in this.areaGrid)
			{
				if (area.EnemyList.Contains(enemy))
				{
					area.EnemyList.Remove(enemy);
				}
			}
		}

		#endregion // オブジェクト削除

		public Area[,] GetAreaGrid()
		{
			return this.areaGrid;
		}

		/// <summary>
		/// 当たり判定の評価を実行します。
		/// </summary>
		public void EvaluateCollision()
		{
			foreach (var area in this.areaGrid)
			{
				area.CollisionOccurred = false;

				if (null != area.Player)
				{

					#region 自機と敵弾
					{
						foreach (var ammo in area.EnemyAmmoList)
						{
							if (this.EvaluateCollision(area.Player, ammo))
							{
								area.CollisionOccurred = true;

								area.Player.RegisterCollidedEntity(ammo);
								ammo.RegisterCollidedEntity(area.Player);
							}
						}
					}
					#endregion // 自機と敵弾

					#region 自機と敵
					{
						var collidedEnemy = new List<IEnemy>();

						foreach (var enemy in area.EnemyList)
						{
							if (this.EvaluateCollision(area.Player, enemy))
							{
								area.CollisionOccurred = true;

								area.Player.RegisterCollidedEntity(enemy);
								enemy.RegisterCollidedEntity(area.Player);
							}
						}
					}
					#endregion // 自機と敵

				}

				if (0 < area.PlayerAmmoList.Count &&
					0 < area.EnemyList.Count)
				{

					#region 自弾と敵
					{
						foreach (var ammo in area.PlayerAmmoList)
						{
							foreach (var enemy in area.EnemyList)
							{
								if (this.EvaluateCollision(enemy, ammo))
								{
									area.CollisionOccurred = true;

									enemy.RegisterCollidedEntity(ammo);
									ammo.RegisterCollidedEntity(enemy);
								}
							}
						}
					}
					#endregion // 自弾と敵

				}
			}

			// 衝突結果の解決
			{
				foreach (var area in this.areaGrid)
				{
					if (!area.CollisionOccurred)
					{
						continue;
					}

					if (null != area.Player)
					{
						area.Player.DetectedCollision();
					}

					foreach (var item in area.PlayerAmmoList)
					{
						item.DetectedCollision();
					}

					foreach (var item in area.EnemyList)
					{
						item.DetectedCollision();
					}

					foreach (var item in area.EnemyAmmoList)
					{
						item.DetectedCollision();
					}
				}
			}
		}

		#endregion // public メソッド

		#region private メソッド

		/// <summary>
		/// エリアグリッドにオブジェクトを登録します。
		/// </summary>
		/// <param name="fatalArea">登録オブジェクトの当たり判定領域</param>
		/// <param name="registerAction">オブジェクト登録時の動作</param>
		private void RegisterObject(FatalArea fatalArea, Action<Area> registerAction)
		{
			var leftTop = fatalArea.leftTop;
			var rightBottom = fatalArea.rightBottom;


			// エリアの設定（始点）
			int leftTopAreaRow = 0;
			int leftTopAreaCol = 0;
			{
				while ((leftTopAreaRow + 1) * AreaHeight <= leftTop.Y)
				{
					leftTopAreaRow++;
					if (AreaGridRowCount == leftTopAreaRow)
					{
						leftTopAreaRow = AreaGridRowCount - 1;
						break;
					}
				}

				while ((leftTopAreaCol + 1) * AreaWidth <= leftTop.X)
				{
					leftTopAreaCol++;
					if (AreaGridColCount == leftTopAreaCol)
					{
						leftTopAreaCol = AreaGridColCount - 1;
						break;
					}

				}

				registerAction(this.areaGrid[leftTopAreaRow, leftTopAreaCol]);
			}

			// エリアの設定（～終点）
			{
				int areaRow = leftTopAreaRow;
				while ((areaRow * AreaHeight) < rightBottom.Y)
				{
					int areaCol = leftTopAreaCol;
					while ((areaCol * AreaWidth) < rightBottom.X)
					{
						registerAction(this.areaGrid[areaRow, areaCol]);

						areaCol++;
						if (AreaGridColCount == areaCol)
						{
							break;
						}
					}

					areaRow++;
					if (AreaGridRowCount == areaRow)
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// エリアグリッドの初期化を行います。
		/// </summary>
		private void InitializeAreaGrid()
		{
			this.areaGrid = new Area[AreaGridRowCount, AreaGridColCount];

			int row = 0;
			while (row < AreaGridRowCount)
			{
				int col = 0;
				while (col < AreaGridColCount)
				{
					this.areaGrid[row, col] = new Area(new Point()
						{
							X = row * AreaHeight,
							Y = col * AreaWidth,
						});

					col++;
				}

				row++;
			}
		}

		/// <summary>
		/// 隣接エリアの設定を行います。
		/// </summary>
		private void UpdateAroundArea()
		{
			int row = 0;
			while (row < AreaGridRowCount)
			{
				int col = 0;
				while (col < AreaGridColCount)
				{
					var area = this.areaGrid[row, col];

					if (0 == row)
					{

						#region 最上段

						// 左端
						if (0 == col)
						{
							area.AroundArea = new List<Area>()
							{

								// 右
								this.areaGrid[0, 1],

								// 右斜め下
								this.areaGrid[1, 1],

								// 下
								this.areaGrid[1, 0],
							};
						}
						// 右端
						else if ((AreaGridColCount - 1) == col)
						{
							area.AroundArea = new List<Area>()
							{

								// 下
								this.areaGrid[1, col],

								// 左斜下
								this.areaGrid[1, col - 1],

								// 左
								this.areaGrid[0, col -1],
							};
						}
						else
						{
							area.AroundArea = new List<Area>()
							{

								// 右
								this.areaGrid[0, col + 1],

								// 右斜め下
								this.areaGrid[1, col + 1],

								// 下
								this.areaGrid[1, col],

								// 左斜下
								this.areaGrid[1, col - 1],

								// 左
								this.areaGrid[0, col - 1],
							};

						}

						#endregion // 最上段

					}
					else if ((AreaGridRowCount - 1) == row)
					{

						#region 最下段

						// 左端
						if (0 == col)
						{
							area.AroundArea = new List<Area>()
							{

								// 上
								this.areaGrid[row - 1, col],

								// 右斜め上
								this.areaGrid[row - 1, col + 1],

								// 右
								this.areaGrid[row, col + 1],
							};
						}
						// 右端
						else if ((AreaGridColCount - 1) == col)
						{
							area.AroundArea = new List<Area>()
							{

								// 左
								this.areaGrid[row, col -1],

								// 左斜め上
								this.areaGrid[row - 1, col -1],

								// 上
								this.areaGrid[row - 1, col],
							};
						}
						else
						{
							area.AroundArea = new List<Area>()
							{

								// 左
								this.areaGrid[row, col -1],

								// 左斜め上
								this.areaGrid[row - 1, col -1],

								// 上
								this.areaGrid[row - 1, col],

								// 右斜め上
								this.areaGrid[row - 1, col + 1],

								// 右
								this.areaGrid[row, col + 1],
							};

						}

						#endregion // 最下段

					}
					else
					{

						#region 中段

						// 左端
						if (0 == col)
						{
							area.AroundArea = new List<Area>()
							{

								// 上
								this.areaGrid[row - 1, col],

								// 右斜め上
								this.areaGrid[row - 1, col + 1],

								// 右
								this.areaGrid[row, col + 1],

								// 右斜め下
								this.areaGrid[row + 1, col + 1],

								// 下
								this.areaGrid[row + 1, col],
							};
						}
						// 右端
						else if ((AreaGridColCount - 1) == col)
						{
							area.AroundArea = new List<Area>()
							{
								
								// 下
								this.areaGrid[row + 1, col],

								// 左斜め下
								this.areaGrid[row + 1, col - 1],

								// 左
								this.areaGrid[row, col -1],

								// 左斜め上
								this.areaGrid[row - 1, col -1],

								// 上
								this.areaGrid[row - 1, col],
							};
						}
						else
						{
							area.AroundArea = new List<Area>()
							{

								// 上
								this.areaGrid[row - 1, col],

								// 右斜め上
								this.areaGrid[row - 1, col + 1],

								// 右
								this.areaGrid[row, col + 1],

								// 右斜め下
								this.areaGrid[row + 1, col + 1],

								// 下
								this.areaGrid[row + 1, col],

								// 左斜め下
								this.areaGrid[row + 1, col - 1],

								// 左
								this.areaGrid[row, col -1],

								// 左斜め上
								this.areaGrid[row - 1, col -1],
							};

						}

						#endregion // 中段

					}

					col++;
				}

				row++;
			}
		}

		/// <summary>
		/// 自機と敵弾の衝突判定
		/// </summary>
		/// <param name="player">自機</param>
		/// <param name="ammo">敵弾</param>
		/// <returns>衝突が発生した場合、true。そうでない場合はfalse。</returns>
		private bool EvaluateCollision(IUnit player, IAmmo ammo)
		{
			return this.EvaluateCollisionBase(player.GetFatalArea(), ammo.GetFatalArea());
		}

		/// <summary>
		/// 自機と敵の衝突判定
		/// </summary>
		/// <param name="player">自機</param>
		/// <param name="enemy">敵</param>
		/// <returns>衝突が発生した場合、true。そうでない場合はfalse。</returns>
		private bool EvaluateCollision(IUnit player, IEnemy enemy)
		{
			return this.EvaluateCollisionBase(player.GetFatalArea(), enemy.GetFatalArea());
		}

		/// <summary>
		/// 衝突判定の基本ロジック
		/// </summary>
		/// <param name="target1">衝突検知対象1</param>
		/// <param name="target2">衝突検知対象2</param>
		/// <returns>衝突が発生した場合、true。そうでない場合はfalse。</returns>
		private bool EvaluateCollisionBase(FatalArea target1, FatalArea target2)
		{
			if (target2.leftTop.X < target1.rightBottom.X &&
				target1.leftTop.X < target2.rightBottom.X &&
				target2.leftTop.Y < target1.rightBottom.Y &&
				target1.leftTop.Y < target2.rightBottom.Y)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#endregion // private メソッド

		private class AmmoUnitPair
		{
			public AmmoUnitPair(IAmmo ammo, IUnit unit)
			{
				this.Ammo = ammo;
				this.Unit = unit;
			}

			public IAmmo Ammo { get; set; }
			public IUnit Unit { get; set; }
		}
	}

	/// <summary>
	/// 画面を分割したエリアを表します。
	/// </summary>
	public class Area
	{

		#region コンストラクタ

		public Area(Point origin)
		{
			this.PlayerAmmoList = new List<IAmmo>();
			this.EnemyAmmoList = new List<IAmmo>();
			this.EnemyList = new List<IEnemy>();
			this.Player = null;
			this.AroundArea = null;

			this.Origin = origin;
		}

		#endregion // コンストラクタ

		#region プロパティ

		/// <summary>
		/// エリアに存在する自弾のリストを取得します。
		/// </summary>
		public List<IAmmo> PlayerAmmoList { get; private set; }

		/// <summary>
		/// エリアに存在する敵弾のリストを取得します。
		/// </summary>
		public List<IAmmo> EnemyAmmoList { get; private set; }

		/// <summary>
		/// エリアに存在する敵のリストを取得します。
		/// </summary>
		public List<IEnemy> EnemyList { get; private set; }

		/// <summary>
		/// エリアに存在する自機を取得または設定します。
		/// </summary>
		public IUnit Player { get; set; }

		/// <summary>
		/// このエリアの隣接エリアを取得または設定します。
		/// </summary>
		public List<Area> AroundArea { get; set; }

		/// <summary>
		/// このエリアの開始座標を取得します。
		/// </summary>
		public Point Origin { get; private set; }

		/// <summary>
		/// このエリアで衝突が発生したかを示す値を取得または設定します。
		/// </summary>
		public bool CollisionOccurred { get; set; }

		#endregion // プロパティ

	}
}
