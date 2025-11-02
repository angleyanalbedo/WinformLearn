using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using System.IO;
using System.Collections.Generic;

public class SMBFileCopy
{
    public static void CopyFilesFromSMB(string smbPath, string destinationPath, List<string> excludePatterns, Action<int> progressCallback)
    {
        // 检查SMB路径和目标路径是否有效
        if (!Directory.Exists(smbPath))
        {
            throw new DirectoryNotFoundException("SMB path does not exist.");
        }

        if (!Directory.Exists(destinationPath))
        {
            Directory.CreateDirectory(destinationPath);
        }

        // 获取SMB路径下的所有文件和文件夹
        string[] files = Directory.GetFiles(smbPath, "*.*", SearchOption.AllDirectories);
        string[] directories = Directory.GetDirectories(smbPath, "*.*", SearchOption.AllDirectories);

        int totalFiles = files.Length;
        int filesCopied = 0;

        // 复制文件
        foreach (string file in files)
        {
            // 检查是否需要排除
            if (ShouldExclude(file, excludePatterns))
            {
                continue;
            }

            string relativePath = file.Substring(smbPath.Length + 1); // 获取相对路径
            string destinationFilePath = Path.Combine(destinationPath, relativePath);

            // 确保目标文件夹存在
            string destinationDirectory = Path.GetDirectoryName(destinationFilePath);
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // 复制文件
            File.Copy(file, destinationFilePath, true);

            // 更新进度
            filesCopied++;
            progressCallback((int)((double)filesCopied / totalFiles * 100));
        }

        // 复制文件夹结构（如果需要）
        foreach (string directory in directories)
        {
            // 检查是否需要排除
            if (ShouldExclude(directory, excludePatterns))
            {
                continue;
            }

            string relativePath = directory.Substring(smbPath.Length + 1); // 获取相对路径
            string destinationDirectoryPath = Path.Combine(destinationPath, relativePath);

            // 创建目标文件夹
            Directory.CreateDirectory(destinationDirectoryPath);
        }
    }

    private static bool ShouldExclude(string path, List<string> excludePatterns)
    {
        foreach (string pattern in excludePatterns)
        {
            if (path.Contains(pattern))
            {
                return true;
            }
        }
        return false;
    }

}
