using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace stringprocess
{
    

    internal class Program
    {
        public static List<(int start, int end)> GetBracketContentRanges(List<string> lists)
        {
            List<(int, int)> result = new List<(int, int)>();
            Stack<int> stack = new Stack<int>();

            for (int i = 0; i < lists.Count; i++)
            {
                string s = lists[i].Trim();

                if (s == "(")
                {
                    stack.Push(i);
                }
                else if (s == ")")
                {
                    if (stack.Count == 0)
                        return new List<(int, int)>();   // 多余右括号

                    int start = stack.Pop();
                    int contentStart = start + 1;
                    int contentEnd = i - 1;

                    result.Add((contentStart, contentEnd));
                }
            }

            if (stack.Count > 0)
                return new List<(int, int)>();           // 多余左括号

            return result;
        }

        /**************************************************
         ** 功能： 获取自定义括号标记中的内容区间
         **       返回每一对括号的完整内容范围（full range）
         **       以及去除所有子括号后的最外层内容区间（flat range）
         **
         ** 输入：
         **   lists      —— 字符串列表，用于解析的序列
         **   openToken  —— 表示左括号的标记（任意字符串）
         **   closeToken —— 表示右括号的标记（任意字符串）
         **
         ** 输出：
         **   List<(int fullStart, int fullEnd, int flatStart, int flatEnd)>
         **     fullStart/fullEnd —— 每个括号对内完整内容的起止索引
         **     flatStart/flatEnd —— 去除子括号后的最外层内容的起止索引
         **     若括号不匹配（多余左括号/右括号）则返回空列表
         **
         ** 说明：
         **   1. 支持任意嵌套深度，例如 A ( B ( C ) D ) 结构
         **   2. fullRange=去掉括号标记后的完整区间，包含子括号
         **   3. flatRange=过滤所有子括号后剩余的直接内容所在区间
         **      例如：( A ( B C ) D ) 的 flatRange 为 [A D]
         **   4. 返回顺序为从内层到外层（根据括号匹配顺序）
         **
         ** 版本：1.0.0
         ** 日期：2025-11-24
         **************************************************/

        public static List<(int fullStart, int fullEnd, int flatStart, int flatEnd)> GetBracketContentRanges(List<string> lists, string openToken, string closeToken)
        {
            var result = new List<(int, int, int, int)>();
            var stack = new Stack<int>();
            var pairs = new List<(int open, int close)>();

            // 1) 先匹配所有括号对
            for (int i = 0; i < lists.Count; i++)
            {
                var s = lists[i];
                if (s == openToken)
                    stack.Push(i);
                else if (s == closeToken)
                {
                    if (stack.Count == 0)
                        return new List<(int, int, int, int)>(); // 不匹配

                    int open = stack.Pop();
                    pairs.Add((open, i));
                }
            }

            if (stack.Count > 0)
                return new List<(int, int, int, int)>(); // 不匹配

            // 2) 按 open 位置排序（内层括号先处理）
            pairs.Sort((a, b) => a.open.CompareTo(b.open));

            // 3) 对每个括号区间计算 flat 区间（去掉子括号）
            foreach (var p in pairs)
            {
                int fullStart = p.open + 1;
                int fullEnd = p.close - 1;

                if (fullStart > fullEnd)
                {
                    result.Add((fullStart, fullEnd, -1, -1));
                    continue;
                }

                // flat: 去掉所有子区间后，只保留最外层 token
                int flatStart = -1;
                int flatEnd = -1;

                for (int i = fullStart; i <= fullEnd; i++)
                {
                    // 判断 i 是否落在某个子括号区间内，如果是则跳过
                    bool isInsideChild = false;

                    foreach (var child in pairs)
                    {
                        if (child.open > p.open && child.close < p.close)
                        {
                            if (i >= child.open && i <= child.close)
                            {
                                isInsideChild = true;
                                break;
                            }
                        }
                    }

                    if (!isInsideChild)
                    {
                        if (flatStart == -1) flatStart = i;
                        flatEnd = i;
                    }
                }

                result.Add((fullStart, fullEnd, flatStart, flatEnd));
            }

            return result;
        }


        /// <summary>
        /// 获取括号内的内容索引列表（支持嵌套）
        /// 例如： ( a b c d ( t y ) i )
        /// 返回：
        ///   [1,2,3,4,9]   // 外层括号内容（排除子括号）
        ///   [7,8]         // 内层括号内容
        /// </summary>
        public static List<List<int>> GetBracketContentIndex(
            List<string> lists,
            string openToken = "(",
            string closeToken = ")")
        {
            var stack = new Stack<int>();
            var pairs = new List<(int open, int close)>();

            // --- 第一步：先获得所有括号对 ---
            for (int i = 0; i < lists.Count; i++)
            {
                if (lists[i] == openToken)
                    stack.Push(i);
                else if (lists[i] == closeToken)
                {
                    if (stack.Count == 0) return new List<List<int>>();
                    int open = stack.Pop();
                    pairs.Add((open, i));
                }
            }
            if (stack.Count > 0) return new List<List<int>>();

            // 按 open 从小到大排序（外层 → 内层）
            pairs.Sort((a, b) => a.open.CompareTo(b.open));

            var result = new List<List<int>>();

            // --- 第二步：对每个括号区间提取内容（去掉子括号） ---
            foreach (var p in pairs)
            {
                int start = p.open + 1;
                int end = p.close - 1;

                var idx = new List<int>();

                for (int i = start; i <= end; i++)
                {
                    bool insideChild = false;

                    // 判断是否属于子括号区间
                    foreach (var child in pairs)
                    {
                        if (child.open > p.open && child.close < p.close)
                        {
                            if (i >= child.open && i <= child.close)
                            {
                                insideChild = true;
                                break;
                            }
                        }
                    }

                    if (!insideChild)
                    {
                        idx.Add(i);
                    }
                }

                result.Add(idx);
            }

            return result;
        }

        static void Main(string[] args)
        {
            List<string> input = new List<string>()
            {
                "(", "A", "(", "B", "C", ")", "D", ")"
            };
            var ranges = GetBracketContentRanges(input);


        }
    }
}
