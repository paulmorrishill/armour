using System;
using System.Collections.Generic;
using System.Linq;
using ArmControl.Kinematics;

namespace ArmControl
{
    public class ArmManipulator
    {
        private ArmController ArmController;
        private readonly ArmPresenter Presenter;
        private Vector3D CurrentPosition;
        private InverseKinematicsCalculator InverseKinematicsCalculator;
        private KinematicChain KinematicChain;
        private Dictionary<int, int> CurrentServoPositions;
        private Vector3D LastSafePosition;
        private const double LargeIncrement = 0.01;
        private const double SmallIncrement = 0.001;

        private const int LargeServoIncrement = 25;
        private const int SmallServoIncrement = 10;

        private List<ArmState> Recording { get; }

        public ArmManipulator(ArmController armController, ArmPresenter presenter, InverseKinematicsCalculator inverseKinematicsCalculator,
            KinematicChain kinematicChain)
        {
            KinematicChain = kinematicChain;
            InverseKinematicsCalculator = inverseKinematicsCalculator;
            ArmController = armController;
            Presenter = presenter;
            CurrentPosition = new Vector3D
            {
                X = 0.061,
                Y = 0.0,
                Z = 0.1
            };
            CurrentServoPositions = new Dictionary<int, int>();
            Recording = new List<ArmState>();
        }

        private void SetArmToCurrentPosition()
        {
            try
            {
                InverseKinematicsCalculator.AdjustKinematicChainForPosition(KinematicChain, CurrentPosition);
                var x = KinematicChain.InputLinks[0].Theta;
                var y = KinematicChain.InputLinks[1].Theta;
                var z = KinematicChain.InputLinks[2].Theta;
                ArmController.SetPosition(x, y, z, 16000);
                LastSafePosition = CurrentPosition;
                Presenter.ArmPositionChanged(CurrentPosition);
            }
            catch (UnreachablePositionException)
            {
                CurrentPosition = LastSafePosition;
                Presenter.TargetPositionUnreachable();
            }
        }

        public Vector3D GetCurrentArmPosition()
        {
            return CurrentPosition;
        }

        public void IncrementY(bool precisely = false)
        {
            CurrentPosition.Y += GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }

        public void IncrementZ(bool precisely = false)
        {
            CurrentPosition.Z += GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }

        public void IncrementX(bool precisely = false)
        {
            CurrentPosition.X += GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }


        public void DecrementY(bool precisely = false)
        {
            CurrentPosition.Y -= GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }

        public void DecrementZ(bool precisely = false)
        {
            CurrentPosition.Z -= GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }

        public void DecrementX(bool precisely = false)
        {
            CurrentPosition.X -= GetMovementIncrement(precisely);
            SetArmToCurrentPosition();
        }

        private double GetMovementIncrement(bool isPrecise)
        {
            return isPrecise ? SmallIncrement : LargeIncrement;
        }

        public void HomeArm()
        {
            CurrentPosition = new Vector3D(0.061, 0.0, 0.1);
            ArmController.HomeArm();
            SetArmToCurrentPosition();
        }

        public void IncrementServoPosition(int servoIndex, bool precisely = false)
        {
            InitServoPosition(servoIndex);
            CurrentServoPositions[servoIndex] += precisely ? SmallServoIncrement : LargeServoIncrement;
            SetServoPosition(servoIndex, CurrentServoPositions[servoIndex]);
        }

        private void InitServoPosition(int servoIndex)
        {
            if (CurrentServoPositions.ContainsKey(servoIndex)) return;
            CurrentServoPositions.Add(servoIndex, 1400);
        }

        public void SetPosition(double x, double y, double z)
        {
            CurrentPosition.X = x;
            CurrentPosition.Y = y;
            CurrentPosition.Z = z;
            SetArmToCurrentPosition();
        }

        public void SetServoPosition(int servoIndex, int position)
        {
            InitServoPosition(servoIndex);
            ArmController.SetServoPosition(servoIndex, position);
            CurrentServoPositions[servoIndex] = position;
            Presenter.ServoPositionChanged(servoIndex, position);
        }

        public void RecordStep()
        {
            var clonedServoPositions = CurrentServoPositions.ToDictionary(entry => entry.Key,
                entry => entry.Value);
            Recording.Add(new ArmState
            {
                Position = CurrentPosition,
                ServoPositions = clonedServoPositions
            });
            NumberOfStepsChanged();
        }

        public void PlayBackSteps()
        {
            foreach (var armState in Recording)
            {
                ApplyState(armState);
            }
        }

        private void ApplyState(ArmState armState)
        {
            if (armState.Dwell != null)
            {
                ArmController.Dwell(armState.Dwell.Value);
                return;
            }
            CurrentPosition = armState.Position;
            SetArmToCurrentPosition();
            foreach (var servoPos in armState.ServoPositions)
            {
                if (CurrentServoPositions[servoPos.Key] == servoPos.Value)
                    continue;
                SetServoPosition(servoPos.Key, servoPos.Value);
            }
        }

        private class ArmState
        {
            public Vector3D Position;
            public Dictionary<int, int> ServoPositions { get; set; }
            public int? Dwell { get; set; }
        }

        public void DecrementServoPosition(int servoIndex, bool precisely = false)
        {
            InitServoPosition(servoIndex);
            CurrentServoPositions[servoIndex] -= precisely ? SmallServoIncrement : LargeServoIncrement;
            SetServoPosition(servoIndex, CurrentServoPositions[servoIndex]);
        }

        public void ClearRecording()
        {
            Recording.Clear();
            NumberOfStepsChanged();
        }

        public void PlayBackStep(int stepIndex)
        {
            if (stepIndex > Recording.Count - 1)
            {
                Presenter.StepSpecifiedNotFound();
                return;
            }
            ApplyState(Recording[stepIndex]);
        }

        public void DeleteLastStep()
        {
            if (Recording.Count == 0) return;
            Recording.RemoveAt(Recording.Count - 1);
            NumberOfStepsChanged();
        }

        public void AddDwellStep()
        {
            Recording.Add(new ArmState
            {
                Dwell = 1000
            });
            NumberOfStepsChanged();
        }

        public void NumberOfStepsChanged()
        {
            Presenter.NumberOfStepsInRecordingChanged(Recording.Count);
        }
    }
}