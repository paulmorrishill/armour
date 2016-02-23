namespace RepetierArmController
{
  public interface GCodeCreator
  {
    string GenerateFanOffCommand(int fanNumber);
    string GenerateFanOnCommand(int fanNumber);
    string GenerateMove(double x, double y, double z, double feedRate);
    string GenerateServoMove(int servoIndex, int value);
    string GenerateHomeCommand();
    string GenerateDwellCommand(int milliseconds);
  }
}