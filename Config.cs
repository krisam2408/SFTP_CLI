using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTPTest
{
    /// <summary>
    /// Clase de configuración homóloga a AppSettings. Se alimenta de archivo json en directorio raíz.
    /// </summary>
    public class Config
    {
        private const string cfgPath = "appconfig.default.json";

        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
        public string Destination { get; set; }

        private static Config instance;
        public static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    string path, content;
                    try
                    {
                        path = Path.GetFullPath(cfgPath);
                        content = File.ReadAllText(path);
                    }
                    catch (FileNotFoundException)
                    {
                        path = Path.GetFullPath($"../../{cfgPath}");
                        content = File.ReadAllText(path);
                    }
                    instance = JsonConvert.DeserializeObject<Config>(content);
                }
                return instance;
            }
        }
    }
}
