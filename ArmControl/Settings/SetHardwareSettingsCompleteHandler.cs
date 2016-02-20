namespace ArmControl.Settings
{
  public interface SetHardwareSettingsCompleteHandler
  {
    void SettingsSet();
    void SettingsInvalid();
  }
}