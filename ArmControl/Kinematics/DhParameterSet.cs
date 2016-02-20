using System;

namespace ArmControl.Kinematics
{
  public class DhParameterSet
  {
    public double R { get; protected set; } //Distance along the common normal
    public double Alpha { get; protected set; } //The angle of the new Z axis relative to the previous Z e.g. 90degrees
    public double D { get; protected set; } //The distance along the previous Z to the common normal
    public double Theta { get; protected set; } //The angle around the previous z between the old and new axes

    public DhParameterSet(double d, double r, double alpha)
    {
      R = r;
      Alpha = alpha;
      D = d;
    }

    public void SetTheta(double theta)
    {
      Theta = theta;
    }

    public Matrix4D ToMatrix()
    {
      var matrix = new Matrix4D();

      matrix.Values[0, 0] = Cos(Theta);
      matrix.Values[0, 1] = Sin(Theta);
      matrix.Values[0, 2] = 0;
      matrix.Values[0, 3] = 0;

      matrix.Values[1, 0] = -Sin(Theta)*Cos(Alpha);
      matrix.Values[1, 1] = Cos(Theta)*Cos(Alpha);
      matrix.Values[1, 2] = Sin(Alpha);
      matrix.Values[1, 3] = 0;

      matrix.Values[2, 0] = Sin(Theta)*Sin(Alpha);
      matrix.Values[2, 1] = -Cos(Theta)*Sin(Alpha);
      matrix.Values[2, 2] = Cos(Alpha);
      matrix.Values[2, 3] = 0;

      matrix.Values[3, 0] = R*Cos(Theta);
      matrix.Values[3, 1] = R*Sin(Theta);
      matrix.Values[3, 2] = D;
      matrix.Values[3, 3] = 1;
      
      return matrix;
    }

    private double DegToRad (double degrees) => (Math.PI / 180) * degrees;
    private double Sin (double a) => Math.Sin(DegToRad(a));
    private double Cos (double a) => Math.Cos(DegToRad(a));
  }
}