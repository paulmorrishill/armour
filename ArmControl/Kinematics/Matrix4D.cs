namespace ArmControl.Kinematics
{
  public class Matrix4D 
  {
    public double[,] Values { get; }
    public static Matrix4D operator +(Matrix4D matrix1, Matrix4D matrix2)
    {
      var resultant = new Matrix4D();
      for (var i = 0; i < resultant.Values.GetLength(0); i++)
      {
        for (var j = 0; j < resultant.Values.GetLength(0); j++)
        {
          resultant.Values[i, j] = matrix1.Values[i, j] + matrix2.Values[i, j];
        }
      }
      return resultant;
    }

    public static Matrix4D operator *(Matrix4D matrix1, Matrix4D matrix2)
    {
      var a = matrix2.Values;
      var b = matrix1.Values;
      var c = new double[a.GetLength(0), b.GetLength(1)];
      for (int i = 0; i < c.GetLength(0); i++)
      {
        for (int j = 0; j < c.GetLength(1); j++)
        {
          c[i, j] = 0;
          for (int k = 0; k < a.GetLength(1); k++) // OR k<b.GetLength(0)
            c[i, j] = c[i, j] + a[i, k] * b[k, j];
        }
      }

      return new Matrix4D(c);
    }

    public Matrix4D()
    {
      Values = new double[4, 4];
    }
    
    public Matrix4D(double[,] values)
    {
      Values = values;
    }
  }
}