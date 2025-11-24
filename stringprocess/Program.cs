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
        public static List<List<int>> GetBraketRange(List<string> lists)
        {
            List<List<int>> res = new List<List<int>>();
            int deep = -1;
            foreach (string str in lists)
            {
                if (str.Trim().Equals("("))
                {
                    deep++;
                    res.Add(new List<int>());
                    res[deep].Add(lists.IndexOf(str));
                }
                else if (str.Trim().Equals(")") && deep == -1)
                {
                    return new List<List<int>>();
                }
                else if (str.Trim().Equals(")") && deep > -1)
                {
                    res[deep].Add(lists.IndexOf(str));
                    deep--;

                }
                else if (deep > -1)
                {
                    res[deep].Add(lists.IndexOf(str));
                }
            }
            if (deep > -1)
            {
                return new List<List<int>>();
            }
            return res;
        }
        static void Main(string[] args)
        {
        }
    }
}
