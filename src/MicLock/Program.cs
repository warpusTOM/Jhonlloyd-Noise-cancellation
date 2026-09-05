using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace MicLock {
    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDevice {
        int Activate(ref Guid id, int clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object interfacePointer);
        int OpenPropertyStore(int stgmAccess, out IntPtr properties);
        int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
        int GetState(out int state);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceCollection {
        int GetCount(out uint count);
        int Item(uint index, out IMMDevice device);
    }

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator {
        int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);
        int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
        int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice endpoint);
        int RegisterEndpointNotificationCallback(IntPtr client);
        int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    public class MMDeviceEnumeratorComObj { }

    [Guid("657804FA-D6AD-4496-8560-E5D579D04EEA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolumeCallback {
        [PreserveSig]
        int OnNotify(IntPtr pNotify);
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAudioEndpointVolume {
        int RegisterControlChangeNotify(IAudioEndpointVolumeCallback client);
        int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback client);
        int GetChannelCount(out int channelCount);
        int SetMasterVolumeLevel(float levelDb, ref Guid eventContext);
        int SetMasterVolumeLevelScalar(float level, ref Guid eventContext);
        int GetMasterVolumeLevel(out float levelDb);
        int GetMasterVolumeLevelScalar(out float level);
        int SetChannelVolumeLevel(uint channelNumber, float levelDb, ref Guid eventContext);
        int SetChannelVolumeLevelScalar(uint channelNumber, float level, ref Guid eventContext);
        int GetChannelVolumeLevel(uint channelNumber, out float levelDb);
        int GetChannelVolumeLevelScalar(uint channelNumber, out float level);
        int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, ref Guid eventContext);
        int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
        int GetVolumeStepInfo(out uint step, out uint stepCount);
        int VolumeStepUp(ref Guid eventContext);
        int VolumeStepDown(ref Guid eventContext);
        int QueryHardwareSupport(out uint hardwareSupportMask);
        int GetVolumeRange(out float volumeMindB, out float volumeMaxdB, out float volumeIncrementdB);
    }

    public class VolumeCallback : IAudioEndpointVolumeCallback {
        private readonly IAudioEndpointVolume _vol;
        public VolumeCallback(IAudioEndpointVolume vol) {
            _vol = vol;
        }

        public int OnNotify(IntPtr pNotify) {
            try {
                float level;
                if (_vol.GetMasterVolumeLevelScalar(out level) == 0) {
                    if (level < 0.999f) {
                        Guid ctx = Program.LockEventContext;
                        _vol.SetMasterVolumeLevelScalar(1.0f, ref ctx);
                    }
                }
            } catch { }
            return 0;
        }
    }

    public static class Program {
        public static readonly Guid LockEventContext = new Guid("6A4C7E32-9F8B-4D5A-B231-C05E87F41D92");
        private static readonly HashSet<string> _activeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<object> _keepAlive = new List<object>();

        [STAThread]
        public static void Main(string[] args) {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "Global\\WindowsMicLock100Percent", out createdNew)) {
                if (!createdNew) return;

                var enumerator = (IMMDeviceEnumerator)(new MMDeviceEnumeratorComObj());
                var iid = typeof(IAudioEndpointVolume).GUID;

                while (true) {
                    try {
                        IMMDeviceCollection collection;
                        // 1 = eCapture, 1 = DEVICE_STATE_ACTIVE
                        if (enumerator.EnumAudioEndpoints(1, 1, out collection) == 0 && collection != null) {
                            uint count;
                            collection.GetCount(out count);
                            for (uint i = 0; i < count; i++) {
                                IMMDevice dev;
                                if (collection.Item(i, out dev) == 0 && dev != null) {
                                    string devId;
                                    dev.GetId(out devId);

                                    object o;
                                    if (dev.Activate(ref iid, 23, IntPtr.Zero, out o) == 0 && o != null) {
                                        var vol = (IAudioEndpointVolume)o;
                                        float level;
                                        if (vol.GetMasterVolumeLevelScalar(out level) == 0) {
                                            if (level < 0.999f) {
                                                Guid ctx = LockEventContext;
                                                vol.SetMasterVolumeLevelScalar(1.0f, ref ctx);
                                            }
                                        }

                                        if (!_activeIds.Contains(devId)) {
                                            var cb = new VolumeCallback(vol);
                                            vol.RegisterControlChangeNotify(cb);
                                            _activeIds.Add(devId);
                                            _keepAlive.Add(cb);
                                            _keepAlive.Add(vol);
                                        }
                                    }
                                }
                            }
                        }
                    } catch { }

                    Thread.Sleep(250);
                }
            }
        }
    }
}
