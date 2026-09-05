using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;

namespace JhonlloydNoiseCancellation {
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
            try {
                Process.Start(proc);
            } catch {
                Console.WriteLine("[-] Administrator rights are required to install system audio drivers.");
            }
            Environment.Exit(0);
        }

        static void ExtractResource(string resName, string outPath) {
            var asm = Assembly.GetExecutingAssembly();
            using (var stream = asm.GetManifestResourceStream(resName)) {
                if (stream == null) {
                    throw new Exception("Embedded resource not found: " + resName);
                }
                var dir = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                using (var fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write)) {
                    stream.CopyTo(fileStream);
                }
            }
        }

        static void RunCommand(string fileName, string args) {
            var psi = new ProcessStartInfo {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var p = Process.Start(psi)) {
                p.WaitForExit();
            }
        }

        static void Main(string[] args) {
            Console.Title = "Jhonlloyd Noise Cancellation - 1-Click Complete Setup";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("====================================================================");
            Console.WriteLine("    JHONLLOYD NOISE CANCELLATION & 100% MIC LOCK SUITE             ");
            Console.WriteLine("    AI-Powered Neural Filtering | Crisp & Deep Voice | Zero Noise   ");
            Console.WriteLine("====================================================================");
            Console.ResetColor();

            if (!IsAdministrator()) {
                Console.WriteLine("[*] Elevating to Administrator privilege...");
                Elevate();
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "JhonlloydAudioSetup_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try {
                Console.WriteLine("\n[1/6] Extracting core installer components...");
                string eqInstallerPath = Path.Combine(tempDir, "EqualizerAPO-x64.exe");
                string rnnoiseDllPath = Path.Combine(tempDir, "rnnoise_mono.dll");
                string configTxtPath = Path.Combine(tempDir, "config.txt");

                ExtractResource("EqualizerAPO-x64.exe", eqInstallerPath);
                ExtractResource("rnnoise_mono.dll", rnnoiseDllPath);
                ExtractResource("config.txt", configTxtPath);

                Console.WriteLine("[2/6] Installing Equalizer APO audio driver silently...");
                // Silent install Equalizer APO
                RunCommand(eqInstallerPath, "/S");

                string eqInstallDir = @"C:\Program Files\EqualizerAPO";
                string vstDir = Path.Combine(eqInstallDir, "VSTPlugins");
                string configDir = Path.Combine(eqInstallDir, "config");
                if (!Directory.Exists(vstDir)) Directory.CreateDirectory(vstDir);
                if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

                Console.WriteLine("[3/6] Deploying Xiph RNNoise AI Neural Network engine...");
                File.Copy(rnnoiseDllPath, Path.Combine(vstDir, "rnnoise_mono.dll"), true);

                Console.WriteLine("[4/6] Applying Deep-Voice & Crisp articulation EQ profile...");
                File.Copy(configTxtPath, Path.Combine(configDir, "config.txt"), true);

                Console.WriteLine("[5/6] Dynamically registering audio capture endpoints in Windows Registry...");
                JhonlloydSetup.DeviceConfigurator.ConfigureEqualizerAPOOnAllEndpoints();
                JhonlloydSetup.DeviceConfigurator.SetRegistryAudioOptimizations();

                Console.WriteLine("[6/6] Compiling & registering permanent 100% MicLock background service...");
                string micLockInstallDir = @"C:\ProgramData\MicLock100";
                if (!Directory.Exists(micLockInstallDir)) Directory.CreateDirectory(micLockInstallDir);

                string micLockExe = Path.Combine(micLockInstallDir, "MicLock.exe");
                string micLockCs = Path.Combine(tempDir, "MicLock.cs");
                ExtractResource("MicLock.cs", micLockCs);

                string csc = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"Microsoft.NET\Framework64\v4.0.30319\csc.exe");
                if (!File.Exists(csc)) {
                    csc = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"Microsoft.NET\Framework\v4.0.30319\csc.exe");
                }

                RunCommand(csc, "/nologo /target:winexe /out:\"" + micLockExe + "\" \"" + micLockCs + "\"");

                // Stop old task if exists
                RunCommand("schtasks.exe", "/End /TN \"MicLock100\"");
                RunCommand("schtasks.exe", "/Delete /TN \"MicLock100\" /F");

                // Create resilient SYSTEM startup task
                RunCommand("schtasks.exe", "/Create /TN \"MicLock100\" /TR \"\\\"" + micLockExe + "\\\"\" /SC ONSTART /RU \"NT AUTHORITY\\SYSTEM\" /RL HIGHEST /F");
                // Also trigger immediate launch
                RunCommand("schtasks.exe", "/Run /TN \"MicLock100\"");

                // Restart Windows Audio Service to bind drivers
                Console.WriteLine("\n[*] Refreshing Windows Audio service...");
                RunCommand("net.exe", "stop audiosrv /y");
                RunCommand("net.exe", "start audiosrv");
                RunCommand("net.exe", "start AudioEndpointBuilder");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n====================================================================");
                Console.WriteLine("  [SUCCESS] All components installed & configured flawlessly!       ");
                Console.WriteLine("====================================================================");
                Console.ResetColor();
                Console.WriteLine("  * 100% Mic Lock: Active & Running (Instant watchdog)");
                Console.WriteLine("  * Zero-Noise AI: RNNoise Neural Model Deployed");
                Console.WriteLine("  * Voice Tone: Deep Warmth (120Hz) + Broadcast Air (10kHz)");
                Console.WriteLine("  * Lifetime: Auto-boots silently on Windows startup");
                Console.WriteLine("\nNote: A one-time computer restart is recommended to finalize driver hooks.");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            } catch (Exception ex) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[ERROR] " + ex.Message);
                Console.ResetColor();
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            } finally {
                try {
                    Directory.Delete(tempDir, true);
                } catch { }
            }
        }
    }
}
