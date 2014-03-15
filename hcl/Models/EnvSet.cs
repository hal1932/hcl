using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace hcl.Models
{
    class EnvSet
    {
        public EnvSet(string name, string currentSet = "")
        {
            var filename = "";

            if (name == "current")
            {
                filename = Path.Combine("envset", currentSet);
                if (File.Exists(filename))
                {
                    var tmpfile = filename + ".txt";
                    File.Copy(filename, tmpfile);
                    var process = new Process();
                    process.StartInfo.FileName = tmpfile;
                    process.Start();
                }
                return;
            }

            filename = Path.Combine("envset", name);
            if (!File.Exists(filename)) throw new ArgumentException("invalid envset file: " + name);

            using (var reader = new StreamReader(filename))
            {
                while (reader.Peek() > 0)
                {
                    var line = reader.ReadLine();
                    var item = line.Trim().Split(',');
                    var key = item[0];
                    var value = ConvertEnv(item[1]);
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }


        private static string ConvertEnv(string name)
        {
            if (!name.Contains("%")) return name;

            var indices = new List<int>();
            var current = 0;
            while (current < name.Length)
            {
                var index = name.IndexOf('%', current);
                if (index >= 0) indices.Add(index);
                else break;

                current = index + 1;
            }

            var value = "";
            for (int i = 0; i < indices.Count; i += 2)
            {
                var oldValue = name.Substring(indices[i], indices[i + 1] + 1 - indices[i]);
                var newValue = Environment.GetEnvironmentVariable(oldValue.Trim('%'));
                value = name.Replace(oldValue, newValue);
            }

            return value;
        }
    }
}
