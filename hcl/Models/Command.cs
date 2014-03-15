using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace hcl.Models
{
    class Command
    {
        private static Command _instance;
        public static Command Instance
        {
            get
            {
                if (_instance == null) _instance = new Command();
                return _instance;
            }
        }


        private Dictionary<string, FileInfo> _dic = new Dictionary<string, FileInfo>();
        private string _configFileName;


        public bool Initialize(string filename)
        {
            using (var reader = new StreamReader(@"dic.txt"))
            {
                while (reader.Peek() > 0)
                {
                    var line = reader.ReadLine();
                    if (line.Length == 0) continue;
                    var item = line.Trim().Split(',');

                    var key = item[0];
                    var value = item[1];
                    if (File.Exists(value))
                    {
                        _dic[key] = new FileInfo(value);
                    }
                }
            }
            _configFileName = filename;
            return true;
        }


        public bool SaveConfig(string filename = null)
        {
            if (filename == null) filename = _configFileName;

            using (var writer = new StreamWriter(filename))
            {
                foreach (var item in _dic)
                {
                    writer.WriteLine(string.Format("{0},{1}", item.Key, item.Value.FullName));
                }
            }

            return true;
        }


        public bool AddCommand(string key, string command)
        {
            if (!File.Exists(command)) return false;
            _dic[key] = new FileInfo(command);
            return true;
        }


        public bool Execute(string key, params string[] args)
        {
            FileInfo command;
            if (!_dic.TryGetValue(key, out command)) return false;

            var process = new Process();
            process.StartInfo.FileName = command.FullName;
            process.StartInfo.Arguments = string.Join(" ", args);

            process.Start();
            return true;
        }
    }
}
