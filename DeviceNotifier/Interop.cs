using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DeviceNotifier
{
    // ReSharper disable InconsistentNaming
    internal static class Messages
    {
        public const int WM_DEVICECHANGE = 0x0219;
    }

    internal static class WParams
    {
        public const int DBT_DEVNODESCHANGED = 0x0007;
        public const int DBT_DEVICEARRIVAL = 0x8000;
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    }

    internal static class DeviceTypes
    {
        public const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
        public const int DBT_DEVTYP_HANDLE = 0x00000006;
        public const int DBT_DEVTYP_OEM = 0x00000000;
        public const int DBT_DEVTYP_PORT = 0x00000003;
        public const int DBT_DEVTYP_VOLUME = 00000002;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DEV_BROADCAST_HDR
    {
        public int dbch_size;
        public int dbch_devicetype;
        public int dbch_reserved;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string dbcc_name;
    };

    internal static class User32
    {
        [Flags]
        public enum Flags : int
        {
            DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000,
            DEVICE_NOTIFY_SERVICE_HANDLE = 0x00000001,
            DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 0x00000004,
        }

        [DllImport("user32")]
        public static extern IntPtr RegisterDeviceNotification(IntPtr recipient, IntPtr filter, int flags);

        [DllImport("user32")]
        public static extern int UnregisterDeviceNotification(IntPtr handle);
    }

    internal static class Kernel32
    {
        [Flags]
        public enum Flags
        {
            FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000
        }

        [DllImport("kernel32")]
        public static extern int GetLastError();

        [DllImport("kernel32")]
        public static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageIn, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);
    }

    // ReSharper restore InconsistentNaming
}
