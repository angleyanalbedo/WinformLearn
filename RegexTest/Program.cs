using System.Text;
using System.Text.RegularExpressions;

namespace RegexTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            string filePath = "chinese_text.txt";
            Encoding detectedEncoding = EnhancedEncodingDetector.DetectFileEncoding(filePath);

            Console.WriteLine($"检测到的编码: {detectedEncoding.EncodingName}");
            Console.WriteLine($"Web名称: {detectedEncoding.WebName}");
            Console.WriteLine($"代码页: {detectedEncoding.CodePage}");

            // 使用检测到的编码读取文件
            string content = File.ReadAllText(filePath, detectedEncoding);
            Console.WriteLine(content);
            string[] lines = new string[]
        {
            "#define MAX_VALUE 100",
            "#define MIN_VALUE (50)",
            "#define DEBUG // This is a debug macro",
            "int x = 10; // This is not a macro",
            "// This is a comment",
            "#define TEST // This is a test macro"
        };

            string pattern = @"^\s*#define\s+([_a-zA-Z][_a-zA-Z0-9]*)\s*(.*)?\s*(?://.*)?$";

            foreach (string line in lines)
            {
                Match match = Regex.Match(line, pattern);
                if (match.Success)
                {
                    string macroName = match.Groups[1].Value;
                    string macroValue = match.Groups[2].Success ? match.Groups[2].Value : "N/A";
                    string comment = match.Groups[3].Success ? match.Groups[3].Value : "No comment";

                    Console.WriteLine($"Macro Name: {macroName}, Macro Value: {macroValue}, Comment: {comment}");
                }
                else
                {
                    Console.WriteLine("Not a macro definition or comment.");
                }
            }
        }
    }
}
