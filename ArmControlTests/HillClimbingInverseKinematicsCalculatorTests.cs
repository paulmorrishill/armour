using ArmControl.Kinematics;
using ArmControl.Kinematics.Dobot;
using Should;
using Xunit;
using Xunit.Sdk;

namespace ArmControlTests
{
  public class HillClimbingInverseKinematicsCalculatorTests
  {
    private DobotDhKinematicChain KinematicChain;
    private HillClimbingInverseKinematicsCalculator Calc;

    public HillClimbingInverseKinematicsCalculatorTests()
    {
      KinematicChain = new DobotDhKinematicChain();
      Calc = new HillClimbingInverseKinematicsCalculator();
    }

    [Fact]
    public void WhenAPositionIsUnreachableItThrowsAnUnreachablePositionException()
    {
      Assert.Throws<UnreachablePositionException>(() => TryToAchievePosition(10, 0, 0));
    }

    [Fact]
    public void ItCanAchieveAnAchievablePosition()
    {
      TryToAchievePosition(0.160, 0.0, 0.235);
    }

    [Fact]
    public void ItCanAchieveAnAchievablePositionUsingAllThetas()
    {
      TryToAchievePosition(0.194, 0.146, 0.138);
    }

    [Fact]
    public void ItCanAvhieveAnAchievablePositionWhereTheFirstAttemptWillProduceAnInvalidPosition()
    {
      TryToAchievePosition(0.2, 0.0, 0.1);
    }

    [Fact]
    public void ItCanAchieveSomeProblemPositions()
    {
      TryToAchievePosition(0, 0.179, 0.05);
    }

    private void TryToAchievePosition(double x, double y, double z)
    {
      Calc.AdjustKinematicChainForPosition(KinematicChain, new Vector3D
      {
        X = x,
        Y = y,
        Z = z
      });

      var pos = KinematicChain.CalculateResultantPosition();
      pos.X.ShouldEqual(x, .0001);
      pos.Y.ShouldEqual(y, .0001);
      pos.Z.ShouldEqual(z, .0001);

      KinematicChain.IsValidPosition().ShouldBeTrue();
    }
  }
}