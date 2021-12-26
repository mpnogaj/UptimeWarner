using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using UptimeWarner.Properties;

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
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                foreach (string arg in args.Skip(1))
                {
                    switch (arg)
                    {
                        case "-h":
                        case "--help":
                            var stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(Resources.availableFlags);
                            stringBuilder.AppendLine(
                                Resources.createDefaultConfigFlagDesc);
                            stringBuilder.AppendLine(
                                Resources.startWithDefaultConfigFlagDesc);
                            MessageBox.Show(stringBuilder.ToString(), Resources.help);
                            return;
                        case "--create-default-config-file":
                            CreateDefaultConfig();
                            return;
                        case "--start-with-default-config":
                            ConfigManager.Current = ConfigManager.Default;
                            break;
                        default:
                            MessageBox.Show(Resources.badFlag);
                            return;
                    }
                }
            }
            LoadConfig();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new AppContext());
        }
        
        private static void CreateDefaultConfig()
        {
            var serializer = new XmlSerializer(typeof(Config));
            var file = new FileStream(ConfigFile, FileMode.Create, FileAccess.Write);
            serializer.Serialize(file, ConfigManager.Default);
            file.Close();
        }

        private static void LoadConfig()
        {
            if(ConfigManager.Current != null) return;
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
                CreateDefaultConfig();
            }
        }
    }
}