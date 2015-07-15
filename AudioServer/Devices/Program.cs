using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Devices
{
    class Program
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEOUTCAPS
        {
            public short WMid;
            public short WPid;
            public int VDriverVersion;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string SzPname;
            public int DwFormats;
            public short WChannels;
            public short WReserved;
            public int DwSupport;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct WAVEINCAPS
        {
            public ushort WMid;
            public ushort WPid;
            public uint VDriverVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string SzPname;
            public uint DwFormats;
            public ushort WChannels;
            public ushort WReserved1;
        }

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(UIntPtr hWaveOut, ref WAVEOUTCAPS pwoc, uint cbwoc);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInGetDevCaps(UIntPtr hWaveIn, ref WAVEINCAPS pwic, uint cbwoc);

        static void Main(string[] args)
        {
            var deviceCount = waveOutGetNumDevs();
            Console.WriteLine("Найдено {0} устаройств вывода: ", deviceCount);
            for (var i = 0; i < deviceCount; ++i) {
                var wOutCaps = new WAVEOUTCAPS();
                waveOutGetDevCaps(new UIntPtr((uint)i), ref wOutCaps, (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS)));
                Console.WriteLine("Имя:\"{0}\" Каналов:{1}", wOutCaps.SzPname, wOutCaps.WChannels);
            }

            Console.WriteLine();

            deviceCount = waveInGetNumDevs();
            Console.WriteLine("Найдено {0} устаройств ввода: ", deviceCount);
            for (var i = 0; i < deviceCount; ++i) {
                var wInCaps = new WAVEINCAPS();
                waveInGetDevCaps(new UIntPtr((uint)i), ref wInCaps, (uint)Marshal.SizeOf(typeof(WAVEOUTCAPS)));
                Console.WriteLine("Имя:\"{0}\" Каналов:{1}", wInCaps.SzPname, wInCaps.WChannels);
            }

            Console.WriteLine("\nДля выхода нажмите Enter.");
            Console.ReadLine();
        }
    }
}
