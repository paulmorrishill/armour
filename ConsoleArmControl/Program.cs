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

            var manipulator = new ArmManipulator(controller, new ConsoleArmPresenter(), new HillClimbingInverseKinematicsCalculator(), new DobotDhKinematicChain());
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
                }
            }

        }
    }

    internal class ConsoleArmPresenter : ArmPresenter
    {
        private Vector3D CurrentPosition;
        private bool PositionUnreachable;
        private Dictionary<int, int> ServoPositions = new Dictionary<int, int>();
        private int StepsInRecording;
        private bool StepWasNotFound;

        public ConsoleArmPresenter()
        {
            Render();
        }

        public void ArmPositionChanged(Vector3D currentPosition)
        {
            CurrentPosition = currentPosition;
            PositionUnreachable = false;
            Render();
        }

        public void TargetPositionUnreachable()
        {
            PositionUnreachable = true;
            Render();
        }

        public void ServoPositionChanged(int servoIndex, int position)
        {
            if (!ServoPositions.ContainsKey(servoIndex))
                ServoPositions.Add(servoIndex, 0);
            ServoPositions[servoIndex] = position;
            Render();
        }

        public void NumberOfStepsInRecordingChanged(int numberOfSteps)
        {
            StepsInRecording = numberOfSteps;
            Render();
        }

        public void StepSpecifiedNotFound()
        {
            StepWasNotFound = true;
            Render();
        }

        private string R(double v)
        {
            return Math.Round(v, 3).ToString();
        }

        private void PrintLine(string line, ConsoleColor color)
        {
            var colorBefore = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = colorBefore;
        }

        private void Render()
        {
            Console.Clear();
            PrintLine("Arm State", ConsoleColor.Yellow);
            Console.WriteLine($"Current position ({R(CurrentPosition.X)}, {R(CurrentPosition.Y)}, {R(CurrentPosition.Z)})");
            Console.Write("Servo positions");
            foreach (var servoPosition in ServoPositions)
            {
                Console.Write($" {servoPosition.Key}: {servoPosition.Value}");
            }
            Console.WriteLine();

            if (PositionUnreachable)
                PrintLine("Attempted position move but position was unreachable.", ConsoleColor.Red);
            
            Console.WriteLine($"{StepsInRecording} steps in recording - Press P to play back.");
            if (StepWasNotFound)
            {
                PrintLine("Step specified was not found", ConsoleColor.Red);
                StepWasNotFound = false;
            }

            Console.WriteLine();
            Console.WriteLine();

            PrintLine("Controls", ConsoleColor.Yellow);
            Console.WriteLine("Arrows move arm in XY plane");
            Console.WriteLine("+/- moves Z up and down");    
            Console.WriteLine("WASD for servo control");

            Console.WriteLine();
            Console.WriteLine();

            PrintLine("Record and playback", ConsoleColor.Yellow);
            Console.WriteLine("Space bar adds step to recording");
            Console.WriteLine("P plays back recording");
            Console.WriteLine("G to go to a specific step in the recording");
            Console.WriteLine("O to add a 1 second dwell step");
            Console.WriteLine("Backspace deletes last step in recording");
            Console.WriteLine("DEL clears recording");    
        }
    }
}
