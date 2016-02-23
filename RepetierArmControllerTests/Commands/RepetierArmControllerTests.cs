using Moq;
using RepetierArmController;
using RepetierArmController.Communication;
using Should;
using Xunit;

namespace RepetierArmControllerTests.Commands
{
  public class RepetierArmControllerTests
  {
    private Mock<GCodeCreator> GcodeGenerator;
    private RepetierCommunicatorSpy Communicator;
    private GcodeArmController Controller;

    public RepetierArmControllerTests()
    {
      GcodeGenerator = new Mock<GCodeCreator>();
      Communicator = new RepetierCommunicatorSpy();
      Controller = new GcodeArmController(Communicator, GcodeGenerator.Object);
    }

    [Fact]
    public void CanHomeArm()
    {
      GcodeGenerator.Setup(r => r.GenerateHomeCommand()).Returns("HOME");

      Controller.HomeArm();

      Communicator.LastSentCommand.ShouldEqual("HOME");
      Communicator.HasWaitedForReadySinceLastCommand.ShouldBeTrue();
    }

    [Fact]
    public void CanSendMoveArmCommand()
    {
      GcodeGenerator.Setup(g => g.GenerateMove(2, 5, 7, 8)).Returns("MOVE");

      Controller.SetPosition(2, 5, 7, 8);

      Communicator.LastSentCommand.ShouldEqual("MOVE");
      Communicator.HasWaitedForReadySinceLastCommand.ShouldBeTrue();
    }

    [Fact]
    public void CanSetDeviceStateToTrue()
    {
      GcodeGenerator.Setup(r => r.GenerateFanOnCommand(1)).Returns("DEVICEON");

      Controller.SetDeviceState(1, true);
      Communicator.LastSentCommand.ShouldEqual("DEVICEON");
      Communicator.HasWaitedForReadySinceLastCommand.ShouldBeTrue();
    }

    [Fact]
    public void CanSetDeviceStateToFalse()
    {
      GcodeGenerator.Setup(r => r.GenerateFanOffCommand(7)).Returns("DEVICEOFF");

      Controller.SetDeviceState(7, false);
      Communicator.LastSentCommand.ShouldEqual("DEVICEOFF");
      Communicator.HasWaitedForReadySinceLastCommand.ShouldBeTrue();
    }

    [Fact]
    public void CanSetServoPosition()
    {
      GcodeGenerator.Setup(r => r.GenerateServoMove(4, 123)).Returns("SERV");
      GcodeGenerator.Setup(r => r.GenerateDwellCommand(100)).Returns("DWELL");
      Controller.SetServoPosition(4, 123);

      Communicator.LastBeforeLastSentCommand.ShouldEqual("SERV");
      Communicator.LastSentCommand.ShouldEqual("DWELL");
      Communicator.HasWaitedForReadySinceLastCommand.ShouldBeTrue();
    }

    [Fact]
    public void ItCanConnectToArm()
    {
      var mockPort = new Mock<RepetierCommunicator>();
      var controller = new GcodeArmController(mockPort.Object, null);

      controller.ConnectToArm("PORT");

      mockPort.Verify(r => r.Connect("PORT"));
      mockPort.Verify(r => r.WaitForReady());
    }
  }
}