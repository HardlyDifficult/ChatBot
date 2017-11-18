using FlaUI.UIA3;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;

namespace HD
{
  public static class Obs
  {
    public static void StartStreaming()
    {
      Application app = Application.Attach("obs64.exe");
      using (var automation = new UIA3Automation())
      {
        Window window = app.GetMainWindow(automation);
        Button button1 = window.FindFirstDescendant(cf => cf.ByText("Start Streaming"))?.AsButton();
        button1?.Invoke();
      }
    }
  }
}
