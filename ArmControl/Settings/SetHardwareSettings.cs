namespace ArmControl.Settings
{
  public interface SetHardwareSettings
  {
    void Execute(HardwareSettings settings, SetHardwareSettingsCompleteHandler handler);
  }
}