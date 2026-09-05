# Jhonlloyd Noise Cancellation & 100% Mic Lock Suite

[![Platform: Windows 10/11](https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011%20(64--bit)-blue.svg)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Zero 3rd Party Apps](https://img.shields.io/badge/Runtime-Headless%20Native%20Driver-brightgreen.svg)](#)

A standalone, 1-click zero-configuration audio enhancement suite for Windows. It provides **real-time AI neural noise suppression (RNNoise)**, **automatic 100% microphone volume locking**, **chest-resonance deep voice enhancement**, and **broadcast air articulation** at the operating system driver level.

No heavy GUI apps, no tray bloat, no subscription fees, and zero latency.

---

## 🚀 Key Features

* **1-Click All-in-One Automated Setup (`Jhonlloyd-Noise-cancellation.exe`)**:
  * Self-extracting, fully bundled installer.
  * Silently installs and configures system APO audio drivers.
  * Dynamically scans and binds to **all** active microphone and audio capture endpoints in the Windows Registry with automated privilege escalation (`SeTakeOwnershipPrivilege`).
  * Deploys the neural network models and custom DSP acoustic filters.
  * Zero user input required.

* **Permanent 100% Microphone Lock**:
  * Native C# background daemon (`MicLock.exe`) running under `NT AUTHORITY\SYSTEM`.
  * Monitors Windows CoreAudio COM endpoints via `IAudioEndpointVolumeCallback` and high-speed watchdog.
  * Instantly snaps microphone volume back to 100% if Discord, Steam, games, or web browsers attempt to auto-adjust or lower it.
  * Disables Windows communication ducking (`UserDuckingPreference = 3`) and exclusive device control locks.

* **Studio-Grade Neural Noise Suppression (Xiph RNNoise)**:
  * Runs a Recurrent Neural Network (RNN) deep learning model inside the OS capture stream every 10 milliseconds.
  * Completely erases mechanical keyboard clatter, mouse clicks, fan roar, air conditioning, and room reverb—even while you are actively speaking.

* **Deep Voice & Crisp Broadcast EQ**:
  * **Anti-Clipping Preamp Headroom (`-3 dB`)**: Eliminates digital squaring and audio cutting caused by loud inputs at 100% volume.
  * **Chest Resonance Boost (`+3.5 dB @ 120 Hz, Q 1.0`)**: Imparts a rich, warm, baritone radio-podcast presence.
  * **Sub-Rumble High-Pass (`45 Hz`)**: Filters out desk thumps and AC hum while preserving deep vocal undertones.
  * **Air & Articulation Shelf (`+2.0 dB @ 10 kHz`)**: Keeps consonant clarity razor-sharp.

* **Clean Rollback Uninstaller (`Jhonlloyd-Noise-cancellation-Uninstall.exe`)**:
  * 1-click uninstaller that removes the background services and restores Windows audio defaults completely.

---

## 📦 Quick Installation

1. Download or run **`Jhonlloyd-Noise-cancellation.exe`**.
2. Click **Yes** on the Windows UAC prompt.
3. The installer extracts all dependencies, configures your registry, applies the AI models, and launches the volume locking service.
4. **Restart your PC once** to finalize the driver hooks.

---

## 🛠️ Recommended Voice App Settings (Discord, Teams, Zoom)

Because Jhonlloyd Noise Cancellation operates at the Windows driver level, apps already receive clean, deep, isolated voice:
* In **Discord Settings → Voice & Video**:
  * Set **Noise Suppression** to **None**.
  * Turn **OFF** *"Automatically determine input sensitivity"* (slide slider to manual / `-60 dB`).
  * Turn **OFF** *"Echo Cancellation"*.

---

## 📁 Repository Structure

```
├── Jhonlloyd-Noise-cancellation.exe            # 1-Click Standalone Bundled Installer
├── Jhonlloyd-Noise-cancellation-Uninstall.exe  # 1-Click Clean Uninstaller
├── build_installer.ps1                         # Compiler script to build installer from source
├── presets/
│   └── deep-crisp.txt                         # Studio Deep Voice & Crisp Articulation DSP
├── src/
│   ├── MicLock/
│   │   └── Program.cs                         # Native C# CoreAudio 100% volume watchdog service
│   └── Installer/
│       ├── InstallerMain.cs                   # Standalone self-extracting installer orchestrator
│       ├── Configurator.cs                    # Registry ownership escalation & endpoint binder
│       └── Uninstall.cs                       # Complete cleanup & rollback utility
└── bin/
    ├── EqualizerAPO-x64.exe                   # Silent APO driver engine
    └── rnnoise_mono.dll                       # Neural network VST processing library
```

---

## 📜 License

Distributed under the [MIT License](LICENSE). Built for personal and creator audio excellence.
