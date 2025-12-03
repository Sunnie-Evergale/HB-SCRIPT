using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Globalization;

namespace HoneyBeeScriptTool
{
    class TextConverter
    {
        public TextConverter()
        {
            if (validCharacters == null)
            {
                validCharacters = GetValidCharacters();
            }
        }

        static readonly Encoding shiftJisWithThrow = Encoding.GetEncoding("shift-jis", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        static readonly Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        static byte[] validCharacters = null;//GetValidCharacters();
        static char invalidChar = Encoding.GetEncoding("shift-jis").GetString(new byte[] { 0xFF, 0xFF })[0];

        private static byte[] GetValidCharacters()
        {
            byte[] validCharacters = new byte[65536];

            for (int i = 0; i < 65536; i++)
            {
                byte result = 0;
                int b = i & 0xFF;
                int b2 = i >> 8;

                if (b >= ' ' && b <= '~')
                {
                    result = 1;
                }

                if (b >= 0xA2 && b <= 0xA5)
                {
                    //accept halfwidth katakana for brackets, commas, and the dot
                    result = 1;
                }
                //is valid two-byte Shift-Jis sequence
                if ((b >= 0x81 && b <= 0x84) || (b >= 0x87 && b <= 0x9F) || (b >= 0xE0 && b <= 0xEA) || (b >= 0xED && b <= 0xEE) || (b >= 0xFA) && b <= 0xFC)
                {
                    if (b2 >= 0x40 && b2 <= 0xFC && b2 != 0x7F)
                    {
                        //try
                        //{
                        var bytes2 = new byte[] { (byte)b, (byte)b2 };
                        string decoded = shiftJis.GetString(bytes2);
                        if (decoded[0] == invalidChar)
                        {
                            result = 0;
                        }
                        else
                        {
                            result = 2;
                        }
                        //}
                        //catch (DecoderFallbackException)
                        //{
                        //    result = 0;
                        //}
                    }
                }
                validCharacters[i] = result;
            }
            return validCharacters;
        }



        static int IsValidCharacter(byte[] bytes, int i)
        {
            if (i >= bytes.Length)
            {
                return 0;
            }
            int b1 = bytes[i];
            int b2 = 0;
            if (i + 1 < bytes.Length)
            {
                b2 = bytes[i + 1];
            }
            return validCharacters[b1 + (b2 << 8)];

        }

        static int NumberOfValidBytesAfterPosition(byte[] bytes, int startPosition)
        {
            int count = 0;
            int i = 0;

            while (true)
            {
                int validCount = IsValidCharacter(bytes, i + startPosition);
                if (validCount == 0)
                {
                    break;
                }
                i += validCount;
                count += validCount;
            }
            return count;
        }

        static byte[] _lookForLabel = new byte[] { (byte)0x06, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x00, (byte)0x06 };

        private static int IndexOf(byte[] array, byte[] lookForArray, int startIndex)
        {
            for (int i = startIndex; i <= array.Length - lookForArray.Length; i++)
            {
                if (array[i] == lookForArray[0])
                {
                    int i2;
                    for (i2 = 1; i2 < lookForArray.Length; i2++)
                    {
                        if (array[i + i2] != lookForArray[i2])
                        {
                            break;
                        }
                    }
                    if (i2 == lookForArray.Length)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public string[] ConvertText(byte[] bytes)
        {
            HashSet<int> LabelReferences = new HashSet<int>();
            HashSet<int> Labels = new HashSet<int>();
            //find labels
            {
                int pos = -1;
                do
                {
                    pos = pos + 1;
                    pos = IndexOf(bytes, _lookForLabel, pos);
                    if (pos >= 0)
                    {
                        int addr = pos + _lookForLabel.Length;
                        if (addr + 4 <= bytes.Length)
                        {
                            int value = BitConverter.ToInt32(bytes, addr);
                            Labels.Add(value);
                            LabelReferences.Add(addr);
                        }
                    }
                } while (pos >= 0);
            }

            const int minimumLength = 3;
            const int minimumLengthJ = 2;
            int previousStartPosition = -1;
            int previousLength = -1;
            List<int> stringPositions = new List<int>();

            for (int startPosition = 0; startPosition < bytes.Length; startPosition++)
            {
                int length = NumberOfValidBytesAfterPosition(bytes, startPosition);
                byte terminator = 255;
                if (startPosition + length < bytes.Length)
                {
                    terminator = bytes[startPosition + length];
                }
                if ((length >= minimumLength || ((length >= minimumLengthJ) && (IsValidCharacter(bytes, startPosition) == 2))) &&
                    terminator == 0)
                {
                    stringPositions.Add(startPosition);
                    previousStartPosition = startPosition;
                    previousLength = length;

                    startPosition += length;
                }
                else
                {
                    if (length > 4)
                    {

                    }
                }
            }

            List<string> list = new List<string>();
            int inputPosition = 0;
            for (int i = 0; i <= stringPositions.Count; i++)
            {
                int stringPosition = 0;
                if (i >= stringPositions.Count)
                {
                    stringPosition = bytes.Length;
                }
                else
                {
                    stringPosition = stringPositions[i];
                }

                if (inputPosition < stringPosition)
                {
                    StringBuilder sb = new StringBuilder();
                    //output some escaped characters
                    while (inputPosition < stringPosition)
                    {
                        if (LabelReferences.Contains(inputPosition))
                        {
                            int value = BitConverter.ToInt32(bytes, inputPosition);
                            string labelName = "lbl" + value.ToString("X4");
                            if (sb.Length > 0)
                            {
                                list.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            list.Add(":@" + labelName);
                            inputPosition += 4;
                        }
                        if (Labels.Contains(inputPosition))
                        {
                            string labelName = "lbl" + inputPosition.ToString("X4");
                            if (sb.Length > 0)
                            {
                                list.Add(sb.ToString());
                                sb.Length = 0;
                            }
                            list.Add("::" + labelName);
                        }

                        sb.Append("\\x" + bytes[inputPosition].ToString("X2"));
                        inputPosition++;
                    }
                    list.Add(sb.ToString());
                }
                if (inputPosition < bytes.Length)
                {
                    int stringLength = NumberOfValidBytesAfterPosition(bytes, inputPosition);
                    var text = shiftJis.GetString(bytes, inputPosition, stringLength);
                    text = text.Replace("\\x", "\\\\x");
                    list.Add(text);
                    inputPosition += stringLength + 1; //discard the null terminator
                }
            }
            return list.ToArray();
        }

        public byte[] ConvertToBytes(string[] lines)
        {
            Dictionary<string, int> labels = new Dictionary<string, int>();
            Dictionary<int, string> fixups = new Dictionary<int, string>();

            var ms = new MemoryStream();
            var bw = new BinaryWriter(ms);
            foreach (var line in lines)
            {
                if (line.StartsWith("\\x"))
                {
                    //line of control characters
                    for (int i = 0; i < line.Length; i += 4)
                    {
                        if (line[i] == '\\')
                        {
                            byte b;
                            if (Byte.TryParse(line.Substring(i + 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b))
                            {
                                bw.Write(b);
                            }
                        }
                    }
                }
                else if (line.StartsWith("#"))  //comment
                {

                }
                else if (line.StartsWith("::")) //label
                {
                    string labelName = line.Substring(2);
                    if (labelName != "lbl0020" && labelName != "lbl002D")
                    {

                    }
                    labels[labelName] = (int)ms.Position;
                }
                else if (line.StartsWith(":@")) //label reference
                {
                    string labelName = line.Substring(2);
                    if (labelName != "lbl0020" && labelName != "lbl002D")
                    {

                    }
                    fixups[(int)ms.Position] = labelName;
                    int defaultValue = 0;
                    if (labelName.StartsWith("lbl") && labelName.Length == 7)
                    {
                        defaultValue = int.Parse(labelName.Substring(3), NumberStyles.HexNumber);
                    }
                    bw.Write(defaultValue);
                }
                else if (line.Length > 0)
                {
                    //null-terminated string
                    bw.WriteNullTerminatedString(line);
                }
            }
            bw.Flush();
            var br = new BinaryReader(ms);

            foreach (var fixup in fixups)
            {
                var address = fixup.Key;
                var labelName = fixup.Value;
                if (labels.ContainsKey(labelName))
                {
                    int value = labels[labelName];
                    ms.Position = address;
                    int oldValue = br.ReadInt32();
                    ms.Position = address;
                    if (oldValue != value)
                    {

                    }
                    bw.Write(value);
                }
                else
                {
                    throw new ArgumentException("Label not found: " + labelName);
                }
            }
            ms.Position = ms.Length;

            return ms.ToArray();
        }

        /// <summary>
        /// Removes lines that are comments or begin with control characters
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public string[] RemoveLines(string[] lines, bool JapaneseOnly)
        {
            var list = new List<string>();
            foreach (var line in lines)
            {
                if (line.StartsWith("\\x"))
                {

                }
                else if (line.StartsWith("#"))
                {

                }
                else if (line.Length > 0)
                {
                    if (JapaneseOnly && !LineContainsJapaneseText(line))
                    {

                    }
                    else
                    {
                        list.Add(line);
                    }
                }
            }
            return list.ToArray();
        }

        private bool LineContainsJapaneseText(string line)
        {
            foreach (var c in line)
            {
                if (c >= 0x2000)
                {
                    return true;
                }
            }
            return false;
        }

        public string[] ReplaceLines(string[] inputLines, string[] modifiedLines, bool JapaneseOnly)
        {
            int i2 = 0;

            var list = new List<string>();
            foreach (var line in inputLines)
            {
                if (line.StartsWith("\\x"))
                {
                    list.Add(line);
                }
                else if (line.StartsWith("#"))
                {
                    list.Add(line);
                }
                else if (line.Length > 0)
                {
                    if (JapaneseOnly && !LineContainsJapaneseText(line))
                    {
                        list.Add(line);
                    }
                    else
                    {
                        bool found = false;
                        string line2 = "";
                        if (i2 < modifiedLines.Length)
                        {
                            line2 = modifiedLines[i2];
                            while (true)
                            {
                                if (line2.StartsWith("\\x"))
                                {

                                }
                                else if (line2.StartsWith("#"))
                                {

                                }
                                else if (line2.Length > 0)
                                {
                                    found = true;
                                    break;
                                }
                                i2++;
                                if (i2 >= inputLines.Length)
                                {
                                    found = false;
                                    break;
                                }
                                line2 = modifiedLines[i2];
                            }
                            i2++;
                        }
                        if (found)
                        {
                            list.Add(line2);
                        }
                        else
                        {
                            list.Add(line);
                        }
                    }
                }
            }
            return list.ToArray();
        }
    }
}
