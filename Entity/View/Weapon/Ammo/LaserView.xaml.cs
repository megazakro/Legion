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

using Legion.Entity.Weapon.Ammo.Base;
using Legion.Environment;
using Legion.Util;

namespace Legion.Entity.View.Weapon.Ammo
{
	public partial class LaserView : UserControl
	{

		#region フィールド

		#endregion // フィールド

		#region コンストラクタ

		public LaserView()
		{
			InitializeComponent();
		}

		#endregion // コンストラクタ

		#region プロパティ

		#endregion // プロパティ

		#region public メソッド

		public void RefreshLaser(Point position, int laserWidth)
		{
			this.outer.StrokeThickness = laserWidth;

			Path[] pathes = { this.outer, this.core };
			foreach (var path in pathes)
			{
				path.SetValue(Path.DataProperty, new PathGeometry()
				{
					Figures = new PathFigureCollection()
					{
						new PathFigure()
						{
							StartPoint = position,
							IsClosed = false,
							Segments = new PathSegmentCollection()
							{
								new BezierSegment()
								{
									Point1 = new Point(position.X - laserWidth, position.Y),
									Point2 = new Point(GameSettings.WindowWidth, position.Y),
									Point3 = new Point(GameSettings.WindowWidth, position.Y),
								},
							},
						},
					},
				});
			}
		}

		#endregion // public メソッド

	}
}
