using System;
using System.Windows;
using Legion.Entity;
using Legion.Util;
using Legion.Environment;

namespace Legion.AI.SteeringBehaviors
{

	/// <summary>
	/// 対象に向かって直進する操舵行動。
	/// </summary>
	public class Straight : ISteeringBehaviors
	{

		public static void Execute(IEntity entity, Point targetPoint)
		{
			var myPoint = entity.GetFatalAreaCenter();

			// ベクトルの取得
			Vector2D vector = new Vector2D(targetPoint.X - myPoint.X, targetPoint.Y - myPoint.Y);
			vector.Normalize();

			entity.Heading = vector;
			entity.CurrentVector = vector * entity.Velocity * GameSettings.ElapsedTime;
		}
	}
}
