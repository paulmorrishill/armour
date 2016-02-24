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

        private string R(double v)
        {
            return Math.Round(v, 2).ToString();
        }

        private void Render()
        {
            Console.Clear();
            Console.WriteLine($"Current position ({R(CurrentPosition.X)}, {R(CurrentPosition.Y)}, {R(CurrentPosition.Z)})");
            Console.Write("Servo positions");
            foreach (var servoPosition in ServoPositions)
            {
                Console.Write($" {servoPosition.Key}: {servoPosition.Value}");
            }
            Console.WriteLine();
            if (PositionUnreachable)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Attempted position move but position was unreachable.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine($"{StepsInRecording} steps in recording - Press P to play back.");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(" ======================= ");
            Console.WriteLine();
            Console.WriteLine("Controls");
            Console.WriteLine("Arrows move arm in XY plane, +- moves in Z.");    
            Console.WriteLine("WASD for servo control.");    
            Console.WriteLine("Space bar adds step to recording. P plays back recording. DEL clears recording.");    
        }
    }
}
