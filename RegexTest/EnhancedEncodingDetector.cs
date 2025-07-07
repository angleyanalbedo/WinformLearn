using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegexTest
{

    public class EnhancedEncodingDetector
    {
        public static Encoding DetectFileEncoding(string filePath)
        {
            // 默认返回UTF-8（无BOM）
            Encoding defaultEncoding = new UTF8Encoding(false);

            try
            {
                // 读取文件前4个字节（检测BOM）
                byte[] bomBuffer = new byte[4];
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    file.Read(bomBuffer, 0, 4);
                }

                // 检查BOM（字节顺序标记）
                if (bomBuffer.Length >= 3 && bomBuffer[0] == 0xEF && bomBuffer[1] == 0xBB && bomBuffer[2] == 0xBF)
                {
                    return Encoding.UTF8; // UTF-8 with BOM
                }
                else if (bomBuffer.Length >= 2 && bomBuffer[0] == 0xFE && bomBuffer[1] == 0xFF)
                {
                    return Encoding.BigEndianUnicode; // UTF-16 BE
                }
                else if (bomBuffer.Length >= 2 && bomBuffer[0] == 0xFF && bomBuffer[1] == 0xFE)
                {
                    // 可能是UTF-16 LE或UTF-32 LE
                    if (bomBuffer.Length >= 4 && bomBuffer[2] == 0 && bomBuffer[3] == 0)
                    {
                        return Encoding.UTF32; // UTF-32 LE
                    }
                    return Encoding.Unicode; // UTF-16 LE
                }
                else if (bomBuffer.Length >= 4 && bomBuffer[0] == 0 && bomBuffer[1] == 0 &&
                         bomBuffer[2] == 0xFE && bomBuffer[3] == 0xFF)
                {
                    return new UTF32Encoding(true, true); // UTF-32 BE
                }

                // 如果没有BOM，进行更深入的编码分析
                return DetectEncodingWithoutBom(filePath) ?? defaultEncoding;
            }
            catch
            {
                return defaultEncoding;
            }
        }

        private static Encoding DetectEncodingWithoutBom(string filePath)
        {
            // 读取文件内容进行分析（前4096字节）
            byte[] buffer = new byte[4096];
            int bytesRead;

            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                bytesRead = file.Read(buffer, 0, buffer.Length);
            }

            // 1. 检查UTF-8
            if (IsLikelyUtf8(buffer, bytesRead))
            {
                return new UTF8Encoding(false);
            }

            // 2. 检查UTF-16 LE/BE
            var utf16Encoding = CheckUtf16Encoding(buffer, bytesRead);
            if (utf16Encoding != null)
            {
                return utf16Encoding;
            }

            // 3. 检查GBK/GB18030
            if (IsLikelyGbEncoding(buffer, bytesRead))
            {
                // 优先尝试GB18030（最新标准，包含所有汉字）
                try
                {
                    Encoding gb18030 = Encoding.GetEncoding("GB18030");
                    string testString = gb18030.GetString(buffer, 0, bytesRead);

                    // 检查是否有替换字符和有效中文比例
                    if (!testString.Contains('�') && HasReasonableChineseRatio(testString))
                    {
                        return gb18030;
                    }
                }
                catch { }

                // 回退到GBK（兼容性更好）
                try
                {
                    Encoding gbk = Encoding.GetEncoding("GBK");
                    string testString = gbk.GetString(buffer, 0, bytesRead);

                    if (!testString.Contains('�') && HasReasonableChineseRatio(testString))
                    {
                        return gbk;
                    }
                }
                catch { }

                // 如果都不理想，返回系统默认ANSI编码
                return Encoding.Default;
            }

            // 4. 默认返回系统ANSI编码
            return Encoding.Default;
        }
        // 辅助方法：检查字符串中合理的中文字符比例
        private static bool HasReasonableChineseRatio(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            int chineseCount = 0;
            int totalCount = 0;

            foreach (char c in text)
            {
                // 简单中文字符范围判断（CJK统一汉字）
                if (c >= 0x4E00 && c <= 0x9FFF)
                {
                    chineseCount++;
                }
                // 扩展汉字范围判断
                else if (c >= 0x3400 && c <= 0x4DBF) // 扩展A
                {
                    chineseCount++;
                }
                // 可以添加更多中文相关字符范围

                // 只统计非控制字符
                if (!char.IsControl(c)) totalCount++;
            }

            // 如果有至少10%的字符是中文，认为合理
            return totalCount > 0 && (chineseCount * 10 >= totalCount);
        }
        private static bool IsLikelyUtf8(byte[] buffer, int length)
        {
            int utf8Score = 0;
            int asciiScore = 0;

            for (int i = 0; i < length; i++)
            {
                byte b = buffer[i];

                // ASCII字符（0-127）
                if (b <= 0x7F)
                {
                    asciiScore++;
                    continue;
                }

                // 检查UTF-8多字节序列
                int followingBytes = 0;
                if ((b & 0xE0) == 0xC0) followingBytes = 1; // 2字节序列
                else if ((b & 0xF0) == 0xE0) followingBytes = 2; // 3字节序列
                else if ((b & 0xF8) == 0xF0) followingBytes = 3; // 4字节序列
                else return false; // 无效的UTF-8起始字节

                // 检查后续字节是否都是10xxxxxx
                for (int j = 1; j <= followingBytes; j++)
                {
                    if (i + j >= length) return false;
                    if ((buffer[i + j] & 0xC0) != 0x80) return false;
                }

                utf8Score += followingBytes + 1;
                i += followingBytes;
            }

            // 如果有足够多的UTF-8多字节序列，则认为是UTF-8
            return utf8Score > 0 && (utf8Score > asciiScore / 10);
        }

        private static Encoding CheckUtf16Encoding(byte[] buffer, int length)
        {
            if (length < 2) return null;

            int evenNulls = 0; // 偶数位置为0的计数
            int oddNulls = 0;  // 奇数位置为0的计数
            int textChars = 0; // 可打印字符计数

            for (int i = 0; i < length - 1; i += 2)
            {
                byte b1 = buffer[i];
                byte b2 = buffer[i + 1];

                // 统计null字节
                if (b1 == 0) oddNulls++;
                if (b2 == 0) evenNulls++;

                // 检查是否可打印字符（ASCII范围）
                if ((b1 >= 0x20 && b1 <= 0x7E && b2 == 0) ||
                    (b2 >= 0x20 && b2 <= 0x7E && b1 == 0))
                {
                    textChars++;
                }
            }

            // 判断UTF-16 BE或LE
            if (textChars > 0)
            {
                if (evenNulls > oddNulls * 10) return Encoding.BigEndianUnicode;
                if (oddNulls > evenNulls * 10) return Encoding.Unicode;
            }

            return null;
        }

        // 改进的GB编码检测方法
        private static bool IsLikelyGbEncoding(byte[] buffer, int length)
        {
            int gbCharCount = 0;
            int totalChecked = 0;
            int consecutiveGb = 0;
            int maxConsecutiveGb = 0;

            for (int i = 0; i < length - 1; i++)
            {
                byte b1 = buffer[i];
                byte b2 = buffer[i + 1];

                // GBK/GB18030双字节字符范围
                bool isGbChar = (b1 >= 0x81 && b1 <= 0xFE) &&
                               (b2 >= 0x40 && b2 <= 0xFE && b2 != 0x7F);

                if (isGbChar)
                {
                    gbCharCount++;
                    consecutiveGb++;
                    maxConsecutiveGb = Math.Max(maxConsecutiveGb, consecutiveGb);
                    i++; // 跳过第二个字节
                    totalChecked += 2;
                }
                else if (b1 <= 0x7F) // ASCII字符
                {
                    consecutiveGb = 0;
                    totalChecked++;
                }
                else // 无效字节
                {
                    consecutiveGb = 0;
                    totalChecked += 2;
                    i++;
                }
            }

            // 判断条件：
            // 1. 检测到足够多的GB字符
            // 2. 有连续的GB字符序列（减少误判）
            return totalChecked > 0 &&
                   gbCharCount > 3 &&
                   maxConsecutiveGb >= 2 &&
                   (gbCharCount * 10 >= totalChecked / 2);
        }
    }
}
