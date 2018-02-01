using FlaUI.UIA3;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Input;

namespace HD
{
  public static class Obs
  {
    public static void StopStreaming()
    {
      Application app = Application.Attach("obs64.exe");
      using (UIA3Automation automation = new UIA3Automation())
      {
        Window window = app.GetMainWindow(automation);
        Button button1 = window.FindFirstDescendant(cf 
          => cf.ByText("Stop Streaming"))?.AsButton();
        button1?.Invoke();
      }
    }

    public static void ChangeScene(
      bool moveUp)
    {
      Application app = Application.Attach("obs64.exe");
      using (UIA3Automation automation = new UIA3Automation())
      {
        Window window = app.GetMainWindow(automation);
        Window button1 = window.FindFirstDescendant(cf
          => cf.ByText("Scenes"))?.AsWindow();
        ListBox box = button1.FindFirstDescendant(cf => cf.ByControlType(FlaUI.Core.Definitions.ControlType.List)).AsListBox();
        box.Select(0);
        Keyboard.Press(moveUp ? FlaUI.Core.WindowsAPI.VirtualKeyShort.UP : FlaUI.Core.WindowsAPI.VirtualKeyShort.DOWN);
      }
    }

    public static void MuteMic(bool isMuted)
    {
      Application app = Application.Attach("obs64.exe");
      using (var automation = new UIA3Automation())
      {
        Window window = app.GetMainWindow(automation);
        CheckBox button = window.FindFirstDescendant(cf =>
        cf.ByText("Mute 'Mic/Aux'"))?.AsCheckBox();
        if (button.IsChecked != isMuted)
        {
          button.Click();
        }
      }
    }
  }
}
