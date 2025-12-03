namespace HoneyBeeScriptTool
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblScriptPath = new System.Windows.Forms.Label();
            this.scriptFileTextBox = new System.Windows.Forms.TextBox();
            this.openScriptButton = new System.Windows.Forms.Button();
            this.lblPath = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.openPathButton = new System.Windows.Forms.Button();
            this.extractScriptButton = new System.Windows.Forms.Button();
            this.replaceScriptButton = new System.Windows.Forms.Button();
            this.extractTextCodesCheckBox = new System.Windows.Forms.CheckBox();
            this.japaneseTextOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblScriptPath
            // 
            this.lblScriptPath.AutoSize = true;
            this.lblScriptPath.Location = new System.Drawing.Point(57, 17);
            this.lblScriptPath.Name = "lblScriptPath";
            this.lblScriptPath.Size = new System.Drawing.Size(124, 13);
            this.lblScriptPath.TabIndex = 0;
            this.lblScriptPath.Text = "Path to &Script File (.ARC)";
            // 
            // scriptFileTextBox
            // 
            this.scriptFileTextBox.Location = new System.Drawing.Point(185, 14);
            this.scriptFileTextBox.Name = "scriptFileTextBox";
            this.scriptFileTextBox.Size = new System.Drawing.Size(302, 20);
            this.scriptFileTextBox.TabIndex = 1;
            // 
            // openScriptButton
            // 
            this.openScriptButton.Location = new System.Drawing.Point(493, 12);
            this.openScriptButton.Name = "openScriptButton";
            this.openScriptButton.Size = new System.Drawing.Size(75, 23);
            this.openScriptButton.TabIndex = 2;
            this.openScriptButton.Text = "Open...";
            this.openScriptButton.UseVisualStyleBackColor = true;
            this.openScriptButton.Click += new System.EventHandler(this.openScriptButton_Click);
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Location = new System.Drawing.Point(6, 47);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(173, 13);
            this.lblPath.TabIndex = 3;
            this.lblPath.Text = "&Path to hold extracted/edited script";
            // 
            // pathTextBox
            // 
            this.pathTextBox.Location = new System.Drawing.Point(185, 44);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.Size = new System.Drawing.Size(302, 20);
            this.pathTextBox.TabIndex = 4;
            // 
            // openPathButton
            // 
            this.openPathButton.Location = new System.Drawing.Point(493, 42);
            this.openPathButton.Name = "openPathButton";
            this.openPathButton.Size = new System.Drawing.Size(75, 23);
            this.openPathButton.TabIndex = 5;
            this.openPathButton.Text = "Open...";
            this.openPathButton.UseVisualStyleBackColor = true;
            this.openPathButton.Click += new System.EventHandler(this.openPathButton_Click);
            // 
            // extractScriptButton
            // 
            this.extractScriptButton.Location = new System.Drawing.Point(302, 70);
            this.extractScriptButton.Name = "extractScriptButton";
            this.extractScriptButton.Size = new System.Drawing.Size(129, 23);
            this.extractScriptButton.TabIndex = 8;
            this.extractScriptButton.Text = "&Extract Script";
            this.extractScriptButton.UseVisualStyleBackColor = true;
            this.extractScriptButton.Click += new System.EventHandler(this.extractScriptButton_Click);
            // 
            // replaceScriptButton
            // 
            this.replaceScriptButton.Location = new System.Drawing.Point(437, 70);
            this.replaceScriptButton.Name = "replaceScriptButton";
            this.replaceScriptButton.Size = new System.Drawing.Size(131, 23);
            this.replaceScriptButton.TabIndex = 9;
            this.replaceScriptButton.Text = "&Replace Script";
            this.replaceScriptButton.UseVisualStyleBackColor = true;
            this.replaceScriptButton.Click += new System.EventHandler(this.replaceScriptButton_Click);
            // 
            // extractTextCodesCheckBox
            // 
            this.extractTextCodesCheckBox.AutoSize = true;
            this.extractTextCodesCheckBox.Location = new System.Drawing.Point(9, 75);
            this.extractTextCodesCheckBox.Name = "extractTextCodesCheckBox";
            this.extractTextCodesCheckBox.Size = new System.Drawing.Size(111, 17);
            this.extractTextCodesCheckBox.TabIndex = 6;
            this.extractTextCodesCheckBox.Text = "Extract text codes";
            this.extractTextCodesCheckBox.UseVisualStyleBackColor = true;
            this.extractTextCodesCheckBox.CheckedChanged += new System.EventHandler(this.extractTextCodesCheckBox_CheckedChanged);
            // 
            // japaneseTextOnlyCheckBox
            // 
            this.japaneseTextOnlyCheckBox.AutoSize = true;
            this.japaneseTextOnlyCheckBox.Location = new System.Drawing.Point(126, 75);
            this.japaneseTextOnlyCheckBox.Name = "japaneseTextOnlyCheckBox";
            this.japaneseTextOnlyCheckBox.Size = new System.Drawing.Size(120, 17);
            this.japaneseTextOnlyCheckBox.TabIndex = 7;
            this.japaneseTextOnlyCheckBox.Text = "Japanese Text Only";
            this.japaneseTextOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 104);
            this.Controls.Add(this.japaneseTextOnlyCheckBox);
            this.Controls.Add(this.extractTextCodesCheckBox);
            this.Controls.Add(this.replaceScriptButton);
            this.Controls.Add(this.extractScriptButton);
            this.Controls.Add(this.openPathButton);
            this.Controls.Add(this.pathTextBox);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.openScriptButton);
            this.Controls.Add(this.scriptFileTextBox);
            this.Controls.Add(this.lblScriptPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Honey Bee Script Extractor/Inserter";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblScriptPath;
        private System.Windows.Forms.TextBox scriptFileTextBox;
        private System.Windows.Forms.Button openScriptButton;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.Button openPathButton;
        private System.Windows.Forms.Button extractScriptButton;
        private System.Windows.Forms.Button replaceScriptButton;
        private System.Windows.Forms.CheckBox extractTextCodesCheckBox;
        private System.Windows.Forms.CheckBox japaneseTextOnlyCheckBox;

    }
}

