using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Legion.Entity.View
{
	public enum ViewType
	{

		#region Unit

		Player,

		Velites,

		#endregion // Unit

		#region Ammo

		EnemyNormalShot,

		Laser,

		Bullet,

		Missile,

		#endregion // Ammo

		#region Effect

		Smoke,

		#endregion // Effect

	}
}
