﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AudioServer.Alvasoft.AudioServer.Communication
{
    /// <summary>
    /// Структура, которая прилетает по сети от клиентов при команде на воспроизведения звука.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SoundCommand
    {
        public Int32 channelCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public Int32[] channels;
        public Int32 priority;
        public Int32 fileCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 * 20)]
        public byte[] files;
    }
}
