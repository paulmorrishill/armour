using System.Threading;
using System.Threading.Tasks;
using Moq;
using RepetierArmController.Communication;
using Should;
using Xunit;

namespace RepetierArmControllerTests
{
  public class SerialRepetierCommunicatorTests
  {
    private SerialRepetierCommunicator Communicator;
    private Mock<SerialConnection> MockSerialConnection;

    public SerialRepetierCommunicatorTests()
    {
      MockSerialConnection = new Mock<SerialConnection>();
      Communicator = new SerialRepetierCommunicator(MockSerialConnection.Object);
      MockSerialConnection.Setup(r => r.ReadLine()).Returns("");
    }

    [Fact]
    public void ItShouldBeAbleToIssueACommandDownTheSerialPort()
    {
      Communicator.SendCommand("TEST");
      MockSerialConnection.Verify(s => s.WriteLine("TEST"));
    }

    [Fact]
    public void ItShouldConnectTheSerialPort()
    {
      Communicator.Connect("PORT");
      MockSerialConnection.Verify(r => r.Connect("PORT"));
    }

    [Fact]
    public void ItWillWaitForTheNextOkAckWhenAskedToWait()
    {
      var t = new Task(() => Communicator.WaitForReady());
      t.Start();
      Thread.Sleep(50); //100% sure now that it's waiting
      t.Status.ShouldEqual(TaskStatus.Running);

      MockSerialConnection.Setup(r => r.ReadLine()).Returns("ok");

      Thread.Sleep(50); //Give it a chance to finish waiting
      t.Status.ShouldEqual(TaskStatus.RanToCompletion);      
    }

    [Fact]
    public void ItWillNotFinishWaitingBeforeHearingWait()
    {
      var t = new Task(() => Communicator.WaitForReady());
      t.Start();
      MockSerialConnection.Setup(r => r.ReadLine()).Returns("not ok");
      Thread.Sleep(50); //Give it a chance to finish waiting
      t.Status.ShouldEqual(TaskStatus.RanToCompletion);
    }

  }
}