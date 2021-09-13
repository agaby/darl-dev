using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class FileConnectivity : IBlobConnectivity
    {
        public string implementation => nameof(FileConnectivity);
        private IConfiguration _config;
        private ILogger _logger;
        private string filepath;


        public FileConnectivity(IConfiguration config, ILogger<FileConnectivity> logger)
        {
            _config = config;
            _logger = logger;
            filepath = _config["BLOBFILEPATH"];
            Init();
        }

        public async Task<bool> Delete(string name)
        {
            if(await Exists(name))
            {
                File.Delete(CreatePath(name));
                return true;
            }
            return false;
        }

        private void Init()
        {
            var backgroundUserId = _config["SINGLEUSERID"];
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            var existing = Directory.GetFiles(filepath);
            //get each embedded file ending in .graph.
            foreach (var name in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (name.EndsWith(".graph"))
                {
                    var filename = name.Substring(name.Remove(name.Length - 6).LastIndexOf('.') + 1);
                    if (!existing.Any(a => a == filename))
                    {
                        string path = filepath + Path.DirectorySeparatorChar + backgroundUserId + "_" + filename;
                        using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
                        {
                            if (resource != null)
                            {
                                using (var file = new FileStream(path, FileMode.Create, FileAccess.Write))
                                {
                                    resource.CopyTo(file);
                                }
                            }
                        }
                    }
                }

            }
        }

        public Task<bool> Exists(string name)
        {
            return Task.FromResult<bool>(File.Exists(CreatePath(name)));
        }

        public List<string> List(string prefix)
        {
            var list = new List<string>();
            foreach(var l in Directory.EnumerateFiles(filepath,prefix + '_' + '*'))
            {
                int loc = l.LastIndexOf(prefix);
                list.Add(l.Substring(loc + prefix.Length + 1));
            }
            return list;
        }

        public async Task<byte[]> Read(string name)
        {            
            return await File.ReadAllBytesAsync(CreatePath(name));
        }

        public async Task Write(string name, byte[] data)
        {
            await File.WriteAllBytesAsync(CreatePath(name), data);
        }

        /// <summary>
        /// OS agnostic create path
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private string CreatePath(string filename)
        {
            return filepath + Path.DirectorySeparatorChar + filename;
        }
    }
}
