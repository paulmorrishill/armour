namespace RepetierArmController.Communication
{
  public class SerialRepetierCommunicator : RepetierCommunicator
  {
    private SerialConnection Connection;

    public SerialRepetierCommunicator(SerialConnection connection)
    {
      Connection = connection;
    }

    public void Connect(string port)
    {
      Connection.Connect(port);
    }

    public void SendCommand(string command)
    {
      Connection.WriteLine(command);
    }

    private void WaitForOkOnSerial()
    {
      while (true)
      {
        var rec = Connection.ReadLine();
        if (rec.Contains("ok") || rec.Contains("wait"))
          break;
      }
    }
    public void WaitForReady()
    {
      WaitForOkOnSerial();
    }
  }
}