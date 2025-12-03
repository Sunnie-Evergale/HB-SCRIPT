using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace HoneyBeeScriptTool
{
    public static class TextWrap
    {
        public static string WordWrapLines(string lines, int rightMargin)
        {
            var linesArray = lines.Split(new string[] { @"\n" }, StringSplitOptions.None);
            if (linesArray.Length == 1)
            {
                return WordWrap(linesArray[0], rightMargin);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var line in linesArray)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(@"\n");
                    }
                    sb.Append(WordWrap(line, rightMargin));
                }
                return sb.ToString();
            }
        }

        public static string WordWrap(string line, int rightMargin)
        {
            //wrap by adding spaces to fill to the right margin
            int rightCharIndex = FindCharacterIndexOfRightMargin(line, rightMargin);
            int indentSize = GetIndentSize(line);
            string indent = "".PadRight(indentSize, ' ');
            indent += indent;

            if (line.Length > rightCharIndex)
            {
                StringBuilder newLine = new StringBuilder();
                while (line.Length > rightCharIndex)
                {
                    int position = TextWrap.FindSplitPoint(line, rightCharIndex);
                    if (position <= indentSize)
                    {
                        position = rightCharIndex;
                    }
                    string fragment = line.Substring(0, position);
                    //eat spaces
                    while (position < line.Length && line[position] == ' ')
                    {
                        position++;
                    }
                    //add newline
                    fragment += @"\n";
                    newLine.Append(fragment);
                    line = indent + line.Substring(position);

                    rightCharIndex = FindCharacterIndexOfRightMargin(line, rightMargin);
                }
                newLine.Append(line);
                return newLine.ToString();
            }
            return line;
        }

        public static int FindCharacterIndexOfRightMargin(string messageText, int rightMargin)
        {
            int stringLength = messageText.Length;
            float x = 0;
            float targetX = rightMargin;
            float w = 0;
            int i = 0;
            while (x < targetX && i < stringLength)
            {
                char c = messageText[i];
                if (c >= 0x80 && !(c >= 0xFF61 && c <= 0xFF9F))
                {
                    w = 2;
                }
                else
                {
                    w = 1;
                }
                x += w;
                if (x > targetX)
                {
                    break;
                }
                i++;
            }
            return i;
        }

        public static int GetIndentSize(string line)
        {
            int indentSize = 0;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (indentCharacters.Contains(c))
                {
                    indentSize++;
                }
                else
                {
                    break;
                }
            }
            return indentSize;
        }

        static HashSet<char> indentCharacters = new HashSet<char>("　‘“（〔［｛〈《「『【".ToCharArray());
        static HashSet<char> terminatingCharacters = new HashSet<char>("）〕］｝〉》」』】。、，．：？！".ToCharArray());

        public static int FindSplitPoint(string line, int characterIndex)
        {
            int initialPosition = characterIndex;
            bool foundSpace = false;
            while (characterIndex >= 0)
            {
                //find a space, then find a non-space
                char c = line[characterIndex];

                if (c >= 0x80 && !terminatingCharacters.Contains(c))
                {
                    break;
                }
                if (c == ' ')
                {
                    if (!foundSpace)
                    {
                        foundSpace = true;
                    }
                }
                else
                {
                    if (foundSpace)
                    {
                        break;
                    }
                }
                characterIndex--;
            }
            characterIndex++;
            if (characterIndex == 0)
            {
                characterIndex = initialPosition;
            }
            return characterIndex;
        }
    }
}
