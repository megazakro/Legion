using System;
using System.Windows;
using Legion.Entity;
using Legion.Util;
using Legion.Environment;

namespace Legion.AI.SteeringBehaviors
{

	/// <summary>
	/// 対象を追いかける操舵行動。
	/// </summary>
	public class Seek : ISteeringBehaviors
	{

		/// <summary>
		/// 操舵行動を実行します。
		/// </summary>
		/// <param name="seeker">追跡主体。</param>
		/// <param name="targetPoint">目標の座標。</param>
		/// <param name="maxTurnAngle">最大の回転角度。</param>
		/// <param name="steeringForce">操舵力。</param>
		public static void Execute(IEntity seeker, Point targetPoint, int maxTurnAngle, double steeringForce)
		{
			var seekerCenter = seeker.GetFatalAreaCenter();

			// 敵とのベクトルを取得
			Vector2D toTarget = new Vector2D(targetPoint.X - seekerCenter.X, targetPoint.Y - seekerCenter.Y);
			toTarget.Normalize();

			// 理想的な操舵ベクトルを取得
			Vector2D steering = seeker.Heading + toTarget;

			// 操舵ベクトルと、現在の進路ベクトルとの角度が一定以上にならないよう補正
			var cosMap = TrigonometricFunction.GetCosMap();
			var sinMap = TrigonometricFunction.GetSinMap();

			if (steering.GetDot(seeker.Heading) < cosMap[maxTurnAngle])
			{
				double x, y;
				int degree;

				if (Vector2D.Clockwise.Clockwise == seeker.Heading.GetClockwise(steering))
				{
					degree = -maxTurnAngle;
				}
				else
				{
					degree = maxTurnAngle;
				}

				x = (seeker.Heading.X * cosMap[degree]) - (seeker.Heading.Y * sinMap[degree]);
				y = (seeker.Heading.X * sinMap[degree]) + (seeker.Heading.Y * cosMap[degree]);

				steering = new Vector2D(x, y);
			}

			steering = steering * steeringForce * GameSettings.ElapsedTime;

			Vector2D finalVector = seeker.CurrentVector + steering;
			finalVector.Normalize();

			seeker.Heading = finalVector;
			seeker.CurrentVector = finalVector * seeker.Velocity * GameSettings.ElapsedTime;
		}
	}
}
