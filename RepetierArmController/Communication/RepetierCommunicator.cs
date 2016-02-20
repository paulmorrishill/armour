namespace RepetierArmController.Communication
{
  public interface RepetierCommunicator
  {
    void SendCommand(string command);
    void WaitForReady();
    void Connect(string port);
  }
}