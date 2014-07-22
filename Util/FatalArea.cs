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

namespace Legion.Util
{

	/// <summary>
	/// 当たり判定を示す領域を表す構造体です。
	/// </summary>
	public struct FatalArea
	{
		public Point leftTop;
		public Point rightBottom;

		public FatalArea(Point leftTop, Point rightBottom)
		{
			this.leftTop = leftTop;
			this.rightBottom = rightBottom;
		}
	}
}
