using ArmControl.Kinematics;

namespace ArmControl
{
    public interface ArmPresenter
    {
        void ArmPositionChanged(Vector3D currentPosition);
        void TargetPositionUnreachable();
        void ServoPositionChanged(int servoIndex, int position);
        void NumberOfStepsInRecordingChanged(int numberOfSteps);
    }
}