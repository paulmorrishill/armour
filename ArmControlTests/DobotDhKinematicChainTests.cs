using ArmControl.Kinematics.Dobot;
using Should;
using Xunit;

namespace ArmControlTests
{
  public class DobotDhKinematicChainTests
  {
    // (90 to 0°)    (0 to -90°)
    //  @================@========
    //  | The arm is setup so that toward the base is negative
    //  | Arm segment one straight vertical is 90 horizontal is 0
    //  | Arm segment two horizontal is 0 straight down is -90;
    //  |
    //[===] (0 to 360°)

    private DobotDhKinematicChain ArmChain;
    private const int SafeZPosition = -40;
    private const int VerticalYPosition = 90;
    private const int YHomePosition = 99;
    public DobotDhKinematicChainTests()
    {
      ArmChain = new DobotDhKinematicChain();
    }

    private void SetThetas(double t1, double t2, double t3)
    {
      ArmChain.InputLinks[0].SetTheta(t1);
      ArmChain.InputLinks[1].SetTheta(t2);
      ArmChain.InputLinks[2].SetTheta(t3);
    }

    [Fact]
    public void IncludesTheDobotHardwareConstraintsOnTheSecondArmWhenCalculatingTheResultantChain()
    {
      SetThetas(0, 60, 10);
      var resultantLinks = ArmChain.GetResultantLinks();
      resultantLinks[2].Theta.ShouldEqual(-50);
    }

    // Arm in position
    // (0°)   (0°)
    //  @=======@========
    //  |
    //  |
    //[===] (0°)
    [Fact]
    public void CanCalculateSimpleStraightLineExtendedOrientation()
    {
      SetThetas(0, 0, 0);
      var plainRightAnglePosition = ArmChain.CalculateResultantPosition();
      plainRightAnglePosition.X.ShouldEqual(0.290, 0.1);
      plainRightAnglePosition.Y.ShouldEqual(0.0, 0.1);
      plainRightAnglePosition.Z.ShouldEqual(0.105, 0.1);
    }

    // Arm in position
    // (0°)
    //  @=======
    //  |
    //  |
    //  @ (90°)
    //  |
    //  |
    //[===] (0°)
    [Fact]
    public void CanCalculateArmAtRightAngle()
    {
      SetThetas(0, 90, 0);
      var plainRightAnglePosition = ArmChain.CalculateResultantPosition();
      plainRightAnglePosition.X.ShouldEqual(0.160, 0.1);
      plainRightAnglePosition.Y.ShouldEqual(0.0, 0.1);
      plainRightAnglePosition.Z.ShouldEqual(0.235, 0.1);
    }


    // Arm in position
    // (0°)
    //  @=======
    //  |
    //  |
    //  @ (90°)
    //  |
    //  |
    //[===] (90°)
    [Fact]
    public void CanCalculateArmAtRotatedOnBaseAt90()
    {
      SetThetas(90, 90, 0);
      var plainRightAnglePosition = ArmChain.CalculateResultantPosition();
      plainRightAnglePosition.X.ShouldEqual(0, 0.1);
      plainRightAnglePosition.Y.ShouldEqual(0.160, 0.1);
      plainRightAnglePosition.Z.ShouldEqual(0.235, 0.1);
    }

    // Arm in position
    //    (-45°)
    //      @
    //    /   \
    //   /     \
    //  @ (45°)
    //  |
    //  |
    //[===] (0°)
    [Fact]
    public void CanCalculateEndPositionWithArmSegmentsAt45Deg()
    {
      SetThetas(0, 45, -45);
      var plainRightAnglePosition = ArmChain.CalculateResultantPosition();
      plainRightAnglePosition.X.ShouldEqual(0.205, 0.1);
      plainRightAnglePosition.Y.ShouldEqual(0.0, 0.1);
      plainRightAnglePosition.Z.ShouldEqual(0.084, 0.1);
    }

    // @ =====
    // \
    //  \ <--- Too far back
    //   \
    //    @ (105°)
    //    |
    //  [===]
    [Fact]
    public void DoesNotAllowYAxisPast105Degrees()
    {
      AssertPositionValidity(0, 99, SafeZPosition, true); //After 45 degrees moving the Y makes no difference to the Z
      AssertPositionValidity(0, 100, SafeZPosition, false);
    }

    //      / <-- Too far up
    //     /
    //    @ (15°)
    //    |
    //    |
    //    |
    //    @ (90°)
    //    |
    //  [===]
    [Fact]
    public void DoesNotAllowZAxisAbove15Degrees()
    {
      AssertPositionValidity(0, VerticalYPosition, 14.5, true);
      AssertPositionValidity(0, VerticalYPosition, 16, false);
    }

    // @ \ (-60°)
    // \  \  
    //  \  \   <-- Z collides with Y
    //   \ 
    //    @ (105°)
    //    |
    //  [===]
    [Fact]
    public void DoesNotAllowZAxisBelowMinus60WhenXIsInHomePosition()
    {
      AssertPositionValidity(0, YHomePosition, -59, true);
      AssertPositionValidity(0, YHomePosition, -61, false);
    }


    //         @(-79°)
    //       / / 
    //      / /
    //     / /
    //    @ (70°)
    //    |
    //  [===]
    [Fact]
    public void TheZAxisRangeIncreasesAsTheYIncreases()
    {
      AssertPositionValidity(0, 70, -79, true);
      AssertPositionValidity(0, 70, -81, false);
    }

    //           @(-105°) <-- Joint can't move this far
    //         / / 
    //       / /  
    //     / /
    //    @ (45°)
    //    |
    //  [===]
    [Fact]
    public void ThereIsAnMaximumLowerBoundToTheFreedomMovingTheYArmProvidesToTheZArm()
    {
      AssertPositionValidity(0, 45, -104, true);
      AssertPositionValidity(0, 45, -106, false);
      AssertPositionValidity(0, 25, -106, false);
    }

    //  (-20°)
    //    @ \ 
    //    |   \         
    //  [===]   \       
    //            @     
    //            |
    //            |
    [Fact]
    public void CanMoveTheYToItsLowestPoint()
    {
      AssertPositionValidity(0, -20, -70, true);
      AssertPositionValidity(0, -21, -70, false);
    }

    //         (10°) <-- Original upper bound - (Y - 40) = 15 - 5 = 10 can't move this far
    //           @ ========== 
    //         /  
    //       /  
    //     / 
    //    @ (40°)
    //    |
    //  [===]
    [Fact]
    public void After45DegreesOfMovementOnTheYAxisTheZAxisUpperBoundReducesWithTheYAxis()
    {
      AssertPositionValidity(0, 40, 9, true);
      AssertPositionValidity(0, 40, 11, false);
      AssertPositionValidity(0, 30, 19, false);  
    }

      [Fact]
      public void DoesNotAllowXOutsideOf0And250()
      {
            AssertPositionValidity(0, 40, 9, true);
            AssertPositionValidity(250, 40, 9, true);
            AssertPositionValidity(-1, 40, 9, false);
            AssertPositionValidity(251, 40, 9, false);
        }

    private void AssertPositionValidity(double x, double y, double z, bool isValid, string message = "")
    {
      SetThetas(x, y, z);
      ArmChain.IsValidPosition().ShouldEqual(isValid, message);
    }

  }
}