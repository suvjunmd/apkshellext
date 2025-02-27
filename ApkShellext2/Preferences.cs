﻿using SharpShell.Diagnostics;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using ApkShellext2.Properties;
using System.Globalization;

namespace ApkShellext2 {

    public partial class Preferences : Form {
        public Preferences() {
            InitializeComponent();
        }

        private bool formLoaded = false;
        private bool updateChecked = false;
        private Thread thUpdate;
        private string version = "";

        private void Preferences_Load(object sender, EventArgs e) {
            Utility.Localize();

            #region Initialize text
            this.Text = Resources.strPreferencesCaption;
            this.Icon = Icon.FromHandle(Utility.ResizeBitmap(Properties.Resources.logo, 16).GetHicon());

            // Dropdown
            if (!formLoaded) {
                CultureInfo[] culs = Utility.getSupportedLanguages();
                combLanguage.Text = culs[0].NativeName;
                foreach (var l in culs) {
                    combLanguage.Items.Add(l.NativeName);
                    if (l.LCID == Thread.CurrentThread.CurrentUICulture.LCID) {
                        combLanguage.Text = l.NativeName;
                    }
                }
            }

            // Labels
            lblLanguage.Text = Resources.strLanguages;
            label1.Text = string.Format(Resources.strCurrVersion, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            lblNewVer.Text = Resources.strCheckingNewVersion;
            lblRenamePattern.Text = Resources.strRenamePattern;

            // buttons
            btnUpdate.Image = Utility.ResizeBitmap(Properties.Resources.GitHub, 16);
            btnCancel.Text = Resources.btnCancel;
            btnUpdate.Text = Resources.btnGitHub;
            toolTip1.SetToolTip(btnUpdate, Resources.strGotoProjectSite);

            // Check boxes
            ckRename.Text = Resources.strRenameWithVersionCode;
            toolTip1.SetToolTip(ckRename, Resources.strRenameToolTip);
            ckShowPlay.Text = Resources.strAlwaysShowGooglePlay;
            toolTip1.SetToolTip(ckShowPlay, Resources.strAlwaysShowGooglePlayToolTip);
            ckShowOverlay.Text = Resources.strShowOverlayIcon;
            toolTip1.SetToolTip(ckShowOverlay, Resources.strShowOverlayIconToolTip);
            ckRename.Checked = (Utility.getRegistrySetting(Utility.keyRenameWithVersionCode) == 1);
            ckShowPlay.Checked = (Utility.getRegistrySetting(Utility.keyAlwaysShowGooglePlay) == 1);
            ckShowOverlay.Checked = (Utility.getRegistrySetting(Utility.keyShowOverlay) == 1);
            ckShowMenuIcon.Checked = (Utility.getRegistrySetting(Utility.keyShowMenuIcon, 1) == 1);
            ckShowMenuIcon.Text = Resources.strShowContextMenuIcon;
            checkBox4.Text = Resources.strShowIpaIcon;
            checkBox4.Checked = (Utility.getRegistrySetting(Utility.keyShowIpaIcon, 100) == 1);
            ckShowAppxIcon.Text = Resources.strShowAppxIcon;
            ckShowAppxIcon.Checked = (Utility.getRegistrySetting(Utility.keyShowAppxIcon, 100) == 1);

            #endregion

            #region check update thread
            if (!updateChecked) {
                timer1.Interval = 1000;
                timer1.Enabled = true;
                thUpdate = new Thread(new ThreadStart(() => { version = Utility.getLatestVersion(); }));
                thUpdate.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                thUpdate.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                thUpdate.Start();
            }
            #endregion

            btnCancel.Focus();
            formLoaded = true;
        }

         private void btnCancel_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyRenameWithVersionCode, ckRename.Checked ? 1 : 0);
        }


        private void combLanguage_SelectedIndexChanged(object sender, EventArgs e) {
            CultureInfo[] supported = Utility.getSupportedLanguages();
            if (formLoaded && supported[combLanguage.SelectedIndex].LCID != Thread.CurrentThread.CurrentCulture.LCID) {
                Utility.setRegistrySetting("language", supported[combLanguage.SelectedIndex].LCID);
                this.OnLoad(e);
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyShowIpaIcon, checkBox4.Checked ? 1 : 0);
            SharpShell.Interop.Shell32.SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        private void timer1_Tick(object sender, EventArgs e) {
            if (version == "") {
                lblNewVer.Text = Resources.strCheckingNewVersion;
                btnUpdate.Text = Resources.btnGitHub;
            } else {
                string[] latestV = version.Split(new Char[] { '.' });
                string[] curV = GetType().Assembly.GetName().Version.ToString().Split(new Char[] { '.' });

                // version number should be always 4 parts
                for (int i = 0; i < latestV.Length; i++) {
                    if (latestV[i] != curV[i]) {
                        if (int.Parse(latestV[i]) > int.Parse(curV[i])) {
                            lblNewVer.Text = string.Format(Resources.strNewVersionAvailible, version);
                            btnUpdate.Text = Resources.btnUpdate;
                            btnUpdate.Image = Utility.ResizeBitmap(Properties.Resources.udpate, 16);
                            toolTip1.SetToolTip(btnUpdate, Resources.btnUpdateToolTip);
                        } else {
                            lblNewVer.Text = Resources.strGotLatest;
                            btnUpdate.Text = Resources.btnGitHub;
                        }
                        timer1.Enabled = false;
                        return;
                    }
                }
            }
        }

        private void ckShowAppxIcon_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyShowAppxIcon, ckShowAppxIcon.Checked ? 1 : 0);           
            SharpShell.Interop.Shell32.SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        private void ckShowOverlay_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyShowOverlay, ckShowOverlay.Checked ? 1 : 0);
            SharpShell.Interop.Shell32.SHChangeNotify(0x08000000, 0, IntPtr.Zero, IntPtr.Zero);
        }

        private void btnUpdate_Click(object sender, EventArgs e) {
            System.Diagnostics.Process.Start(string.Format(Resources.urlGithubHomeWithVersion, Assembly.GetExecutingAssembly().GetName().Version.ToString()));
        }

        private void ckShowMenuIcon_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyShowMenuIcon, ckShowMenuIcon.Checked ? 1 : 0);
        }

        private void ckShowPlay_CheckedChanged(object sender, EventArgs e) {
            Utility.setRegistrySetting(Utility.keyAlwaysShowGooglePlay, ckShowPlay.Checked ? 1 : 0);
        }
    }
}
