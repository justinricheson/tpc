using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace TPC.Group
{
    public class AppConfig
    {
        public string ID { get; set; }
        public string TCPServerAddress { get; set; }
        public string TCPServerPort { get; set; }
        public string GroupMcg { get; set; }
        public string GroupPort { get; set; }
        public string JoinTimeout { get; set; }
        public string ResponseTimeout { get; set; }
        public string ElectionTimeout { get; set; }
        public string LeaveTimeout { get; set; }

        public static AppConfig GetConfig()
        {
            var doc = XDocument.Load(GetConfigPath());
            var config = doc.Element(Constants.ELEMENT_CONFIG);

            return new AppConfig()
            {
                ID              = Guid.NewGuid().ToString(),
                TCPServerAddress= config.Element(Constants.ELEMENT_TCP_SERVER_ADDRESS).Value,
                TCPServerPort   = config.Element(Constants.ELEMENT_TCP_SERVER_PORT).Value,
                GroupMcg        = config.Element(Constants.ELEMENT_GROUP_MCG).Value,
                GroupPort       = config.Element(Constants.ELEMENT_GROUP_PORT).Value,
                JoinTimeout     = config.Element(Constants.ELEMENT_JOIN_TIMEOUT).Value,
                ResponseTimeout = config.Element(Constants.ELEMENT_RESPONSE_TIMEOUT).Value,
                ElectionTimeout = config.Element(Constants.ELEMENT_ELECTION_TIMEOUT).Value,
                LeaveTimeout    = config.Element(Constants.ELEMENT_LEAVE_TIMEOUT).Value
            };
        }

        private static string GetConfigPath()
        {
            string fullPath = Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(fullPath) + Constants.RESOURCES_SUBDIR;

            return Path.Combine(directory, Constants.CONFIG_FILENAME);
        }
    }
}
