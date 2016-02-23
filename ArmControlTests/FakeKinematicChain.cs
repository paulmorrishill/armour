using System.Collections.Generic;
using ArmControl.Kinematics;

namespace ArmControlTests
{
  public class FakeKinematicChain : KinematicChain
  {
    public List<DhParameterSet> InputLinks { get; set; }
    public const int KinematicsOffsetX = 32;
    public const int KinematicsOffsetY = 56;
    public const int KinematicsOffsetZ = 78;

    public void SetLinksToPosition(Vector3D position)
    {
      var newLinks = new List<DhParameterSet>
      {
        new DhParameterSet(0, 0, 0),
        new DhParameterSet(0, 0, 0),
        new DhParameterSet(0, 0, 0)
      };

      newLinks[0].SetTheta(position.X + KinematicsOffsetX);
      newLinks[1].SetTheta(position.Y + KinematicsOffsetY);
      newLinks[2].SetTheta(position.Z + KinematicsOffsetZ);
      InputLinks = newLinks;
    }

    public List<DhParameterSet> GetResultantLinks() => null;

    public bool IsValidPosition() => false;

    public Vector3D CalculateResultantPosition() => default(Vector3D);

    public void Randomize() { }
  }
}