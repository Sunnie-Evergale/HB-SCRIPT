using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HoneyBeeScriptTool
{
    /// <summary>
    /// Indicates that a string has a fixed length when being read or written
    /// </summary>
    public class FixedLengthStringAttribute : Attribute
    {
        public int StringLength;
        public FixedLengthStringAttribute(int stringLength)
        {
            this.StringLength = stringLength;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class FileExtensionEntry
    {
        [FixedLengthString(4)]
        public string FileExtension;
        public int FileCount, AddressOfFirstEntry;
        public static readonly int Size = 4 + 4 + 4;

        public override string ToString()
        {
            return ReflectionUtil.ToString(this);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ArchiveEntry
    {
        [FixedLengthString(13)]
        public string FileName;
        public int Length, Address;
        public static readonly int Size = 13 + 4 + 4;

        public override string ToString()
        {
            return ReflectionUtil.ToString(this);
        }

        public ArchiveEntry Clone()
        {
            return (ArchiveEntry)MemberwiseClone();
        }
    }

    public class ArchiveHeader
    {
        public int NumberOfFileExtensions;
        public FileExtensionEntry[] fileExtensions;
        public ArchiveEntry[] entries;

        public void Read(BinaryReader br)
        {
            NumberOfFileExtensions = br.ReadInt32();

            long fileLength = br.BaseStream.Length;
            long maximumPossibleNumberOfFileExtensions = (fileLength - 4) / (ArchiveEntry.Size + FileExtensionEntry.Size);
            if (maximumPossibleNumberOfFileExtensions > 65536)
            {
                maximumPossibleNumberOfFileExtensions = 65536;
            }

            //validate
            if (NumberOfFileExtensions > maximumPossibleNumberOfFileExtensions || NumberOfFileExtensions <= 0)
            {
                throw new FormatException("Invalid number of file extensions" + NumberOfFileExtensions.ToString());
            }

            fileExtensions = br.ReadObjects<FileExtensionEntry>(NumberOfFileExtensions);

            //validate file extensions
            int nextAddress = fileExtensions.Length * FileExtensionEntry.Size + 4;
            for (int i = 0; i < fileExtensions.Length; i++)
            {
                int pos = i * FileExtensionEntry.Size + 4;
                var ext = fileExtensions[i];
                if (ext.AddressOfFirstEntry != nextAddress)
                {
                    throw new FormatException("Position of first filename entry is wrong, it should be " + nextAddress.ToString("X") + " but instead is " + ext.AddressOfFirstEntry.ToString("X"));
                }
                nextAddress += ext.FileCount * (ArchiveEntry.Size);
                if (!Util.IsLegalFilename(ext.FileExtension))
                {
                    throw new FormatException("Filename \"" + ext.FileExtension + "\" contains illegal charactes.");
                }
            }

            List<ArchiveEntry> entiresList = new List<ArchiveEntry>();
            //read filenames
            foreach (var ext in fileExtensions)
            {
                var newEntries = br.ReadObjects<ArchiveEntry>(ext.FileCount);
                string extension = "." + ext.FileExtension;
                for (int i = 0; i < newEntries.Length; i++)
                {
                    var entry = newEntries[i];
                    entry.FileName += extension;
                }
                entiresList.AddRange(newEntries);
            }
            this.entries = entiresList.ToArray();

            //validate entries
            nextAddress = (int)br.BaseStream.Position;

            foreach (var entry in entries)
            {
                if (entry.Address != nextAddress)
                {
                    throw new FormatException("File data should be at address " + nextAddress.ToString("X") + " but is at address " + entry.Address.ToString("X") + " instead!");
                }
                if (!Util.IsLegalFilename(entry.FileName))
                {
                    throw new FormatException("File name \"" + entry.FileName + "\" is invalid!");
                }
                nextAddress += entry.Length;
                if (nextAddress > fileLength)
                {
                    throw new FormatException("Address " + nextAddress.ToString("X") + " exceeds package file size!");
                }
            }
        }

        public ArchiveHeader()
        {

        }

        public ArchiveHeader(string[] fileNames)
        {
            AddFileNames(fileNames);
        }

        private void AddFileNames(string[] fileNames)
        {
            string lastFileExtension = null;
            int countWithCurrentExtension = 0;

            List<ArchiveEntry> entries = new List<ArchiveEntry>();
            List<FileExtensionEntry> extensions = new List<FileExtensionEntry>();
            string extension = null;

            Action FinishExtension = () =>
            {
                if (lastFileExtension != null)
                {
                    var newExtension = new FileExtensionEntry();
                    newExtension.FileExtension = lastFileExtension;
                    newExtension.AddressOfFirstEntry = -1;
                    newExtension.FileCount = countWithCurrentExtension;
                    extensions.Add(newExtension);
                }
                countWithCurrentExtension = 0;
                lastFileExtension = extension;
            };


            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = fileNames[i];
                extension = Util.GetExtensionWithoutDot(fileName);
                if (extension != lastFileExtension)
                {
                    FinishExtension();
                }
                countWithCurrentExtension++;
                var entry = new ArchiveEntry();
                entry.Address = -1;
                entry.Length = -1;
                entry.FileName = fileName;
                entries.Add(entry);
            }
            FinishExtension();

            this.entries = entries.ToArray();
            this.fileExtensions = extensions.ToArray();
            this.NumberOfFileExtensions = this.fileExtensions.Length;

            FixExtensions();
        }

        private void FixExtensions()
        {
            int pos = 4 + NumberOfFileExtensions * FileExtensionEntry.Size;
            for (int i = 0; i < NumberOfFileExtensions; i++)
            {
                var ext = this.fileExtensions[i];
                ext.AddressOfFirstEntry = pos;
                pos += ext.FileCount * ArchiveEntry.Size;
            }
        }

        public void Write(BinaryWriter bw)
        {
             bw.Write(this.NumberOfFileExtensions);
            bw.WriteObjects(this.fileExtensions);
            var entriesCopy = (ArchiveEntry[])this.entries.Clone();
            for (int i = 0; i < entriesCopy.Length; i++)
            {
                var entry = entries[i].Clone();
                //remove extension from filenames
                entry.FileName = Path.GetFileNameWithoutExtension(entry.FileName);
                entriesCopy[i] = entry;
            }
            bw.WriteObjects(entriesCopy);
        }
    }

    public class ScriptFile
    {
        public bool ExtractAllCodes;
        public bool JapaneseOnly;

        ArchiveHeader archiveHeader = new ArchiveHeader();

        FileStream fileStream;
        BinaryReader br;
        BinaryWriter bw;

        static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

        public void ExtractAllFiles(string fileName, string exportPath)
        {
            this.fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.br = new BinaryReader(this.fileStream);
            this.archiveHeader.Read(br);

            foreach (var entry in this.archiveHeader.entries)
            {
                //extract the file
                string outputPath = Path.Combine(exportPath, entry.FileName);

                if (Path.GetExtension(entry.FileName).Equals(".WSC", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = br.ReadBytes(entry.Length);
                    DecodeFile(bytes);
                    File.WriteAllBytes(outputPath, bytes);

                    var textConverter = new TextConverter();
                    var originalLines = textConverter.ConvertText(bytes);
                    var lines = originalLines;
                    //string[] newLines = originalLines;


                    if (!this.ExtractAllCodes)
                    {
                        lines = textConverter.RemoveLines(lines, JapaneseOnly);

                        //validation
                        //newLines = textConverter.ReplaceLines(originalLines, lines, JapaneseOnly);
                    }
                    //validation

                    //var newBytes = textConverter.ConvertToBytes(newLines);
                    //if (!newBytes.SequenceEqual(bytes))
                    //{
                    //
                    //}

                    File.WriteAllLines(Path.ChangeExtension(outputPath, ".TXT"), lines, shiftJis);
                }
                else
                {
                    using (var outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        CopyStream(outputFileStream, this.fileStream, entry.Length);
                        outputFileStream.Close();
                    }
                }

            }
            this.Dispose();
        }

        private void DecodeFile(byte[] bytes)
        {
            //rotate all bytes right by two bits
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                int a = b & 3;
                b = b >> 2;
                b |= (a << 6);
                bytes[i] = (byte)b;
            }
        }

        private void EncodeFile(byte[] bytes)
        {
            //rotate all bytes left by two bits
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                int a = b & 0xC0;
                b = (b << 2) & 0xFF;
                b |= (a >> 6);
                bytes[i] = (byte)b;
            }
        }

        private void CopyStream(Stream outputFileStream, Stream inputFileStream, int length)
        {
            const int bufferSize = 65536;
            byte[] buffer = new byte[bufferSize];

            int remaining = length;
            while (remaining > 0)
            {
                int count;
                if (remaining < bufferSize)
                {
                    count = remaining;
                }
                else
                {
                    count = bufferSize;
                }
                inputFileStream.Read(buffer, 0, count);
                outputFileStream.Write(buffer, 0, count);
                remaining -= count;
            }
        }

        string currentFile = "";

        public void ReplaceAllFiles(string fileName, string outputFileName, string exportPath)
        {

            //list the files
            var filesInDirectory = Directory.GetFiles(exportPath, "*", SearchOption.TopDirectoryOnly);
            filesInDirectory = RemoveTextFiles(filesInDirectory);
            filesInDirectory = SortFileNames(filesInDirectory);

            var outputFileNames = MakeUppercase(RemoveInitialPath(filesInDirectory, exportPath));

            //prepare a header
            var ms = new MemoryStream();
            var bw2 = new BinaryWriter(ms);

            archiveHeader = new ArchiveHeader(outputFileNames);

            this.fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            this.bw = new BinaryWriter(this.fileStream);

            archiveHeader.Write(bw);

            //load files and write them to the package file
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                int position = (int)bw.BaseStream.Position;
                string inputFileName = filesInDirectory[i];

                if (Path.GetExtension(inputFileName).Equals(".WSC", StringComparison.OrdinalIgnoreCase))
                {
                    var originalBytes = File.ReadAllBytes(inputFileName);
                    var textConverter = new TextConverter();
                    var originalLines = textConverter.ConvertText(originalBytes);

                    byte[] bytes = originalBytes;

                    string textFileName = Path.ChangeExtension(inputFileName, ".TXT");
                    if (File.Exists(textFileName))
                    {
                        currentFile = textFileName;

                        var replacementLines = File.ReadAllLines(textFileName, shiftJis);

                        if (this.ExtractAllCodes)
                        {
                            bytes = textConverter.ConvertToBytes(replacementLines);
                        }
                        else
                        {
                            WordWrapLines(replacementLines);
                            var newLines = textConverter.ReplaceLines(originalLines, replacementLines, this.JapaneseOnly);
                            bytes = textConverter.ConvertToBytes(newLines);
                        }
                    }
                    EncodeFile(bytes);
                    bw.Write(bytes);
                }
                else
                {
                    using (var inputFileStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        CopyStream(bw.BaseStream, inputFileStream, (int)inputFileStream.Length);
                    }
                }

                int newPosition = (int)bw.BaseStream.Position;
                int fileLength = newPosition - position;
                archiveHeader.entries[i].Address = position;
                archiveHeader.entries[i].Length = fileLength;
            }

            this.bw.BaseStream.Position = 0;
            archiveHeader.Write(bw);

            this.Dispose();
        }

        private void WordWrapLines(string[] replacementLines)
        {
            for (int i = 0; i < replacementLines.Length; i++)
            {
                string suffix = "";
                string line = replacementLines[i];

                if (line.EndsWith("%K%P"))
                {
                    suffix = "%K%P";
                    line = line.Substring(0, line.Length - 4);
                }
                else
                {
                    //if (line.Length > 14)
                    //{
                    //    MessageBox.Show("warning: line " + (i + 1).ToString() + " of file \"" + currentFile + "\"\r\n:" +
                    //        "Looks like a character name might be too long!");
                    //}
                }
                line = TextWrap.WordWrapLines(line, 66);
                line += suffix;
                replacementLines[i] = line;
            }
        }

        private string[] MakeUppercase(string[] filesInDirectory)
        {
            return filesInDirectory.Select(f => f.ToUpperInvariant()).ToArray();
        }

        private string[] RemoveInitialPath(string[] filesInDirectory, string initialPath)
        {
            string[] files = (string[])filesInDirectory.Clone();
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                if (file.StartsWith(initialPath, StringComparison.OrdinalIgnoreCase))
                {
                    file = file.Substring(initialPath.Length);
                }
                if (file.StartsWith("/") || file.StartsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    file = file.Substring(1);
                }
                files[i] = file;
            }
            return files;
        }

        private string[] RemoveTextFiles(string[] filesInDirectory)
        {
            return filesInDirectory.Where(f => !f.EndsWith(".TXT", StringComparison.OrdinalIgnoreCase)).ToArray();
        }

        private string[] SortFileNames(string[] filesInDirectory)
        {
            return filesInDirectory.OrderBy(f => f.ToUpperInvariant(), StringComparer.Ordinal).OrderBy(f => Path.GetExtension(f).ToUpperInvariant(), StringComparer.Ordinal).ToArray();
        }



        void Dispose()
        {
            if (fileStream != null)
            {
                if (fileStream.CanWrite)
                {
                    fileStream.Flush();
                }
                fileStream.Close();
            }
        }
    }
}
