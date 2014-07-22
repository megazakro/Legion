using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Legion.Entity.Unit.Enemy
{
	public interface IEnemy : IUnit
	{

		#region プロパティ

		long Score { get; }
		int Exp { get; }

		#endregion // プロパティ

		#region public メソッド

		#endregion // public メソッド

	}
}
