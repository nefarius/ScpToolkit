using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using CsvHelper;
using HidSharp;
using ScpControl.Driver;
using ScpControl.ScpCore;
using ScpControl.Usb.Gamepads;
using ScpControl.Utilities;

namespace ScpDebugInfoCollector
{
    internal class Program
    {
        private static readonly string ScpLogsDir = Path.Combine(GlobalConfiguration.AppDirectory, "Logs");
        private static readonly string WorkingDirectory = Path.Combine(Path.GetTempPath(), "ScpToolkitDebugger");

        private static void Main(string[] args)
        {
            Console.WriteLine("ScpToolkit Debug Info Collector");
            Console.WriteLine();

            Console.WriteLine("This utility will collect information that helps solving your issues.");
            Console.WriteLine("If you haven't yet please connect all your controllers to your PC now.");
            Console.WriteLine("Make sure all ScpToolkit programs are closed, also shut down the service!");

            Console.WriteLine();
            Console.WriteLine("Press any key if you're ready.");
            Console.ReadKey();
            Console.WriteLine();

            try
            {
                if (!Directory.Exists(WorkingDirectory))
                    Directory.CreateDirectory(WorkingDirectory);

                var usbDevicesCsv = Path.Combine(WorkingDirectory, "USB-Devices.csv");
                var hidDevicesCsv = Path.Combine(WorkingDirectory, "HID-Devices.csv");
                const string setupApiLog = @"C:\Windows\inf\setupapi.dev.log";
                var sysinfoFile = Path.Combine(WorkingDirectory, "system.txt");

                // dump USB device info
                Console.WriteLine("Enumerating USB devices...");
                using (
                    var file = new StreamWriter(usbDevicesCsv, false, new UTF8Encoding(true))
                    )
                {
                    var usbDevices = WdiWrapper.Instance.UsbDeviceList;

                    using (var csv = new CsvWriter(file))
                    {
                        csv.Configuration.Delimiter = ";";

                        csv.WriteHeader<WdiUsbDevice>();
                        csv.WriteRecords(usbDevices);
                    }
                }

                // dump HID device info
                Console.WriteLine("Enumerating HID devices...");
                using (
                    var file = new StreamWriter(hidDevicesCsv, false, new UTF8Encoding(true))
                    )
                {
                    var hidDevices = UsbGenericGamepad.LocalHidDevices;

                    using (var csv = new CsvWriter(file))
                    {
                        csv.Configuration.Delimiter = ";";

                        csv.WriteHeader<HidDevice>();
                        csv.WriteRecords(hidDevices);
                    }
                }

                using (var file = File.CreateText(sysinfoFile))
                {
                    file.WriteLine(OsInfoHelper.OsInfo);
                }

                var unixTimestamp = (int) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var targetArchive = Path.Combine(desktopPath,
                    string.Format("ScpToolkit_Log-Package_{0}_{1}.zip",
                        Environment.UserName, unixTimestamp));

                Console.WriteLine("Creating ZIP-file...");

                if (File.Exists(targetArchive))
                    File.Delete(targetArchive);

                using (var archive = ZipFile.Open(targetArchive, ZipArchiveMode.Create))
                {
                    Console.WriteLine("Adding {0}", usbDevicesCsv);
                    archive.CreateEntryFromFile(usbDevicesCsv, Path.GetFileName(usbDevicesCsv));

                    Console.WriteLine("Adding {0}", hidDevicesCsv);
                    archive.CreateEntryFromFile(hidDevicesCsv, Path.GetFileName(hidDevicesCsv));

                    Console.WriteLine("Adding {0}", setupApiLog);
                    archive.CreateEntryFromFile(setupApiLog, Path.GetFileName(setupApiLog));

                    Console.WriteLine("Adding {0}", sysinfoFile);
                    archive.CreateEntryFromFile(sysinfoFile, Path.GetFileName(sysinfoFile));

                    foreach (var file in Directory.GetFiles(ScpLogsDir))
                    {
                        Console.WriteLine("Adding {0}", file);
                        archive.CreateEntryFromFile(file, Path.GetFileName(file));
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Data collection finished, an archive named {0} was created on your Desktop",
                    Path.GetFileName(targetArchive));
                Console.WriteLine("Please submit this file to the forums with a description of your issues.");

                Directory.Delete(WorkingDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }

            Console.WriteLine("Press any key to close this window.");
            Console.ReadKey();
        }
    }
}