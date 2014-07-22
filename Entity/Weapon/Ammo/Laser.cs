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
using Legion.Entity.View;
using Legion.Entity.View.Weapon.Ammo;

namespace Legion.Entity.Weapon.Ammo
{
	public partial class Laser : AmmoBase
	{

		#region フィールド

		private static Dictionary<int, LevelRelatedContainer> levelMap = new Dictionary<int, LevelRelatedContainer>()
		{
			{1, new	LevelRelatedContainer(){ BasePower = 0.010, LaserWidth = 4, }},
			{2, new	LevelRelatedContainer(){ BasePower = 0.012, LaserWidth = 4, }},
			{3, new	LevelRelatedContainer(){ BasePower = 0.013, LaserWidth = 6, }},
			{4, new	LevelRelatedContainer(){ BasePower = 0.014, LaserWidth = 6, }},
			{5, new	LevelRelatedContainer(){ BasePower = 0.015, LaserWidth = 8, }},
			{6, new	LevelRelatedContainer(){ BasePower = 0.016, LaserWidth = 8, }},
			{7, new	LevelRelatedContainer(){ BasePower = 0.017, LaserWidth = 10, }},
			{8, new	LevelRelatedContainer(){ BasePower = 0.018, LaserWidth = 10, }},
			{9, new	LevelRelatedContainer(){ BasePower = 0.020, LaserWidth = 14, }},
		};

		FatalArea fatalArea;

		private LaserView view;

		private double basePower;

		private int laserWidth;
		private int laserHalfWidth;

		#endregion // フィールド

		#region コンストラクタ

		public Laser()
		{
			this.fatalArea = new FatalArea(new Point(-1, -1), new Point(GameSettings.MainAreaWidth, -1));
			this.Power = 0;
		}

		#endregion // コンストラクタ

		#region プロパティ

		public override DamageSourceType DamageSourceType
		{
			get
			{
				return DamageSourceType.Laser;
			}
		}

		public override ViewType ViewType
		{
			get
			{
				return ViewType.Laser;
			}
		}

		public override UserControl View
		{
			get
			{
				return this.view;
			}
		}

		#endregion // プロパティ

		#region public メソッド

		public override void Activate(Point primaryPoint, UserControl view)
		{
			base.Activate(primaryPoint, view);

			this.view = view as LaserView;

			var container = levelMap[GrowthManager.Instance.LaserGrowth.Level];
			this.basePower = container.BasePower;
			this.laserWidth = container.LaserWidth;
			this.laserHalfWidth = this.laserWidth / 2;

			this.AdjustPosition(primaryPoint);
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.Status = EntityStatus.Deletable;
			AmmoManager.Instance.ChangeAmmoStatus(this);
		}

		public override void AdjustPosition(Point position)
		{
			this.Origin = position;

			this.fatalArea.leftTop.X = this.Origin.X;
			this.fatalArea.leftTop.Y = this.Origin.Y - this.laserHalfWidth;

			this.fatalArea.rightBottom.Y = this.Origin.Y + this.laserHalfWidth;

			CollisionMonitor.Instance.RegisterPlayerAmmo(this);
		}

		public override FatalArea GetFatalArea()
		{
			return this.fatalArea;
		}

		public override void DetectedCollision()
		{
			this.Power = this.basePower / GameSettings.ElapsedTime;
		}

		public override void RefreshView()
		{
			if (null == this.view)
			{
				return;
			}

			this.view.RefreshLaser(this.Origin, this.laserHalfWidth);
		}

		public override void UpdateFrame()
		{
			base.UpdateFrame();
		}

		#endregion // public メソッド

		/// <summary>
		/// レベルに関連する事項を格納するコンテナです。
		/// </summary>
		private class LevelRelatedContainer
		{

			/// <summary>
			/// 時間あたりの威力
			/// </summary>
			public double BasePower { get; set; }

			/// <summary>
			/// レーザーの太さ
			/// </summary>
			public int LaserWidth { get; set; }
		}

	}
}
