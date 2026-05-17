# INSTALL — driver-moodle setup on a fresh PC

This project depends on the **Microsoft Access Database Engine** (an OS-level Windows component that is NOT shipped with .NET or Visual Studio). Without it, every WCF call that touches the database will fail with:

> `Microsoft.ACE.OLEDB.12.0` provider is not registered on the local machine

This document explains how to set up a brand-new PC so the project runs.

## Prerequisites
- Windows 10 or 11
- Visual Studio 2019 / 2022 (Community is fine)
- .NET Framework 4.8 Developer Pack
- WCF Tooling (see Step 2 below)
- Microsoft Access Database Engine 2016 (see Step 1)

## Step 1 — Install Access Database Engine

### Option A (recommended): automated
1. Open **PowerShell as Administrator**
2. `cd` into the repo root
3. Run:
   ```powershell
   .\scripts\install-access-engine.ps1
   ```
4. Wait for `SUCCESS. Access Database Engine installed.`

### Option B: manual
1. Download from <https://www.microsoft.com/en-us/download/details.aspx?id=54920>
2. Pick the installer that matches your Visual Studio Build target:
   - **x64 / Any CPU** → `AccessDatabaseEngine_X64.exe`
   - **x86 / "Prefer 32-bit" checked** → `AccessDatabaseEngine.exe`
3. Run it. If Office is already installed in the OTHER bitness, the GUI installer will refuse. Workaround: from an elevated CMD, run with `/quiet`:
   ```
   AccessDatabaseEngine_X64.exe /quiet
   ```

After install: **fully close and reopen Visual Studio.**

## Step 2 — Install WCF Tooling (VS 2022 only)

VS 2022 dropped the legacy WCF Service Library host (`WcfSvcHost.exe`). To restore it:

1. Open **Visual Studio Installer**
2. Click **Modify** next to VS 2022
3. Go to the **Individual components** tab and check:
   - ✅ **Windows Communication Foundation**
   - ✅ **.NET Framework 4.8 SDK**
   - ✅ **.NET Framework 4.8 targeting pack**
4. Click **Modify** and wait

## Step 3 — Open and run

1. Open `WcfServiceLibrary1\WcfServiceLibrary1.sln`
2. Right-click the host project (`WcfServiceHost` once it exists, otherwise `WcfServiceLibrary1`) → **Set as Startup Project**
3. F5

For the WPF / Web / MAUI clients, open their respective solutions in the matching folder (`Driver\`, `driver-client\`, `driver-maui\`).

## Troubleshooting

### `Microsoft.ACE.OLEDB.12.0 provider is not registered`
Or the Hebrew equivalent: **ספק 12 או 16 לא רשום במחשב**.

Meaning: Access Database Engine is not installed, or the bitness doesn't match. Go back to Step 1.

If both 16.0 and 12.0 are now installed but you still see this: your project bitness probably mismatches.
- VS → Project Properties → Build → **Platform target**
- If it's `Any CPU` with **Prefer 32-bit** checked, you need the **x86** engine
- If it's `x64`, you need the **x64** engine

### `A project with an output of class library cannot be started directly`
You set `WcfServiceLibrary1` (a DLL) as the startup project. Set `WcfServiceHost` (the EXE) instead, OR install WCF Tooling from Step 2.

### Connection works on old PC but fails on new PC
The old PC had Office installed (which bundles the OleDb driver). The new one doesn't. Follow Step 1.
