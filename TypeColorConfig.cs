using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hansoft.Jean.Behavior.TypeColorBehavior
{
    public class TypeColorConfig
    {
        private static Dictionary<string, string> typeColors = null;

        public static string GetTypeColor(string type)
        {
            type = type.ToLower();
            if (typeColors == null)
                TypeColorConfig.ReadConfig();

            if (typeColors.ContainsKey(type))
                return typeColors[type];
            return null;
        }

        public static void ReadConfig()
        {
            typeColors = new Dictionary<string, string>();
            string currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            FileInfo fInfo = new FileInfo(Path.Combine(currentDirectory, "TypeColors.xml"));
            if (!fInfo.Exists)
                throw new ArgumentException("Could not find settings file " + fInfo.FullName);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fInfo.FullName);
            XmlElement documentElement = xmlDocument.DocumentElement;
            if (documentElement.Name != "TypeColors")
                throw new FormatException("The root element of the settings file must be of type TypeColors, got " + documentElement.Name);

            XmlNodeList topNodes = documentElement.ChildNodes;

            foreach (XmlNode node in topNodes)
            {
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                XmlElement el = (XmlElement)node;
                string type = "";
                string color = "";
                foreach (XmlAttribute attr in el.Attributes)
                {
                    if (attr.Name.ToLower() == "type")
                        type = attr.Value.ToLower();
                    else if (attr.Name.ToLower() == "color")
                        color = attr.Value;
                }
                typeColors.Add(type, color);
            }
        }
    }
}
