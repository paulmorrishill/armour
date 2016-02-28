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
        private Mock<ArmPresenter> MockPresenter;

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
            MockPresenter = new Mock<ArmPresenter>();

            Manipulator = new ArmManipulator(MockController.Object, MockPresenter.Object, KinematicsCalculatorMock.Object, fakeChain);
        }

        [Fact]
        public void ItInitialisesAtAReachablePosition()
        {
            var currentPos = Manipulator.GetCurrentArmPosition();
            currentPos.ShouldEqual(new Vector3D(0.061, 0.0, 0.1));
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
            AssertPositionIs(0.0, 0.07, 0.1);
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
            MockPresenter.Verify(r => r.TargetPositionUnreachable());
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
            MockPresenter.Verify(r => r.ServoPositionChanged(0, 148));
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

            Manipulator.SetServoPosition(0, 0);
            Manipulator.SetServoPosition(1, 0);

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
            Manipulator.RecordStep();
            Manipulator.RecordStep();

            Manipulator.SetServoPosition(1, 123);
            MockController.Reset();


            Manipulator.PlayBackSteps();
            MockController.Verify(c => c.SetServoPosition(1, 67), Times.Once);
        }

        [Fact]
        public void CanClearRecording()
        {
            Manipulator.RecordStep();
            Manipulator.ClearRecording();
            Manipulator.PlayBackSteps();
            AssertNoPositionWasSet();
            MockPresenter.Verify(r => r.NumberOfStepsInRecordingChanged(0));
        }

        [Fact]
        public void CanPlayBackSpecificStepInRecording()
        {
            Manipulator.SetPosition(2, 4, 6);
            Manipulator.RecordStep();
            Manipulator.SetPosition(4, 6, 8);
            Manipulator.RecordStep();
            Manipulator.SetPosition(6, 7, 9);

            MockController.Reset();
            Manipulator.PlayBackStep(1);

            AssertPositionWasSetTo(4, 6, 8);
        }

        [Fact]
        public void WhenTheStepRequestedToPlayBackWasNotFoundItFiresThePresenter()
        {
            Manipulator.RecordStep();
            Manipulator.PlayBackStep(10);

            MockPresenter.Verify(r => r.StepSpecifiedNotFound());
        }

        [Fact]
        public void AfterRecordingAStepThePresenterIsUpdated()
        {
            Manipulator.SetPosition(50, 100, 150);

            Manipulator.RecordStep();
            Manipulator.RecordStep();
            MockPresenter.Verify(r => r.NumberOfStepsInRecordingChanged(1));
            MockPresenter.Verify(r => r.NumberOfStepsInRecordingChanged(2));
        }

        [Fact]
        public void CanDeleteLastStepInRecording()
        {
            Manipulator.SetPosition(50, 100, 150);
            Manipulator.RecordStep();
            Manipulator.SetPosition(3, 400, 50);
            Manipulator.RecordStep();

            MockPresenter.Reset();
            MockController.Reset();

            Manipulator.DeleteLastStep();

            Manipulator.PlayBackSteps();
            AssertPositionIs(50, 100, 150);
            MockPresenter.Verify(r => r.NumberOfStepsInRecordingChanged(1));
        }

        [Fact]
        public void WhenThereIsNoLastStepToDeleteItDoesNothing()
        {
            Manipulator.DeleteLastStep();
        }

        private void AssertNoPositionWasSet()
        {
            MockController.Verify(r => r.SetPosition(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
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

        [Fact]
        public void WhenThePositionChangesThePositionUpdatePresenterIsCalled()
        {
            Manipulator.SetPosition(2, 4, 6);
            MockPresenter.Verify(r => r.ArmPositionChanged(new Vector3D(2, 4, 6)));
        }

        [Fact]
        public void CanAddADwellStepToTheRecording()
        {
            Manipulator.SetPosition(120, 0, 40);
            Manipulator.AddDwellStep();
            Manipulator.PlayBackSteps();

            MockController.Verify(r => r.Dwell(1000));

            AssertPositionIs(120, 0, 40);
            MockPresenter.Verify(r => r.NumberOfStepsInRecordingChanged(1));
        }

        [Fact]
        public void CanDwell()
        {
            Manipulator.Dwell();
            MockController.Verify(r => r.Dwell(1000));
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