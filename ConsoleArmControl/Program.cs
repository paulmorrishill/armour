using System;
using System.Collections.Generic;
using ArmControl.Kinematics;
using ArmControl.Kinematics.Dobot;
using RepetierArmController;
using RepetierArmController.Communication;

namespace ConsoleArmControl
{
  class Program
  {
    private static GcodeArmController _controller;
    private static DobotDhKinematicChain _kinematicChain;
    private static Vector3D _lastSafePosition;
    private static Vector3D _currentPosition;
    private static int _currentServoPos1;
    private static int _currentServoPos2;

    static void Main(string[] args)
    {
      var con = new SerialRepetierCommunicator(new SerialPortSerialConnection());
      var gcodeCreator = new RepetierGCodeCreator();
      _controller = new GcodeArmController(con, gcodeCreator);
      _kinematicChain = new DobotDhKinematicChain();
      var positionSlots  = new Dictionary<int, Vector3D>();
      _currentServoPos1 = 1400;
      _currentServoPos2 = 1400;
      _controller.ConnectToArm("COM25");
      _controller.HomeArm();
      var homePosition = new Vector3D(0.061, 0.0, 0.1);
      _currentPosition = homePosition;
      _lastSafePosition = _currentPosition;

      var stateList = new List<State>();

      while (true)
      {
        var key = Console.ReadKey(true);
        var change = 0.01;
        var servoChange = 25;
        if (key.Modifiers == ConsoleModifiers.Shift)
        {
          change = 0.001;
          servoChange = 10;
        }
        switch (key.Key)
        {
          case ConsoleKey.LeftArrow:
            _currentPosition.X -= change;
            break;
          case ConsoleKey.RightArrow:
            _currentPosition.X += change;
            break;
          case ConsoleKey.UpArrow:
            _currentPosition.Y += change;
            break;
          case ConsoleKey.DownArrow:
            _currentPosition.Y -= change;
            break;
        }

        switch (key.KeyChar.ToString().ToLower())
        {
          case "-":
            _currentPosition.Z -= change;
            break;
          case "+":
            _currentPosition.Z += change;
            break;
          case "h":
            _controller.HomeArm();
            _currentPosition = homePosition;
            break;
          case "s":
            _currentServoPos1 += servoChange;
            _controller.SetServoPosition(0, _currentServoPos1);
            break;
          case "w":
            _currentServoPos1 -= servoChange;
            _controller.SetServoPosition(0, _currentServoPos1);
            break;
          case "a":
            _currentServoPos2 += servoChange;
            _controller.SetServoPosition(1, _currentServoPos2);
            break;
          case "d":
            _currentServoPos2 -= servoChange;
            _controller.SetServoPosition(1, _currentServoPos2);
            break;
          case " ":
            stateList.Add(new State
            {
              Position = _currentPosition,
              ServoPosition1 = _currentServoPos1,
              ServoPosition2 = _currentServoPos2
            });
            break;
          case "p":
            
            break;
        }

        int positionNum;
        bool isNumber = int.TryParse(key.KeyChar.ToString(), out positionNum);
        if (isNumber)
        {
          if (key.Modifiers == ConsoleModifiers.Alt)
          {
            if (positionSlots.ContainsKey(positionNum))
            {
              positionSlots[positionNum] = _currentPosition;
            }
            else
            {
              positionSlots.Add(positionNum, _currentPosition);
            }
          }
          else
          {
            if(positionSlots.ContainsKey(positionNum)) _currentPosition = positionSlots[positionNum];
          }
        }

        try
        {
          AdjustChainForPosition(_kinematicChain, _currentPosition.X, _currentPosition.Y, _currentPosition.Z);
          ApplyChain(_kinematicChain, _controller);
          var x = _kinematicChain.InputLinks[0].Theta;
          var y = _kinematicChain.InputLinks[1].Theta;
          var z = _kinematicChain.InputLinks[2].Theta;
          Console.Clear();
          _lastSafePosition = _currentPosition;
          Console.WriteLine($"Current position ({_currentPosition.X}, {_currentPosition.Y}, {_currentPosition.Z})");
          Console.WriteLine($"Current angles ({x}°, {y}°, {z}°)");
        }
        catch (UnreachablePositionException)
        {
          _currentPosition = _lastSafePosition;
          Console.WriteLine($"Could not reach position: {_currentPosition.X}, {_currentPosition.Y}, {_currentPosition.Z}");
        }

        Console.WriteLine($"Servo 1 {_currentServoPos1} Servo 2 {_currentServoPos2}");

        Console.WriteLine("");
      }

    }

    private static List<Vector3D> Interpolate(Vector3D vector1, Vector3D vector2, int amount)
    {
      var deltaX = vector2 - vector1;
      deltaX /= amount;
      var interpolated = new List<Vector3D>();
      
      for(var i = 0; i < amount; i++)
        interpolated.Add(vector1 + (deltaX * i));

      return interpolated;
    }

    private static void AdjustChainForPosition(KinematicChain chain, double x, double y, double z)
    {
      var calc = new HillClimbingInverseKinematicsCalculator();
      calc.AdjustKinematicChainForPosition(chain, new Vector3D
      {
        X = x,
        Y = y,
        Z = z
      });
    }

    private static void ApplyChain(KinematicChain chain, GcodeArmController controller)
    {
      var links = chain.InputLinks;
      controller.SetPosition(links[0].Theta, links[1].Theta, links[2].Theta, 14000);
    }
  }

  class State
  {
    public Vector3D Position { get; set; }
    public int ServoPosition1 { get; set; }
    public int ServoPosition2 { get; set; }
  }
}
