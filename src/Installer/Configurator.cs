using System;
using System.IO;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace JhonlloydSetup {
    public static class DeviceConfigurator {
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen);

        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct TokPriv1Luid {
            public int Count;
            public long Luid;
            public int Attr;
        }

        public static void EnablePrivileges() {
            IntPtr hToken = IntPtr.Zero;
            if (OpenProcessToken(Process.GetCurrentProcess().Handle, 0x20 | 0x8, ref hToken)) {
                EnablePrivilege(hToken, "SeTakeOwnershipPrivilege");
                EnablePrivilege(hToken, "SeRestorePrivilege");
                EnablePrivilege(hToken, "SeBackupPrivilege");
            }
        }

        private static void EnablePrivilege(IntPtr hToken, string privName) {
            long luid = 0;
            if (LookupPrivilegeValue(null, privName, ref luid)) {
                TokPriv1Luid tp = new TokPriv1Luid { Count = 1, Luid = luid, Attr = 2 };
                AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static void GrantFullControl(RegistryKey key) {
            try {
                var security = key.GetAccessControl(AccessControlSections.All);
                var adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
                security.SetOwner(adminSid);
                key.SetAccessControl(security);

                var userSid = WindowsIdentity.GetCurrent().User;
                security = key.GetAccessControl(AccessControlSections.All);
                security.AddAccessRule(new RegistryAccessRule(adminSid, RegistryRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                security.AddAccessRule(new RegistryAccessRule(userSid, RegistryRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
                key.SetAccessControl(security);
            } catch { }
        }

        public static void ConfigureEqualizerAPOOnAllEndpoints() {
            EnablePrivileges();
            string capturePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture";
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                using (var captureKey = baseKey.OpenSubKey(capturePath, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership | RegistryRights.ChangePermissions | RegistryRights.FullControl)) {
                    if (captureKey == null) return;
                    GrantFullControl(captureKey);

                    foreach (var devSubName in captureKey.GetSubKeyNames()) {
                        try {
                            using (var devKey = captureKey.OpenSubKey(devSubName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership | RegistryRights.ChangePermissions | RegistryRights.FullControl)) {
                                if (devKey == null) continue;
                                GrantFullControl(devKey);

                                RegistryKey fxKey = null;
                                try {
                                    fxKey = devKey.OpenSubKey("FxProperties", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership | RegistryRights.ChangePermissions | RegistryRights.FullControl);
                                } catch { }

                                if (fxKey == null) {
                                    try { fxKey = devKey.CreateSubKey("FxProperties"); } catch { }
                                }

                                if (fxKey != null) {
                                    using (fxKey) {
                                        GrantFullControl(fxKey);
                                        // Equalizer APO Capture Engine GUID
                                        fxKey.SetValue("{d04e05a6-594b-4fb6-a80d-01af5eed7d1d},5", "{EACD2258-FCAC-4FF4-B36D-419E924A6D79}", RegistryValueKind.String);
                                        Console.WriteLine("  [+] Attached Equalizer APO to device: " + devSubName);
                                    }
                                }
                            }
                        } catch { }
                    }
                }
            }
        }

        public static void SetRegistryAudioOptimizations() {
            try {
                // Disable Windows Audio Ducking (Set to 'Do nothing' = 3)
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Multimedia\Audio")) {
                    if (key != null) key.SetValue("UserDuckingPreference", 3, RegistryValueKind.DWord);
                }
                Console.WriteLine("  [+] Windows Ducking preference set to 'Do Nothing'.");
            } catch { }
        }
    }
}
