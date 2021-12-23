using System;
using System.Xml.Serialization;

namespace UptimeWarner
{
    public class Config
    {
        [XmlElement]
        public string ConfigVersion { get; set; }
        
        [XmlElement]
        public int WarningTime { get; set; }
        [XmlElement]
        public int CriticalDeltaTime { get; set; }
        
        
        [XmlIgnore]
        public TimeSpan WarningTimeSpan => TimeSpan.FromDays(WarningTime);
        [XmlIgnore]
        public TimeSpan CriticalDeltaTimeSpan => TimeSpan.FromDays(CriticalDeltaTime);
    }
}