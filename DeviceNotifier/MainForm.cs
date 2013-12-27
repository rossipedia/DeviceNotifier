using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DeviceNotifier
{
    // ReSharper disable once RedundantNameQualifier
    [System.ComponentModel.DesignerCategory("")]
    internal class MainForm : Form
    {
        private readonly NotifyIcon _icon;
        private IntPtr _notificationHandle;
        private readonly MessagesForm _popup;
        private readonly Timer _timer;

        public MainForm(NotifyIcon icon, MessagesForm popup, Timer timer)
        {
            _icon = icon;
            _popup = popup;
            _timer = timer;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            RegisterForNotifications();
            base.OnLoad(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            UnregisterNotifications();

            _popup.Close();
            _popup.Dispose();

            base.OnClosing(e);
        }

        private void RegisterForNotifications()
        {
            var filter = new DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
                dbcc_devicetype = DeviceTypes.DBT_DEVTYP_DEVICEINTERFACE,
                // dbcc_classguid = new Guid(0x25dbce51, 0x6c8f, 0x4a72, 0x8a,0x6d,0xb5,0x4c,0x2b,0x4f,0xc8,0x35)
            };

            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(filter));

            try
            {
                Marshal.StructureToPtr(filter, ptr, false);
                var result = User32.RegisterDeviceNotification(this.Handle, ptr, (int)User32.Flags.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
                if (result == IntPtr.Zero)
                {
                    throw new Exception("Could not register for device notifications: " + GetLastFormattedError());
                }
                _notificationHandle = result;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private string GetLastFormattedError()
        {
            var sb = new StringBuilder(2048);
            var error = Kernel32.GetLastError();
            var msgLength = Kernel32.FormatMessage((int)Kernel32.Flags.FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, error, 0 /* don't specify lang */, sb, sb.Capacity, IntPtr.Zero);
            return sb.ToString(0, msgLength);
        }

        private void UnregisterNotifications()
        {
            if (User32.UnregisterDeviceNotification(_notificationHandle) == 0)
            {
                throw new Exception("Could not unregister device notifications: " + GetLastFormattedError());
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Handle WM_DEVICECHANGE
            if (m.Msg != Messages.WM_DEVICECHANGE)
            {
                base.WndProc(ref m);
                return;
            }

            switch (m.WParam.ToInt32())
            {
            case WParams.DBT_DEVNODESCHANGED:

                break;

            case WParams.DBT_DEVICEARRIVAL:
            case WParams.DBT_DEVICEREMOVECOMPLETE:
                var header = (DEV_BROADCAST_HDR)Marshal.PtrToStructure(m.LParam, typeof(DEV_BROADCAST_HDR));
                switch (header.dbch_devicetype)
                {
                case DeviceTypes.DBT_DEVTYP_DEVICEINTERFACE:
                    HandleDeviceInterface(m.LParam, m.WParam.ToInt32() == WParams.DBT_DEVICEARRIVAL);
                    break;
                }
                break;
            }
        }

        private void HandleDeviceInterface(IntPtr lParam, bool connected)
        {
            var dvi = (DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE));

            var msg = string.Format("Device \"{0}\" {1}connected", GetDeviceName(dvi) , connected ? "" : "dis");
            _popup.AddMessage(msg);
        }
        
        private static string GetDeviceName(DEV_BROADCAST_DEVICEINTERFACE dvi)
        {
            var parts = dvi.dbcc_name.Split('#');
            if (parts.Length <= 3) return "(Unkown device): " + dvi.dbcc_name;
            var type = parts[0].Substring(parts[0].IndexOf(@"?\", StringComparison.Ordinal) + 2);
            var iid = parts[1];
            var uid = parts[2];

            var regPath = string.Format(@"SYSTEM\CurrentControlSet\Enum\{0}\{1}\{2}", type, iid, uid);
            using (var key = Registry.LocalMachine.OpenSubKey(regPath))
            {
                if (key == null) return "(Unkown device): " + dvi.dbcc_name;
                var friendlyName = key.GetValue("FriendlyName");
                var devDesc = key.GetValue("DeviceDesc");


                if (friendlyName != null) return friendlyName.ToString().Split(';').Last();
                if (devDesc != null) return devDesc.ToString().Split(';').Last();
            }

            return "(Unkown device): " + dvi.dbcc_name;
        }
    }
}