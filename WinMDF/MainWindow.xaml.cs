using Microsoft.Win32;
using System;
using System.Configuration;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WinMDF
{
    public partial class MainWindow : Window
    {
        #region App Stuff
        private int PREFERRED_DISPLAY_MODE = 4;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                SourceInitialized += MainWindow_SourceInitialized;

                int.TryParse(ConfigurationManager.AppSettings["PreferredDisplayMode"].ToString(), out PREFERRED_DISPLAY_MODE);

                if (ConfigurationManager.AppSettings["ShowNotification"].ToString().ToLower() == bool.TrueString.ToLower())
                {
                    NotifyIcon nIcon = new NotifyIcon();
                    nIcon.Icon = new Icon(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Icon.ico")).Stream);
                    nIcon.Text = "WinMDF";
                    nIcon.ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Close", new EventHandler(delegate {
                    System.Windows.Application.Current.Shutdown();
                }))});
                    nIcon.Visible = true;
                }

                if (ConfigurationManager.AppSettings["AddToStartup"].ToString().ToLower() == bool.TrueString.ToLower())
                {
                    RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                    rk.SetValue(System.Windows.Application.Current.MainWindow.GetType().Assembly.GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show($"Error: {e.Message}");
            }
        }

        void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            RegisterForPowerNotifications();
            HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(new HwndSourceHook(WndProc));
            Visibility = Visibility.Hidden;
        }
        #endregion

        #region Lid Stuff
        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int WM_POWERBROADCAST = 0x0218;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        private bool? PreviousLidState = null;

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private void RegisterForPowerNotifications()
        {
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(
                new WindowInteropHelper(System.Windows.Application.Current.Windows[0]).Handle,
                ref GUID_LIDSWITCH_STATE_CHANGE,
                DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_POWERBROADCAST:
                    OnPowerBroadcast(wParam, lParam);
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                var ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));

                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    var isLidOpen = ps.Data != 0;

                    if (isLidOpen && !isLidOpen == PreviousLidState)
                    {
                        LidStatusChanged();
                    }

                    PreviousLidState = isLidOpen;
                }
            }
        }
        #endregion

        private void LidStatusChanged()
        {
            if (GetScreenCount() < 2)
                return;

            switch (PREFERRED_DISPLAY_MODE)
            {
                case 1:
                    CloneDisplays();
                    break;
                case 2:
                    ExtendDisplays();
                    break;
                case 3:
                    InternalDisplay();
                    break;
                case 4:
                    ExternalDisplay();
                    break;
                default:
                    ExtendDisplays();
                    break;
            }
        }

        private int GetScreenCount()
        {
            System.Management.ManagementObjectSearcher monitorObjectSearch = new System.Management.ManagementObjectSearcher("SELECT * FROM Win32_DesktopMonitor");
            return monitorObjectSearch.Get().Count;
        }

        #region Display Config Stuff
        [Flags]
        public enum SetDisplayConfigFlags : uint
        {
            SDC_TOPOLOGY_INTERNAL = 0x00000001,
            SDC_TOPOLOGY_CLONE = 0x00000002,
            SDC_TOPOLOGY_EXTEND = 0x00000004,
            SDC_TOPOLOGY_EXTERNAL = 0x00000008,
            SDC_APPLY = 0x00000080
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern long SetDisplayConfig(uint numPathArrayElements,
            IntPtr pathArray, uint numModeArrayElements, IntPtr modeArray, SetDisplayConfigFlags flags);

        void CloneDisplays()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_CLONE | SetDisplayConfigFlags.SDC_APPLY);
        }

        void ExtendDisplays()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_EXTEND | SetDisplayConfigFlags.SDC_APPLY);
        }

        void InternalDisplay()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_INTERNAL | SetDisplayConfigFlags.SDC_APPLY);
        }

        void ExternalDisplay()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_EXTERNAL | SetDisplayConfigFlags.SDC_APPLY);
        }
        #endregion
    }
}
