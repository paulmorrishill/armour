using System.Linq;

namespace ArmControl.Kinematics
{
  public class HillClimbingInverseKinematicsCalculator : InverseKinematicsCalculator
  {
    private double[] CurrentThetas;
    private double CurrentDelta;
    private int NumberOfThetas;
    private double LastDistanceToTarget;
    private KinematicChain KinematicChain;
    private Vector3D TargetPosition;
    private int RandomizeCount;

    public void AdjustKinematicChainForPosition(KinematicChain kinematicChain, Vector3D targetPosition)
    {
      RandomizeCount = 0;
      TargetPosition = targetPosition;
      KinematicChain = kinematicChain;
      SetCurrentThetasFromChain();
      CurrentDelta = 30;
      NumberOfThetas = kinematicChain.InputLinks.Select(link => link.Theta).ToArray().Length;
      LastDistanceToTarget = kinematicChain.CalculateResultantPosition().EuclidianDistanceTo(targetPosition);

      while (true)
      {
        var thisDeltaIsAxhausted = true;
        for (var i = NumberOfThetas-1; i >= 0; i--)
        {
          var positionWasImproved = TryToImproveThetaByUsingDelta(i);
          if (positionWasImproved) thisDeltaIsAxhausted = false;
        }
        if (thisDeltaIsAxhausted) 
          CurrentDelta /= 2;

        if (LastDistanceToTarget < 0.00001)
        {
          ApplyThetas();
          return; //Reached goal
        }

        if (CurrentDelta < 0.00001)
          RandomizeThetas();
      }
    }

    private void SetCurrentThetasFromChain()
    {
      CurrentThetas = KinematicChain.InputLinks.Select(link => link.Theta).ToArray();
    }

    private void RandomizeThetas()
    {
      RandomizeCount++;
      if(RandomizeCount > 15) throw new UnreachablePositionException();
      KinematicChain.Randomize();
      SetCurrentThetasFromChain();
      LastDistanceToTarget = GetNewDistanceToTarget();
      CurrentDelta = 30;
    }


    private bool TryToImproveThetaByUsingDelta(int thetaIndex)
    {
      var original = CurrentThetas[thetaIndex];
      CurrentThetas[thetaIndex] = original + CurrentDelta;
      var distanceWithPositiveDelta = GetNewDistanceToTarget();
      var positiveInvalid = !IsChainValid();
      CurrentThetas[thetaIndex] = original - CurrentDelta;
      var distanceWithNegativeDelta = GetNewDistanceToTarget();
      var negativeInvalid = !IsChainValid();

      var improvementWithPositive = LastDistanceToTarget - distanceWithPositiveDelta;
      var improvementWithNegative = LastDistanceToTarget - distanceWithNegativeDelta;

      //Invalid positions do not count as an improvement
      if (negativeInvalid) 
        improvementWithNegative = -1;
      if (positiveInvalid) 
        improvementWithPositive = -1;
      
      if (improvementWithNegative <= 0 && improvementWithPositive <= 0)
      {
        CurrentThetas[thetaIndex] = original;
        return false;
      }

      if (improvementWithPositive > improvementWithNegative)
      {
        CurrentThetas[thetaIndex] = original + CurrentDelta;
        LastDistanceToTarget = distanceWithPositiveDelta;
        return true;
      }

      CurrentThetas[thetaIndex] = original - CurrentDelta;
      LastDistanceToTarget = distanceWithNegativeDelta;
      return true;
    }

    private bool IsChainValid()
    {
      ApplyThetas();
      return KinematicChain.IsValidPosition();
    }

    private void ApplyThetas()
    {
      for (var i = 0; i < CurrentThetas.Length; i++)
      {
        KinematicChain.InputLinks[i].SetTheta(CurrentThetas[i]);
      }
    }

    private double GetNewDistanceToTarget()
    {
      ApplyThetas();
      return KinematicChain.CalculateResultantPosition().EuclidianDistanceTo(TargetPosition);
    }
  }
}