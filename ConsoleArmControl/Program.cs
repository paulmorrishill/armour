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
    static void Main(string[] args)
    {
      var serial = new SerialPortSerialConnection();
      var con = new SerialRepetierCommunicator(serial);
      var gcodeCreator = new RepetierGCodeCreator();
      var controller = new GcodeArmController(con, gcodeCreator);
      var kinematicChain = new DobotDhKinematicChain();
      
      controller.ConnectToArm("COM23");
      controller.HomeArm();
      var homePosition = new Vector3D(0.061, 0.0, 0.1);
      Vector3D currentPosition = homePosition.Clone();
      Vector3D lastSafePosition = currentPosition.Clone();

      while (true)
      {
        var key = Console.ReadKey();
        var change = 0.01;
        if (key.Modifiers == ConsoleModifiers.Shift)
        {
          change = 0.001;
        }
        switch (key.Key)
        {
          case ConsoleKey.LeftArrow:
            currentPosition.X -= change;
            break;
          case ConsoleKey.RightArrow:
            currentPosition.X += change;
            break;
          case ConsoleKey.UpArrow:
            currentPosition.Y += change;
            break;
          case ConsoleKey.DownArrow:
            currentPosition.Y -= change;
            break;
        }

        switch (key.KeyChar)
        {
          case '-':
            currentPosition.Z -= change;
            break;
          case '+':
            currentPosition.Z += change;
            break;
          case 'h':
            controller.HomeArm();
            currentPosition = homePosition.Clone();
            break;
        }

        try
        {
          AdjustChainForPosition(kinematicChain, currentPosition.X, currentPosition.Y, currentPosition.Z);
          ApplyChain(kinematicChain, controller);
          var x = kinematicChain.InputLinks[0].Theta;
          var y = kinematicChain.InputLinks[1].Theta;
          var z = kinematicChain.InputLinks[2].Theta;
          Console.WriteLine($"Moved to position ({currentPosition.X}, {currentPosition.Y}, {currentPosition.Z}) ({x}°, {y}°, {z}°)");
          lastSafePosition = currentPosition.Clone();
        }
        catch (UnreachablePositionException)
        {
          Console.WriteLine("Could not reach position.");
          currentPosition = lastSafePosition.Clone();
        }
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
}
