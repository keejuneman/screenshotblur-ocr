namespace ImageBlurApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panel;
        private System.Windows.Forms.Button uploadButton;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.TextBox dateTextBox;
        private System.Windows.Forms.RadioButton entryRadioButton;
        private System.Windows.Forms.RadioButton exitRadioButton;
        private System.Windows.Forms.Button setAbsoluteModeButton;
        private System.Windows.Forms.Button setSelectModeButton;
        private System.Windows.Forms.Button saveDirectoryButton;
        private System.Windows.Forms.Button saveDirectoryOpenButton;
        private System.Windows.Forms.CheckBox hideConfirmationCheckBox;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TrackBar blurTrackBar;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            buttonPanel = new Panel();
            label2 = new Label();
            label1 = new Label();
            button2 = new Button();
            blurTrackBar = new TrackBar();
            checkBox1 = new CheckBox();
            button1 = new Button();
            progressBar = new ProgressBar();
            uploadButton = new Button();
            saveButton = new Button();
            resetButton = new Button();
            dateTextBox = new TextBox();
            entryRadioButton = new RadioButton();
            exitRadioButton = new RadioButton();
            setAbsoluteModeButton = new Button();
            setSelectModeButton = new Button();
            saveDirectoryButton = new Button();
            saveDirectoryOpenButton = new Button();
            hideConfirmationCheckBox = new CheckBox();
            panel = new Panel();
            buttonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)blurTrackBar).BeginInit();
            SuspendLayout();
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(label2);
            buttonPanel.Controls.Add(label1);
            buttonPanel.Controls.Add(button2);
            buttonPanel.Controls.Add(blurTrackBar);
            buttonPanel.Controls.Add(checkBox1);
            buttonPanel.Controls.Add(button1);
            buttonPanel.Controls.Add(progressBar);
            buttonPanel.Controls.Add(uploadButton);
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(resetButton);
            buttonPanel.Controls.Add(dateTextBox);
            buttonPanel.Controls.Add(entryRadioButton);
            buttonPanel.Controls.Add(exitRadioButton);
            buttonPanel.Controls.Add(setAbsoluteModeButton);
            buttonPanel.Controls.Add(setSelectModeButton);
            buttonPanel.Controls.Add(saveDirectoryButton);
            buttonPanel.Controls.Add(saveDirectoryOpenButton);
            buttonPanel.Controls.Add(hideConfirmationCheckBox);
            buttonPanel.Dock = DockStyle.Top;
            buttonPanel.Location = new Point(0, 0);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new Size(1600, 70);
            buttonPanel.TabIndex = 1;
            buttonPanel.Paint += buttonPanel_Paint;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 8F);
            label2.ForeColor = Color.Black;
            label2.Location = new Point(370, 17);
            label2.Name = "label2";
            label2.Size = new Size(29, 13);
            label2.TabIndex = 16;
            label2.Text = "날짜";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 8F);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(540, 44);
            label1.Name = "label1";
            label1.Size = new Size(78, 13);
            label1.TabIndex = 15;
            label1.Text = "Blur 강도 조절";
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button2.Font = new Font("맑은 고딕", 8F);
            button2.Location = new Point(1457, 40);
            button2.Name = "button2";
            button2.Size = new Size(62, 24);
            button2.TabIndex = 14;
            button2.Text = "Reboot";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // blurTrackBar
            // 
            blurTrackBar.Location = new Point(619, 38);
            blurTrackBar.Maximum = 30;
            blurTrackBar.Minimum = 1;
            blurTrackBar.Name = "blurTrackBar";
            blurTrackBar.Size = new Size(324, 45);
            blurTrackBar.TabIndex = 0;
            blurTrackBar.Value = 15;
            blurTrackBar.Scroll += blurTrackBar_Scroll;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Font = new Font("맑은 고딕", 8F);
            checkBox1.Location = new Point(370, 43);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(97, 17);
            checkBox1.TabIndex = 13;
            checkBox1.Text = "OCR 비활성화";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            button1.Font = new Font("맑은 고딕", 8F);
            button1.Location = new Point(1525, 40);
            button1.Name = "button1";
            button1.Size = new Size(63, 23);
            button1.TabIndex = 12;
            button1.Text = "피드백";
            button1.Click += button1_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressBar.Location = new Point(1374, 9);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(214, 25);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.TabIndex = 11;
            // 
            // uploadButton
            // 
            uploadButton.Font = new Font("맑은 고딕", 8F);
            uploadButton.Location = new Point(12, 11);
            uploadButton.Name = "uploadButton";
            uploadButton.Size = new Size(100, 25);
            uploadButton.TabIndex = 0;
            uploadButton.Text = "이미지 업로드";
            uploadButton.Click += uploadButton_Click;
            // 
            // saveButton
            // 
            saveButton.Font = new Font("맑은 고딕", 8F);
            saveButton.Location = new Point(644, 9);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(132, 25);
            saveButton.TabIndex = 1;
            saveButton.Text = "💾 저장(Ctrl+S)";
            saveButton.Click += saveButton_Click;
            // 
            // resetButton
            // 
            resetButton.Font = new Font("맑은 고딕", 8F);
            resetButton.Location = new Point(264, 39);
            resetButton.Name = "resetButton";
            resetButton.Size = new Size(100, 25);
            resetButton.TabIndex = 3;
            resetButton.Text = "초기화";
            resetButton.Click += resetButton_Click;
            // 
            // dateTextBox
            // 
            dateTextBox.Font = new Font("맑은 고딕", 8F);
            dateTextBox.Location = new Point(410, 11);
            dateTextBox.Name = "dateTextBox";
            dateTextBox.Size = new Size(100, 22);
            dateTextBox.TabIndex = 4;
            // 
            // entryRadioButton
            // 
            entryRadioButton.Checked = true;
            entryRadioButton.FlatStyle = FlatStyle.System;
            entryRadioButton.Font = new Font("맑은 고딕", 8F);
            entryRadioButton.Location = new Point(518, 10);
            entryRadioButton.Name = "entryRadioButton";
            entryRadioButton.Size = new Size(56, 24);
            entryRadioButton.TabIndex = 5;
            entryRadioButton.TabStop = true;
            entryRadioButton.Text = "입실";
            // 
            // exitRadioButton
            // 
            exitRadioButton.Font = new Font("맑은 고딕", 8F);
            exitRadioButton.Location = new Point(582, 10);
            exitRadioButton.Name = "exitRadioButton";
            exitRadioButton.Size = new Size(56, 24);
            exitRadioButton.TabIndex = 6;
            exitRadioButton.Text = "퇴실";
            // 
            // setAbsoluteModeButton
            // 
            setAbsoluteModeButton.Font = new Font("맑은 고딕", 8F);
            setAbsoluteModeButton.Location = new Point(12, 38);
            setAbsoluteModeButton.Name = "setAbsoluteModeButton";
            setAbsoluteModeButton.Size = new Size(120, 25);
            setAbsoluteModeButton.TabIndex = 7;
            setAbsoluteModeButton.Text = "시간 범위 지정";
            setAbsoluteModeButton.Click += setAbsoluteModeButton_Click;
            // 
            // setSelectModeButton
            // 
            setSelectModeButton.Font = new Font("맑은 고딕", 8F);
            setSelectModeButton.Location = new Point(138, 39);
            setSelectModeButton.Name = "setSelectModeButton";
            setSelectModeButton.Size = new Size(120, 25);
            setSelectModeButton.TabIndex = 8;
            setSelectModeButton.Text = "수강생 범위 지정";
            setSelectModeButton.Click += setSelectModeButton_Click;
            // 
            // saveDirectoryButton
            // 
            saveDirectoryButton.Font = new Font("맑은 고딕", 8F);
            saveDirectoryButton.Location = new Point(118, 11);
            saveDirectoryButton.Name = "saveDirectoryButton";
            saveDirectoryButton.Size = new Size(120, 25);
            saveDirectoryButton.TabIndex = 9;
            saveDirectoryButton.Text = "저장 경로 지정";
            saveDirectoryButton.Click += saveDirectoryButton_Click;
            // 
            // saveDirectoryOpenButton
            // 
            saveDirectoryOpenButton.Font = new Font("맑은 고딕", 8F);
            saveDirectoryOpenButton.Location = new Point(244, 11);
            saveDirectoryOpenButton.Name = "saveDirectoryOpenButton";
            saveDirectoryOpenButton.Size = new Size(120, 25);
            saveDirectoryOpenButton.TabIndex = 9;
            saveDirectoryOpenButton.Text = "저장 경로 열기";
            saveDirectoryOpenButton.Click += openSaveDirectoryButton_Click;
            // 
            // hideConfirmationCheckBox
            // 
            hideConfirmationCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            hideConfirmationCheckBox.Font = new Font("맑은 고딕", 8F);
            hideConfirmationCheckBox.Location = new Point(782, 9);
            hideConfirmationCheckBox.Name = "hideConfirmationCheckBox";
            hideConfirmationCheckBox.Size = new Size(135, 25);
            hideConfirmationCheckBox.TabIndex = 10;
            hideConfirmationCheckBox.Text = "확인창 미노출";
            hideConfirmationCheckBox.UseVisualStyleBackColor = true;
            // 
            // panel
            // 
            panel.BackColor = SystemColors.ControlDark;
            panel.Dock = DockStyle.Fill;
            panel.Location = new Point(0, 70);
            panel.Name = "panel";
            panel.Size = new Size(1600, 880);
            panel.TabIndex = 0;
            panel.Paint += panel_Paint;
            panel.MouseDown += pictureBox_MouseDown;
            panel.MouseMove += pictureBox_MouseMove;
            panel.MouseUp += pictureBox_MouseUp;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1600, 950);
            Controls.Add(panel);
            Controls.Add(buttonPanel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "고소 회피 툴";
            buttonPanel.ResumeLayout(false);
            buttonPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)blurTrackBar).EndInit();
            ResumeLayout(false);
        }

        private System.Windows.Forms.Button button1;
        private CheckBox checkBox1;
        private Button button2;
        private Label label1;
        private Label label2;
    }
}
