using System;
using System.Runtime.InteropServices;
using AudioServer.WinMM.Structures;

namespace AudioServer.WinMM.DllImports
{
    /// <summary>
    /// Класс включает в себя испортированные функции для winmm.dll.
    /// http://www.pinvoke.net/default.aspx/winmm.PlaySound
    /// </summary>
    internal static class NativeMethods
    {        
        public delegate void WaveOutProc(IntPtr hwo, WAVEOUTMESSAGE uMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint waveInGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutGetDevCaps(UIntPtr hWaveOut, ref WAVEOUTCAPS pwoc, uint cbwoc);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInGetDevCaps(UIntPtr hWaveIn, ref WAVEINCAPS pwic, uint cbwoc);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutOpen(ref IntPtr hWaveOut, uint uDeviceId, ref WAVEFORMATEX lpFormat,
            WaveOutProc dwCallback, IntPtr dwInstance, uint dwFlags);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInOpen(ref IntPtr hWaveIn, uint deviceId, ref WAVEFORMATEX lpFormat, 
            IntPtr dwCallBack, uint dwInstance, uint dwFlags);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutPrepareHeader(IntPtr hWaveOut, ref WAVEHDR lpWaveOutHdr, uint uSize);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInPrepareHeader(IntPtr hWaveIn, ref WAVEHDR pwh, uint cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInAddBuffer(IntPtr hWaveIn, ref WAVEHDR pwh, uint cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutWrite(IntPtr hWaveOut, ref WAVEHDR pwh, uint cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInStart(IntPtr hWaveIn);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutUnprepareHeader(IntPtr hWaveOut, ref WAVEHDR pwh, uint cbwh);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutClose(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInClose(IntPtr hwi);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveOutReset(IntPtr hWaveOut);

        [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint waveInReset(IntPtr hWaveIn);
    }
}
