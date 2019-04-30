using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DAFT
{
    class CSVSerializer
    {
        private FileStream fs;

        internal CSVSerializer(FileStream fs)
        {
            this.fs = fs;
        }

        internal void WriteObject<T>(List<T> o)
        {
            List<string> names = new List<string>();
            StringBuilder line = new StringBuilder();
            FieldInfo[] fields = o.First().GetType().GetFields();
            foreach (var field in fields)
            {
                line.AppendFormat("\"{0}\",", field.Name);
                names.Add(field.Name);
            }
            line.Append("\n");

            foreach (var m in o)
            {
                foreach (string item in names)
                {
                    line.AppendFormat("\"{0}\",", typeof(T).GetField(item).GetValue(m));
                }
                line.Append("\n");
            }
            _WriteObject(line.ToString());
        }

        internal void WriteObject<T>(T o)
        {
            List<string> names = new List<string>();
            StringBuilder line = new StringBuilder();

            FieldInfo[] fields = o.GetType().GetFields();
            foreach (var field in fields)
            {
                line.AppendFormat("\"{0}\",", field.Name);
                names.Add(field.Name);
            }
            line.Append("\n");

            foreach (string item in names)
            {
                line.AppendFormat("\"{0}\",", typeof(T).GetField(item).GetValue(o));
            }
            line.Append("\n");
            _WriteObject(line.ToString());
        }

        private void _WriteObject(string output)
        {
            byte[] info = new UTF8Encoding(true).GetBytes(output);
            fs.Write(info, 0, info.Length);
        }
    }
}
