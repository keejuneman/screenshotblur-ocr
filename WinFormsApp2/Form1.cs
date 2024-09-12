using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR.Models.Local;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace ImageBlurApp
{
    public partial class Form1 : Form
    {
        private Image? originalImage;
        private Image? finalImage;
        private Rectangle selectedArea;
        private Rectangle absoluteArea;
        private string? saveDirectory;
        private bool absoluteMode;
        private bool isDragging;
        private float imageRatio;
        private bool isImageUploaded = false;
        private string? uploadedFilePath;
        private float imageScaleRatio = 1.0f;
        private BackgroundWorker backgroundWorker;
        private TextBox inputTextBox;
        private TextBox ocrTextBox;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;

            this.ClientSize = new System.Drawing.Size(1600, 950);
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            saveDirectory = LoadDirectoryPath();
            //MessageBox.Show($"�ҷ��� ���� ���: {saveDirectory}", "��� Ȯ��", MessageBoxButtons.OK, MessageBoxIcon.Information);


            dateTextBox.Text = DateTime.Now.ToString("yyMMdd");
            this.Resize += new EventHandler(Form1_Resize);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            ResizePanel();

            backgroundWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            panel.AllowDrop = true;
            panel.DragEnter += new DragEventHandler(panel_DragEnter);
            panel.DragDrop += new DragEventHandler(panel_DragDrop);

            // ���� ���۸��� ����� �г� ����
            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, panel, new object[] { true });
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ResizePanel();
            if (originalImage != null)
            {
                DisplayImage(originalImage);
            }
        }

        private void ResizePanel()
        {
            int formWidth = 1600; // ������ â �ʺ�
            int formHeight = 950; // ������ â ����
            int buttonPanelHeight = buttonPanel.Height; // ��ư �г� ���� (������ 70)

            // ��ư �г� ���̸� ����Ͽ� �̹��� �г��� ���� ���
            int availableHeight = formHeight - buttonPanelHeight;

            float aspectRatio = 1920f / 1080f;
            int panelWidth = (int)(availableHeight * aspectRatio);
            int panelHeight = availableHeight;

            // �г��� �ʺ� â �ʺ񺸴� Ŭ ��� �ʺ� ���� ���̸� ����
            if (panelWidth > formWidth)
            {
                panelWidth = formWidth;
                panelHeight = (int)(formWidth / aspectRatio);
            }

            // �г� ũ�� ����
            panel.Size = new System.Drawing.Size(panelWidth, panelHeight);

            // �г��� �߾ӿ� ��ġ��Ű��, ��ư �г� �Ʒ��� ��ġ�ϵ��� ��
            panel.Location = new System.Drawing.Point(
                (formWidth - panelWidth) / 2,
                buttonPanelHeight // ��ư �г� �ٷ� �Ʒ��� ��ġ
            );
        }

        private void uploadButton_Click(object? sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    originalImage = Image.FromFile(openFileDialog.FileName);
                    if (originalImage != null)
                    {
                        uploadedFilePath = openFileDialog.FileName;
                        DisplayImage(originalImage);
                        isImageUploaded = true;
                        Application.DoEvents();
                        resetButton_Click(null, EventArgs.Empty);
                    }
                }
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("�̹��� ������ ��ȿ���� �ʽ��ϴ�. �ٸ� ������ �����ϼ���.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    originalImage = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"�̹����� �ε��ϴ� �� ������ �߻��߽��ϴ�: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    originalImage = null;
                }
            }
        }

        private void DisplayImage(Image image)
        {
            if (image == null) return;

            float widthRatio = (float)panel.Width / image.Width;
            float heightRatio = (float)panel.Height / image.Height;
            imageScaleRatio = Math.Min(widthRatio, heightRatio);

            int newWidth = (int)(image.Width * imageScaleRatio);
            int newHeight = (int)(image.Height * imageScaleRatio);
            int x = (panel.Width - newWidth) / 2;
            int y = (panel.Height - newHeight) / 2;

            Graphics g = panel.CreateGraphics();
            g.Clear(panel.BackColor);
            g.DrawImage(image, x, y, newWidth, newHeight);
        }

        private void panel_Paint(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;

            Image imageToDisplay = finalImage ?? originalImage; // finalImage�� ������ originalImage�� ǥ��
            if (imageToDisplay != null)
            {
                float widthRatio = (float)panel.Width / imageToDisplay.Width;
                float heightRatio = (float)panel.Height / imageToDisplay.Height;
                float scaleRatio = Math.Min(widthRatio, heightRatio);

                int newWidth = (int)(imageToDisplay.Width * scaleRatio);
                int newHeight = (int)(imageToDisplay.Height * scaleRatio);
                int offsetX = (panel.Width - newWidth) / 2;
                int offsetY = (panel.Height - newHeight) / 2;

                var imageRectangle = new Rectangle(offsetX, offsetY, newWidth, newHeight);
                graphics.DrawImage(imageToDisplay, imageRectangle); // �̹��� �׸���

                // ���� ���� �׸���
                if (absoluteArea != Rectangle.Empty)
                {
                    System.Drawing.Point topLeft = GetPanelCoordinates(new System.Drawing.Point(absoluteArea.Left, absoluteArea.Top));
                    System.Drawing.Point bottomRight = GetPanelCoordinates(new System.Drawing.Point(absoluteArea.Right, absoluteArea.Bottom));
                    Rectangle boxAbsoluteArea = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                    graphics.DrawRectangle(new Pen(Color.Blue, 2), boxAbsoluteArea); // �Ķ��� ������ ���� ���� �׸���
                }

                // ���� ���� �׸���
                if (selectedArea != Rectangle.Empty)
                {
                    System.Drawing.Point topLeft = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Left, selectedArea.Top));
                    System.Drawing.Point bottomRight = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Right, selectedArea.Bottom));

                    Rectangle boxSelectedArea = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

                    // �г� ũ�⿡ �°� ���� ���� ��� Ȯ��
                    boxSelectedArea = new Rectangle(
                        Math.Max(0, boxSelectedArea.X),
                        Math.Max(0, boxSelectedArea.Y),
                        Math.Min(panel.Width - boxSelectedArea.X, boxSelectedArea.Width),
                        Math.Min(panel.Height - boxSelectedArea.Y, boxSelectedArea.Height)
                    );

                    graphics.DrawRectangle(new Pen(Color.Red, 2), boxSelectedArea); // ������ ������ ���� ���� �׸���
                }
            }
        }


        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                System.Drawing.Point imageCoords = GetImageCoordinates(new System.Drawing.Point(e.X, e.Y));

                if (absoluteMode)
                {
                    // ���� ���� �巡�� ���� ��ǥ ����
                    absoluteArea = new Rectangle(imageCoords.X, imageCoords.Y, 0, 0);
                }
                else
                {
                    // ���� ���� �巡�� ���� ��ǥ ����
                    selectedArea = new Rectangle(imageCoords.X, imageCoords.Y, 0, 0);
                }
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Drawing.Point imageCoords = GetImageCoordinates(new System.Drawing.Point(e.X, e.Y));

                if (absoluteMode)
                {
                    UpdateDraggingArea(ref absoluteArea, imageCoords);
                }
                else
                {
                    UpdateDraggingArea(ref selectedArea, imageCoords);
                }

                // ����� ������ �ٽ� �׸���
                //Rectangle redrawRect = GetRedrawRectangle();
                //panel.Invalidate(redrawRect);

                panel.Invalidate();
            }
        }

        private Rectangle GetRedrawRectangle()
        {
            // ���� ������ �ִ� ��� �ش� ������ �ٽ� �׸��� ���� �޼���
            System.Drawing.Point topLeft = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Left, selectedArea.Top));
            System.Drawing.Point bottomRight = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Right, selectedArea.Bottom));

            return new Rectangle(
                topLeft.X, topLeft.Y,
                bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;

            if (selectedArea.Width <= 0 || selectedArea.Height <= 0)
            {
                return;
            }

            if (originalImage != null)
            {
                // �̹��� ��� �������� ���� ������ ����ǵ���
                selectedArea = Rectangle.Intersect(selectedArea, new Rectangle(0, 0, originalImage.Width, originalImage.Height));
                absoluteArea = Rectangle.Intersect(absoluteArea, new Rectangle(0, 0, originalImage.Width, originalImage.Height));
            }

            // �� ���� �� �г� �ٽ� �׸���
            ApplyBlur();
            panel.Invalidate();
        }

        // ���ο� UpdateDraggingArea �Լ� �߰�
        private void UpdateDraggingArea(ref Rectangle area, System.Drawing.Point currentCoords)
        {
            // �巡�� ���⿡ ������� ���� ��ǥ�� ���� ��ǥ�� ����� �簢�� ���� ������Ʈ
            int startX = Math.Min(area.Left, currentCoords.X);
            int startY = Math.Min(area.Top, currentCoords.Y);
            int endX = Math.Max(area.Left, currentCoords.X);
            int endY = Math.Max(area.Top, currentCoords.Y);

            area = new Rectangle(startX, startY, endX - startX, endY - startY);
        }

        private void ApplyBlur()
        {
            if (originalImage != null)
            {
                Bitmap bitmapImage = new Bitmap(originalImage);
                Bitmap blurImage = new Bitmap(bitmapImage);
                Mat matImage = OpenCvSharp.Extensions.BitmapConverter.ToMat(blurImage);

                int blurIntensity = blurTrackBar.Value;
                if (blurIntensity % 2 == 0) blurIntensity += 1;

                // �� ������ �� ���̱� ���� �� ũ�⸦ ������Ŵ
                int enhancedBlurIntensity = blurIntensity * 3;  // ������ 3��� ���� (�ʿ信 ���� ���� ����)
                OpenCvSharp.Size blurSize = new OpenCvSharp.Size(enhancedBlurIntensity, enhancedBlurIntensity);

                Mat mask = Mat.Ones(matImage.Size(), MatType.CV_8U) * 255;

                if (selectedArea != Rectangle.Empty)
                {
                    Rect selectedRect = new Rect(selectedArea.Left, selectedArea.Top, selectedArea.Width, selectedArea.Height);
                    mask[selectedRect].SetTo(Scalar.All(0));
                }

                if (absoluteArea != Rectangle.Empty)
                {
                    Rect absoluteRect = new Rect(absoluteArea.Left, absoluteArea.Top, absoluteArea.Width, absoluteArea.Height);
                    mask[absoluteRect].SetTo(Scalar.All(0));
                }

                Mat blurredMat = matImage.Clone();
                Cv2.GaussianBlur(matImage, blurredMat, blurSize, 0);
                blurredMat.CopyTo(matImage, mask);

                finalImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(matImage);
                DisplayImage(finalImage);
            }
        }

        private System.Drawing.Point GetImageCoordinates(System.Drawing.Point panelCoords)
        {
            if (originalImage == null) return panelCoords;

            float widthRatio = (float)panel.Width / originalImage.Width;
            float heightRatio = (float)panel.Height / originalImage.Height;
            float scaleRatio = Math.Min(widthRatio, heightRatio);

            int offsetX = (panel.Width - (int)(originalImage.Width * scaleRatio)) / 2;
            int offsetY = (panel.Height - (int)(originalImage.Height * scaleRatio)) / 2;

            int imageX = (int)((panelCoords.X - offsetX) / scaleRatio);
            int imageY = (int)((panelCoords.Y - offsetY) / scaleRatio);

            imageX = Math.Max(0, Math.Min(imageX, originalImage.Width - 1));
            imageY = Math.Max(0, Math.Min(imageY, originalImage.Height - 1));

            return new System.Drawing.Point(imageX, imageY);
        }

        private System.Drawing.Point GetPanelCoordinates(System.Drawing.Point imageCoords)
        {
            if (originalImage == null) return imageCoords;

            float widthRatio = (float)panel.Width / originalImage.Width;
            float heightRatio = (float)panel.Height / originalImage.Height;
            float scaleRatio = Math.Min(widthRatio, heightRatio);

            int offsetX = (panel.Width - (int)(originalImage.Width * scaleRatio)) / 2;
            int offsetY = (panel.Height - (int)(originalImage.Height * scaleRatio)) / 2;

            int panelX = (int)(imageCoords.X * scaleRatio + offsetX);
            int panelY = (int)(imageCoords.Y * scaleRatio + offsetY);

            return new System.Drawing.Point(panelX, panelY);
        }

        private void blurTrackBar_Scroll(object sender, EventArgs e)
        {
            ApplyBlur();
        }

        private void resetButton_Click(object? sender, EventArgs e)
        {
            if (originalImage != null)
            {
                DisplayImage(originalImage);
                selectedArea = Rectangle.Empty;
                absoluteArea = Rectangle.Empty;
                finalImage = null;
            }
        }

        private void saveDirectoryButton_Click(object? sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    saveDirectory = folderDialog.SelectedPath;
                    // ������ ��θ� ����
                    SaveDirectoryPath(saveDirectory);
                }
            }
        }

        private string LoadDirectoryPath()
        {
            // ������ �����ϸ� ���� ��θ� �о����, ������ �� ���ڿ� ��ȯ
            if (File.Exists("save_directory.txt"))
            {
                return File.ReadAllText("save_directory.txt");
            }
            return string.Empty;
        }


        private void SaveDirectoryPath(string path)
        {
            // ���� ��θ� ���Ͽ� ����
            File.WriteAllText("save_directory.txt", path);
        }

        private void openSaveDirectoryButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(saveDirectory) && Directory.Exists(saveDirectory))
            {
                try
                {
                    Process.Start("explorer.exe", saveDirectory);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"���� ��θ� ���� �� ������ �߻��߽��ϴ�: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("���� ��ΰ� ��ȿ���� �ʰų� �������� �ʾҽ��ϴ�.", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                saveDirectoryButton_Click(null, EventArgs.Empty);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://www.notion.so/",
                UseShellExecute = true
            });
        }


        private void saveButton_Click(object sender, EventArgs e)
        {
            if (!isImageUploaded)
            {
                MessageBox.Show("�̹����� �����ϴ�. �̹����� ���ε��ϼ���.", "���", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uploadButton_Click(null, EventArgs.Empty);
                return;
            }
            else if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("���� ��ΰ� �������� �ʾҽ��ϴ�. ���� ��θ� �����ϼ���.", "���", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                saveDirectoryButton_Click(null, EventArgs.Empty);
                return;
            }

            // Show popup for input if checkbox is checked
            string inputText = "";
            if (checkBox1.Checked)
            {
                using (InputPopup popup = new InputPopup())
                {
                    if (popup.ShowDialog() == DialogResult.OK)
                    {
                        inputText = popup.InputText; // Capture the input from the popup
                    }
                }
                // ���α׷����ٸ� ������� ����
                progressBar.Visible = false;
            }
            else
            {
                // ProgressBar �ʱ�ȭ
                progressBar.Value = 0;
                progressBar.Visible = true;
            }

            // �񵿱� ���� �۾� ����
            Task.Run(() => SaveImageAsync(inputText));
        }

        private async Task SaveImageAsync(string inputText)
        {
            if (uploadedFilePath != null)
            {
                string extension = Path.GetExtension(uploadedFilePath).ToLower();
                System.Drawing.Imaging.ImageFormat imageFormat = extension switch
                {
                    ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
                    ".bmp" => System.Drawing.Imaging.ImageFormat.Bmp,
                    ".gif" => System.Drawing.Imaging.ImageFormat.Gif,
                    ".png" => System.Drawing.Imaging.ImageFormat.Png,
                    _ => System.Drawing.Imaging.ImageFormat.Png
                };

                string ocrText = inputText;

                if (!checkBox1.Checked)
                {
                    ocrText = ExtractTextFromSelectedArea(); // Use OCR if checkbox is not checked
                }

                string date = dateTextBox.Text;
                string entryExit = entryRadioButton.Checked ? "�Խ�" : "���";

                string filename = $"{date}_{entryExit}_{ocrText}{extension}";
                filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(saveDirectory, filename);
                //MessageBox.Show($"���� ���� ���: {savePath}", "��� Ȯ��", MessageBoxButtons.OK, MessageBoxIcon.Information);


                //MessageBox.Show($"���� ���: {savePath}", "���� ��� Ȯ��", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (File.Exists(savePath))
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("������ �̹� �����մϴ�. �ٸ� �̸����� �����ϼ���.", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                    return;
                }

                if (finalImage != null)
                {
                    if (!checkBox1.Checked)
                    {
                        for (int i = 0; i <= 100; i += 25) // ����� ������Ʈ (4�ܰ�� �ùķ��̼�)
                        {
                            await Task.Delay(500); // ���� �ùķ��̼��� ���� ����

                            // ���� ��Ȳ ������Ʈ
                            Invoke(new Action(() =>
                            {
                                progressBar.Value = i;
                            }));
                        }
                    }

                    finalImage.Save(savePath, imageFormat);

                    // ���� �Ϸ� �� ProgressBar �����
                    Invoke(new Action(() =>
                    {
                        if (!checkBox1.Checked)
                        {
                            progressBar.Value = 100;
                            progressBar.Visible = false;
                        }

                        if (!hideConfirmationCheckBox.Checked)
                        {
                            MessageBox.Show($"�̹����� {savePath}�� ����Ǿ����ϴ�.", "���� �Ϸ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }));
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("���ε�� ���� ��ΰ� ��ȿ���� �ʽ��ϴ�.", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int progress = 0;
            string savePath = SaveImage(ref progress);

            // ���� ��Ȳ ������Ʈ (���� �Ϸ�)
            backgroundWorker.ReportProgress(100);
        }

        private string SaveImage(ref int progress)
        {
            if (uploadedFilePath != null)
            {
                string extension = Path.GetExtension(uploadedFilePath).ToLower();
                System.Drawing.Imaging.ImageFormat imageFormat = extension switch
                {
                    ".jpg" or ".jpeg" => System.Drawing.Imaging.ImageFormat.Jpeg,
                    ".bmp" => System.Drawing.Imaging.ImageFormat.Bmp,
                    ".gif" => System.Drawing.Imaging.ImageFormat.Gif,
                    ".png" => System.Drawing.Imaging.ImageFormat.Png,
                    _ => System.Drawing.Imaging.ImageFormat.Png
                };

                string ocrText = ExtractTextFromSelectedArea();
                string date = dateTextBox.Text;
                string entryExit = entryRadioButton.Checked ? "�Խ�" : "���";

                string filename = $"{date}_{entryExit}_{ocrText}{extension}";
                filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(saveDirectory, filename);

                if (File.Exists(savePath))
                {
                    throw new Exception("������ �̹� �����մϴ�. �ٸ� �̸����� �����ϼ���.");
                }

                if (finalImage != null)
                {
                    // �̹��� ����
                    for (int i = 0; i <= 100; i += 25) // 4�ܰ�� ����� ������Ʈ
                    {
                        Task.Delay(500).Wait(); // �۾� �ùķ��̼��� ���� ����
                        backgroundWorker.ReportProgress(i);
                    }

                    finalImage.Save(savePath, imageFormat);
                }

                return savePath;
            }

            throw new Exception("���ε�� ���� ��ΰ� ��ȿ���� �ʽ��ϴ�.");
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Visible = false;

            if (e.Error != null)
            {
                MessageBox.Show($"���� �� ������ �߻��߽��ϴ�: {e.Error.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("�̹����� ���������� ����Ǿ����ϴ�.", "���� �Ϸ�", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string ExtractTextFromSelectedArea()
        {
            if (originalImage != null && selectedArea != Rectangle.Empty)
            {
                Bitmap croppedImage = new Bitmap(selectedArea.Width, selectedArea.Height);
                using (Graphics g = Graphics.FromImage(croppedImage))
                {
                    g.DrawImage(originalImage, 0, 0, selectedArea, GraphicsUnit.Pixel);
                }

                string tempImagePath = Path.Combine(Path.GetTempPath(), "temp_cropped_image.png");
                croppedImage.Save(tempImagePath);

                FullOcrModel model = LocalFullModels.KoreanV4;

                using (Mat src = Cv2.ImRead(tempImagePath))
                using (PaddleOcrAll ocr = new PaddleOcrAll(model, PaddleDevice.Mkldnn())
                {
                    AllowRotateDetection = true,
                    Enable180Classification = false
                })
                {
                    PaddleOcrResult result = ocr.Run(src);
                    return result.Text.Replace(" ", "").Trim();
                }
            }
            return string.Empty;
        }

        private void setAbsoluteModeButton_Click(object sender, EventArgs e)
        {
            absoluteMode = true;
        }

        private void setSelectModeButton_Click(object sender, EventArgs e)
        {
            absoluteMode = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            // ocrTextBox�� null���� Ȯ��
            if (ocrTextBox != null)
            {
                if (checkBox1.Checked)
                {
                    // Disable OCR functionality
                    ocrTextBox.Enabled = false;
                    progressBar.Visible = false; // ���α׷����ٵ� ����
                }
                else
                {
                    // Enable OCR functionality
                    ocrTextBox.Enabled = true;
                    progressBar.Visible = true; // ���α׷����� ǥ��
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ���� ���� ���� ���α׷��� ��θ� �����ɴϴ�.
            string applicationPath = Application.ExecutablePath;

            // �� �ν��Ͻ��� �����ϱ� ���� ���μ��� ����
            System.Diagnostics.Process.Start(applicationPath);

            // ���� �ν��Ͻ��� ����
            Application.Exit();
        }

        private void buttonPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
                e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        // DragDrop �̺�Ʈ �ڵ鷯
        private void panel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ������ ��ӵ� ��� ó��
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string filePath = files[0];
                    if (Path.GetExtension(filePath).ToLower() == ".jpg" ||
                        Path.GetExtension(filePath).ToLower() == ".jpeg" ||
                        Path.GetExtension(filePath).ToLower() == ".png" ||
                        Path.GetExtension(filePath).ToLower() == ".bmp" ||
                        Path.GetExtension(filePath).ToLower() == ".gif")
                    {
                        // �̹��� ������ ���ε�
                        originalImage = Image.FromFile(filePath);
                        uploadedFilePath = filePath;
                        DisplayImage(originalImage);
                        isImageUploaded = true;
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                // �̹����� ���� ��ӵ� ��� ó��
                originalImage = (Image)e.Data.GetData(DataFormats.Bitmap);
                DisplayImage(originalImage);
                isImageUploaded = true;
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                // Ŭ�����忡 �̹����� �ִ��� Ȯ���ϰ� ��������
                if (Clipboard.ContainsImage())
                {
                    originalImage = Clipboard.GetImage();
                    DisplayImage(originalImage);
                    isImageUploaded = true;

                    // Ŭ�����忡�� �̹����� ������ ��� �⺻ ���� ��ο� ���� �̸��� ����
                    string defaultFileName = $"clipboard_image_{DateTime.Now:yyyyMMdd_HHmmss}.png"; // �⺻ ���� �̸�
                    uploadedFilePath = Path.Combine(saveDirectory, defaultFileName); // ���� ��ο� ���ϸ� ����
                }
            }

            if (e.Control && e.KeyCode == Keys.S)
            {
                saveButton_Click(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }


    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }

    public class InputPopup : Form
    {
        private TextBox inputTextBox;
        private Button okButton;
        public string InputText { get; private set; }

        public InputPopup()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "�̸��� �Է��ϼ���.";
            this.Size = new System.Drawing.Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;


            inputTextBox = new TextBox
            {
                Location = new System.Drawing.Point(10, 10),
                Size = new System.Drawing.Size(260, 30)
            };
            this.Controls.Add(inputTextBox);

            okButton = new Button
            {
                Text = "OK",
                Location = new System.Drawing.Point(100, 50),
                Size = new System.Drawing.Size(75, 30)
            };
            okButton.Click += OkButton_Click;
            this.Controls.Add(okButton);
            this.AcceptButton = okButton;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            InputText = inputTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


    }
}
