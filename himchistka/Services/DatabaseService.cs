using System;
using System.IO;
using System.Xml.Serialization;
using himchistka.Models;

namespace himchistka.Services
{
    public sealed class DatabaseService
    {
        private readonly string _dbPath;
        private readonly object _sync = new object();
        private AppState _state;

        public DatabaseService(string dbPath = null)
        {
            _dbPath = string.IsNullOrWhiteSpace(dbPath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app-database.xml")
                : dbPath;

            _state = LoadOrCreate();
        }

        public AppState State => _state;

        public void Save()
        {
            lock (_sync)
            {
                var serializer = new XmlSerializer(typeof(AppState));
                var directory = Path.GetDirectoryName(_dbPath);
                if (!string.IsNullOrWhiteSpace(directory))
                    Directory.CreateDirectory(directory);

                using (var stream = File.Create(_dbPath))
                    serializer.Serialize(stream, _state);
            }
        }

        private AppState LoadOrCreate()
        {
            lock (_sync)
            {
                if (!File.Exists(_dbPath))
                    return new AppState();

                try
                {
                    var serializer = new XmlSerializer(typeof(AppState));
                    using (var stream = File.OpenRead(_dbPath))
                    {
                        var state = serializer.Deserialize(stream) as AppState;
                        return state ?? new AppState();
                    }
                }
                catch
                {
                    return new AppState();
                }
            }
        }
    }
}
