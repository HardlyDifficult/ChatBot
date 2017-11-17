using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Input;
using static System.Windows.Input.Key;

namespace HD
{
  public class KeyPressMonitor
  {
    #region Data
    #region Keys
    public static readonly Key[]
      keysToCheck =
      {
        OemPlus, OemMinus,  Back, Space, Escape, Key.Decimal, Enter, Separator, Delete,
        Left, Up, Right, Down, OemPeriod, Multiply, Add, Subtract, Divide, OemSemicolon, OemComma,OemPeriod, OemQuestion, OemTilde,
        OemOpenBrackets, OemPipe, OemCloseBrackets, OemQuotes, OemBackslash,
        NumPad0, NumPad1, NumPad2, NumPad3, NumPad4, NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
        D0, D1, D2, D3, D4, D5, D6, D7, D8, D9, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
      },
      modifierKeysToCheck =
      {
        LWin, RWin, LeftShift, RightShift
      },
      specialKeysToCheck =
      {
        LeftCtrl, RightCtrl, LeftAlt, RightAlt
      },
      standaloneKeysToCheck =
      {
        F1, F2, F3, F4, F5, F6, F7, F8,F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,
        CapsLock, PageDown, PageUp, Insert, End, Home, PrintScreen, Help,
        BrowserBack, BrowserFavorites, BrowserForward, BrowserHome, BrowserRefresh, BrowserSearch, BrowserStop, VolumeDown, VolumeUp, VolumeMute,
        MediaNextTrack, MediaPlayPause, MediaPreviousTrack, MediaStop, OemCopy, Play, Zoom
      };
    #endregion

    static readonly TimeSpan howLongToDisplayKeysFor = TimeSpan.FromSeconds(3);

    readonly List<KeystrokeInfo> keysPressed = new List<KeystrokeInfo>();
    readonly List<KeystrokeInfo> modifierKeysPressed = new List<KeystrokeInfo>();
    readonly List<KeystrokeInfo> specialKeysPressed = new List<KeystrokeInfo>();
    #endregion

    #region Init
    public KeyPressMonitor()
    {
      System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
      dispatcherTimer.Tick += OnTick;
      dispatcherTimer.Interval = new TimeSpan(0);
      dispatcherTimer.Start();
    }
    #endregion

    #region Events
    void OnTick(
      object sender,
      EventArgs e)
    {
      RemoveStaleEntries();
      LookForKeyUp();
      CheckKeys();
      UpdateFile();
    }
    #endregion

    #region Private Write
    /// <summary>
    /// Remove any key which has not been pressed for the last howLongToDisplayKeysFor time.
    /// </summary>
    void RemoveStaleEntries()
    {
      DateTime now = DateTime.Now;
      for (int i = keysPressed.Count - 1; i >= 0; i--)
      {
        KeystrokeInfo keyEvent = keysPressed[i];
        if (now - keyEvent.lastTimePressed > howLongToDisplayKeysFor)
        {
          keysPressed.RemoveAt(i);
        }
      }
    }

    void LookForKeyUp()
    {
      LookForKeyUp(keysPressed);
      LookForKeyUp(specialKeysPressed);
      LookForKeyUp(modifierKeysPressed);

      void LookForKeyUp(
        List<KeystrokeInfo> list)
      {
        foreach (var key in list)
        {
          if (key.isDown && !Keyboard.IsKeyDown(key.key))
          {
            key.isDown = false;
          }
        }
      }
    }

    void CheckKeys()
    {
      CheckKeys(specialKeysToCheck, specialKeysPressed);
      CheckKeys(modifierKeysToCheck, modifierKeysPressed);
      CheckNormalKeys();

      void CheckNormalKeys()
      {
        bool specialDown = false;
        List<KeystrokeInfo> specialKeysDown = new List<KeystrokeInfo>();
        foreach (KeystrokeInfo key in specialKeysPressed)
        {
          if (key.isDown)
          {
            specialDown = true;
            specialKeysDown.Add(key);
          }
        }

        foreach (KeystrokeInfo key in modifierKeysPressed)
        {
          if (key.isDown)
          {
            specialKeysDown.Add(key);
          }
        }

        CheckKeys(standaloneKeysToCheck, keysPressed, specialKeysDown);

        //if (specialDown)
        {
          CheckKeys(keysToCheck, keysPressed, specialKeysDown);
        }
      }

      bool CheckKeys(
        Key[] keysList,
        List<KeystrokeInfo> list,
        List<KeystrokeInfo> specialKeys = null)
      {
        bool found = false;
        foreach (Key key in keysList)
        {
          if (Keyboard.IsKeyDown(key))
          {
            found = true;
            if (FindKeyAndUpdateTime(list, key) == false)
            {
              list.Add(new KeystrokeInfo(key, specialKeys));
            }
          }
        }

        return found;
      }
    }

    static bool FindKeyAndUpdateTime(
      List<KeystrokeInfo> list,
      Key key)
    {
      foreach (KeystrokeInfo keyDown in list)
      {
        if (keyDown.key == key)
        {
          if (keyDown.isDown)
          {
            keyDown.lastTimePressed = DateTime.Now;
            return true;
          }
        }
      }

      return false;
    }

    void UpdateFile()
    {
      StringBuilder builder = new StringBuilder();

      foreach (KeystrokeInfo key in keysPressed)
      {
        builder.AppendLine(key.Formatted());
      }

      File.WriteAllText("..\\Keystrokes.txt", builder.ToString());
    }
    #endregion
  }
}
