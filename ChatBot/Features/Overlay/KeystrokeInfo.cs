using System;
using System.Collections.Generic;
using System.Windows.Input;
using static System.Windows.Input.Key;

namespace HD
{
  public class KeystrokeInfo
  {
    #region Data
    public readonly Key key;

    /// <summary>
    /// True if the key is currently down.
    /// This will switch to false for a period of time before being removed.
    /// </summary>
    public bool isDown;

    /// <summary>
    /// This is updated each tick as the key is held down.
    /// </summary>
    public DateTime lastTimePressed;

    readonly List<KeystrokeInfo> specialKeys;
    #endregion

    public KeystrokeInfo(Key key, List<KeystrokeInfo> specialKeys)
    {
      this.key = key;
      lastTimePressed = DateTime.Now;
      isDown = true;
      this.specialKeys = specialKeys;
    }

    internal string Formatted()
    {
      return $"{SpecialKeyString()}{KeyString(key)}";
    }

    static string KeyString(Key key)
    {
      switch (key)
      {
        case A:
        case B:
        case C:
        case D:
        case E:
        case F:
        case G:
        case H:
        case I:
        case J:
        case K:
        case L:
        case M:
        case N:
        case O:
        case P:
        case Q:
        case R:
        case S:
        case T:
        case U:
        case V:
        case W:
        case X:
        case Y:
        case Z:
        case Back:
        case Tab:
        case Enter:
        case CapsLock:
        case Escape:
        case Space:
        case PageUp:
        case PageDown:
        case End:
        case Home:
        case Left:
        case Up:
        case Right:
        case Down:
        case PrintScreen:
        case Insert:
        case Delete:
        case Help:
        case Multiply:
        case Add:
        case Separator:
        case Subtract:
        case Key.Decimal:
        case Divide:
        case F1:
        case F2:
        case F3:
        case F4:
        case F5:
        case F6:
        case F7:
        case F8:
        case F9:
        case F10:
        case F11:
        case F12:
        case F13:
        case F14:
        case F15:
        case F16:
        case F17:
        case F18:
        case F19:
        case F20:
        case F21:
        case F22:
        case F23:
        case F24:
        case NumLock:
        case Scroll:
        case Play:
        case Zoom:
          return key.ToString();
        case D0:
        case D1:
        case D2:
        case D3:
        case D4:
        case D5:
        case D6:
        case D7:
        case D8:
        case D9:
        case LWin:
        case RWin:
          return key.ToString().Substring(1);
        case NumPad0:
        case NumPad1:
        case NumPad2:
        case NumPad3:
        case NumPad4:
        case NumPad5:
        case NumPad6:
        case NumPad7:
        case NumPad8:
        case NumPad9:
          return key.ToString().Substring(6);
        case LeftShift:
        case LeftCtrl:
        case LeftAlt:
          return key.ToString().Substring(4);
        case RightShift:
        case RightCtrl:
        case RightAlt:
          return key.ToString().Substring(5);
        case BrowserBack:
        case BrowserForward:
        case BrowserRefresh:
        case BrowserStop:
        case BrowserSearch:
        case BrowserFavorites:
        case BrowserHome:
          return key.ToString().Substring(7);
        case VolumeMute:
        case VolumeDown:
        case VolumeUp:
          return key.ToString().Substring(6);
        case MediaNextTrack:
        case MediaPreviousTrack:
        case MediaStop:
        case MediaPlayPause:
          return key.ToString().Substring(5);
        case OemSemicolon:
          return ";";
        case OemPlus:
          return "+";
        case OemComma:
          return ",";
        case OemMinus:
          return "-";
        case OemPeriod:
          return ".";
        case OemTilde:
          return "~";
        case OemQuestion:
          return "?";
        case OemOpenBrackets:
          return "[";
        case OemPipe:
          return "|";
        case OemCloseBrackets:
          return "]";
        case OemQuotes:
          return "\"";
        case OemBackslash:
          return "\\";
        case OemCopy:
        case OemBackTab:
          return key.ToString().Substring(3);
        default:
          return key.ToString();
      }
    }

    string SpecialKeyString()
    {
      var message = "";
      if (specialKeys != null)
      {
        foreach (var key in specialKeys)
        {
          message += $"{KeyString(key.key)} + ";
        }
      }

      return message;
    }
  }
}