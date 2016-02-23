using System;

namespace RepetierArmController.Communication
{
  public class RepetierGCodeCreator : GCodeCreator
  {
    public string GenerateMove(double x, double y, double z, double feedRate)
    {
      return "G0 X" + GetSafedouble(x) + " Y" + GetSafedouble(y) + " Z" + GetSafedouble(z) + " F" + GetSafedouble(feedRate);
    }

    private string GetSafedouble(double v)
    {
      return Math.Round(v, 5).ToString();
    }

    public string GenerateServoMove(int servoIndex, int value)
    {
      return $"M340 P{servoIndex} S{value}";
    }

    public string GenerateHomeCommand()
    {
      return "G28";
    }

    public string GenerateDwellCommand(int milliseconds)
    {
      return $"G4 P{milliseconds}";
    }

    public string GenerateFanOnCommand(int fanNumber)
    {
      return $"M106 P{fanNumber}";
    }

    public string GenerateFanOffCommand(int fanNumber)
    {
      return $"M107 P{fanNumber}";
    }
  }
}
