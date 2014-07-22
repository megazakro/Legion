using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;

using Legion.Entity.Weapon.Ammo.Base;
using Legion.Environment;
using Legion.Util;
using Legion.Entity.View;
using Legion.Entity.View.Weapon.Ammo;
using Legion.AI.SteeringBehaviors;
using Legion.Entity.Unit;
using Legion.Entity.Effect;

namespace Legion.Entity.Weapon.Ammo
{
	public partial class Missile : AmmoBase
	{

		#region enum

		private enum EngineStatus
		{
			Stop,

			Boost,

			Idle,
		}

		#endregion // enum

		#region フィールド

		private static Dictionary<int, LevelRelatedContainer> levelMap = new Dictionary<int, LevelRelatedContainer>()
		{
			{1, new	LevelRelatedContainer(){ BasePower = 0.010, MaxVelocity = 600, Acceleration=40, MaxTurnAngle=10 }},
			{2, new	LevelRelatedContainer(){ BasePower = 0.012, MaxVelocity = 650, Acceleration=50, MaxTurnAngle=10 }},
			{3, new	LevelRelatedContainer(){ BasePower = 0.015, MaxVelocity = 700, Acceleration=50, MaxTurnAngle=10 }},
			{4, new	LevelRelatedContainer(){ BasePower = 0.018, MaxVelocity = 750, Acceleration=60, MaxTurnAngle=30 }},
			{5, new	LevelRelatedContainer(){ BasePower = 0.020, MaxVelocity = 800, Acceleration=60, MaxTurnAngle=30 }},
			{6, new	LevelRelatedContainer(){ BasePower = 0.022, MaxVelocity = 850, Acceleration=70, MaxTurnAngle=30 }},
			{7, new	LevelRelatedContainer(){ BasePower = 0.024, MaxVelocity = 900, Acceleration=70, MaxTurnAngle=60 }},
			{8, new	LevelRelatedContainer(){ BasePower = 0.028, MaxVelocity = 950, Acceleration=80, MaxTurnAngle=60 }},
			{9, new	LevelRelatedContainer(){ BasePower = 0.030, MaxVelocity = 1000, Acceleration=90, MaxTurnAngle=30 }},
		};

		private MissileView view;

		private IUnit target;

		private double basePower;
		private double maxVelocity;
		private double acceleration;
		private int maxTurnAngle;

		private bool exploding = false;
		private bool explodEnding = false;

		private double explodingSpan = 1.0;
		private double explodingRemain = 0;

		private EngineStatus engineStatus = EngineStatus.Stop;
		private double stopRemain = 0.5;

		private bool seekable = false;

		private Point lastCenter;

		#endregion // フィールド

		#region コンストラクタ

		public Missile()
		{
		}

		#endregion // コンストラクタ

		#region プロパティ

		public override DamageSourceType DamageSourceType
		{
			get
			{
				return DamageSourceType.Missile;
			}
		}

		public override ViewType ViewType
		{
			get
			{
				return ViewType.Missile;
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

			this.view = view as MissileView;
			{
				this.view.Initialize();
				this.view.Rotate(0);
			}

			var container = levelMap[GrowthManager.Instance.MissileGrowth.Level];
			{
				this.basePower = container.BasePower;
				this.maxVelocity = container.MaxVelocity;
				this.acceleration = container.Acceleration;
				this.maxTurnAngle = container.MaxTurnAngle;
			}

			this.AdjustPosition(primaryPoint);
		}

		public override void Deactivate()
		{
			base.Deactivate();

			this.target = null;
			this.Power = 0;
			this.Status = EntityStatus.Deletable;
			AmmoManager.Instance.ChangeAmmoStatus(this);
		}

		public override void AdjustPosition(Point position)
		{
			this.Origin = position;

			CollisionMonitor.Instance.RegisterPlayerAmmo(this);
		}

		public override FatalArea GetFatalArea()
		{
			if (exploding)
			{
				return new FatalArea(new Point(this.Origin.X + 0, this.Origin.Y + 0),
					new Point(this.Origin.X + 48, this.Origin.Y + 48));
			}
			else
			{
				return new FatalArea(new Point(this.Origin.X + 16, this.Origin.Y + 16),
					new Point(this.Origin.X + 32, this.Origin.Y + 32));
			}
		}

		/// <summary>
		/// 命中判定のある領域の中心を取得します。
		/// </summary>
		/// <returns>命中判定のある領域の中心を表す座標。</returns>
		public override Point GetFatalAreaCenter()
		{
			return new Point(this.Origin.X + 24, this.Origin.Y + 24);
		}

		public override void DetectedCollision()
		{
			if (0 < this.collidedUnitQueue.Count)
			{
				if (!this.exploding)
				{
					this.exploding = true;
					this.explodingRemain = this.explodingSpan;
					this.Velocity = 0;
					this.CurrentVector *= this.Velocity;
					this.target = null;

					if (null != this.view)
					{
						this.view.BeginExplode();
					}
				}

				this.Power = this.basePower / GameSettings.ElapsedTime;
			}
		}

		public override void RefreshView()
		{
			if (null == this.view)
			{
				return;
			}

			Canvas.SetLeft(this.view, this.Origin.X);
			Canvas.SetTop(this.view, this.Origin.Y);

			if (!this.exploding)
			{
				switch (this.engineStatus)
				{
					case EngineStatus.Boost:
					case EngineStatus.Idle:

						if (this.seekable)
						{
							int degree = 0;
							if (1 == this.Heading.X)
							{
								degree = 0;
							}
							else if (1 == this.Heading.Y)
							{
								degree = 90;
							}
							else if (-1 == this.Heading.X)
							{
								degree = 180;
							}
							else if (-1 == this.Heading.Y)
							{
								degree = 270;
							}
							else
							{

								// X = 1, Y = 0のベクトルとの角度を計算
								// cos = (v1.x * v2.x + v1.y * v2.y) = v1.x
								double cos = this.Heading.X;

								double radian = Math.Acos(cos);

								// ラジアンを度に変換
								degree = (int)Math.Floor(radian / Math.PI * 180d);

								if (this.Heading.Y < 0)
								{
									degree = -degree;
								}
							}

							this.view.Rotate(degree);
						}

						break;
					case EngineStatus.Stop:
					default:
						break;
				}
			}
		}

		public void SetTarget(IUnit target)
		{
			this.target = target;
		}

		public override void UpdateFrame()
		{
			base.UpdateFrame();

			if (this.exploding)
			{
				this.explodingRemain -= GameSettings.ElapsedTime;

				if (0 < this.explodingRemain && this.explodingRemain <= 0.5)
				{
					if (!explodEnding)
					{
						explodEnding = true;
						if (null != this.view)
						{
							this.view.EndExplode();
						}
					}
				}
				else if (this.explodingRemain <= 0)
				{
					this.Deactivate();
				}
			}
			else
			{
				switch (this.engineStatus)
				{
					case EngineStatus.Stop:
						this.stopRemain -= GameSettings.ElapsedTime;
						if (this.stopRemain <= 0)
						{
							this.engineStatus = EngineStatus.Boost;
						}

						return;
					case EngineStatus.Boost:
						this.Velocity += this.acceleration;
						if (this.maxVelocity < this.Velocity)
						{
							this.Velocity = this.maxVelocity;
						}

						break;
					case EngineStatus.Idle:

						break;
					default:
						break;
				}

#if SMOKE

				// スモーク演出 重いのでコンパイルオプションで無効化
				{
					int availableSmoke = EntityViewManager.Instance.GetAvailableViewCount(ViewType.Smoke);

					Debug.AddMessageLine("availableSmoke:" + availableSmoke.ToString());

					if (0 < availableSmoke)
					{
						Point currentCenter = this.GetFatalAreaCenter();
						if (null != lastCenter && 0 < lastCenter.X && 0 < lastCenter.Y)
						{
							int fromX = (int)lastCenter.X;
							int fromY = (int)lastCenter.Y;
							int toX = (int)currentCenter.X;
							int toY = (int)currentCenter.Y;

							double deltaX = toX - fromX;
							double deltaY = toY - fromY;

							if (0 != deltaX || 0 != deltaY)
							{
								List<Point> points = new List<Point>();

								int incrementX = 1;
								int incrementY = 1;

								if (deltaX < 0)
								{
									incrementX = -1;
									deltaX = -deltaX;
								}

								if (deltaY < 0)
								{
									incrementY = -1;
									deltaY = -deltaY;
								}

								if (3 < deltaX || 3 < deltaY)
								{
									int x = fromX;
									int y = fromY;
									bool skip = true;
									if (deltaY < deltaX)
									{
										double counter = deltaY - deltaX;
										while (x != toX)
										{
											if (0 <= counter)
											{
												y += incrementY;
												counter -= deltaX;
											}

											counter += deltaY;
											x += incrementX;
											if (!skip)
											{
												points.Add(new Point(x, y));
												skip = true;
											}
											else
											{
												skip = false;
											}
										}
									}
									else
									{
										double counter = deltaX - deltaY;
										while (y != toY)
										{
											if (0 <= counter)
											{
												x += incrementX;
												counter -= deltaY;
											}

											counter += deltaX;
											y += incrementY;
											if (!skip)
											{
												points.Add(new Point(x, y));
												skip = true;
											}
											else
											{
												skip = false;
											}
										}
									}

									foreach (var point in points)
									{
										if (availableSmoke < 10)
										{
											break;
										}

										var smoke = new Smoke();
										var view = EntityViewManager.Instance.GetView(Entity.View.ViewType.Smoke);
										smoke.Activate(point, view);
										availableSmoke--;
									}
								}
							}
						}

						lastCenter = currentCenter;
					}
				}

#endif

				// 敵を狙う
				if (seekable)
				{
					if (null != target && EntityStatus.Active == this.target.Status)
					{
						var thisCenter = this.GetFatalAreaCenter();

						var enemyCenter = this.target.GetFatalAreaCenter();

						Seek.Execute(this, enemyCenter, this.maxTurnAngle, this.acceleration * 5);
					}
				}
				else
				{
					Vector2D horizontal = new Vector2D(1, 0);

					Vector2D steering = horizontal * this.acceleration * GameSettings.ElapsedTime;

					Vector2D finalVector = this.CurrentVector + steering;
					finalVector.Normalize();

					this.Heading = finalVector;
					this.CurrentVector = finalVector * this.Velocity * GameSettings.ElapsedTime;

					// 水平ベクトルに近づいたらシーカーをON
					if (TrigonometricFunction.GetCosMap()[10] < this.Heading.X)
					{
						this.seekable = true;
					}
				}
			}
		}

		#endregion // public メソッド

		/// <summary>
		/// レベルに関連する事項を格納するコンテナです。
		/// </summary>
		private class LevelRelatedContainer
		{

			/// <summary>
			/// 威力
			/// </summary>
			public double BasePower { get; set; }

			/// <summary>
			/// 最高速度
			/// </summary>
			public double MaxVelocity { get; set; }

			/// <summary>
			/// 加速度
			/// </summary>
			public double Acceleration { get; set; }

			/// <summary>
			/// 最大回転角度
			/// </summary>
			public int MaxTurnAngle { get; set; }
		}

	}
}
