// TODO does not work

//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//public class InterceptKeys
//{
//  public event Action onKeyDown;

//  public static readonly InterceptKeys instance = new InterceptKeys();

//  const int WH_KEYBOARD_LL = 13;
//  const int WM_KEYDOWN = 0x0100;
//  static LowLevelKeyboardProc _proc = HookCallback;
//  static IntPtr _hookID = IntPtr.Zero;

//  InterceptKeys()
//  {
//    Debug.Assert(instance == null || instance == this);

//    _hookID = SetHook(_proc);
//    UnhookWindowsHookEx(_hookID);
//  }

//  private static IntPtr SetHook(LowLevelKeyboardProc proc)
//  {
//    using (Process curProcess = Process.GetCurrentProcess())
//    using (ProcessModule curModule = curProcess.MainModule)
//    {
//      return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
//          GetModuleHandle(curModule.ModuleName), 0);
//    }
//  }

//  private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

//  private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
//  {
//    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
//    {
//      int vkCode = Marshal.ReadInt32(lParam);
//      instance.onKeyDown?.Invoke();
//    }

//    return CallNextHookEx(_hookID, nCode, wParam, lParam);
//  }

//  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//  private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

//  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//  [return: MarshalAs(UnmanagedType.Bool)]
//  private static extern bool UnhookWindowsHookEx(IntPtr hhk);

//  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//  private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

//  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
//  private static extern IntPtr GetModuleHandle(string lpModuleName);
//}