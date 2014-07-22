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
	/// 成長要素をもつ項目を表します。
	/// </summary>
	public class Growth
	{

		#region 定数

		public const int MaxLevel = 9;

		#endregion // 定数

		#region フィールド

		long maxExp;

		#endregion // フィールド

		#region コンストラクタ

		public Growth(int level, long exp, long maxExp)
		{
			this.Level = level;
			this.Exp = exp;
			this.maxExp = maxExp;
			this.OnLevelUp = () => { };
			this.NextExpMap = new Dictionary<int, long>();
		}

		#endregion // コンストラクタ

		#region プロパティ

		public long Exp { get; private set; }
		public long NextExp { get; private set; }
		public int Level { get; private set; }

		public Action OnLevelUp { get; set; }

		/// <summary>
		/// レベルをキー、次の経験値を値とするマップを取得または設定します。
		/// </summary>
		public Dictionary<int, long> NextExpMap { get; set; }

		#endregion // プロパティ

		#region public メソッド

		/// <summary>
		/// 経験値を加算します。
		/// </summary>
		/// <param name="exp">経験値</param>
		public void AddExp(long exp)
		{
			this.Exp += exp;
			if (maxExp < this.Exp)
			{
				this.Exp = maxExp;
			}

			if (this.Level < MaxLevel)
			{
				if (this.NextExp <= this.Exp)
				{
					this.LevelUp();
				}
			}
			else if (MaxLevel == this.Level)
			{
				if (this.NextExp <= this.Exp)
				{
					this.Exp = this.NextExp;
				}
			}
		}

		/// <summary>
		/// レベルアップします。
		/// </summary>
		public void LevelUp()
		{
			this.Level++;
			this.NextExp = this.NextExpMap[this.Level];

			this.OnLevelUp();
		}

		#endregion // public メソッド

	}
}
