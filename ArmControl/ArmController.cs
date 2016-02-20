namespace ArmControl
{
    public interface ArmController
    {
      void ConnectToArm(string port);
      void HomeArm();
      void SetPosition(double x, double y, double z, double feedRate);
      void SetServoPosition(int servoIndex, int position);
      void SetDeviceState(int deviceIndex, bool state);
    }
}
