using System.Collections.Generic;

namespace ArmControl.Kinematics
{
  public abstract class KinematicChainBase : KinematicChain
  {
    public List<DhParameterSet> InputLinks { get; protected set; }
    public abstract List<DhParameterSet> GetResultantLinks();
    public abstract bool IsValidPosition();
    public abstract void Randomize();
    public virtual Vector3D CalculateResultantPosition()
    {
      Matrix4D resultant = new Matrix4D();
      resultant.Values[0, 0] = 1; //Base has Z axis pointing up
      resultant.Values[1, 1] = 1;
      resultant.Values[2, 2] = 1;
      resultant.Values[3, 3] = 1;
      var links = GetResultantLinks();
      for (var i = 0; i < links.Count; i++)
      {
        var thisSetMatrix = links[i].ToMatrix();
        resultant = resultant * thisSetMatrix;
      }
      var translation = new Vector3D
      {
        X = resultant.Values[3, 0],
        Y = resultant.Values[3, 1],
        Z = resultant.Values[3, 2]
      };
      return translation;
    }

  }
}