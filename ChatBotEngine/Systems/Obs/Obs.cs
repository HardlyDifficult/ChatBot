using FlaUI.UIA3;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;

namespace HD
{
  public static class Obs
  {
    public static void StopStreaming()
    {
      Application app = Application.Attach("obs64.exe");
      using (var automation = new UIA3Automation())
      {
        Window window = app.GetMainWindow(automation);
        Button button1 = window.FindFirstDescendant(cf => cf.ByText("Stop Streaming"))?.AsButton();
        button1?.Invoke();
      }
    }

    //public static void MuteDesktopAudio(bool isMuted)
    //{
    //  Application app = Application.Attach("obs64.exe");
    //  using (var automation = new UIA3Automation())
    //  {
    //    Window window = app.GetMainWindow(automation);
    //    CheckBox button = window.FindFirstDescendant(cf => cf.ByText("Mute 'Desktop Audio'"))?.AsCheckBox();
    //    if (button.IsChecked != isMuted)
    //    {
    //      button.Click();
    //    }
    //  }
    //}
  }
}
