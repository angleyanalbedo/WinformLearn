using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMBCopy
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length <= 2)
            {
                Console.WriteLine("Usage: Program <sourcePath> <destinationPath> <excludePatterns>");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());

            }
            else
            {
                RunCommandLineMode(args);
            }
            
        }

        private static void RunCommandLineMode(string[] args)
        {
            string srcdir = args[0];
            string dstdir = args[1];
            var exclude = args[2].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            try
            {
                // 调用文件复制方法
                SMBFileCopy.CopyFilesFromSMB(srcdir, dstdir, exclude, progress =>
                {
                    DisplayProgressBar(progress);
                });

                Console.WriteLine("文件复制完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误: " + ex.Message);
            }
        }
        private static void DisplayProgressBar(int progress)
        {
            const int progressBarWidth = 50; // 进度条宽度
            int progressBars = (int)(progress / 100.0 * progressBarWidth);

            // 清除上一行
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);

            // 绘制进度条
            Console.Write($"Progress: [{new string('#', progressBars)}{new string('-', progressBarWidth - progressBars)}] {progress}%");
        }
    }
}
