using System.Collections.Generic;

namespace ArmControl.Kinematics
{
  public interface KinematicChain
  {
    List<DhParameterSet> InputLinks { get; }
    List<DhParameterSet> GetResultantLinks();
    bool IsValidPosition();
    Vector3D CalculateResultantPosition();
    /// <summary>
    /// When the hill climbing algorithm gets stuck in a local maxima randomize to try a different area of the solution space
    /// </summary>
    void Randomize();
  }
}