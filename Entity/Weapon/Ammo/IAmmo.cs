using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Util;
using Legion.Entity.Unit;

namespace Legion.Entity.Weapon.Ammo
{

	/// <summary>
	/// 弾を表すオブジェクトのインターフェイスです。
	/// </summary>
	public interface IAmmo : IEntity
	{

		#region プロパティ

		/// <summary>
		/// 弾の威力を取得します。
		/// </summary>
		double Power { get; set; }

		#endregion // プロパティ

	}
}
