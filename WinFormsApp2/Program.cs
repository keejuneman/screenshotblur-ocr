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
            // ���ø����̼� ���� �ʱ�ȭ
            ApplicationConfiguration.Initialize();

            // Form1�� WinFormsApp2 ���ӽ����̽��� ������ Ȯ��
            Application.Run(new Form1());
        }
    }
}
