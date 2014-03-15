using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace hcl.Models
{
    class PresetCommand
    {
        public enum Type
        {
            None,
            SwitchEnvSet,
            EditConfig,
        }


        private static Dictionary<string, Type> _typeDic = new Dictionary<string, Type>()
        {
            { "envset", Type.SwitchEnvSet },
            { "config", Type.EditConfig },
        };

        private Dictionary<string, Type> _commandDic = new Dictionary<string, Type>();
        

        public bool Initialize()
        {
            using (var reader = new StreamReader(@"preset.txt"))
            {
                while (reader.Peek() > 0)
                {
                    var line = reader.ReadLine();
                    var item = line.Trim().Split(',');
                    _commandDic[item[0]] = _typeDic[item[1]];
                }
            }
            return true;
        }

        public Type GetCommandType(string command)
        {
            var type = Type.None;
            _commandDic.TryGetValue(command, out type);
            return type;
        }
    }
}
