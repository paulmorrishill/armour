using System;

namespace ArmControl.Kinematics
{
  public struct Vector3D
  {
    public double X;
    public double Y;
    public double Z;
    public Vector3D(double x, double y, double z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public static Vector3D operator -(Vector3D a, Vector3D b)
    {
      return new Vector3D(a.X-b.X, a.Y-b.Y, a.Z-b.Z);
    }

    public static Vector3D operator +(Vector3D a, Vector3D b)
    {
      return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vector3D operator /(Vector3D a, int b)
    {
      return new Vector3D(a.X / b, a.Y / b, a.Z / b);
    }

    public static Vector3D operator *(Vector3D a, int b)
    {
      return new Vector3D(a.X * b, a.Y * b, a.Z * b);
    }

    public double Difference(Vector3D vector)
    {
      var dx = Math.Abs(X - vector.X);
      var dy = Math.Abs(Y - vector.Y);
      var dz = Math.Abs(Z - vector.Z);
      return dx + dy + dz;
    }

    public double EuclidianDistanceTo(Vector3D vector)
    {
      var dx = X - vector.X;
      var dy = Y - vector.Y;
      var dz = Z - vector.Z;
      return Math.Sqrt(dx*dx + dy*dy + dz*dz);
    }

    public Vector3D Clone()
    {
      return new Vector3D(X, Y, Z);
    }
  }
}