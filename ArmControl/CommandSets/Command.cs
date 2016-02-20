namespace ArmControl.CommandSets
{
  public interface Command
  {
    string GetDescription();
    void Execute(ArmController armController);
  }
}