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
using System.Collections.Generic;

namespace Legion.Util
{

	/// <summary>
	/// 三角関数に関する機能を提供します。
	/// </summary>
	public class TrigonometricFunction
	{

		#region フィールド

		private static Dictionary<int, double> cosMap = new Dictionary<int, double>()
		{
			{5, 0.9961947},
			{-5, 0.9961947},

			{10, 0.98480775},
			{-10, 0.98480775},

			{30, 0.8660254},
			{-30, 0.8660254},

			{60, 0.5},
			{-60, 0.5},

			{90, 0.0},
			{-90, 0.0},
		};

		private static Dictionary<int, double> sinMap = new Dictionary<int, double>()
		{
			{5, 0.08715574},
			{-5, -0.08715574},

			{10, 0.17364818},
			{-10, -0.17364818},

			{30, 0.5},
			{-30, -0.5},

			{60, 0.8660254},
			{-60, -0.8660254},

			{90, 1.0},
			{-90, -1.0},
		};

		#endregion // フィールド

		#region public static メソッド

		public static Dictionary<int, double> GetCosMap()
		{
			return cosMap;
		}

		public static Dictionary<int, double> GetSinMap()
		{
			return sinMap;
		}

		#endregion // public static メソッド


	}
}
