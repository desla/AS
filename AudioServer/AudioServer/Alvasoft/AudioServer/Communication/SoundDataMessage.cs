﻿using System.Runtime.InteropServices;

namespace Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Структура, которая прилетает по сети от клиентов при команде на воспроизведения звука.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SoundCommand
    {
        public int channelCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public int[] channels;
        public int priority;
        public int fileCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 * 20)]
        public char[] files;
    }
}
