using ArmControl;
using ArmControl.Kinematics;
using Moq;
using Should;
using Xunit;

namespace ArmControlTests
{
  public class ArmManipulatorTests
  {
    private ArmManipulator Manipulator;
    private Mock<ArmController> MockController;
    private Mock<InverseKinematicsCalculator> KinematicsCalculatorMock;


    public ArmManipulatorTests()
    {
      MockController = new Mock<ArmController>();
      KinematicsCalculatorMock = new Mock<InverseKinematicsCalculator>();
      var fakeChain = new FakeKinematicChain();

      KinematicsCalculatorMock.Setup(r => r.AdjustKinematicChainForPosition(fakeChain, It.IsAny<Vector3D>()))
        .Callback<KinematicChain, Vector3D>((chain, targetPosition) =>
        {
          ((FakeKinematicChain)chain).SetLinksToPosition(targetPosition);
        });

      Manipulator = new ArmManipulator(MockController.Object, KinematicsCalculatorMock.Object, fakeChain);
    }

    [Fact]
    public void CanMakeLargeIncrementsInPositions()
    {
     Manipulator.IncrementX();
     AssertPositionIs(0.01, 0, 0);
     Manipulator.IncrementY();
     AssertPositionIs(0.01, 0.01, 0);
     Manipulator.IncrementZ();
     AssertPositionIs(0.01, 0.01, 0.01);
    }

    [Fact]
    public void CanMakeSmallIncrementsInPositions()
    {
      Manipulator.IncrementX(true);
      AssertPositionIs(0.001, 0, 0);
      Manipulator.IncrementY(true);
      AssertPositionIs(0.001, 0.001, 0);
      Manipulator.IncrementZ(true);
      AssertPositionIs(0.001, 0.001, 0.001);
    }

    [Fact]
    public void CanMakeLargeDecrementInPositions()
    {
      Manipulator.DecrementX();
      AssertPositionIs(-0.01, 0, 0);
      Manipulator.DecrementY();
      AssertPositionIs(-0.01, -0.01, 0);
      Manipulator.DecrementZ();
      AssertPositionIs(-0.01, -0.01, -0.01);
    }

    [Fact]
    public void CanMakeSmallDecrementsInPositions()
    {
      Manipulator.DecrementX(true);
      AssertPositionIs(-0.001, 0, 0);
      Manipulator.DecrementY(true);
      AssertPositionIs(-0.001, -0.001, 0);
      Manipulator.DecrementZ(true);
      AssertPositionIs(-0.001, -0.001, -0.001);
    }

    [Fact]
    public void CanHomeArm()
    {
      Manipulator.HomeArm();
      MockController.Verify(r => r.HomeArm());
      AssertPositionIs(0.061, 0.0, 0.1);
    }

    [Fact]
    public void CanIncrementServoPosition()
    {
      Manipulator.IncrementServoPosition(0);
      MockController.Verify(r => r.SetServoPosition(0, 1425));
      Manipulator.IncrementServoPosition(0);
      MockController.Verify(r => r.SetServoPosition(0, 1450));
    }

    [Fact]
    public void WhenAPositionIsUnreachableItStaysAtTheLastSafePosition()
    {
      Manipulator.IncrementX();
      var safePosition = Manipulator.GetCurrentArmPosition();
      KinematicsCalculatorMock.Setup(
        k => k.AdjustKinematicChainForPosition(It.IsAny<KinematicChain>(), It.IsAny<Vector3D>()))
        .Throws<UnreachablePositionException>();

      Manipulator.IncrementX();
      AssertPositionIs(safePosition.X, safePosition.Y, safePosition.Z);
    }

    [Fact]
    public void ItFiresThePresenterOnPositionChange()
    {
      
    }

    private void AssertPositionIs(double x, double y, double z)
    {
      var position = Manipulator.GetCurrentArmPosition();
      position.X.ShouldEqual(x);
      position.Y.ShouldEqual(y);
      position.Z.ShouldEqual(z);
      MockController.Verify(r => r.SetPosition(
      x + FakeKinematicChain.KinematicsOffsetX, 
      y + FakeKinematicChain.KinematicsOffsetY, 
      z + FakeKinematicChain.KinematicsOffsetZ, 16000));
    }
  } 
}