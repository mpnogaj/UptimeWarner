using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace UptimeWarner
{
    internal static class Program
    {
        private const string ConfigFile = "config.xml";

        /// <summary>
        /// Entry point
        /// </summary>
        [STAThread]
        private static void Main()
        {
            LoadConfig();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }

        private static void LoadConfig()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Config));
                var file = new FileStream(ConfigFile, FileMode.Open, FileAccess.Read);
                var xmlReader = XmlReader.Create(file);
                ConfigManager.Current = (Config)serializer.Deserialize(xmlReader);
                file.Close();
                if (string.IsNullOrEmpty(ConfigManager.Current.ConfigVersion) ||
                    ConfigManager.Current.ConfigVersion != ConfigManager.CurrentVersion.ToString())
                    throw new Exception();
            }
            catch
            {
                ConfigManager.Current = ConfigManager.Default;
                // Create default config file
                var serializer = new XmlSerializer(typeof(Config));
                var file = new FileStream(ConfigFile, FileMode.Create, FileAccess.Write);
                serializer.Serialize(file, ConfigManager.Current);
                file.Close();
            }
        }
    }
}