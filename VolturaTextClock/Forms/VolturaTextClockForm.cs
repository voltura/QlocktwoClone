﻿using System;
using System.IO;
using System.Windows.Forms;
using static VolturaTextClock.Program;
using VolturaTextClock.Library;

namespace VolturaTextClock
{
    public partial class VolturaTextClockForm : Form
    {
        private SettingsForm m_SettingsForm;
        private TextClockTheme m_Theme;

        public VolturaTextClockForm()
        {
            InitializeComponent();
            SetTheme();
            UpdateUI();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
        }

        public SettingsForm ApplicationSettingsForm
        {
            get
            {
                if (m_SettingsForm == null)
                {
                    m_SettingsForm = new SettingsForm();
                }
                return m_SettingsForm;
            }
            set => m_SettingsForm = value;
        }

        private void SetTheme()
        {
            m_Theme = new TextClockTheme
            {
                BackgroundImagePath = SaveImageToDisk(),
                Language = TextClockTheme.LANGUAGE.Swedish,
                ClockImageFullPath = Path.Combine(Path.GetTempPath(), "clock.png")
            };
        }

        private void SetFormLocationFromSettings()
        {
            string formLocation = AppConfig.GetValue<string>("mainFormLocation", "40,40");
            string[] formLeftAndTop = formLocation.Split(new char[] { ',' });
            if (formLeftAndTop.Length == 2)
            {
                int tmpLeft = Convert.ToInt32(formLeftAndTop[0]);
                int tmpTop = Convert.ToInt32(formLeftAndTop[1]);
                if (tmpTop >= 0)
                {
                    Location = new System.Drawing.Point(tmpLeft, tmpTop);
                }
            }
        }

        private void UpdateUI()
        {
            clockPictureBox.MoveOtherWithMouse(this);
            clockPictureBox.SendToBack();
        }

        private void ShowSettingsForm()
        {
            Log.Info = "Displayed settings form";
            if (ApplicationSettingsForm.Visible)
            {
                ApplicationSettingsForm.BringToFront();
                ApplicationSettingsForm.Focus();
            }
            else
            {
                if (TopMost)
                {
                    TopMost = false;
                }
                ApplicationSettingsForm.ShowDialog(this);
                TopMost = AppConfig.GetValue("alwaysOnTop", false);
                pinPicBox.Image = TopMost ? Properties.Resources.unpin : Properties.Resources.pin;
            }
        }

        private void ToogleButtons()
        {
            settingsPicBox.Visible = pinPicBox.Visible = closePicBox.Visible = minimizePicBox.Visible = !minimizePicBox.Visible;
            clockPictureBox.SendToBack();
        }

        private static string SaveImageToDisk()
        {
            string path = Path.Combine(Path.GetTempPath(), "bg.png");

            try
            {
                if (File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                }
                Properties.Resources.background.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Log.Error = ex;
            }
            return path.Replace('\\', '/');
        }

        private void UpdateClockText()
        {
            if (Visible && (m_SettingsForm == null || m_SettingsForm.Visible == false))
                clockPictureBox.Image = TextClock.GetImage(m_Theme);  // clock image that adhears to the scale factor set in Windows (dpi)
            StartClockTimer();
        }

        private void StartClockTimer()
        {
            if (!clockTimer.Enabled)
            {
                clockTimer.Interval = 60000 - DateTime.Now.Millisecond;
                clockTimer.Start();
            }
        }

        private void SaveFormLocation()
        {
            if (Location.X >= 0)
            {
                AppConfig.AddOrUpdateAppSetting("mainFormLocation", $"{Location.X},{Location.Y}");
            }
        }

        private void VolturaTextClockForm_Load(object sender, EventArgs e)
        {
            UpdateClockText();
            SetFormLocationFromSettings();
            TopMost = AppConfig.GetValue("alwaysOnTop", false);
            pinPicBox.Image = TopMost ? Properties.Resources.unpin : Properties.Resources.pin;
            Visible = true;
        }

        private void OptionsPicBox_Click(object sender, EventArgs e)
        {
            ToogleButtons();
        }

        private void SettingsPicBox_Click(object sender, EventArgs e)
        {
            ShowSettingsForm();
        }

        /*private void CloseSettingsForm()
        {
            if (ApplicationSettingsForm.WindowState != FormWindowState.Normal ||
                !ApplicationSettingsForm.Visible)
            {
                return;
            }

            ApplicationSettingsForm.Visible = false;
            ApplicationSettingsForm.Close();
        }*/

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            clockTimer.Stop();
            UpdateClockText();
        }

        private void ClosePicBox_Click(object sender, EventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show(this, "Do you want to close the application?", 
                "Close application", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
            {
                Application.Exit();
            }
        }

        private void VolturaTextClockForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveFormLocation();
        }

        private void PinPicBox_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
            pinPicBox.Image = TopMost ? Properties.Resources.unpin : Properties.Resources.pin;
            AppConfig.AddOrUpdateAppSetting("alwaysOnTop", TopMost);
            ToogleButtons();
        }

        private void MinimizePicBox_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ToogleButtons();
        }

        private const int CS_DROPSHADOW = 0x20000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }
}
