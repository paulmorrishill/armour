using System;
using ArmControl.Kinematics;

namespace ArmControl
{
  public class ArmPresenter
  {
    public void ArmStateUpdated(Vector3D currentPosition)
    {
      
    }
  }

  public class ArmManipulator
  {
    private ArmController ArmController;
    private Vector3D CurrentPosition;
    private InverseKinematicsCalculator InverseKinematicsCalculator;
    private KinematicChain KinematicChain;
    private int CurrentServoPosition;
    private Vector3D LastSafePosition;
    private const double LargeIncrement = 0.01;
    private const double SmallIncrement = 0.001;


    public ArmManipulator(ArmController armController, InverseKinematicsCalculator inverseKinematicsCalculator, KinematicChain kinematicChain)
    {
      KinematicChain = kinematicChain;
      InverseKinematicsCalculator = inverseKinematicsCalculator;
      ArmController = armController;
      CurrentPosition = new Vector3D();
      CurrentServoPosition = 1400;
    }

    public void SetArmToCurrentPosition()
    {
      try
      {
        InverseKinematicsCalculator.AdjustKinematicChainForPosition(KinematicChain, CurrentPosition);
        var x = KinematicChain.InputLinks[0].Theta;
        var y = KinematicChain.InputLinks[1].Theta;
        var z = KinematicChain.InputLinks[2].Theta;
        ArmController.SetPosition(x, y, z, 16000);
        LastSafePosition = CurrentPosition;
      }
      catch (Exception)
      {
        CurrentPosition = LastSafePosition;
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

    public void IncrementServoPosition(int servoIndex)
    {
      CurrentServoPosition += 25;
      ArmController.SetServoPosition(servoIndex, CurrentServoPosition);
    }

  }
}