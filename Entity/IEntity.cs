using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Entity.Unit;
using Legion.Entity.Weapon.Ammo;
using Legion.Util;
using Legion.Entity.View;

namespace Legion.Entity
{

	/// <summary>
	/// 画面上に表示されるオブジェクトに対して適用されるインターフェイスです。
	/// </summary>
	public interface IEntity
	{

		#region プロパティ

		/// <summary>
		/// オブジェクトの速度を取得または設定します。
		/// </summary>
		double Velocity { get; set; }

		/// <summary>
		/// オブジェクトの進路を示す単位ベクトルを取得または設定します。
		/// </summary>
		Vector2D Heading { get; set; }

		/// <summary>
		/// オブジェクトの現時点のベクトルを取得または設定します。
		/// </summary>
		Vector2D CurrentVector { get; set; }

		/// <summary>
		/// オブジェクトの原点を表す座標を取得します。
		/// </summary>
		Point Origin { get; }

		/// <summary>
		/// ダメージ発生源の種別を取得します。
		/// </summary>
		DamageSourceType DamageSourceType { get; }

		/// <summary>
		/// オブジェクトの状態を取得します。
		/// </summary>
		EntityStatus Status { get; }

		/// <summary>
		/// オブジェクトのビューを取得または設定します。
		/// </summary>
		/// <returns></returns>
		UserControl View { get; }

		/// <summary>
		/// オブジェクトのビューの種別を取得します。
		/// </summary>
		ViewType ViewType { get; }

		#endregion // プロパティ

		/// <summary>
		/// オブジェクトの出現時の動作を定義します。
		/// </summary>
		/// <param name="primaryPoint">弾の初期位置。</param>
		void Activate(Point origin, UserControl view);

		/// <summary>
		/// オブジェクトの消滅時の動作を定義します。
		/// </summary>
		void Deactivate();

		/// <summary>
		/// オブジェクトの位置調整を行います。
		/// </summary>
		/// <param name="position"></param>
		void AdjustPosition(Point position);

		/// <summary>
		/// 命中判定のある領域を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の原点、終点の座標を表す配列。</returns>
		FatalArea GetFatalArea();

		/// <summary>
		/// 命中判定のある領域の中心を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の中心を表す座標。</returns>
		Point GetFatalAreaCenter();

		/// <summary>
		/// 他オブジェクトとの衝突時の動作を定義します。
		/// </summary>
		void DetectedCollision();

		/// <summary>
		/// 衝突が発生したユニットエンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		void RegisterCollidedEntity(IUnit unit);

		/// <summary>
		/// 衝突が発生した弾エンティティを登録します。
		/// </summary>
		/// <param name="unit"></param>
		void RegisterCollidedEntity(IAmmo unit);

		/// <summary>
		/// オブジェクトのレベルアップを行います。
		/// </summary>
		/// <param name="damageSourceType"></param>
		/// <param name="level"></param>
		void LevelUp(DamageSourceType damageSourceType, int level);

		/// <summary>
		/// ビューの表示を更新します。
		/// </summary>
		void RefreshView();

		/// <summary>
		/// フレーム更新時の動作を定義します。
		/// </summary>
		void UpdateFrame();
	}
}
