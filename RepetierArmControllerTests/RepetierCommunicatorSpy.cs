using System;
using RepetierArmController.Communication;

namespace RepetierArmControllerTests
{
  public class RepetierCommunicatorSpy : RepetierCommunicator
  {
    public string LastSentCommand;
    public string LastBeforeLastSentCommand;
    public bool HasWaitedForReadySinceLastCommand;

    public void SendCommand(string command)
    {
      LastBeforeLastSentCommand = LastSentCommand;
      LastSentCommand = command;
      HasWaitedForReadySinceLastCommand = false;
    }

    public void WaitForReady()
    {
      HasWaitedForReadySinceLastCommand = true;
    }

    public void Connect(string port)
    {
      throw new NotImplementedException();
    }
  }
}