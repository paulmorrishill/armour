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
        public void ItInitialisesAtAReachablePosition()
        {
            AssertPositionIs(0.061, 0.0, 0.1);
        }

        [Fact]
        public void CanMakeLargeIncrementsInPositions()
        {
            Manipulator.SetPosition(0, 0, 0);

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
            Manipulator.SetPosition(0, 0, 0);

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
            Manipulator.SetPosition(0, 0, 0);

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
            Manipulator.SetPosition(0, 0, 0);

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
        public void CanIncrementServoPositionPrecisely()
        {
            Manipulator.IncrementServoPosition(0, true);
            MockController.Verify(r => r.SetServoPosition(0, 1410));
            Manipulator.IncrementServoPosition(0, true);
            MockController.Verify(r => r.SetServoPosition(0, 1420));
        }

        [Fact]
        public void CanDecrementServoPosition()
        {
            Manipulator.DecrementServoPosition(0);
            MockController.Verify(r => r.SetServoPosition(0, 1375));
        }


        [Fact]
        public void CanDecrementServoPositionPrecisely()
        {
            Manipulator.DecrementServoPosition(0, true);
            MockController.Verify(r => r.SetServoPosition(0, 1390));
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
        public void CanSetPositionDirectly()
        {
            Manipulator.SetPosition(1, 2, 3);
            AssertPositionIs(1, 2, 3);
        }

        [Fact]
        public void CanSetServoPositionDirectly()
        {
            Manipulator.SetServoPosition(0, 123);
            Manipulator.IncrementServoPosition(0);
            MockController.Verify(r => r.SetServoPosition(0, 148));
        }

        [Fact]
        public void CanSetDifferentServoPositionDirectly()
        {
            Manipulator.SetServoPosition(2, 123);
            Manipulator.IncrementServoPosition(0);
            MockController.Verify(r => r.SetServoPosition(2, 123));
        }

        [Fact]
        public void CanAddSomeStatesToTheCommandSetAndPlayItBack()
        {
            Manipulator.SetPosition(1, 2, 3);
            Manipulator.SetServoPosition(0, 40);
            Manipulator.RecordStep();

            Manipulator.SetPosition(2, 3, 4);
            Manipulator.SetServoPosition(1, 67);
            Manipulator.RecordStep();

            MockController.Reset();
            Manipulator.PlayBackSteps();

            AssertPositionWasSetTo(1, 2, 3);
            AssertPositionIs(2, 3, 4);
            MockController.Verify(c => c.SetServoPosition(0, 40));
            MockController.Verify(c => c.SetServoPosition(1, 67));
        }

        [Fact]
        public void WhenAddingStatesToTheRecordingChangingTheServoValuesAfterDoesNotChangeTheRecording()
        {
            Manipulator.SetServoPosition(1, 67);
            Manipulator.RecordStep();

            Manipulator.SetServoPosition(1, 123);

            Manipulator.PlayBackSteps();
            MockController.Verify(c => c.SetServoPosition(1, 67));
        }

        [Fact]
        public void DoesNotSendServoPositionsWhenTheyHaveNotChangedSinceTheLastStepInPlayback()
        {
            Manipulator.SetServoPosition(1, 67);
            MockController.Reset();

            Manipulator.RecordStep();
            Manipulator.RecordStep();

            Manipulator.PlayBackSteps();
            MockController.Verify(c => c.SetServoPosition(1, 67), Times.Once);
        }

        [Fact]
        public void DoesNotScrewUpWhenThePreviousStepDidNotIncludeAServoInTheCurrentStep()
        {
            Manipulator.SetServoPosition(1, 67);
            Manipulator.RecordStep();

            Manipulator.SetServoPosition(2, 67);
            Manipulator.RecordStep();

            Manipulator.PlayBackSteps();
        }


        private void AssertPositionIs(double x, double y, double z)
        {
            var position = Manipulator.GetCurrentArmPosition();
            position.X.ShouldEqual(x);
            position.Y.ShouldEqual(y);
            position.Z.ShouldEqual(z);
            AssertPositionWasSetTo(x, y, z);
        }

        private void AssertPositionWasSetTo(double x, double y, double z)
        {
            MockController.Verify(r => r.SetPosition(
            x + FakeKinematicChain.KinematicsOffsetX,
            y + FakeKinematicChain.KinematicsOffsetY,
            z + FakeKinematicChain.KinematicsOffsetZ, 16000));
        }
    }
}