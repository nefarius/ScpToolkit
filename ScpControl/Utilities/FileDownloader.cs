using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ScpControl.ScpCore;

namespace ScpControl.Utilities
{
    public class ProgressChangedEventArgs : EventArgs
    {
        public ProgressChangedEventArgs(int percentage)
        {
            CurrentProgressPercentage = percentage;
        }

        public int CurrentProgressPercentage { get; private set; }
    }

    public class FileDownloadCompletedEventArgs : EventArgs
    { }

    public delegate void ProgressChangedEventHandler(object sender, ProgressChangedEventArgs e);

    public delegate void FileDownloadCompletedEventHandler(object sender, FileDownloadCompletedEventArgs e);

    public class FileDownloader : IDisposable
    {
        private readonly WebClient _client = new WebClient();

        private readonly SemaphoreSlim _waitForDownloadFinished = new SemaphoreSlim(0, 1);

        public FileDownloader(string url) : this(new Uri(url))
        {
        }

        public FileDownloader(Uri url)
        {
            Url = url;

            _client.DownloadProgressChanged += (sender, args) =>
            {
                // calculate percentage
                var bytesIn = double.Parse(args.BytesReceived.ToString());
                var totalBytes = double.Parse(args.TotalBytesToReceive.ToString());
                var percentage = bytesIn / totalBytes * 100;

                var evArgs = new ProgressChangedEventArgs(int.Parse(Math.Truncate(percentage).ToString()));

                // notify subscribers about progress
                OnProgressChanged(evArgs);
            };

            _client.DownloadFileCompleted += (sender, args) =>
            {
                OnFileDownloadCompleted(new FileDownloadCompletedEventArgs());
            };
        }

        ~FileDownloader()
        {
            Dispose();
        }

        public Uri Url { get; private set; }

        public event ProgressChangedEventHandler ProgressChanged;

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e);
        }

        public event FileDownloadCompletedEventHandler FileDownloadCompleted;

        protected virtual void OnFileDownloadCompleted(FileDownloadCompletedEventArgs e)
        {
            _waitForDownloadFinished.Release();

            if (FileDownloadCompleted != null)
                FileDownloadCompleted(this, e);
        }

        public async Task DownloadAsync(string targetPath)
        {
            if (_client == null)
                return;

            var dir = Path.GetDirectoryName(targetPath) ?? string.Empty;

            // if path isn't absolute, create subdirectory in current folder to download file to
            if (!Path.IsPathRooted(targetPath))
            {
                targetPath = Path.Combine(GlobalConfiguration.AppDirectory, "Downloads", dir);

                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);
            }
            else
            {
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }

            // download file non-blocking
            _client.DownloadFileAsync(Url, targetPath);

            await _waitForDownloadFinished.WaitAsync();
        }

        public void Dispose()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
