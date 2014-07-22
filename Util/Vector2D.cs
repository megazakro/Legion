
using System;
namespace Legion.Util
{

	/// <summary>
	/// ベクトルを表す構造体です。
	/// </summary>
	public struct Vector2D
	{

		#region enum

		/// <summary>
		/// 回転方向を表す列挙。
		/// </summary>
		public enum Clockwise
		{

			/// <summary>
			/// 時計回り
			/// </summary>
			Clockwise = 1,

			/// <summary>
			/// 反時計回り
			/// </summary>
			Anticlockwise = -1,
		}

		#endregion // enum

		#region フィールド

		public double X;
		public double Y;

		#endregion // フィールド

		#region コンストラクタ

		public Vector2D(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}

		#endregion // コンストラクタ

		#region プロパティ

		#endregion プロパティ

		#region 演算子のオーバーロード

		#region +

		public static Vector2D operator +(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X + right.X, left.Y + right.Y);
		}

		#endregion // +

		#region -

		public static Vector2D operator -(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X - right.X, left.Y - right.Y);
		}

		#endregion // -

		#region *

		public static Vector2D operator *(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X * right.X, left.Y * right.Y);
		}

		public static Vector2D operator *(Vector2D left, double right)
		{
			return new Vector2D(left.X * right, left.Y * right);
		}

		public static Vector2D operator *(double left, Vector2D right)
		{
			return new Vector2D(left * right.X, left * right.Y);
		}

		#endregion // *

		#region /

		public static Vector2D operator /(Vector2D left, Vector2D right)
		{
			return new Vector2D(left.X / right.X, left.Y / right.Y);
		}

		public static Vector2D operator /(Vector2D left, double right)
		{
			return new Vector2D(left.X / right, left.Y / right);
		}

		#endregion // /

		#region ==

		public static bool operator ==(Vector2D left, Vector2D right)
		{
			return (left.X == right.X && left.Y == right.Y);
		}

		#endregion // ==

		#region !=

		public static bool operator !=(Vector2D left, Vector2D right)
		{
			return (left.X != right.X || left.Y != right.Y);
		}

		#endregion // !=

		#endregion // 演算子のオーバーロード

		#region public メソッド

		#region override

		public override bool Equals(object obj)
		{
			if (null == obj)
			{
				return false;
			}

			if (!(obj is Vector2D))
			{
				return false;
			}

			var param = (Vector2D)obj;

			return (this.X == param.X && this.Y == param.Y);
		}

		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ (this.Y.GetHashCode());
		}

		#endregion // override

		/// <summary>
		/// XとYにゼロを設定します。
		/// </summary>
		public void SetZero()
		{
			this.X = 0;
			this.Y = 0;
		}

		/// <summary>
		/// XとYがいずれもゼロであるかどうかを判定します。
		/// </summary>
		/// <returns>XとYがいずれもゼロである場合、true。</returns>
		public bool IsZero()
		{
			if (0 == this.X && 0 == this.Y)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// ベクトルの長さを取得します。
		/// </summary>
		/// <returns>ベクトルの長さ。</returns>
		public double GetLength()
		{
			return Math.Sqrt((this.X * this.X) + (this.Y * this.Y));
		}

		/// <summary>
		/// ベクトルの長さの2乗を取得します。
		/// </summary>
		/// <returns>ベクトルの長さの2乗。</returns>
		public double GetLengthSquare()
		{
			return ((this.X * this.X) + (this.Y * this.Y));
		}

		/// <summary>
		/// このベクトルと、対象のベクトルの内積を取得します。
		/// </summary>
		/// <param name="v">対象のベクトル。</param>
		/// <returns>このベクトルと、対象のベクトルの内積。</returns>
		public double GetDot(Vector2D vector)
		{
			return this.X * vector.X + this.Y * vector.Y;
		}

		/// <summary>
		/// 対象のベクトルがこのベクトルに対し、時計回り方向にあるかどうかを取得します。
		/// </summary>
		/// <param name="vector">対象のベクトル。</param>
		/// <returns>ベクトルの方向。</returns>
		public Clockwise GetClockwise(Vector2D vector)
		{
			if ((this.X * vector.Y) < (this.Y * vector.X))
			{
				return Clockwise.Clockwise;
			}
			else
			{
				return Clockwise.Anticlockwise;
			}

		}

		/// <summary>
		/// このベクトルに垂直なベクトルを取得します。
		/// </summary>
		/// <returns></returns>
		public Vector2D GetPerp()
		{
			return new Vector2D(-this.Y, this.X);
		}

		/// <summary>
		/// このベクトルと対象のベクトルとの間の距離を取得します。
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public double GetDistance(Vector2D vector)
		{
			double xSeparation = vector.X - this.X;
			double ySeparation = vector.Y - this.Y;

			return Math.Sqrt((xSeparation * xSeparation) + (ySeparation * ySeparation));
		}

		/// <summary>
		/// このベクトルと対象のベクトルとの間の距離の2乗を取得します。
		/// </summary>
		/// <param name="vector"></param>
		/// <returns></returns>
		public double GetDistanceSquare(Vector2D vector)
		{
			double xSeparation = vector.X - this.X;
			double ySeparation = vector.Y - this.Y;

			return ((xSeparation * xSeparation) + (ySeparation * ySeparation));
		}

		/// <summary>
		/// ベクトルの長さを、最大値まで切り詰めます
		/// </summary>
		/// <param name="maxLength"></param>
		public void Truncate(double maxLength)
		{
			if (maxLength < this.GetLength())
			{
				this.Normalize();

				this *= maxLength;
			}
		}

		/// <summary>
		/// このベクトルを単位ベクトルに変換します。
		/// </summary>
		public void Normalize()
		{
			double length = this.GetLength();

			if (double.Epsilon < length)
			{
				this.X /= length;
				this.Y /= length;
			}
		}

		/// <summary>
		/// このベクトルの逆ベクトルを取得します。
		/// </summary>
		/// <returns></returns>
		public Vector2D GetReverse()
		{
			return new Vector2D(-this.X, -this.Y);
		}

		#endregion // public メソッド

	}
}
