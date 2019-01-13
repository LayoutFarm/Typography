using System;
using System.Runtime.InteropServices;

namespace BrotliSharpLib {
    public static partial class Brotli {
#if PROPER_DETECT
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_INFO {
            public ushort wProcessorArchitecture;
            public ushort wReserved;
            public uint dwPageSize;
            public IntPtr lpMinimumApplicationAddress;
            public IntPtr lpMaximumApplicationAddress;
            public IntPtr dwActiveProcessorMask;
            public uint dwNumberOfProcessors;
            public uint dwProcessorType;
            public uint dwAllocationGranularity;
            public ushort wProcessorLevel;
            public ushort wProcessorRevision;
        }

        [DllImport("kernel32.dll")]
        private static extern void GetNativeSystemInfo(out SYSTEM_INFO info);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private struct utsname {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
            public string sysname;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
            public string nodename;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
            public string release;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
            public string version;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 65)]
            public string machine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            private byte[] padding;
        }

        [DllImport("libc")]
        private static extern int uname(out utsname buf);
#endif

        private enum Endianess {
            Little,
            Big,
            Unknown
        }

        /// <summary>
        /// Detects the endianness of the current CPU
        /// </summary>
        private static unsafe Endianess GetEndianess() {
            uint value = 0xaabbccdd;
            byte* b = (byte*)&value;
            if (b[0] == 0xdd)
                return Endianess.Little;
            if (b[0] == 0xaa)
                return Endianess.Big;
            return Endianess.Unknown;
        }

        /// <summary>
        /// Determines if the current CPU supports unaligned reads
        /// </summary>
        private static bool IsWhitelistedCPU() {
#if PROPER_DETECT
            // Detect the current CPU architecture to enable unaligned reads
            switch (Environment.OSVersion.Platform) {
                // Unix
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                case (PlatformID) 128:
                    utsname buf;
                    if (uname(out buf) == 0) {
                        switch (buf.machine) {
                            case "i386":
                            case "i686":
                            case "x86_64":
                            case "arm":
                            case "armv7l":
                            case "aarch64":
                                return true;
                        }
                    }
                    break;
                // Windows NT
                case PlatformID.Win32NT:
                    SYSTEM_INFO info;
                    GetNativeSystemInfo(out info);
                    switch (info.wProcessorArchitecture) {
                        case 0: // Intel (x86)
                        case 5: // ARM
                        case 9: // AMD64 (x64)
                            return true;
                    }
                    break;
            }

            return false;
#else
            return true;
#endif
        }
    }
}