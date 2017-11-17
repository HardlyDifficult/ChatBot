// TODO we need to figure out keyboard hooks first

//using SnagFree.TrayApp.Core;
//using System;

//namespace HD
//{
//  public class KeystrokeCounter : IBotFeature
//  {
//    GlobalKeyboardHook _globalKeyboardHook;

//    public static int keystrokeCountSinceLastEvent;

//    void IBotFeature.Init()
//    {
//      _globalKeyboardHook = new GlobalKeyboardHook();
//      _globalKeyboardHook.KeyboardPressed += OnKeyPressed;
//    }

//    void OnKeyPressed(
//      object sender,
//      GlobalKeyboardHookEventArgs e)
//    {
//      keystrokeCountSinceLastEvent++;
//    }
//  }
//}
