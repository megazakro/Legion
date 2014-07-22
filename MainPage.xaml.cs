using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Legion.Entity;
using Legion.Entity.Unit;
using Legion.Entity.Unit.Enemy;
using Legion.Entity.Weapon.Ammo;
using Legion.Entity.Weapon.Ammo.Base;
using Legion.Environment;
using Legion.Util;
using Legion.Entity.View;
using System.Threading;
using System.IO.IsolatedStorage;

namespace Legion
{
	public partial class MainPage : UserControl
	{

		#region enum

		private enum GameStatus
		{
			Standby,

			Active,

			Over,
		}

		private enum GameMode
		{
			TimeAttack,
			Survival,
		}

		#endregion // enum

		#region 定数

		private const int EntityLimitLeft = -200;
		private const int EntityLimitTop = -200;
		private const int EntityLimitRight = GameSettings.WindowWidth + 200;
		private const int EntityLimitBottom = GameSettings.WindowHeight + 200;

		#endregion // 定数

		#region フィールド

		private Player player;

		// サバイバルモード時の時間制限（5分）
		private int LimitTime = 300000;

		private double totalElapsedTime;

		private uint lastTick;

		private GameMode currentGameMode;

		private IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;

		#endregion // フィールド

		#region コンストラクタ

		public MainPage()
		{
			InitializeComponent();

			this.SetUp();
		}

		#endregion // コンストラクタ

		#region イベント

		private void CompositionTarget_Rendering(object sender, EventArgs e)
		{

			#region FPS制御
			{
				int currentTick = System.Environment.TickCount;
				if (currentTick < 0)
				{
					currentTick += Int32.MaxValue;
				}

				// 経過時間の設定
				GameSettings.ElapsedTime = (currentTick - lastTick) / 1000f;
				if (0 == GameSettings.ElapsedTime)
				{
					return;
				}

				if (0.016f < GameSettings.ElapsedTime)
				{
					GameSettings.ElapsedTime = 0.016f;
				}

				lastTick = (uint)currentTick;
			}
			#endregion // FPS制御

			this.totalElapsedTime += GameSettings.ElapsedTime;

			this.player.UpdateFrame();
			this.player.RefreshView();

			// 敵の出現要求
			EnemyManager.Instance.UpdateFrame();
			EnemyManager.Instance.GenerateEnemy();

			#region 未表示のオブジェクトを表示
			{

				// 未表示の弾を表示
				{
					while (0 < AmmoManager.Instance.ShowAmmoQueue.Count)
					{
						var ammo = AmmoManager.Instance.ShowAmmoQueue.Dequeue();
						ammo.RefreshView();
						ammo.View.Visibility = System.Windows.Visibility.Visible;
					}
				}

				// 未表示の敵を表示
				{
					while (0 < EnemyManager.Instance.ShowEnemyQueue.Count)
					{
						var enemy = EnemyManager.Instance.ShowEnemyQueue.Dequeue();
						enemy.RefreshView();
						enemy.View.Visibility = System.Windows.Visibility.Visible;
					}
				}

				// 未表示のエフェクトを表示
				{
					while (0 < EffectManager.Instance.ShowEffectQueue.Count)
					{
						var effect = EffectManager.Instance.ShowEffectQueue.Dequeue();
						effect.RefreshView();
						effect.View.Visibility = System.Windows.Visibility.Visible;
					}
				}
			}
			#endregion // 未表示のオブジェクトを表示

			#region エンティティの移動
			{
				List<IEntity> deactivateEntities = new List<IEntity>();

				// 敵の移動
				foreach (var enemy in EnemyManager.Instance.EnemyList)
				{
					if (EntityStatus.Active != enemy.Status)
					{
						continue;
					}

					var newPoint = new Point(enemy.Origin.X + enemy.CurrentVector.X,
						enemy.Origin.Y + enemy.CurrentVector.Y);

					if (newPoint.X < EntityLimitLeft || EntityLimitRight < newPoint.X ||
						newPoint.Y < EntityLimitTop || EntityLimitBottom < newPoint.Y)
					{
						deactivateEntities.Add(enemy);
					}
					else
					{
						enemy.UpdateFrame();
						enemy.AdjustPosition(newPoint);
						enemy.RefreshView();
					}
				}

				// 弾の移動
				foreach (var ammo in AmmoManager.Instance.AmmoList)
				{
					if (EntityStatus.Active != ammo.Status)
					{
						continue;
					}

					var newPoint = new Point(ammo.Origin.X + ammo.CurrentVector.X,
						ammo.Origin.Y + ammo.CurrentVector.Y);

					if (newPoint.X < EntityLimitLeft || EntityLimitRight < newPoint.X ||
						newPoint.Y < EntityLimitTop || EntityLimitBottom < newPoint.Y)
					{
						deactivateEntities.Add(ammo);
					}
					else
					{
						ammo.UpdateFrame();
						ammo.AdjustPosition(newPoint);
						ammo.RefreshView();
					}
				}

				foreach (var item in deactivateEntities)
				{
					item.Deactivate();
				}
			}
			#endregion // エンティティの移動

			#region エフェクトのフレーム更新
			{
				foreach (var effect in EffectManager.Instance.EffectList)
				{
					if (EntityStatus.Active != effect.Status)
					{
						continue;
					}

					effect.UpdateFrame();
					effect.RefreshView();
				}
			}
			#endregion // エフェクトのフレーム更新

			// 衝突判定
			CollisionMonitor.Instance.EvaluateCollision();

			#region 不要なオブジェクトを非表示
			{

				// 不要な弾を非表示
				while (0 < AmmoManager.Instance.HideAmmoQueue.Count)
				{
					var ammo = AmmoManager.Instance.HideAmmoQueue.Dequeue();
					ammo.View.Visibility = System.Windows.Visibility.Collapsed;
					EntityViewManager.Instance.FreeView(ammo.ViewType, ammo.View);
				}

				// 不要な敵を非表示
				while (0 < EnemyManager.Instance.HideEnemyQueue.Count)
				{
					var enemy = EnemyManager.Instance.HideEnemyQueue.Dequeue();

					enemy.View.Visibility = System.Windows.Visibility.Collapsed;
					EntityViewManager.Instance.FreeView(enemy.ViewType, enemy.View);
				}

				// 不要なエフェクトを非表示
				while (0 < EffectManager.Instance.HideEffectQueue.Count)
				{
					var effect = EffectManager.Instance.HideEffectQueue.Dequeue();

					effect.View.Visibility = System.Windows.Visibility.Collapsed;
					EntityViewManager.Instance.FreeView(effect.ViewType, effect.View);
				}
			}
			#endregion // 不要なオブジェクトを非表示

			// ダメージをArmorに反映
			{
				List<IEnemy> deactivateItems = new List<IEnemy>();
				foreach (var enemy in EnemyManager.Instance.EnemyList)
				{
					if (EntityStatus.Active != enemy.Status)
					{
						continue;
					}

					enemy.UpdateArmor();
					if (enemy.Armor <= 0)
					{
						deactivateItems.Add(enemy);
					}
				}

				foreach (var item in deactivateItems)
				{
					item.Deactivate();
				}

				this.player.UpdateArmor();
			}

			#region 自機の死亡判定

			if (this.player.Armor <= 0)
			{
				this.GameOver(false);
			}

			#endregion // 自機の死亡判定

			#region タイムオーバー判定

			if (GameMode.TimeAttack == this.currentGameMode)
			{
				if (this.LimitTime < this.totalElapsedTime * 1000)
				{
					this.GameOver(true);
				}
			}

			#endregion // タイムオーバー判定

			#region マネージャーの更新
			{

				// 不要な弾をマネージャーから削除
				foreach (var ammo in AmmoManager.Instance.AmmoList)
				{
					if (EntityStatus.Deletable != ammo.Status)
					{
						continue;
					}

					CollisionMonitor.Instance.RemoveAmmo(ammo);
					AmmoManager.Instance.RemoveAmmo(ammo);
				}
				AmmoManager.Instance.CommitRemove();

				// 不要な敵をマネージャーから削除
				foreach (var enemy in EnemyManager.Instance.EnemyList)
				{
					if (EntityStatus.Deletable != enemy.Status)
					{
						continue;
					}

					CollisionMonitor.Instance.RemoveEnemy(enemy);
					EnemyManager.Instance.RemoveEnemy(enemy);
				}
				EnemyManager.Instance.CommitRemove();
			}
			#endregion // マネージャーの更新

			// 自機の回復
			this.player.RegenerateArmor();

			// コンソールエリアの再描画
			this.RefreshConsole();

			#region システムメッセージの表示

			this.RefreshLog();

			#endregion // システムメッセージの表示

		}

		private void MainPage_KeyDown(object sender, KeyEventArgs e)
		{

			switch (e.Key)
			{
				case Key.W:
					if (null != this.player)
					{
						this.player.DeltaY = -1;
					}

					break;
				case Key.S:
					if (null != this.player)
					{
						this.player.DeltaY = 1;
					}

					break;
				case Key.A:
					if (null != this.player)
					{
						this.player.DeltaX = -1;
					}

					break;
				case Key.D:
					if (null != this.player)
					{
						this.player.DeltaX = 1;
					}

					break;
				case Key.X:
				case Key.P:
					if (null != this.player)
					{
						this.player.ToggleSubWeapon();
					}

					break;
				case Key.Add:
				case Key.K:
					if (null != this.player)
					{
						this.player.SpeedUp();
					}

					break;
				case Key.Subtract:
				case Key.J:
					if (null != this.player)
					{
						this.player.SpeedDown();
					}

					break;
				case Key.Space:
				case Key.Enter:
					if (null != this.player)
					{
						this.player.Fire();
					}

					break;

				case Key.B:
					if (Visibility.Collapsed == this.windowDebug.Visibility)
					{
						this.windowDebug.Visibility = Visibility.Visible;
					}
					else
					{
						this.windowDebug.Visibility = Visibility.Collapsed;
					}

					break;
				case Key.U:
					this.WriteDebug(String.Empty);

					break;
				default:
					break;
			}
		}

		private void MainPage_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.W:
					if (null != this.player)
					{
						if (this.player.DeltaY < 0)
						{
							this.player.DeltaY = 0;
						}
					}

					break;
				case Key.S:
					if (null != this.player)
					{
						if (0 < this.player.DeltaY)
						{
							this.player.DeltaY = 0;
						}
					}

					break;
				case Key.A:
					if (null != this.player)
					{
						if (this.player.DeltaX < 0)
						{
							this.player.DeltaX = 0;
						}
					}

					break;
				case Key.D:
					if (null != this.player)
					{
						if (0 < this.player.DeltaX)
						{
							this.player.DeltaX = 0;
						}
					}

					break;
				case Key.Space:
				case Key.Enter:
					if (null != this.player)
					{
						this.player.CeaseFire();
					}

					break;
				default:
					break;
			}
		}

		#region lblToTitle

		void lblToTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.windowGameOver.Visibility = System.Windows.Visibility.Collapsed;
			this.windowTitle.Visibility = System.Windows.Visibility.Visible;
		}

		void lblToTitle_MouseLeave(object sender, MouseEventArgs e)
		{
			this.backToTitle.Visibility = System.Windows.Visibility.Collapsed;
		}

		void lblToTitle_MouseEnter(object sender, MouseEventArgs e)
		{
			this.backToTitle.Visibility = System.Windows.Visibility.Visible;
		}

		#endregion // lblToTitle

		#region lblToTitle2

		void lblToTitle2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.windowHighScore.Visibility = System.Windows.Visibility.Collapsed;
			this.windowTitle.Visibility = System.Windows.Visibility.Visible;
		}

		void lblToTitle2_MouseLeave(object sender, MouseEventArgs e)
		{
			this.backToTitle2.Visibility = System.Windows.Visibility.Collapsed;
		}

		void lblToTitle2_MouseEnter(object sender, MouseEventArgs e)
		{
			this.backToTitle2.Visibility = System.Windows.Visibility.Visible;
		}

		#endregion // lblToTitle2

		#region lblHighScore

		void lblHighScore_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.ShowHighScore();
		}

		void lblHighScore_MouseLeave(object sender, MouseEventArgs e)
		{
			this.backHighScore.Visibility = System.Windows.Visibility.Collapsed;
		}

		void lblHighScore_MouseEnter(object sender, MouseEventArgs e)
		{
			this.backHighScore.Visibility = System.Windows.Visibility.Visible;
		}

		#endregion // lblHighScore

		#region lblTimeAttack

		void lblTimeAttack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.GameStart(GameMode.TimeAttack);
		}

		void lblTimeAttack_MouseLeave(object sender, MouseEventArgs e)
		{
			this.backTimeAttack.Visibility = System.Windows.Visibility.Collapsed;
		}

		void lblTimeAttack_MouseEnter(object sender, MouseEventArgs e)
		{
			this.backTimeAttack.Visibility = System.Windows.Visibility.Visible;
		}

		#endregion // lblTimeAttack

		#region lblSurvival

		void lblSurvival_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.GameStart(GameMode.Survival);
		}

		void lblSurvival_MouseLeave(object sender, MouseEventArgs e)
		{
			this.backSurvival.Visibility = System.Windows.Visibility.Collapsed;
		}

		void lblSurvival_MouseEnter(object sender, MouseEventArgs e)
		{
			this.backSurvival.Visibility = System.Windows.Visibility.Visible;
		}

		#endregion // lblSurvival

		#endregion // イベント

		#region private メソッド

		private void GameStart(GameMode gamemode)
		{
			this.windowTitle.Visibility = System.Windows.Visibility.Collapsed;
			this.currentGameMode = gamemode;

			switch (gamemode)
			{
				case GameMode.TimeAttack:
					this.InitializeGame();

					this.totalElapsedTime = 0;

					MessageManager.Instance.SystemMessageQueue.Enqueue("Enemy legions begin approaching now.");
					MessageManager.Instance.SystemMessageQueue.Enqueue("Destroy them all. out.");

					CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

					break;
				case GameMode.Survival:
					this.InitializeGame();

					MessageManager.Instance.SystemMessageQueue.Enqueue("Enemy legions begin approaching now.");
					MessageManager.Instance.SystemMessageQueue.Enqueue("Destroy them all. out.");

					CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

					break;
				default:
					break;
			}
		}

		/// <summary>
		/// 初回実行時に必要な手続きを定義します。
		/// </summary>
		private void SetUp()
		{
			this.windowTitle.Opacity = 1;
			this.windowTitle.Visibility = System.Windows.Visibility.Visible;

			this.windowHighScore.Opacity = 1;
			this.windowHighScore.Visibility = System.Windows.Visibility.Collapsed;

			this.windowGameOver.Opacity = 1;
			this.windowGameOver.Visibility = System.Windows.Visibility.Collapsed;

			this.windowDebug.Opacity = 1;

			#region イベントハンドラの登録

			this.lblSurvival.MouseEnter += new MouseEventHandler(lblSurvival_MouseEnter);
			this.lblSurvival.MouseLeave += new MouseEventHandler(lblSurvival_MouseLeave);
			this.lblSurvival.MouseLeftButtonDown += new MouseButtonEventHandler(lblSurvival_MouseLeftButtonDown);

			this.lblTimeAttack.MouseEnter += new MouseEventHandler(lblTimeAttack_MouseEnter);
			this.lblTimeAttack.MouseLeave += new MouseEventHandler(lblTimeAttack_MouseLeave);
			this.lblTimeAttack.MouseLeftButtonDown += new MouseButtonEventHandler(lblTimeAttack_MouseLeftButtonDown);

			this.lblHighScore.MouseEnter += new MouseEventHandler(lblHighScore_MouseEnter);
			this.lblHighScore.MouseLeave += new MouseEventHandler(lblHighScore_MouseLeave);
			this.lblHighScore.MouseLeftButtonDown += new MouseButtonEventHandler(lblHighScore_MouseLeftButtonDown);

			this.lblToTitle.MouseEnter += new MouseEventHandler(lblToTitle_MouseEnter);
			this.lblToTitle.MouseLeave += new MouseEventHandler(lblToTitle_MouseLeave);
			this.lblToTitle.MouseLeftButtonDown += new MouseButtonEventHandler(lblToTitle_MouseLeftButtonDown);

			this.lblToTitle2.MouseEnter += new MouseEventHandler(lblToTitle2_MouseEnter);
			this.lblToTitle2.MouseLeave += new MouseEventHandler(lblToTitle2_MouseLeave);
			this.lblToTitle2.MouseLeftButtonDown += new MouseButtonEventHandler(lblToTitle2_MouseLeftButtonDown);

			KeyDown += new KeyEventHandler(MainPage_KeyDown);
			KeyUp += new KeyEventHandler(MainPage_KeyUp);

			#endregion // イベントハンドラの登録

			// コンソールエリア
			{
				List<TextBlock> clearItems = new List<TextBlock>()
				{
					this.txtLog,

					this.lblArmor, 
					this.lblArmorFull,
					this.lblArmorGauge,
					this.lblArmorLevel,
					this.lblArmorExp,
					this.lblArmorExpNext,

					this.lblSpeed,

					this.lblLaserLevel,
					this.lblLaserExp,
					this.lblLaserExpNext,

					this.lblGunLevel,
					this.lblGunExp,
					this.lblGunExpNext,

					this.lblMissileLevel,
					this.lblMissileExp,
					this.lblMissileExpNext,

					this.lblScore,
				};

				foreach (var item in clearItems)
				{
					item.Text = String.Empty;
				}

				this.lblTime.Text = "00:00.00";
			}

			var allView = EntityViewManager.Instance.GetAllView();
			foreach (var item in allView)
			{
				item.Visibility = System.Windows.Visibility.Collapsed;
				this.MainCanvas.Children.Add(item);
			}
		}

		private void InitializeGame()
		{
			this.txtLog.Text = String.Empty;

			AmmoManager.Instance.Initialize();
			EffectManager.Instance.Initialize();
			EnemyManager.Instance.Initialize();
			GrowthManager.Instance.Initialize();
			ScoreManager.Instance.Initialize();
			MessageManager.Instance.Initialize();
			EntityViewManager.Instance.Initialize();
			EntityViewManager.Instance.HideAllView();

			CollisionMonitor.Instance.Initialieze();

			this.player = new Player();
			PlayerManager.Instance.Player = this.player;
			this.player.AdjustPosition(new Point(20, 176));
			this.player.View.Visibility = System.Windows.Visibility.Visible;

			this.WriteDebug("MainCanvas.Children.Count :" + this.MainCanvas.Children.Count.ToString());

			this.WriteDebug("GetAvailableViewCount Player :" + 
				EntityViewManager.Instance.GetAvailableViewCount(ViewType.Laser).ToString());

			if (null == this.player.View)
			{
				this.WriteDebug("player.View is null");
			}
			else
			{
				this.WriteDebug("player.View is not null");
			}

			GrowthManager.Instance.RegisterOnLevelUp();
		}

		/// <summary>
		/// ゲームオーバー時の動作を定義します。
		/// </summary>
		/// <param name="isTimeUp">タイムアップによるゲームオーバーの場合、true。</param>
		private void GameOver(bool isTimeUp)
		{
			CompositionTarget.Rendering -= CompositionTarget_Rendering;

			if (isTimeUp)
			{
				lblGameOver.Text = "Time Up";
			}
			else
			{
				lblGameOver.Text = "Game Over";
			}

			this.lblLastScore.Text = ScoreManager.Instance.Score.ToString();

			this.SaveHighScore();

			this.windowGameOver.Visibility = System.Windows.Visibility.Visible;
		}

		/// <summary>
		/// ハイスコア画面を表示します。
		/// </summary>
		private void ShowHighScore()
		{
			this.windowTitle.Visibility = System.Windows.Visibility.Collapsed;

			Action<GameMode, TextBlock> setHighScore = (gamemode, lbl) =>
			{
				String key = Enum.GetName(typeof(GameMode), gamemode);
				String value = "0";
				if (appSettings.Contains(key))
				{
					if (null != appSettings[key])
					{
						value = appSettings[key].ToString();
					}
				}

				lbl.Text = value;
			};

			setHighScore(GameMode.Survival, this.lblHighScoreSurvival);
			setHighScore(GameMode.TimeAttack, this.lblHighScoreTimeAttack);
			
			this.windowHighScore.Visibility = System.Windows.Visibility.Visible;
		}

		private void WriteDebug(String message)
		{
			Debug.AddMessageLine(message);
			while (100 < Debug.GetMessageQueue().Count)
			{
				Debug.GetMessageQueue().Dequeue();
			}

			String text = String.Empty;
			foreach (var item in Debug.GetMessageQueue())
			{
				text += item;
			}

			this.txtDebug.Text = text;
		}

		/// <summary>
		/// コンソールエリアの再描画を行います。
		/// </summary>
		private void RefreshConsole()
		{

			// Armor
			{
				int armor = (int)this.player.Armor;
				if (0 == armor && 0 < this.player.Armor)
				{
					armor = 0;
				}
				else if (armor < 0)
				{
					armor = 0;
				}

				int armorFull = (int)this.player.ArmorFull;

				this.lblArmor.Text = armor.ToString();
				this.lblArmorFull.Text = armorFull.ToString();

				{
					String gauge = "|";

					int aliveGauge = 0;
					{
						int value = armor;
						while (0 < value)
						{
							gauge += "■|";
							value -= 50;
							aliveGauge++;
						}
					}

					{
						double value = armorFull - (50 * aliveGauge);
						while (0 < value)
						{
							gauge += "_|";
							value -= 50;
						}
					}

					this.lblArmorGauge.Text = gauge;
				}

				{
					var growth = GrowthManager.Instance.ArmorGrowth;
					this.lblArmorLevel.Text = growth.Level.ToString();
					this.lblArmorExp.Text = growth.Exp.ToString();
					this.lblArmorExpNext.Text = growth.NextExp.ToString();
				}
			}

			// SPEED
			{
				String value = String.Empty;
				int index = 0;
				while (index < this.player.Speed)
				{
					value += "■";
					index++;
				}

				this.lblSpeed.Text = value;
			}

			// TIME
			{
				switch (this.currentGameMode)
				{
					case GameMode.TimeAttack:

						TimeSpan remainTime = new TimeSpan(0, 0, 0, 0, this.LimitTime - (int)(this.totalElapsedTime * 1000));
						if (0 < remainTime.TotalMilliseconds)
						{
							this.lblTime.Text = String.Format("{0:00}:{1:00}.{2:00}",
																remainTime.Minutes,
																remainTime.Seconds,
																remainTime.Milliseconds / 10);
						}
						else
						{
							this.lblTime.Text = "00:00.00";
						}

						break;
					case GameMode.Survival:

						TimeSpan currentTime = new TimeSpan(0, 0, 0, 0, (int)(this.totalElapsedTime * 1000));
						this.lblTime.Text = String.Format("{0:00}:{1:00}.{2:00}",
															currentTime.Minutes,
															currentTime.Seconds,
															currentTime.Milliseconds / 10);


						break;
					default:
						break;
				}

			}

			// SCORE
			{
				this.lblScore.Text = ScoreManager.Instance.Score.ToString();
			}

			// LASER
			{
				var growth = GrowthManager.Instance.LaserGrowth;
				this.lblLaserLevel.Text = growth.Level.ToString();
				this.lblLaserExp.Text = growth.Exp.ToString();
				this.lblLaserExpNext.Text = growth.NextExp.ToString();
			}

			// GUN
			{
				var growth = GrowthManager.Instance.GunGrowth;
				this.lblGunLevel.Text = growth.Level.ToString();
				this.lblGunExp.Text = growth.Exp.ToString();
				this.lblGunExpNext.Text = growth.NextExp.ToString();

				if (this.player.Gun.IsActive)
				{
					this.lblGunStatus.Text = "ON";

				}
				else
				{
					this.lblGunStatus.Text = "OFF";
				}
			}

			// MISSILE
			{
				var growth = GrowthManager.Instance.MissileGrowth;
				this.lblMissileLevel.Text = growth.Level.ToString();
				this.lblMissileExp.Text = growth.Exp.ToString();
				this.lblMissileExpNext.Text = growth.NextExp.ToString();


				if (this.player.Missile.IsActive)
				{
					this.lblMissileStatus.Text = "ON";

				}
				else
				{
					this.lblMissileStatus.Text = "OFF";
				}
			}
		}

		/// <summary>
		/// ログエリアの再描画を行います。
		/// </summary>
		private void RefreshLog()
		{
			var queue = MessageManager.Instance.SystemMessageQueue;
			while (3 < queue.Count)
			{
				queue.Dequeue();
			}

			String text = String.Empty;
			foreach (var item in queue)
			{
				text += item + System.Environment.NewLine;
			}

			this.txtLog.Text = text;
		}

		/// <summary>
		/// ハイスコアの保存を行います。
		/// </summary>
		private void SaveHighScore()
		{
			String key = Enum.GetName(typeof(GameMode), this.currentGameMode);
			long value = ScoreManager.Instance.Score;
			if (appSettings.Contains(key))
			{
				long current = (long)appSettings[key];
				if (current < value)
				{
					appSettings[key] = value;
				}
			}
			else
			{
				appSettings.Add(key, value);
			}
		}

		#endregion // private メソッド

	}
}
