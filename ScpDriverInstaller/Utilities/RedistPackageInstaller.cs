using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ScpControl.Utilities;

namespace ScpDriverInstaller.Utilities
{
    public static class RedistPackageInstaller
    {
        #region Microsoft Visual C++ 2010 Redistributable Package

        private static readonly FileDownloader Msvc2010Sp1X64Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/A/8/0/A80747C3-41BD-45DF-B505-E9710D2744E0/vcredist_x64.exe");

        private static readonly FileDownloader Msvc2010Sp1X86Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/C/6/D/C6D0FD4E-9E53-4897-9B91-836EBA2AACD3/vcredist_x86.exe");

        #endregion

        #region Visual C++ Redistributable Packages für Visual Studio 2013

        private static readonly FileDownloader Msvc2013X64Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x64.exe");

        private static readonly FileDownloader Msvc2013X86Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/2/E/6/2E61CFA4-993B-4DD4-91DA-3737CD5CD6E3/vcredist_x86.exe");

        #endregion

        #region DirectX Offline Installer

        private static readonly FileDownloader DxRedistOfflineInstallerDownloader =
            new FileDownloader(
                "http://download.microsoft.com/download/8/4/A/84A35BF1-DAFE-4AE8-82AF-AD2AE20B6B14/directx_Jun2010_redist.exe");

        #endregion

        #region Xbox 360 Controller Driver for Windows

        private static FileDownloader _xboxDrvWin7X64Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/6/9/4/69446ACF-E625-4CCF-8F56-58B589934CD3/Xbox360_64Eng.exe");

        private static FileDownloader _xboxDrvWin7X86Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/6/9/4/69446ACF-E625-4CCF-8F56-58B589934CD3/Xbox360_32Eng.exe");

        private static FileDownloader _xboxDrvVistaX64Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/6/9/4/69446ACF-E625-4CCF-8F56-58B589934CD3/Xbox360_64Eng.exe");

        private static FileDownloader _xboxDrvVistaX86Downloader =
            new FileDownloader(
                "http://download.microsoft.com/download/6/9/4/69446ACF-E625-4CCF-8F56-58B589934CD3/Xbox360_32Eng.exe");

        #endregion

        private static readonly string TempPathRoot = Path.Combine(Path.GetTempPath(), "SCP_REDIST");

        /// <summary>
        ///     Downloads and installs the Microsoft Visual C++ 2010 Redistributable Package, depending on the hosts architecture.
        /// </summary>
        /// <returns>The async object.</returns>
        public static async Task DownloadAndInstallMsvc2010Async()
        {
            var tempPath = Path.Combine(TempPathRoot, "MSVC2010");
            const string args = "/passive /norestart";

            if (Environment.Is64BitProcess)
            {
                var targetFile = Path.Combine(tempPath, "vcredist_x64.exe");

                await Msvc2010Sp1X64Downloader.DownloadAsync(targetFile);

                await Task.Run(() => Process.Start(targetFile, args).WaitForExit());
            }
            else
            {
                var targetFile = Path.Combine(tempPath, "vcredist_x86.exe");

                await Msvc2010Sp1X86Downloader.DownloadAsync(targetFile);

                await Task.Run(() => Process.Start(targetFile, args).WaitForExit());
            }
        }

        /// <summary>
        ///     Downloads and installs the Visual C++ Redistributable Packages für Visual Studio 2013, depending on the hosts architecture.
        /// </summary>
        /// <returns>The async object.</returns>
        public static async Task DownloadAndInstallMsvc2013()
        {
            var tempPath = Path.Combine(TempPathRoot, "MSVC2013");
            const string args = "/install /passive /norestart";

            if (Environment.Is64BitProcess)
            {
                var targetFile = Path.Combine(tempPath, "vcredist_x64.exe");

                await Msvc2013X64Downloader.DownloadAsync(targetFile);

                await Task.Run(() => Process.Start(targetFile, args).WaitForExit());
            }
            else
            {
                var targetFile = Path.Combine(tempPath, "vcredist_x86.exe");

                await Msvc2013X86Downloader.DownloadAsync(targetFile);

                await Task.Run(() => Process.Start(targetFile, args).WaitForExit());
            }
        }

        public static async Task DownloadAndInstallDirectXRedist()
        {
            // current users temporary directory
            var tempPath = Path.Combine(TempPathRoot, "DXSETUP");

            // build setup path
            var targetFile = Path.Combine(tempPath, "directx_Jun2010_redist.exe");

            // download file
            await DxRedistOfflineInstallerDownloader.DownloadAsync(targetFile);

            // extract setup
            await Task.Run(() => Process.Start(targetFile, string.Format("/Q /T:\"{0}\"", tempPath)).WaitForExit());

            // run actual setup
            await Task.Run(() => Process.Start(Path.Combine(tempPath, "DXSETUP.exe"), "/silent"));
        }
    }
}
