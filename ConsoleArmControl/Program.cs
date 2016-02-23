using System;
using System.Collections.Generic;
using ArmControl;
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
            var con = new SerialRepetierCommunicator(new SerialPortSerialConnection());
            var gcodeCreator = new RepetierGCodeCreator();
            var controller = new GcodeArmController(con, gcodeCreator);
            controller.ConnectToArm("COM25");

            var manipulator = new ArmManipulator(controller, new HillClimbingInverseKinematicsCalculator(), new DobotDhKinematicChain());
            controller.HomeArm();

            while (true)
            {
                var key = Console.ReadKey(true);
                var precisely = key.Modifiers == ConsoleModifiers.Shift;
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        manipulator.DecrementX(precisely);
                        break;
                    case ConsoleKey.RightArrow:
                        manipulator.IncrementX(precisely);
                    break;
                    case ConsoleKey.UpArrow:
                        manipulator.IncrementY(precisely);
                        break;
                    case ConsoleKey.DownArrow:
                        manipulator.DecrementY(precisely);
                        break;
                }

                switch (key.KeyChar.ToString().ToLower())
                {
                    case "-":
                        manipulator.DecrementZ(precisely);
                        break;
                    case "+":
                        manipulator.IncrementZ(precisely);
                        break;
                    case "h":
                        manipulator.HomeArm();
                        break;
                    case "s":
                        manipulator.IncrementServoPosition(0, precisely);
                        break;
                    case "w":
                        manipulator.DecrementServoPosition(0, precisely);
                        break;
                    case "a":
                        manipulator.IncrementServoPosition(1, precisely);
                        break;
                    case "d":
                        manipulator.DecrementServoPosition(1, precisely);
                        break;
                    case " ":
                        manipulator.RecordStep();
                        break;
                    case "p":
                        manipulator.PlayBackSteps();
                        break;
                }

                int positionNum;
                bool isNumber = int.TryParse(key.KeyChar.ToString(), out positionNum);

                /*Console.WriteLine($"Could not reach position: {_currentPosition.X}, {_currentPosition.Y}, {_currentPosition.Z}");

              Console.WriteLine($"Servo 1 {_currentServoPos1} Servo 2 {_currentServoPos2}");

              Console.WriteLine("");*/
            }

        }
    }
}
