using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Legion.Entity.View;
using Legion.Environment;
using Legion.Entity.View.Effect;
using Legion.Entity.Effect.Base;
using Legion.Util;

namespace Legion.Entity.Effect
{
	public class Smoke : EffectBase
	{

		#region フィールド

		private double showRemain = 0.25d;

		private SmokeView view;

		#endregion // フィールド

		#region コンストラクタ

		public Smoke()
		{
		}

		#endregion // コンストラクタ

		#region プロパティ

		public override UserControl View
		{
			get
			{
				return this.view;
			}
		}

		public override ViewType ViewType
		{
			get { return ViewType.Smoke; }
		}

		#endregion // プロパティ

		#region public メソッド

		public override void Activate(Point position, UserControl view)
		{
			base.Activate(position, view);

			this.view = view as SmokeView;
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.Status = EntityStatus.Deletable;
			EffectManager.Instance.ChangeEffectStatus(this);
		}

		public override void RefreshView()
		{
			if (null == this.view)
			{
				return;
			}

			Canvas.SetLeft(this.view, this.Origin.X);
			Canvas.SetTop(this.view, this.Origin.Y);

			this.view.RefreshView(this.showRemain * 4);
		}

		public override void UpdateFrame()
		{
			this.showRemain -= GameSettings.ElapsedTime;

			if (this.showRemain <= 0)
			{
				this.Deactivate();
			}
		}

		#endregion // public メソッド

	}
}
