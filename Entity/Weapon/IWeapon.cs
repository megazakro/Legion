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

namespace Legion.Entity.Weapon
{
	public interface IWeapon
	{

		#region プロパティ

		bool IsActive { get; set; }

		#endregion // プロパティ

		void Fire(Point point);
		void CeaseFire();
		void UpdateFirePosition(Point point);
		void UpdateFrame();
		void LevelUp(int level);
	}
}
