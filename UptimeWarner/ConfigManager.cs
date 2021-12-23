using System;

namespace UptimeWarner
{
    public static class ConfigManager
    {
        public static Version CurrentVersion => new Version(1, 0);
        public static Config Default => new Config
        {
            WarningTime = 1,
            CriticalDeltaTime = 1,
            ConfigVersion = CurrentVersion.ToString()
        };
        public static Config Current { get; set; }
    }
}