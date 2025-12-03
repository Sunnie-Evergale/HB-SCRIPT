using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace HoneyBeeScriptTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GetSettings();
        }

        private void GetSettings()
        {
            scriptFileTextBox.Text = RegistryUtility.GetSetting("ScriptFileName", scriptFileTextBox.Text);
            pathTextBox.Text = RegistryUtility.GetSetting("TextPath", pathTextBox.Text);
            extractTextCodesCheckBox.Checked = RegistryUtility.GetSetting("ExtractAllCodes", false);
            japaneseTextOnlyCheckBox.Checked = RegistryUtility.GetSetting("JapaneseTextOnly", true);
            //UseBinFilesCheckBox.Checked = RegistryUtility.GetSetting("UseBinFiles", true);
        }

        private void ReplaceScript(string fileName, string outputFileName, string exportPath, bool extractAllCodes, bool japaneseOnly)
        {
            var scriptFile = new ScriptFile();
            scriptFile.ExtractAllCodes = extractAllCodes;
            scriptFile.JapaneseOnly = japaneseOnly;
            scriptFile.ReplaceAllFiles(fileName, outputFileName, exportPath);
        }

        private void ExtractScript(string fileName, string exportPath, bool extractAllCodes, bool japaneseOnly)
        {
            try
            {
                var bytes = File.ReadAllBytes(fileName);
                var scriptFile = new ScriptFile();
                scriptFile.ExtractAllCodes = extractAllCodes;
                scriptFile.JapaneseOnly = japaneseOnly;
                scriptFile.ExtractAllFiles(fileName, exportPath);
            }
            catch (IOException ex)
            {
                MessageBox.Show("Unable to read file: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("An error occurred when reading the package file.  Make sure it is a valid .ARC file." + "\r\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void openScriptButton_Click(object sender, EventArgs e)
        {
            OpenScript();
        }

        private void OpenScript()
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "ARC Files (*.arc)|*.arc|All Files (*.*)|*.*";

                var oldFileName = scriptFileTextBox.Text;
                string oldDirectory = "";
                try
                {
                    oldDirectory = Path.GetDirectoryName(oldFileName);
                }
                catch (Exception ex)
                {

                }
                openDialog.FileName = oldFileName;
                if (oldDirectory != "")
                {
                    openDialog.InitialDirectory = oldDirectory;
                }

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    scriptFileTextBox.Text = openDialog.FileName;
                }
            }
        }

        private void extractScriptButton_Click(object sender, EventArgs e)
        {
            ExtractScript();
        }

        private void ExtractScript()
        {
            SaveSettings();

            string path = pathTextBox.Text;
            string scriptFile = scriptFileTextBox.Text;
            if (String.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please provide a directory to extract files to.", "Script Extractor", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (String.IsNullOrEmpty(scriptFile))
            {
                MessageBox.Show("Please provide a script file to extract files from.", "Script Extractor", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to create directory: " + ex.Message, "Script Extractor", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }
            else
            {
                try
                {
                    int fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                    if (fileCount > 0)
                    {
                        if (MessageBox.Show("The output directory is not empty.  Replace files?", "Script Extractor", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read contents of directory: " + ex.Message, "Script Extractor", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }

            this.UseWaitCursor = true;
            //FileContent.cipher = keyTextBox.Text;
            //bool useBinFiles = this.UseBinFilesCheckBox.Checked;
            bool extractCodes = this.extractTextCodesCheckBox.Checked;
            bool japaneseOnly = this.japaneseTextOnlyCheckBox.Checked;
            ExtractScript(scriptFile, path, extractCodes, japaneseOnly);
            this.UseWaitCursor = false;
        }

        private void openPathButton_Click(object sender, EventArgs e)
        {
            OpenPath();
        }

        private void OpenPath()
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.ValidateNames = false;
                saveDialog.OverwritePrompt = false;
                saveDialog.CheckPathExists = true;
                saveDialog.Filter = "All Files (*.*)|*.*";
                var oldFileName = pathTextBox.Text;
                string oldDirectory = "";
                try
                {
                    oldDirectory = Path.GetDirectoryName(oldFileName + @"\");
                }
                catch (Exception ex)
                {

                }
                saveDialog.FileName = "PICK THIS DIRECTORY";
                if (oldDirectory != "")
                {
                    saveDialog.InitialDirectory = oldDirectory;
                }
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    pathTextBox.Text = Path.GetDirectoryName(saveDialog.FileName);
                }
            }
        }

        private void replaceScriptButton_Click(object sender, EventArgs e)
        {
            ReplaceScript();
        }

        private void ReplaceScript()
        {
            SaveSettings();

            string path = pathTextBox.Text;
            string scriptFile = scriptFileTextBox.Text;
            if (String.IsNullOrEmpty(path))
            {
                MessageBox.Show("Please provide a directory to replace files from.", "Script Replacer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if (String.IsNullOrEmpty(scriptFile))
            {
                MessageBox.Show("Please provide a script file to replace data in.", "Script Replacer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            if (!Directory.Exists(path))
            {
                MessageBox.Show("Text directory does not exist!", "Script Replacer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            else
            {
                try
                {
                    int fileCount = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
                    if (fileCount == 0)
                    {
                        MessageBox.Show("Text directory contains no files!", "Script Replacer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read contents of directory: " + ex.Message, "Script Replacer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }

            this.UseWaitCursor = true;
            //FileContent.cipher = keyTextBox.Text;
            //bool useBinFiles = this.UseBinFilesCheckBox.Checked;
            bool extractAllCodes = extractTextCodesCheckBox.Checked;
            bool japaneseOnly = japaneseTextOnlyCheckBox.Checked;
            ReplaceScript(scriptFile, scriptFile, path, extractAllCodes, japaneseOnly);
            this.UseWaitCursor = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            RegistryUtility.SaveSetting("ScriptFileName", scriptFileTextBox.Text);
            RegistryUtility.SaveSetting("TextPath", pathTextBox.Text);
            RegistryUtility.SaveSetting("ExtractAllCodes", extractTextCodesCheckBox.Checked);
            RegistryUtility.SaveSetting("JapaneseTextOnly", japaneseTextOnlyCheckBox.Checked);
            //RegistryUtility.SaveSetting("UseBinFiles", UseBinFilesCheckBox.Checked);
        }

        private void extractTextCodesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (extractTextCodesCheckBox.Checked == false)
            {
                japaneseTextOnlyCheckBox.Enabled = true;
            }
            else
            {
                japaneseTextOnlyCheckBox.Enabled = false;
            }
        }
    }
}
