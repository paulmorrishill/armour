using System;
using System.Collections.Generic;

namespace ArmControl.CommandSets
{
  public interface CommandSet
  {
    void AddPositionChange(double x, double y, double z);
    void AddServoMove(int servoId, int servoPosition);
    void AddDeviceStateChangeToCommandSet(int deviceId, bool state);
    void AddWait(TimeSpan duration);
    void MoveCommandUp(Command command);
    void MoveCommandDown(Command command);
    List<Command> GetAllCommands();
  }
}