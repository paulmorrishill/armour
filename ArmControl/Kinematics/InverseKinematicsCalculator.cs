namespace ArmControl.Kinematics
{
  public interface InverseKinematicsCalculator
  {
    void AdjustKinematicChainForPosition(KinematicChain kinematicChain, Vector3D targetPosition);
  }
}