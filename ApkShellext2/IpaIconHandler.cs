﻿using PlistCS;
using PNGDecrush;
using SharpShell.Attributes;
using SharpShell.Extensions;
using SharpShell.Diagnostics;
using SharpShell.Exceptions;
using SharpShell.ServerRegistration;
using SharpShell.SharpIconHandler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace ApkShellext2 {

    [Guid("a0ac4758-12d3-4dcf-9d12-03faaa3c0a9d")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [COMServerAssociation(AssociationType.ClassOfExtension, ".ipa")]
    public class IpaIconHandler : SharpIconHandler {
        private Bitmap m_icon = null;

        protected override Icon GetIcon(bool smallIcon, uint iconSize) {
            if (Utility.getRegistrySetting(Utility.keyShowIpaIcon, 100) != 1) {
                return null;
            }

            if (smallIcon) {
                try {
                    return addOverlay((int)iconSize);
                } finally {
                    if (m_icon != null) {
                        m_icon.Dispose();
                        m_icon = null;
                    }
                }
            }

            try {
                using (IpaReader ipaReader = new IpaReader(SelectedItemPath)) {
                    m_icon = ipaReader.getImage(new string[] { "CFBundleIcons", "CFBundlePrimaryIcon", "CFBundleIconFiles" });
                }
            } catch {
                 // read error, draw the default icon
                m_icon = new Bitmap((int)iconSize, (int)iconSize);
                using (Graphics g = Graphics.FromImage(m_icon)) {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.FillRectangle(Brushes.White, 0, 0, iconSize, iconSize);
                    Rectangle rec = new Rectangle();
                    if (Properties.Resources.Apple.Width > Properties.Resources.Apple.Height) {
                        rec.Width = (int)iconSize;
                        rec.Height = (int)(Properties.Resources.Apple.Height * iconSize / Properties.Resources.Apple.Width);
                        rec.X = 0;
                        rec.Y = (int)(iconSize - rec.Height) / 2;
                    } else {
                        rec.Width = (int)(Properties.Resources.Apple.Width * iconSize /
                            Properties.Resources.Apple.Height);
                        rec.Height = (int)iconSize;
                        rec.X = (int)(iconSize - rec.Width) / 2;
                        rec.Y = 0;
                    }
                    g.DrawImage(Properties.Resources.Apple, rec);
                }
            }
            return addOverlay((int)iconSize);
        }

        private Icon addOverlay(int iconSize) {
            using (Bitmap b = new Bitmap(iconSize, iconSize)) {
                using (Graphics g = Graphics.FromImage(b)) {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    if (Utility.getRegistrySetting(Utility.keyShowOverlay) == 1) {
                        g.DrawImage(m_icon, (int)(iconSize * 0.05), 0, (int)(iconSize * 0.95), (int)(iconSize * 0.95));
                        int overlaySize = (int)(iconSize * 0.6);
                        overlaySize = (overlaySize >= 64) ? 64 : overlaySize;
                        int targetW = overlaySize;
                        targetW = (targetW >= 16) ? targetW : 16;
                        int targetH = Properties.Resources.Apple.Height * targetW / Properties.Resources.Apple.Width;
                        int targetY = iconSize - targetH;
                        g.DrawImage(Properties.Resources.Apple, 0, targetY, targetW, targetH);
                    } else {
                        g.DrawImage(m_icon, 0, 0, iconSize, iconSize);
                    }
                    return Icon.FromHandle(b.GetHicon());
                }
            }
        }
        [CustomRegisterFunction]
        public static void postDoRegister(Type type, RegistrationType registrationType) {
            Console.WriteLine("Registering " + type.FullName);
            #region Clean up older versions registry
            try {
                using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"\CLSID\" +
                    type.GUID.ToRegistryString() + @"\InprocServer32")) {
                    if (key != null && key.GetSubKeyNames().Count() != 0) {
                        Console.WriteLine("Found old version in registry, cleaning up ...");
                        foreach (var k in key.GetSubKeyNames()) {
                            if (k != type.Assembly.GetName().Version.ToString()) {
                                Registry.ClassesRoot.DeleteSubKeyTree(@"\CLSID\" +
                        type.GUID.ToRegistryString() + @"\InprocServer32\" + k);
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Logging.Error("Cleaning up older version but see exception. "
                     + e.Message);
            }
            #endregion
        }

        protected override void Log(string message) {
            Logging.Log(Path.GetFileName(SelectedItemPath) + "[" + DateTime.Now.ToString() + "]" + message);
        }
    }
}
