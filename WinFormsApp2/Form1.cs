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
            //MessageBox.Show($"불러온 저장 경로: {saveDirectory}", "경로 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);


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

            // 더블 버퍼링을 사용한 패널 설정
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
            int formWidth = 1600; // 고정된 창 너비
            int formHeight = 950; // 고정된 창 높이
            int buttonPanelHeight = buttonPanel.Height; // 버튼 패널 높이 (고정된 70)

            // 버튼 패널 높이를 고려하여 이미지 패널의 높이 계산
            int availableHeight = formHeight - buttonPanelHeight;

            float aspectRatio = 1920f / 1080f;
            int panelWidth = (int)(availableHeight * aspectRatio);
            int panelHeight = availableHeight;

            // 패널의 너비가 창 너비보다 클 경우 너비에 맞춰 높이를 조정
            if (panelWidth > formWidth)
            {
                panelWidth = formWidth;
                panelHeight = (int)(formWidth / aspectRatio);
            }

            // 패널 크기 설정
            panel.Size = new System.Drawing.Size(panelWidth, panelHeight);

            // 패널을 중앙에 위치시키되, 버튼 패널 아래로 위치하도록 함
            panel.Location = new System.Drawing.Point(
                (formWidth - panelWidth) / 2,
                buttonPanelHeight // 버튼 패널 바로 아래에 위치
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
                    MessageBox.Show("이미지 파일이 유효하지 않습니다. 다른 파일을 선택하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    originalImage = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"이미지를 로드하는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            Image imageToDisplay = finalImage ?? originalImage; // finalImage가 없으면 originalImage를 표시
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
                graphics.DrawImage(imageToDisplay, imageRectangle); // 이미지 그리기

                // 절대 영역 그리기
                if (absoluteArea != Rectangle.Empty)
                {
                    System.Drawing.Point topLeft = GetPanelCoordinates(new System.Drawing.Point(absoluteArea.Left, absoluteArea.Top));
                    System.Drawing.Point bottomRight = GetPanelCoordinates(new System.Drawing.Point(absoluteArea.Right, absoluteArea.Bottom));
                    Rectangle boxAbsoluteArea = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                    graphics.DrawRectangle(new Pen(Color.Blue, 2), boxAbsoluteArea); // 파란색 펜으로 절대 영역 그리기
                }

                // 선택 영역 그리기
                if (selectedArea != Rectangle.Empty)
                {
                    System.Drawing.Point topLeft = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Left, selectedArea.Top));
                    System.Drawing.Point bottomRight = GetPanelCoordinates(new System.Drawing.Point(selectedArea.Right, selectedArea.Bottom));

                    Rectangle boxSelectedArea = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

                    // 패널 크기에 맞게 선택 영역 경계 확인
                    boxSelectedArea = new Rectangle(
                        Math.Max(0, boxSelectedArea.X),
                        Math.Max(0, boxSelectedArea.Y),
                        Math.Min(panel.Width - boxSelectedArea.X, boxSelectedArea.Width),
                        Math.Min(panel.Height - boxSelectedArea.Y, boxSelectedArea.Height)
                    );

                    graphics.DrawRectangle(new Pen(Color.Red, 2), boxSelectedArea); // 빨간색 펜으로 선택 영역 그리기
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
                    // 절대 영역 드래그 시작 좌표 저장
                    absoluteArea = new Rectangle(imageCoords.X, imageCoords.Y, 0, 0);
                }
                else
                {
                    // 선택 영역 드래그 시작 좌표 저장
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

                // 변경된 영역만 다시 그리기
                //Rectangle redrawRect = GetRedrawRectangle();
                //panel.Invalidate(redrawRect);

                panel.Invalidate();
            }
        }

        private Rectangle GetRedrawRectangle()
        {
            // 선택 영역이 있는 경우 해당 영역만 다시 그리기 위한 메서드
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
                // 이미지 경계 내에서만 선택 영역이 적용되도록
                selectedArea = Rectangle.Intersect(selectedArea, new Rectangle(0, 0, originalImage.Width, originalImage.Height));
                absoluteArea = Rectangle.Intersect(absoluteArea, new Rectangle(0, 0, originalImage.Width, originalImage.Height));
            }

            // 블러 적용 및 패널 다시 그리기
            ApplyBlur();
            panel.Invalidate();
        }

        // 새로운 UpdateDraggingArea 함수 추가
        private void UpdateDraggingArea(ref Rectangle area, System.Drawing.Point currentCoords)
        {
            // 드래그 방향에 상관없이 시작 좌표와 현재 좌표를 사용해 사각형 영역 업데이트
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

                // 블러 강도를 더 높이기 위해 블러 크기를 증가시킴
                int enhancedBlurIntensity = blurIntensity * 3;  // 강도를 3배로 증가 (필요에 따라 조정 가능)
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
                    // 선택한 경로를 저장
                    SaveDirectoryPath(saveDirectory);
                }
            }
        }

        private string LoadDirectoryPath()
        {
            // 파일이 존재하면 저장 경로를 읽어오고, 없으면 빈 문자열 반환
            if (File.Exists("save_directory.txt"))
            {
                return File.ReadAllText("save_directory.txt");
            }
            return string.Empty;
        }


        private void SaveDirectoryPath(string path)
        {
            // 저장 경로를 파일에 저장
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
                    MessageBox.Show($"저장 경로를 여는 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("저장 경로가 유효하지 않거나 설정되지 않았습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show("이미지가 없습니다. 이미지를 업로드하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                uploadButton_Click(null, EventArgs.Empty);
                return;
            }
            else if (string.IsNullOrEmpty(saveDirectory))
            {
                MessageBox.Show("저장 경로가 설정되지 않았습니다. 저장 경로를 지정하세요.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                // 프로그래스바를 사용하지 않음
                progressBar.Visible = false;
            }
            else
            {
                // ProgressBar 초기화
                progressBar.Value = 0;
                progressBar.Visible = true;
            }

            // 비동기 저장 작업 시작
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
                string entryExit = entryRadioButton.Checked ? "입실" : "퇴실";

                string filename = $"{date}_{entryExit}_{ocrText}{extension}";
                filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(saveDirectory, filename);
                //MessageBox.Show($"최종 저장 경로: {savePath}", "경로 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);


                //MessageBox.Show($"저장 경로: {savePath}", "저장 경로 확인", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (File.Exists(savePath))
                {
                    Invoke(new Action(() =>
                    {
                        MessageBox.Show("파일이 이미 존재합니다. 다른 이름으로 저장하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }));
                    return;
                }

                if (finalImage != null)
                {
                    if (!checkBox1.Checked)
                    {
                        for (int i = 0; i <= 100; i += 25) // 진행률 업데이트 (4단계로 시뮬레이션)
                        {
                            await Task.Delay(500); // 저장 시뮬레이션을 위한 지연

                            // 진행 상황 업데이트
                            Invoke(new Action(() =>
                            {
                                progressBar.Value = i;
                            }));
                        }
                    }

                    finalImage.Save(savePath, imageFormat);

                    // 저장 완료 후 ProgressBar 숨기기
                    Invoke(new Action(() =>
                    {
                        if (!checkBox1.Checked)
                        {
                            progressBar.Value = 100;
                            progressBar.Visible = false;
                        }

                        if (!hideConfirmationCheckBox.Checked)
                        {
                            MessageBox.Show($"이미지가 {savePath}에 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }));
                }
            }
            else
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show("업로드된 파일 경로가 유효하지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int progress = 0;
            string savePath = SaveImage(ref progress);

            // 진행 상황 업데이트 (저장 완료)
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
                string entryExit = entryRadioButton.Checked ? "입실" : "퇴실";

                string filename = $"{date}_{entryExit}_{ocrText}{extension}";
                filename = string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
                string savePath = Path.Combine(saveDirectory, filename);

                if (File.Exists(savePath))
                {
                    throw new Exception("파일이 이미 존재합니다. 다른 이름으로 저장하세요.");
                }

                if (finalImage != null)
                {
                    // 이미지 저장
                    for (int i = 0; i <= 100; i += 25) // 4단계로 진행률 업데이트
                    {
                        Task.Delay(500).Wait(); // 작업 시뮬레이션을 위한 지연
                        backgroundWorker.ReportProgress(i);
                    }

                    finalImage.Save(savePath, imageFormat);
                }

                return savePath;
            }

            throw new Exception("업로드된 파일 경로가 유효하지 않습니다.");
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
                MessageBox.Show($"저장 중 오류가 발생했습니다: {e.Error.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("이미지가 성공적으로 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            // ocrTextBox가 null인지 확인
            if (ocrTextBox != null)
            {
                if (checkBox1.Checked)
                {
                    // Disable OCR functionality
                    ocrTextBox.Enabled = false;
                    progressBar.Visible = false; // 프로그래스바도 숨김
                }
                else
                {
                    // Enable OCR functionality
                    ocrTextBox.Enabled = true;
                    progressBar.Visible = true; // 프로그래스바 표시
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 현재 실행 중인 프로그램의 경로를 가져옵니다.
            string applicationPath = Application.ExecutablePath;

            // 새 인스턴스를 시작하기 위한 프로세스 시작
            System.Diagnostics.Process.Start(applicationPath);

            // 현재 인스턴스를 종료
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

        // DragDrop 이벤트 핸들러
        private void panel_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 파일이 드롭된 경우 처리
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
                        // 이미지 파일을 업로드
                        originalImage = Image.FromFile(filePath);
                        uploadedFilePath = filePath;
                        DisplayImage(originalImage);
                        isImageUploaded = true;
                    }
                }
            }
            else if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                // 이미지가 직접 드롭된 경우 처리
                originalImage = (Image)e.Data.GetData(DataFormats.Bitmap);
                DisplayImage(originalImage);
                isImageUploaded = true;
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                // 클립보드에 이미지가 있는지 확인하고 가져오기
                if (Clipboard.ContainsImage())
                {
                    originalImage = Clipboard.GetImage();
                    DisplayImage(originalImage);
                    isImageUploaded = true;

                    // 클립보드에서 이미지를 가져온 경우 기본 파일 경로와 파일 이름을 설정
                    string defaultFileName = $"clipboard_image_{DateTime.Now:yyyyMMdd_HHmmss}.png"; // 기본 파일 이름
                    uploadedFilePath = Path.Combine(saveDirectory, defaultFileName); // 저장 경로에 파일명 결합
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
            this.Text = "이름을 입력하세요.";
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
