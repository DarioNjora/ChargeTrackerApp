using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using LiteDB;
using ChargeTrackerApp.Models;

namespace ChargeTrackerApp.Services
{
    public class ExportModel
    {
        public List<Device> Devices { get; set; } = new();
        public AppSettings? Settings { get; set; }
    }

    public class DataService : IDisposable
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<Device> _devices;
        private readonly ILiteCollection<AppSettings> _settings;

        public DataService()
        {
            var folder = DataLocationService.GetDataFolder();
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "data.db");
            _db = new LiteDatabase(dbPath);
            _devices = _db.GetCollection<Device>("devices");
            _settings = _db.GetCollection<AppSettings>("settings");
            _devices.EnsureIndex(x => x.Id);
        }

        public List<Device> GetAllDevices() => _devices.FindAll().OrderBy(d => d.Name).ToList();

        public Device? GetDevice(int id) => _devices.FindById(id);

        public int AddDevice(Device device) => _devices.Insert(device);

        public void UpdateDevice(Device device) => _devices.Update(device);

        public void DeleteDevice(int id) => _devices.Delete(id);

        public AppSettings GetSettings()
        {
            var s = _settings.FindById(1);
            if (s == null)
            {
                s = new AppSettings { Id = 1 };
                _settings.Insert(s);
            }
            return s;
        }

        public void SaveSettings(AppSettings settings) => _settings.Upsert(settings);

        public string ExportData()
        {
            var export = new ExportModel
            {
                Devices = GetAllDevices(),
                Settings = GetSettings()
            };
            return JsonSerializer.Serialize(export, new JsonSerializerOptions { WriteIndented = true });
        }

        public void ImportData(string json)
        {
            var import = JsonSerializer.Deserialize<ExportModel>(json);
            if (import == null) return;

            _devices.DeleteAll();
            foreach (var d in import.Devices)
            {
                d.Id = 0;
                _devices.Insert(d);
            }

            if (import.Settings != null)
            {
                import.Settings.Id = 1;
                _settings.Upsert(import.Settings);
            }
        }

        public void Dispose() => _db?.Dispose();
    }
}
