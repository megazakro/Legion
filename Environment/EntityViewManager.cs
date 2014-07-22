using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Legion.Entity.Weapon.Ammo;
using Legion.Entity.Unit;
using Legion.Entity.Unit.Enemy;
using Legion.Util;
using Legion.Entity;
using Legion.Entity.View;
using Legion.Entity.View.Unit.Enemy;
using Legion.Entity.View.Weapon.Ammo;
using System.Collections;
using Legion.Entity.View.Effect;
using Legion.Entity.View.Unit;

namespace Legion.Environment
{

	/// <summary>
	/// エンティティのビューの状態を管理します。
	/// </summary>
	public class EntityViewManager
	{

		#region 定数

		private const int MaxViewCount = 200;

#if SMOKE

		private const int MaxEffectViewCount = 1000;

#else

		private const int MaxEffectViewCount = 0;

#endif

		private const int AppendCount = 100;

		#endregion // 定数

		#region フィールド

		private static readonly EntityViewManager instance = new EntityViewManager();

		#region ユニット

		private Queue<PlayerView> availablePlayers = new Queue<PlayerView>();
		private List<PlayerView> usingPlayers = new List<PlayerView>();

		private Queue<VelitesView> availableVelites = new Queue<VelitesView>();
		private List<VelitesView> usingVelites = new List<VelitesView>();

		#endregion // ユニット

		#region 弾

		private Queue<LaserView> availableLaser = new Queue<LaserView>();
		private List<LaserView> usingLaser = new List<LaserView>();

		private Queue<BulletView> availableBullet = new Queue<BulletView>();
		private List<BulletView> usingBullet = new List<BulletView>();

		private Queue<MissileView> availableMissile = new Queue<MissileView>();
		private List<MissileView> usingMissile = new List<MissileView>();

		private Queue<EnemyNormalShotView> availableEnemyNormalShot = new Queue<EnemyNormalShotView>();
		private List<EnemyNormalShotView> usingEnemyNormalShot = new List<EnemyNormalShotView>();

		#endregion // 弾

		#region エフェクト

		private Queue<SmokeView> availableSmoke = new Queue<SmokeView>();
		private List<SmokeView> usingSmoke = new List<SmokeView>();

		#endregion // エフェクト

		#endregion // フィールド

		#region コンストラクタ

		private EntityViewManager()
		{
			this.Initialize();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public static EntityViewManager Instance
		{
			get { return instance; }
		}

		#endregion // プロパティ

		#region public メソッド

		public void Initialize()
		{
			this.InitializeBuffer<PlayerView>(this.availablePlayers, this.usingPlayers, 1);

			this.InitializeBuffer<VelitesView>(this.availableVelites, this.usingVelites, MaxViewCount);

			this.InitializeBuffer<LaserView>(this.availableLaser, this.usingLaser, 10);

			this.InitializeBuffer<BulletView>(this.availableBullet, this.usingBullet, MaxViewCount);

			this.InitializeBuffer<MissileView>(this.availableMissile, this.usingMissile, 20);

			this.InitializeBuffer<EnemyNormalShotView>(this.availableEnemyNormalShot, this.usingEnemyNormalShot, MaxViewCount);

			this.InitializeBuffer<SmokeView>(this.availableSmoke, this.usingSmoke, MaxEffectViewCount);
		}

		public UserControl GetView(ViewType viewType)
		{
			UserControl result = null;
			switch (viewType)
			{
				case ViewType.Player:
					result = this.GetView<PlayerView>(this.availablePlayers, this.usingPlayers);

					break;
				case ViewType.Velites:
					result = this.GetView<VelitesView>(this.availableVelites, this.usingVelites);

					break;
				case ViewType.EnemyNormalShot:
					result = this.GetView<EnemyNormalShotView>(this.availableEnemyNormalShot, this.usingEnemyNormalShot);

					break;
				case ViewType.Laser:
					result = this.GetView<LaserView>(this.availableLaser, this.usingLaser);

					break;
				case ViewType.Bullet:
					result = this.GetView<BulletView>(this.availableBullet, this.usingBullet);

					break;
				case ViewType.Missile:
					result = this.GetView<MissileView>(this.availableMissile, this.usingMissile);

					break;
				case ViewType.Smoke:
					result = this.GetView<SmokeView>(this.availableSmoke, this.usingSmoke);

					break;
				default:
					break;
			}

			return result;
		}

		public void FreeView(ViewType viewType, UserControl view)
		{
			switch (viewType)
			{
				case ViewType.Player:
					this.FreeView<PlayerView>(view as PlayerView,
						this.availablePlayers, this.usingPlayers);

					break;
				case ViewType.Velites:
					this.FreeView<VelitesView>(view as VelitesView,
						this.availableVelites, this.usingVelites);

					break;
				case ViewType.EnemyNormalShot:
					this.FreeView<EnemyNormalShotView>(view as EnemyNormalShotView,
						this.availableEnemyNormalShot, this.usingEnemyNormalShot);

					break;
				case ViewType.Laser:
					this.FreeView<LaserView>(view as LaserView,
						this.availableLaser, this.usingLaser);

					break;
				case ViewType.Bullet:
					this.FreeView<BulletView>(view as BulletView,
						this.availableBullet, this.usingBullet);

					break;
				case ViewType.Missile:
					this.FreeView<MissileView>(view as MissileView,
						this.availableMissile, this.usingMissile);

					break;
				case ViewType.Smoke:
					this.FreeView<SmokeView>(view as SmokeView,
						this.availableSmoke, this.usingSmoke);

					break;
				default:
					break;
			}
		}

		public List<UserControl> GetAllView()
		{
			var result = new List<UserControl>();

			IEnumerable[] queues =
			{
				this.availablePlayers, this.usingPlayers,
				this.availableVelites, this.usingVelites,
				this.availableLaser, this.usingLaser,
				this.availableEnemyNormalShot, this.usingEnemyNormalShot,
				this.availableBullet, this.usingBullet,
				this.availableMissile, this.usingMissile,
				this.availableSmoke, this.usingSmoke,
			};

			foreach (var queue in queues)
			{
				foreach (var item in queue)
				{
					result.Add(item as UserControl);
				}
			}

			return result;
		}

		public int GetAvailableViewCount(ViewType viewType)
		{
			int count = 0;
			switch (viewType)
			{
				case ViewType.Player:
					count = this.availablePlayers.Count;

					break;
				case ViewType.Velites:
					count = this.availableVelites.Count;

					break;
				case ViewType.EnemyNormalShot:
					count = this.availableEnemyNormalShot.Count;

					break;
				case ViewType.Laser:
					count = this.availableLaser.Count;

					break;
				case ViewType.Bullet:
					count = this.availableBullet.Count;

					break;
				case ViewType.Missile:
					count = this.availableMissile.Count;

					break;
				case ViewType.Smoke:
					count = this.availableSmoke.Count;

					break;
				default:
					break;
			}

			return count;
		}

		/// <summary>
		/// 全てのビューを非表示にします。
		/// </summary>
		public void HideAllView()
		{

			#region ユニット

			this.HideView<PlayerView>(availablePlayers, usingPlayers);

			this.HideView<VelitesView>(availableVelites, usingVelites);

			#endregion // ユニット

			#region 弾

			this.HideView<LaserView>(availableLaser, usingLaser);

			this.HideView<BulletView>(availableBullet, usingBullet);

			this.HideView<MissileView>(availableMissile, usingMissile);

			this.HideView<EnemyNormalShotView>(availableEnemyNormalShot, usingEnemyNormalShot);

			#endregion // 弾

			#region エフェクト

			this.HideView<SmokeView>(availableSmoke, usingSmoke);

			#endregion // エフェクト

		}

		#endregion // public メソッド

		#region private メソッド

		private void InitializeBuffer<View>(Queue<View> availableViews, List<View> usingViews, int bufferSize)
			where View : new()
		{
			foreach (var item in usingViews)
			{
				availableViews.Enqueue(item);
			}

			int index = availableViews.Count;
			while (index < bufferSize)
			{
				availableViews.Enqueue(new View());
				index++;
			}

			usingViews.Clear();
		}

		/// <summary>
		/// 指定された型のビューを取得します。
		/// </summary>
		/// <typeparam name="View"></typeparam>
		/// <param name="availableQueue"></param>
		/// <param name="usingQueue"></param>
		/// <returns></returns>
		private View GetView<View>(Queue<View> availableViews, List<View> usingViews)
			where View : new() 
		{
			var item = availableViews.Dequeue();
			usingViews.Add(item);

			return item;
		}

		/// <summary>
		/// 指定された型のビューを使用可能にします。
		/// </summary>
		/// <typeparam name="View"></typeparam>
		/// <param name="view"></param>
		/// <param name="availableViews"></param>
		/// <param name="usingViews"></param>
		private void FreeView<View>(View view, Queue<View> availableViews, List<View> usingViews)
		{
			if (usingViews.Contains(view))
			{
				usingViews.Remove(view);
				availableViews.Enqueue(view);
			}
		}

		private void HideView<View>(Queue<View> availableViews, List<View> usingViews)
			where View : UserControl
		{
			foreach (var item in availableViews)
			{
				item.Visibility = System.Windows.Visibility.Collapsed;
			}
		}

		#endregion // private メソッド

	}
}
