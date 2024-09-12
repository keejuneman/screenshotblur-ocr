using ImageBlurApp;

namespace WinFormsApp2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 애플리케이션 구성 초기화
            ApplicationConfiguration.Initialize();

            // Form1이 WinFormsApp2 네임스페이스에 속한지 확인
            Application.Run(new Form1());
        }
    }
}
