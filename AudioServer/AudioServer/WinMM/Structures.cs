using System;
using System.Runtime.InteropServices;

namespace AudioServer.WinMM.Structures
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
    public struct WAVEFORMATEX
    {
        public short WFormatTag;
        public short NChannels;
        public int NSamplesPerSec;
        public int NAvgBytesPerSec;
        public short NBlockAlign;
        public short WBitsPerSample;
        public short CbSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct WAVEHDR
    {
        public IntPtr LpData;
        public uint DwBufferLength;
        public uint DwBytesRecorded;
        public IntPtr DwUser;
        public WAVEHDRFLAGS DwFlags;
        public uint DwLoops;
        public IntPtr LpNext;
        public uint Reserved;
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

    public enum WAVEOUTMESSAGE
    {
        WOM_OPEN = 0x3BB,
        WOM_CLOSE = 0x3BC,
        WOM_DONE = 0x3BD
    }

    public enum WAVEFORMATTAG
    {
        WAVE_FORMAT_PCM = 0x01,
        WAVE_FORMAT_ADPCM = 0x02
    }

    public enum WAVEHDRFLAGS
    {
        WHDR_BEGINLOOP = 0x00000004,
        WHDR_DONE = 0x00000001,
        WHDR_ENDLOOP = 0x00000008,
        WHDR_INQUEUE = 0x00000010,
        WHDR_PREPARED = 0x00000002
    }

    public enum WAVEINOPENFLAGS
    {
        CALLBACK_NULL = 0,
        CALLBACK_FUNCTION = 0x30000,
        CALLBACK_EVENT = 0x50000,
        CALLBACK_WINDOW = 0x10000,
        CALLBACK_THREAD = 0x20000,
        WAVE_FORMAT_QUERY = 1,
        WAVE_MAPPED = 4,
        WAVE_FORMAT_DIRECT = 8
    }
}
