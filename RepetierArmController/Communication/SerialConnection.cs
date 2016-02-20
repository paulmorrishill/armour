namespace RepetierArmController.Communication
{
    public interface SerialConnection
    {
      void Connect(string portName);
      void WriteLine(string line);
      bool IsConnected();
      string ReadLine();
    }
}
