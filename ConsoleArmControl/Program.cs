using System;
using System.Globalization;
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

            var manipulator = new ArmManipulator(controller, new ConsoleArmPresenter(), new HillClimbingInverseKinematicsCalculator(), new DobotDhKinematicChain());
            //manipulator.HomeArm();

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
                    case ConsoleKey.Backspace:
                        manipulator.DeleteLastStep();
                        break;
                    case ConsoleKey.Delete:
                        manipulator.ClearRecording();
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
                    case "o":
                        manipulator.AddDwellStep();
                        break;
                    case "g":
                        Console.Write("Play back specific step in recording: ");
                        var step = Console.ReadLine();
                        int stepIndex;
                        var enteredNumber = int.TryParse(step, out stepIndex);
                        if (enteredNumber)
                        {
                            manipulator.PlayBackStep(stepIndex);
                        }
                        break;
                    case "c":
                        Console.Write("Enter coordinates: ");
                        var newPos = Console.ReadLine();
                        if (newPos == "")
                            break;
                        var coords = newPos.Split(',');
                        manipulator.SetPosition(GetDoubleFromString(coords[0]), GetDoubleFromString(coords[1]), GetDoubleFromString(coords[2]));
                        break;
                }
            }
        }

        private static double GetDoubleFromString(string input)
        {
            return double.Parse(input, NumberStyles.Any);
        }
    }
}
