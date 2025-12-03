using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

namespace HoneyBeeScriptTool
{
    public static partial class Extensions
    {
        static readonly Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        public static string ReadFixedLengthString(this BinaryReader br, int length)
        {
            return ReadFixedLengthString(br, length, shiftJis);
        }

        public static string ReadFixedLengthString(this BinaryReader br, int length, Encoding encoding)
        {
            var bytes = br.ReadBytes(length);
            return ReadFixedLengthString(bytes, 0, length, encoding);
        }

        public static string ReadFixedLengthString(this byte[] bytes, int start, int length, Encoding encoding)
        {
            int nullIndex = Array.IndexOf(bytes, (byte)0, start);
            if (nullIndex == -1)
            {
                nullIndex = bytes.Length;
            }
            return encoding.GetString(bytes, start, nullIndex - start);
        }

        public static string ReadFixedLengthString(this byte[] bytes, int start, int length)
        {
            return ReadFixedLengthString(bytes, start, length, shiftJis);
        }

        public static void WriteFixedLengthString(this BinaryWriter bw, string text, int length, Encoding encoding)
        {
            var bytes = new byte[length];
            //should be zero-filled by runtime library

            encoding.GetBytes(text, 0, text.Length, bytes, 0);
            bw.Write(bytes);
        }

        public static void WriteFixedLengthString(this BinaryWriter bw, string text, int length)
        {
            WriteFixedLengthString(bw, text, length, shiftJis);
        }

        public static int EndianSwap(this int value)
        {
            unchecked
            {
                uint uvalue = (uint)value;
                uint uvalue2 = (((uvalue >> 24) & 0xFF) << 0) |
                               (((uvalue >> 16) & 0xFF) << 8) |
                               (((uvalue >> 8) & 0xFF) << 16) |
                               (((uvalue >> 0) & 0xFF) << 24);
                return (int)uvalue2;
            }
        }

        public static T ReadObject<T>(this BinaryReader br) where T : new()
        {
            var obj = new T();
            ReadObject(br, obj);
            return obj;
        }

        public static int[] ReadIntArray(this BinaryReader br, int count)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(br.ReadInt32());
            }
            return list.ToArray();
        }

        public static void WriteIntArray(this BinaryWriter bw, IEnumerable<int> intArray)
        {
            int count = intArray.Count();
            foreach (int value in intArray)
            {
                bw.Write(value);
            }
        }


        public static T[] ReadObjects<T>(this BinaryReader br, int count) where T : new()
        {
            List<T> list = new List<T>();
            for (int i = 0; i < count; i++)
            {
                list.Add(br.ReadObject<T>());
            }
            return list.ToArray();
        }

        public static string ReadNullTerminatedString(this BinaryReader br, Encoding encoding)
        {
            List<byte> bytes = new List<byte>();
            while (true)
            {
                byte b = br.ReadByte();
                if (b != 0)
                {
                    bytes.Add(b);
                }
                else
                {
                    break;
                }
            }
            string str = encoding.GetString(bytes.ToArray());
            return str;
        }

        public static void WriteNullTerminatedString(this BinaryWriter bw, string str, Encoding encoding)
        {
            var bytes = encoding.GetBytes(str);
            bw.Write(bytes);
            bw.Write((byte)0);
        }

        public static string ReadNullTerminatedString(this BinaryReader br)
        {
            return ReadNullTerminatedString(br, shiftJis);
        }

        public static void WriteNullTerminatedString(this BinaryWriter bw, string str)
        {
            WriteNullTerminatedString(bw, str, shiftJis);
        }


        public static void ReadObject(this BinaryReader br, object obj)
        {
            //int lastIntValue = 0;
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                if (!field.IsInitOnly)
                {
                    if (field.FieldType == typeof(int))
                    {
                        int intValue = br.ReadInt32();
                        field.SetValue(obj, intValue);
                        //lastIntValue = intValue;
                    }
                    if (field.FieldType == typeof(string))
                    {
                        var fixedLengthAttribute = field.GetCustomAttributes(false).OfType<FixedLengthStringAttribute>().FirstOrDefault();
                        if (fixedLengthAttribute != null)
                        {
                            string stringValue = br.ReadFixedLengthString(fixedLengthAttribute.StringLength);
                            field.SetValue(obj, stringValue);
                        }
                        else
                        {
                            string stringValue = br.ReadNullTerminatedString();
                            field.SetValue(obj, stringValue);
                        }
                    }
                }
            }
        }

        public static void WriteObject(this BinaryWriter bw, object obj)
        {
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                if (!field.IsInitOnly)
                {
                    if (field.FieldType == typeof(int))
                    {
                        int intValue;
                        intValue = (int)field.GetValue(obj);
                        bw.Write(intValue);
                    }
                    if (field.FieldType == typeof(string))
                    {
                        string stringValue = (string)field.GetValue(obj);
                        var fixedLengthAttribute = field.GetCustomAttributes(false).OfType<FixedLengthStringAttribute>().FirstOrDefault();
                        if (fixedLengthAttribute != null)
                        {
                            bw.WriteFixedLengthString(stringValue, fixedLengthAttribute.StringLength);
                        }
                        else
                        {
                            bw.WriteNullTerminatedString(stringValue);
                        }
                    }
                }
            }
        }

        public static void WriteObjects(this BinaryWriter bw, object[] objects)
        {
            foreach (var obj in objects)
            {
                WriteObject(bw, obj);
            }
        }

    }

    public static partial class ReflectionUtil
    {
        public static string ToString(object obj)
        {
            StringBuilder sb = new StringBuilder();
            var type = obj.GetType();
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public);

            bool needComma = false;
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int) || field.FieldType == typeof(string) || field.FieldType.IsEnum)
                {
                    Util.PrintComma(sb, ref needComma);
                    sb.Append(field.Name + " = " + (field.GetValue(obj) ?? "null").ToString());
                }
            }
            return sb.ToString();

        }
    }

    public static partial class Util
    {
        static HashSet<char> illegalFilenameChars;

        public static bool IsLegalFilename(string fileName)
        {
            if (illegalFilenameChars == null)
            {
                illegalFilenameChars = new HashSet<char>(Path.GetInvalidFileNameChars());
            }

            foreach (var c in fileName)
            {
                if (illegalFilenameChars.Contains(c))
                {
                    return false;
                }
            }
            return true;
        }


        public static void PrintComma(StringBuilder sb, ref bool needComma)
        {
            if (needComma)
            {
                sb.Append(", ");
            }
            else
            {
                needComma = true;
            }
        }

        public static string GetExtensionWithoutDot(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension))
            {
                if (extension.StartsWith("."))
                {
                    extension = extension.Substring(1);
                }
            }
            return extension;
        }
    }
}
