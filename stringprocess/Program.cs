using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace stringprocess
{
    public static List<List<int>> GetBraketRange(List<string> lists)
    {
        Stack<List<int>> stack = new Stack<List<int>>();
        Dictionary<int,List<int>> dict = new Dictionary<int,List<int>>();
        for(var line in lists)
        {
            if (line.Equals("("))
            {
                stack.Push(new List<int>());
            }
            
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
