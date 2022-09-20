using NLog;
using System.IO;
using System.Linq;

namespace EDVTrader.Common
{
    public class JournalWatcher
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        private FileSystemWatcher _watcher;
        private StreamReader? _reader;

        public delegate void JournalChange(string data);
        public event JournalChange? OnChange;

        public JournalWatcher(string searchDirectory)
        {
            _watcher = new FileSystemWatcher(searchDirectory, "Journal*.log")
            {
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileCreated;

            InitializeReader(searchDirectory, true);
        }

        private void InitializeReader(string path, bool findLatest)
        {
            if (findLatest)
            {
                IOrderedEnumerable<FileInfo> files = new DirectoryInfo(path)
                    .GetFiles().Where(x => x.Name.StartsWith("Journal."))
                    .OrderByDescending(x => x.LastWriteTime);

                if (files.Count() == 0)
                    return;

                path = files.First().FullName;

                _logger.Info($"Initializing JournalWatcher with latest journal: {path}.");
            }

            _reader?.Close();

            _reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            if (findLatest)
                _reader.ReadToEnd();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
                return;

            _logger.Info($"Found new journal. Switching to: {e.FullPath}.");
            InitializeReader(e.FullPath, false);
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            string? newData = _reader?.ReadToEnd().Trim();
            if (newData == null || string.IsNullOrWhiteSpace(newData))
                return;

            foreach(string line in newData.Split('\n'))
                OnChange?.Invoke(line);
        }
    }
}
