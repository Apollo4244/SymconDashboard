using System.Text.Json;

namespace SymconDashboard
{
    public static class AppSettingsService
    {
        internal static string FilePath =
            Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static bool IsFirstRun { get; private set; }

        public static AppSettings Load()
        {
            if (!File.Exists(FilePath))
            {
                IsFirstRun = true;
                var defaults = new AppSettings();
                Save(defaults);
                return defaults;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(FilePath, json);
        }
    }
}
