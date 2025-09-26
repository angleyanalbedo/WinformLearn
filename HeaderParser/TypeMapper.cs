using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace HeaderParser
{
    using System.IO;
    using System.Text.RegularExpressions;
    // —— 可扩展类型映射器 ——
    // 支持：1) 指针统一映射 PointerType（默认 LONG）
    //      2) 一组“正则模式 → 目标类型”的规则（按顺序匹配，先中先用）
    //      3) 归一化输入类型（去 const/volatile、折叠空格、小写比对）
    //
    // 规则文件（示例见下）每行格式： <regex> => <TargetType>
    // 另外可配置指针类型： pointer = LONG
    // 支持注释与空行：以 # 开头行忽略
    using System.Xml;

    public class MappingRule
    {
        public string Pattern;
        public string Target;
        public Regex Compiled;

        public MappingRule(string pattern, string target)
        {
            Pattern = pattern;
            Target = target;
            Compiled = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
    }

    public class TypeMapper
    {
        public string PointerType = "LONG";
        public List<MappingRule> Rules = new List<MappingRule>();

        // —— 归一化（与之前 MapToTargetType 里的 Normalize 逻辑一致） ——
        private static string NormalizeCType(string ctype)
        {
            string t = ctype == null ? "" : ctype.Trim();
            t = Regex.Replace(t, @"\bconst\b|\bvolatile\b", "", RegexOptions.IgnoreCase);
            t = Regex.Replace(t, @"\s+", " ");
            t = t.Trim().ToLowerInvariant();
            return t;
        }

        public string MapType(string ctype, int pointerLevel)
        {
            if (pointerLevel > 0) return PointerType;

            string norm = NormalizeCType(ctype);

            // 顺序匹配：命中即返回
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].Compiled.IsMatch(norm))
                    return Rules[i].Target;
            }

            // 未命中：原样返回（支持 typedef 名等）
            return ctype != null ? ctype.Trim() : "";
        }

        // —— 从文本规则加载（推荐） ——
        // 规则文件示例（UTF-8）：
        // # 指针统一映射
        // pointer = LONG
        // # 16位无符号
        // ^(unsigned\s+short|uint16_t)$ => UINT
        // # 32位无符号
        // ^(unsigned\s+int|unsigned\s+long|uint32_t|dword)$ => UDINT
        // # 可选补充：
        // ^(short|int16_t)$ => INT
        // ^(int|long|int32_t)$ => DINT
        // ^(long\s+long|int64_t)$ => LINT
        // ^(unsigned\s+long\s+long|uint64_t)$ => ULINT
        // ^(signed\s+char|char|int8_t)$ => SINT
        // ^(unsigned\s+char|uint8_t|byte)$ => USINT
        // ^(_?bool|bool)$ => BOOL
        // ^float$ => REAL
        // ^double$ => LREAL
        public static TypeMapper LoadFromTextFile(string path)
        {
            TypeMapper m = new TypeMapper();
            if (!File.Exists(path))
            {
                // 文件不存在：加载默认规则
                LoadDefault(m);
                return m;
            }

            using (var sr = new StreamReader(path, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string s = line.Trim();
                    if (s.Length == 0) continue;
                    if (s.StartsWith("#")) continue;

                    // pointer = LONG
                    Match mp = Regex.Match(s, @"^pointer\s*=\s*(\S+)\s*$", RegexOptions.IgnoreCase);
                    if (mp.Success)
                    {
                        m.PointerType = mp.Groups[1].Value.Trim();
                        continue;
                    }

                    // <regex> => <TargetType>
                    int arrow = s.IndexOf("=>");
                    if (arrow > 0)
                    {
                        string pattern = s.Substring(0, arrow).Trim();
                        string target = s.Substring(arrow + 2).Trim();
                        if (pattern.Length > 0 && target.Length > 0)
                            m.Rules.Add(new MappingRule(pattern, target));
                    }
                }
            }

            // 若未配任何规则，降级到默认
            if (m.Rules.Count == 0) LoadDefault(m);
            if (string.IsNullOrEmpty(m.PointerType)) m.PointerType = "LONG";
            return m;
        }

        // —— 也提供 XML 规则加载（可选）——
        // 格式：
        // <TypeMapping pointer="LONG">
        //   <Rule pattern="^(unsigned\s+short|uint16_t)$" target="UINT"/>
        //   <Rule pattern="^(unsigned\s+int|unsigned\s+long|uint32_t|dword)$" target="UDINT"/>
        //   ...
        // </TypeMapping>
        public static TypeMapper LoadFromXml(string path)
        {
            TypeMapper m = new TypeMapper();
            if (!File.Exists(path))
            {
                LoadDefault(m);
                return m;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            XmlElement root = doc.DocumentElement;
            if (root != null && root.Name == "TypeMapping")
            {
                string ptr = root.GetAttribute("pointer");
                if (!string.IsNullOrEmpty(ptr)) m.PointerType = ptr;

                XmlNodeList nodes = root.SelectNodes("Rule");
                foreach (XmlNode n in nodes)
                {
                    XmlElement e = n as XmlElement;
                    if (e == null) continue;
                    string pattern = e.GetAttribute("pattern");
                    string target = e.GetAttribute("target");
                    if (!string.IsNullOrEmpty(pattern) && !string.IsNullOrEmpty(target))
                        m.Rules.Add(new MappingRule(pattern, target));
                }
            }

            if (m.Rules.Count == 0) LoadDefault(m);
            if (string.IsNullOrEmpty(m.PointerType)) m.PointerType = "LONG";
            return m;
        }

        // —— 默认规则（与你前面需求一致） ——
        private static void LoadDefault(TypeMapper m)
        {
            m.PointerType = "LONG"; // 指针统一映射 LONG

            m.Rules.Add(new MappingRule(@"^(unsigned\s+short|uint16_t)$", "UINT"));
            m.Rules.Add(new MappingRule(@"^(unsigned\s+int|unsigned\s+long|uint32_t|dword)$", "UDINT"));

            // 可选常见补充（需要就留，不要就删）
            m.Rules.Add(new MappingRule(@"^(short|int16_t)$", "INT"));
            m.Rules.Add(new MappingRule(@"^(int|long|int32_t)$", "DINT")); // Win32 下 long 为32位
            m.Rules.Add(new MappingRule(@"^(long\s+long|int64_t)$", "LINT"));
            m.Rules.Add(new MappingRule(@"^(unsigned\s+long\s+long|uint64_t)$", "ULINT"));
            m.Rules.Add(new MappingRule(@"^(signed\s+char|char|int8_t)$", "SINT"));
            m.Rules.Add(new MappingRule(@"^(unsigned\s+char|uint8_t|byte)$", "USINT"));
            m.Rules.Add(new MappingRule(@"^(_?bool|bool)$", "BOOL"));
            m.Rules.Add(new MappingRule(@"^float$", "REAL"));
            m.Rules.Add(new MappingRule(@"^double$", "LREAL"));
        }
    }

}
