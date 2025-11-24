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

        static void Main(string[] args)
        {
        }
    }
}
