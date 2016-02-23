using ArmControl;
using RepetierArmController.Communication;

namespace RepetierArmController
{
    public class GcodeArmController : ArmController
    {
        private RepetierCommunicator Communicator;
        private GCodeCreator GcodeCreator;

        public GcodeArmController(RepetierCommunicator communicator, GCodeCreator gcodeCreator)
        {
            Communicator = communicator;
            GcodeCreator = gcodeCreator;
        }

        public void ConnectToArm(string port)
        {
            Communicator.Connect(port);
            Communicator.WaitForReady();
        }

        public void HomeArm()
        {
            Communicator.SendCommand(GcodeCreator.GenerateHomeCommand());
            Communicator.WaitForReady();
        }

        public void SetPosition(double x, double y, double z, double feedRate)
        {
            Communicator.SendCommand(GcodeCreator.GenerateMove(x, y, z, feedRate));
            Communicator.WaitForReady();
        }

        public void SetServoPosition(int servoIndex, int position)
        {
            Communicator.SendCommand(GcodeCreator.GenerateDwellCommand(100));
            Communicator.WaitForReady();
            Communicator.SendCommand(GcodeCreator.GenerateServoMove(servoIndex, position));
            Communicator.WaitForReady();
            Communicator.SendCommand(GcodeCreator.GenerateDwellCommand(100));
            Communicator.WaitForReady();
        }

        public void SetDeviceState(int deviceIndex, bool state)
        {
            var command = state
                            ? GcodeCreator.GenerateFanOnCommand(deviceIndex)
                            : GcodeCreator.GenerateFanOffCommand(deviceIndex);

            Communicator.SendCommand(command);
            Communicator.WaitForReady();
        }

    }
}