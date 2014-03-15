using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace hcl
{
    class KeyHook : IDisposable
    {
        public delegate void OnHook(uint vkCode, bool control, bool alt, bool lshift, bool rshift);

        public event OnHook Hook;


        private delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr SetWindowsHookEx(int hookType, HookProc hookProc, IntPtr module, uint threadId);

        [DllImport("user32.dll", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(IntPtr hook, int code, IntPtr message, IntPtr state);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern int GetAsyncKeyState(uint vkCode);

        [StructLayout(LayoutKind.Sequential)]
        private class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        private enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        private IntPtr _hook;
        private HookProc _hookProc;


        public KeyHook()
        {
            var modules = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
            var hModule = Marshal.GetHINSTANCE(modules[0]);

            _hookProc = new HookProc(OnHookKey);

            var WH_KEYBOARD_LL = 13;
            _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _hookProc, hModule, 0);
            if (_hook == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void Dispose()
        {
            if (_hook != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hook);
            }
        }

        private int OnHookKey(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

            var alt = ((kb.flags & KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN) == KBDLLHOOKSTRUCTFlags.LLKHF_ALTDOWN);
            var control = GetAsyncKeyState(0x11);// VK_CONTROL
            var lshift = GetAsyncKeyState(0xA0);// VK_LSHIFT
            var rshift = GetAsyncKeyState(0xA1);// VK_RSHIFT

            if (Hook != null)
            {
                Hook(kb.vkCode, control != 0, alt, lshift != 0, rshift != 0);
            }

            return CallNextHookEx(_hook, nCode, wParam, lParam);
        }
    }
}
