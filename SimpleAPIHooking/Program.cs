using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAPIHooking
{
    internal class Program
    {
        public delegate void msg(IntPtr p1, string h, string l, int i);
        public static IntPtr FunAdder;
        public static byte[] savedBytes;
        public static void HookedFunction(IntPtr p1, string h, string l, int i)
        {
            Marshal.Copy(savedBytes, 0, FunAdder, savedBytes.Length);
            msg m = (msg)Marshal.GetDelegateForFunctionPointer(FunAdder, typeof(msg));
            m(IntPtr.Zero, "from  Hooked fun  ", "reall ", 0);
        }

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtect( IntPtr lpAddress,
        int dwSize, uint flNewProtect, out uint lpflOldProtect);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        static void Main(string[] args)
        {
            IntPtr LibHAndel = LoadLibrary("User32.dll");
            Console.WriteLine(LibHAndel);
            FunAdder = GetProcAddress(LibHAndel, "MessageBoxA");

            savedBytes = new byte[5];
            Marshal.Copy(FunAdder, savedBytes, 0, 5);

            IntPtr HookedFunAdder = Marshal.GetFunctionPointerForDelegate((msg)HookedFunction);

            byte[] jump = new byte[5];
            jump[0] = 0xe9;

            int offset = (int)HookedFunAdder - (int)FunAdder - 5;
            Array.Copy(BitConverter.GetBytes(offset), 0,jump, 1, 4);
            uint oldProtect = 0;
            VirtualProtect(FunAdder, 5, 0x40, out oldProtect);

            Marshal.Copy(jump, 0, FunAdder, jump.Length);

            msg m = (msg)Marshal.GetDelegateForFunctionPointer(FunAdder, typeof(msg));
            m(IntPtr.Zero, "from  Legit fun  ", "reall ", 0);
            Console.ReadKey();
        }
    }
}
