using System;
using System.IO.Ports;
using System.Text;

namespace RepetierArmController.Communication
{
  public class SerialPortSerialConnection : SerialConnection
  {
    private SerialPort SerialPort;
    private string ReceivedData = "";

    public void Connect(string portName)
    {
      SerialPort = new SerialPort(portName, 115200);
      SerialPort.Open();
    }

    public void WriteLine(string line)
    {
      SerialPort.WriteLine(line);
    }

    public bool IsConnected() => SerialPort.IsOpen;

    public string ReadLine()
    {
      while(!ReceivedData.Contains("\r"))
      {
        var inData = Encoding.ASCII.GetString(new []{ (byte)SerialPort.ReadByte() });
        ReceivedData += inData;
      }
      var outData = ReceivedData;
      ReceivedData = "";
      return outData;
    }
  }
}
