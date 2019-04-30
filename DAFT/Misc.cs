using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DAFT
{
    static class Misc
    {
        internal static string ComputerNameFromInstance(string instance)
        {
            return instance.Split(new string[] { @"\", @"," }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }

        internal static void PrintStructOriginal<T>(T s)
        {
            FieldInfo[] fields = s.GetType().GetFields();
            Console.WriteLine("==========");
            foreach (var xInfo in fields)
            {
                try
                {
                    Console.WriteLine("Field {0,-20}", xInfo.GetValue(s).ToString());
                }
                catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                        Console.WriteLine("Field {0,-20}", null);
                    else
                        Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("==========");
        }

        internal static void PrintStruct<T>(T s)
        {
            Console.WriteLine("==========");
            foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                try
                {
                    string value = string.Empty;
                    if (field.FieldType == typeof(byte[]))
                        value = BitConverter.ToString((byte[])field.GetValue(s));
                    else
                        value = field.GetValue(s).ToString();
                    string[] arrValue = value.Split('\n');

                    Console.WriteLine("{0,-30} {1,-20}", field.Name, arrValue.FirstOrDefault());
                    foreach (var v in arrValue.Skip(1))
                    {
                        Console.WriteLine("{0,-30} {1,-20}", "", v);
                    }

                }
                catch (Exception ex)
                {
                    if (ex is NullReferenceException)
                        Console.WriteLine("{0,-30}", field.Name);
                    else
                        Console.WriteLine(ex.Message);
                }
            }
            Console.WriteLine("==========");
        }

        internal static bool CheckLuhn(string input)
        {
            int sum;
            if (!int.TryParse(input.Substring(input.Length - 1, 1), out sum))
            {
                return false;
            }
            int nDigits = input.Length;
            int parity = nDigits % 2;

            for (int i = 0; i < nDigits - 1; i++)
            {
                int digit;
                if (!int.TryParse(input[i].ToString(), out digit))
                {
                    return false;
                }
                if (parity == i % 2)
                {
                    digit *= 2;
                }
                if (9 < digit)
                {
                    digit -= 9;
                }
                sum += digit;
            }
            return 0 == sum % 10;
        }

        //https://stackoverflow.com/questions/52751224/check-dbnull-before-casting-c-sharp
        private static T _GetValue<T>(SqlDataReader reader, string columnName)
        {
            if (_HasColumn(reader, columnName))
                return reader[columnName] == DBNull.Value ? default(T) : (T)reader[columnName];
            else
                return default(T);      
        }

        //https://stackoverflow.com/questions/373230/check-for-column-name-in-a-sqldatareader-object
        private static bool _HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        internal static T AssignStruct<T>(this SqlDataReader reader, T str)
        {
            object s = str;
            foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                try
                {
                    MethodInfo info = typeof(Misc).GetMethod("_GetValue", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    info = info.MakeGenericMethod(field.FieldType);
                    field.SetValue(s, info.Invoke(null, new object[] { reader, field.Name }));
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                        Console.WriteLine("[-] Invalid Cast: {0}", field.Name);
                    else
                        Console.WriteLine(ex);
                }
            }
            str = (T)s;
            return str;
        }
    }
}