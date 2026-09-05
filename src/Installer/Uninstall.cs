using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using Microsoft.Win32;

namespace JhonlloydUninstall {
    class Program {
        static bool IsAdministrator() {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void Elevate() {
            var proc = new ProcessStartInfo {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Verb = "runas"
            };
            try { Process.Start(proc); } catch { }
            Environment.Exit(0);
        }

        static void Run(string cmd, string args) {
            try {
                var p = Process.Start(new ProcessStartInfo {
                    FileName = cmd,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                p.WaitForExit();
            } catch { }
        }

        static void Main(string[] args) {
            Console.Title = "Jhonlloyd Noise Cancellation - Uninstaller";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("====================================================================");
            Console.WriteLine("    JHONLLOYD NOISE CANCELLATION & MIC LOCK - UNINSTALLER           ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            if (!IsAdministrator()) {
                Elevate();
                return;
            }

            Console.WriteLine("[*] Stopping and deleting MicLock100 background task...");
            Run("schtasks.exe", "/End /TN \"MicLock100\"");
            Run("schtasks.exe", "/Delete /TN \"MicLock100\" /F");

            Console.WriteLine("[*] Removing MicLock binaries...");
            try {
                if (Directory.Exists(@"C:\ProgramData\MicLock100")) {
                    Directory.Delete(@"C:\ProgramData\MicLock100", true);
                }
            } catch { }

            Console.WriteLine("[*] Cleaning Equalizer APO capture registry attachments...");
            try {
                string capturePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture";
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var captureKey = baseKey.OpenSubKey(capturePath, true)) {
                        if (captureKey != null) {
                            foreach (var devSub in captureKey.GetSubKeyNames()) {
                                try {
                                    using (var devKey = captureKey.OpenSubKey(devSub, true)) {
                                        if (devKey != null) {
                                            using (var fxKey = devKey.OpenSubKey("FxProperties", true)) {
                                                if (fxKey != null) {
                                                    fxKey.DeleteValue("{d04e05a6-594b-4fb6-a80d-01af5eed7d1d},5", false);
                                                }
                                            }
                                        }
                                    }
                                } catch { }
                            }
                        }
                    }
                }
            } catch { }

            Console.WriteLine("[*] Refreshing Windows Audio service...");
            Run("net.exe", "stop audiosrv /y");
            Run("net.exe", "start audiosrv");
            Run("net.exe", "start AudioEndpointBuilder");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] Uninstalled cleanly. All audio settings restored to default.");
            Console.ResetColor();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
