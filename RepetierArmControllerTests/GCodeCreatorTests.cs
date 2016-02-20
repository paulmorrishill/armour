using RepetierArmController.Communication;
using Should;
using Xunit;

namespace RepetierArmControllerTests
{
    public class GCodeCreatorTests
    {
      private RepetierGCodeCreator GcodeCreator;

      public GCodeCreatorTests()
      {
        GcodeCreator = new RepetierGCodeCreator();
      }

      [Fact]
      public void ShouldBeAbleToGenerateAMoveCommandForJustX()
      {
        GcodeCreator.GenerateMove(5.2f, 6.1f, 2.8f, 123f)
          .ShouldEqual("G0 X5.2 Y6.1 Z2.8 F123");
      }

      [Fact]
      public void GivenCoordinatesAreTooPrecise_TheValuesAreRoundedTo5Digits()
      {
        GcodeCreator.GenerateMove(1.123456789f, 3.123456789f, 2.123456789f, 15.123456789f)
          .ShouldEqual("G0 X1.12346 Y3.12346 Z2.12346 F15.12346");
      }

      [Fact]
      public void ItCanGenerateSetServoCommands()
      {
        GcodeCreator.GenerateServoMove(2, 55).ShouldEqual("M340 P2 S55");
      }

      [Fact]
      public void ItCanGenerateFanOnCommands()
      {
        GcodeCreator.GenerateFanOnCommand(12).ShouldEqual("M106 P12");
      }

      [Fact]
      public void ItCanGenerateFanOffCommands()
      {
        GcodeCreator.GenerateFanOffCommand(12).ShouldEqual("M107 P12");
      }

      [Fact]
      public void CanGenerateHomeCommand()
      {
        GcodeCreator.GenerateHomeCommand().ShouldEqual("G28");
      }
  }
}
