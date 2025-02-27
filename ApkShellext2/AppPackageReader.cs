﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using ApkQuickReader;
using System.IO;

namespace ApkShellext2 {
    /// <summary>
    /// This is the base class for package reader
    /// defines common method a package reader should have
    /// for reusing and simplify in the code
    /// </summary>
    public class AppPackageReader : IDisposable{
        public enum AppType {
            AndroidApp,
            iOSApp,
            WindowsPhoneAppBundle,
            WindowsPhoneApp
        }
        public virtual string FileName { get; protected set; }
        // App name 
        public virtual string AppName { get { return ""; } }
        // Package Name
        public virtual string PackageName { get { return ""; } }

        // App version
        public virtual string Version { get { return ""; } }
        // Sub version
        public virtual string Revision { get { return ""; } }

        public virtual CultureInfo Culture { get; set; }

        // Publisher
        public virtual string Publisher { get { return ""; } }

        public virtual Bitmap Icon { get { return null; } }

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        bool disposed;
        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing) {
            if (disposed)
                return;
            disposed = true;
        }

        public static AppType getAppType(string path) {
            string suffix = Path.GetExtension(path);
            if (suffix == ".apk") {
                return AppType.AndroidApp;
            } else if (suffix == ".ipa") {
                return AppType.iOSApp;
            } else if (suffix == ".appxbundle") {
                return AppType.WindowsPhoneAppBundle;
            } else if (suffix == ".appx") {
                return AppType.WindowsPhoneApp;
            } else {
                throw new NotSupportedException(suffix + " type is not supported.");
            }
        }

        public static AppPackageReader Read(string path) {
            switch (getAppType(path)) {
                case AppType.AndroidApp:
                    return new ApkReader(path);
                case AppType.iOSApp:
                    return new IpaReader(path);
                case AppType.WindowsPhoneAppBundle:
                    return new AppxBundleReader(path);
                case AppType.WindowsPhoneApp:
                    return new AppxReader(path);
                default :
                    throw new NotSupportedException("File type is not supported.");
            }
        }
    }
}
