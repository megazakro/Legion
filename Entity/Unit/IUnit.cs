using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Entity.Weapon.Ammo;

namespace Legion.Entity.Unit
{

	/// <summary>
	/// ユニットを表すインターフェイスです。
	/// </summary>
	public interface IUnit : IEntity
	{

		/// <summary>
		/// ユニットにダメージを設定します。
		/// </summary>
		/// <param name="damage">ダメージ値。</param>
		/// <param name="datameSourceType">ダメージ発生源の種別。</param>
		void SetDamage(double damage, DamageSourceType datameSourceType);

		#region プロパティ

		/// <summary>
		/// 装甲の強度を取得します。
		/// </summary>
		double Armor { get; }

		#endregion // プロパティ

		#region public メソッド

		/// <summary>
		/// 装甲の強度を更新します。
		/// </summary>
		void UpdateArmor();

		#endregion // public メソッド
		
	}
}
