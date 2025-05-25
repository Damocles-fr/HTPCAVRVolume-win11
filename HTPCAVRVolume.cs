using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace HTPCAVRVolume
{
    public partial class HTPCAVRVolume : Form
    {
        private readonly string config = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\HTPCAVRVolumeConfig.txt";
        private bool _muted = false;
        private GlobalKeyboardHook globalHook;


        private KeyboardHook hookVolUp;
        private KeyboardHook hookVolDown;
        private KeyboardHook hookToggleMute;

        private AVRDevices.IAVRDevice _AVR;

        public HTPCAVRVolume()
        {
            InitializeComponent();
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3).Replace(".0", "");
            Text = $"HTPCAVRVolume v{version}";
        }

        private void HTPCAVRVolume_Load(object sender, EventArgs e)
        {
            LoadDevice();

            globalHook = new GlobalKeyboardHook();

            globalHook.VolumeUpPressed += (s, args) => btnVolUp.PerformClick();
            globalHook.VolumeDownPressed += (s, args) => btnVolDown.PerformClick();
            globalHook.VolumeMutePressed += (s, args) => btnToggleMute.PerformClick();
        }    // Use globalHook instead

        private void LoadDevice()
        {
            try
            {
                string[] deviceInfo = File.ReadAllText(config).Split('=');
                cmbDevice.SelectedItem = deviceInfo[0];
                tbIP.Text = deviceInfo[1];

                switch (deviceInfo[0])
                {
                    case "DenonMarantz":
                        _AVR = new AVRDevices.DenonMarantzDevice(tbIP.Text);
                        break;
                    case "StormAudio":
                        _AVR = new AVRDevices.StormAudioDevice(tbIP.Text);
                        break;
                }
            }
            catch { }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY_MSG_ID)
            {
                switch (GetKey(m.LParam))
                {
                    case Keys.VolumeUp:
                        btnVolUp.PerformClick();
                        break;
                    case Keys.VolumeDown:
                        btnVolDown.PerformClick();
                        break;
                    case Keys.VolumeMute:
                        btnToggleMute.PerformClick();
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private Keys GetKey(IntPtr LParam)
        {
            return (Keys)((LParam.ToInt32()) >> 16);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(config, cmbDevice.SelectedItem.ToString() + "=" + tbIP.Text);
                LoadDevice();
            }
            catch
            {
                MessageBox.Show("Try running as Administrator and saving your config again.", "Error saving config");
            }
        }

        private void BtnVolUp_Click(object sender, EventArgs e)
        {
            _AVR.VolUp();
        }

        private void BtnVolDown_Click(object sender, EventArgs e)
        {
            _AVR.VolDown();
        }

        private void BtnToggleMute_Click(object sender, EventArgs e)
        {
            _AVR.ToggleMute(_muted);
            _muted = !_muted;
        }

        private void HTPCAVRVolume_FormClosed(object sender, FormClosedEventArgs e)
        {
            globalHook?.Dispose();
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void HTPCAVRVolume_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                ShowInTaskbar = false;  // window hidden in the taskbar, but still active.
            }
            else if (WindowState == FormWindowState.Normal)
            {
                notifyIcon.Visible = false;
                ShowInTaskbar = true;
            }
        }

        private void HTPCAVRVolume_Shown(object sender, EventArgs e)
        {
            if (cmbDevice.SelectedItem != null)
            {
                WindowState = FormWindowState.Minimized;
            }
        }
    }
}
