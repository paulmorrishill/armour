using System;
using RepetierArmController.Communication;

namespace RepetierArmControllerTests
{
  public class RepetierCommunicatorSpy : RepetierCommunicator
  {
    public string LastSentCommand;
    public bool HasWaitedForReadySinceLastCommand;

    public void SendCommand(string command)
    {
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