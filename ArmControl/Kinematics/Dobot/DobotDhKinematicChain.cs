using System;
using System.Collections.Generic;

namespace ArmControl.Kinematics.Dobot
{
  public class DobotDhKinematicChain : KinematicChainBase
  {
    private Random Random;

    public override List<DhParameterSet> GetResultantLinks()
    {
      //With the dobot arm the second arm segment is physically linked to the base
      //The angle is relative to the base not the previous segment
      //This changes the kinematic chain
      var secondLink = InputLinks[1];
      var thirdLink = InputLinks[2];
      var fourthLink = InputLinks[3];
      var newThirdLink = new DhParameterSet(thirdLink.D, thirdLink.R, thirdLink.Alpha);
      newThirdLink.SetTheta(thirdLink.Theta - secondLink.Theta);
      //The fourth link represents the toolpoint mount on the dobot, this mount is always parallel with the ground.
      var newFourthLink = new DhParameterSet(fourthLink.D, fourthLink.R, fourthLink.Alpha);
      newFourthLink.SetTheta(-1 * (secondLink.Theta - newThirdLink.Theta));
      var resultantLinks = new List<DhParameterSet>
      {
        InputLinks[0],
        secondLink,
        newThirdLink
      };
      return resultantLinks;
    }

    public override bool IsValidPosition()
    {
      var links = InputLinks;
      var x = links[0].Theta;
      var y = links[1].Theta;
      var z = links[2].Theta;

      var xValid = x > -180 && x < 180;
      var yValid = y <= 105 && y >= -20;
      //The range of the Z axis on the dotot increases with every degree of the x
      var extraZDueToY = Math.Max(90 - y, 0);
      extraZDueToY = Math.Min(extraZDueToY, 45); //Capped out at 45 degrees extra
      var reductionInZDueToY = Math.Min(y - 45, 0);
      
      var zValid = z < (15+reductionInZDueToY) && z > (-60 - extraZDueToY);

      return xValid && yValid && zValid;
    }

    public override void Randomize()
    {
      var x = Random.Next(-180, 180);
      var y = Random.Next(-10, 100);
      var z = Random.Next(-60, 80);
      InputLinks[0].SetTheta(x);
      InputLinks[1].SetTheta(y);
      InputLinks[2].SetTheta(z);
    }

    public DobotDhKinematicChain()
    {
      Random = new Random();
      InputLinks = new List<DhParameterSet>
      {
        new DhParameterSet(0.105f, 0, 90),
        new DhParameterSet(0, 0.130f, 0),
        new DhParameterSet(0, 0.160f, 0),
        new DhParameterSet(0, 0.05f, 0) //The toolpoint mount
      };
    }
  }
}