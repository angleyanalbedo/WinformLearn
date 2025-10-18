// Program.cs  (.NET Framework 4.5)
// 用法：HeaderParse45.exe <header.h> [out.xml]
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HeaderParser
{
    // ======== 数据模型 ========
    public class CField
    {
        public string CType;
        public string Name;
        public int PointerLevel;
        public string ArraySuffix; // 可能为 null/空

        public CField(string ctype, string name, int pointerLevel, string arraySuffix)
        {
            this.CType = ctype;
            this.Name = name;
            this.PointerLevel = pointerLevel;
            this.ArraySuffix = arraySuffix;
        }
    }

    public class CStruct
    {
        public string Name;
        public List<CField> Fields;

        public CStruct(string name, List<CField> fields)
        {
            this.Name = name;
            this.Fields = fields;
        }
    }

    public class CEnumItem
    {
        public string Name;
        public string RawValue;   // 可能为 null
        public long? ParsedValue; // 可能为 null

        public CEnumItem(string name, string rawValue, long? parsedValue)
        {
            this.Name = name;
            this.RawValue = rawValue;
            this.ParsedValue = parsedValue;
        }
    }

    public class CEnum
    {
        public string Name;
        public List<CEnumItem> Items;

        public CEnum(string name, List<CEnumItem> items)
        {
            this.Name = name;
            this.Items = items;
        }
    }

    public class CDefine
    {
        public string Name;
        public string Value; // 原始右值（不含注释）
        public CDefine(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    public class ParseResult
    {
        public List<CStruct> Structs;
        public List<CEnum> Enums;
        public List<CDefine> Defines;

        public ParseResult(List<CStruct> structs, List<CEnum> enums, List<CDefine> defines)
        {
            this.Structs = structs;
            this.Enums = enums;
            this.Defines = defines;
        }
    }

    // ======== 解析器（兼容 .NET 4.5） ========
    public static class CHeaderParser
    {
        // struct
        private static readonly Regex TypedefStructRegex = new Regex(
            @"typedef\s+struct\s*(?<tag>\w+)?\s*\{(?<body>.*?)\}\s*(?<name>\w+)\s*;",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex NamedStructRegex = new Regex(
            @"struct\s+(?<name>\w+)\s*\{(?<body>.*?)\}\s*;",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // enum
        private static readonly Regex TypedefEnumRegex = new Regex(
            @"typedef\s+enum\s*(?<tag>\w+)?\s*\{(?<body>.*?)\}\s*(?<name>\w+)\s*;",
            RegexOptions.Singleline | RegexOptions.Compiled);

        private static readonly Regex NamedEnumRegex = new Regex(
            @"enum\s+(?<name>\w+)\s*\{(?<body>.*?)\}\s*;",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // 字段切分：声明到分号；避免跨花括号
        private static readonly Regex FieldLineRegex = new Regex(
            @"(?<line>[^;{}]+;)",
            RegexOptions.Singleline | RegexOptions.Compiled);

        // 简单 define
        private static readonly Regex SimpleDefineRegex = new Regex(
            @"^[ \t]*#\s*define\s+(?<name>[A-Za-z_]\w*)\s+(?<value>.+?)\s*$",
            RegexOptions.Compiled);

        private static readonly Regex FuncLikeDefineRegex = new Regex(
            @"^[ \t]*#\s*define\s+[A-Za-z_]\w*\s*\(",
            RegexOptions.Compiled);

        public static ParseResult ParseFile(string path)
        {
            string code = File.ReadAllText(path);
            return Parse(code);
        }

        public static ParseResult Parse(string rawCode)
        {
            // 1) 去注释（防止注释里被替换）
            string codeNoComments = StripComments(rawCode);

            // 2) 抓取简单 define（逐行，跳过函数式宏）
            List<CDefine> defines = ExtractSimpleDefines(codeNoComments);

            // 3) 应用简单 define 到无注释代码上（词法级安全替换）
            string code = ApplySimpleDefines(codeNoComments, defines);

            // 4) 归一空白
            code = Regex.Replace(code, @"\s+", " ");

            List<CStruct> structs = new List<CStruct>();
            List<CEnum> enums = new List<CEnum>();

            // structs
            foreach (Match m in TypedefStructRegex.Matches(code))
            {
                string body = m.Groups["body"].Value;
                string name = m.Groups["name"].Value;
                structs.Add(new CStruct(name, ParseFields(body)));
            }
            foreach (Match m in NamedStructRegex.Matches(code))
            {
                string body = m.Groups["body"].Value;
                string name = m.Groups["name"].Value;
                if (!ExistsStruct(structs, name))
                {
                    structs.Add(new CStruct(name, ParseFields(body)));
                }
            }

            // enums
            foreach (Match m in TypedefEnumRegex.Matches(code))
            {
                string body = m.Groups["body"].Value;
                string name = m.Groups["name"].Value;
                enums.Add(ParseEnum(name, body));
            }
            foreach (Match m in NamedEnumRegex.Matches(code))
            {
                string body = m.Groups["body"].Value;
                string name = m.Groups["name"].Value;
                if (!ExistsEnum(enums, name))
                {
                    enums.Add(ParseEnum(name, body));
                }
            }

            return new ParseResult(structs, enums, defines);
        }

        private static bool ExistsStruct(List<CStruct> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Name == name) return true;
            return false;
        }

        private static bool ExistsEnum(List<CEnum> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].Name == name) return true;
            return false;
        }

        public static string StripComments(string code)
        {
            // /* ... */ 多行注释
            code = Regex.Replace(code, @"/\*.*?\*/", "", RegexOptions.Singleline);
            // // ... 行注释
            code = Regex.Replace(code, @"//.*?$", "", RegexOptions.Multiline);
            return code;
        }

        // -------- Struct 字段解析 --------
        private static List<CField> ParseFields(string body)
        {
            List<CField> fields = new List<CField>();

            foreach (Match m in FieldLineRegex.Matches(body))
            {
                string line = m.Groups["line"].Value.Trim();

                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.IndexOf(":") >= 0) continue; // 位域：跳过（可扩展）
                if (line.IndexOf("(") >= 0 || line.IndexOf(")") >= 0) continue; // 函数指针/原型：跳过
                if (line.IndexOf("{") >= 0 || line.IndexOf("}") >= 0) continue;

                line = TrimEndSemicolon(line);

                int idx = LastTypeTokenIndex(line);
                if (idx <= 0 || idx >= line.Length) continue;

                string typePart = line.Substring(0, idx).Trim();
                string declPart = line.Substring(idx).Trim();

                foreach (string decl in SplitTopLevelComma(declPart))
                {
                    string d = decl.Trim();
                    if (string.IsNullOrEmpty(d)) continue;

                    string name;
                    int pointerLevel;
                    string arraySuffix;
                    ParseDeclarator(d, out name, out pointerLevel, out arraySuffix);
                    if (string.IsNullOrEmpty(name)) continue;

                    fields.Add(new CField(typePart, name, pointerLevel, arraySuffix));
                }
            }

            return fields;
        }

        private static string TrimEndSemicolon(string s)
        {
            s = s.Trim();
            if (s.EndsWith(";")) s = s.Substring(0, s.Length - 1);
            return s.Trim();
        }

        private static int LastTypeTokenIndex(string line)
        {
            Match m = Regex.Match(line, @"^(?<type>(?:\w+\s+|\w+\s*\*\s*)+)(?<decl>.+)$");
            if (m.Success) return m.Groups["type"].Value.Length;

            int lastSpace = line.LastIndexOf(' ');
            return lastSpace > 0 ? lastSpace : -1;
        }

        private static List<string> SplitTopLevelComma(string s)
        {
            List<string> parts = new List<string>();
            StringBuilder sb = new StringBuilder();
            int depthParen = 0, depthBracket = 0, depthBrace = 0;

            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                if (ch == ',' && depthParen == 0 && depthBracket == 0 && depthBrace == 0)
                {
                    parts.Add(sb.ToString());
                    sb.Length = 0;
                }
                else
                {
                    if (ch == '(') depthParen++;
                    else if (ch == ')') depthParen = Math.Max(0, depthParen - 1);
                    else if (ch == '[') depthBracket++;
                    else if (ch == ']') depthBracket = Math.Max(0, depthBracket - 1);
                    else if (ch == '{') depthBrace++;
                    else if (ch == '}') depthBrace = Math.Max(0, depthBrace - 1);

                    sb.Append(ch);
                }
            }
            if (sb.Length > 0) parts.Add(sb.ToString());
            return parts;
        }

        private static void ParseDeclarator(string decl, out string name, out int pointerLevel, out string arraySuffix)
        {
            decl = decl.Trim();

            // 数组后缀（多维）
            Match arrayMatch = Regex.Match(decl, @"(\s*(\[[^\]]*\]\s*)+)$");
            arraySuffix = null;
            if (arrayMatch.Success)
            {
                arraySuffix = arrayMatch.Value.Trim();
                decl = decl.Substring(0, arrayMatch.Index).Trim();
            }

            // 前缀 *
            pointerLevel = 0;
            int i = 0;
            while (i < decl.Length && decl[i] == '*')
            {
                pointerLevel++;
                i++;
            }
            decl = decl.Substring(i).Trim();

            Match nameMatch = Regex.Match(decl, @"^[A-Za-z_]\w*$");
            if (nameMatch.Success) name = nameMatch.Value;
            else name = null;
        }

        // -------- Enum 解析 --------
        private static CEnum ParseEnum(string name, string body)
        {
            List<CEnumItem> items = new List<CEnumItem>();
            List<string> parts = SplitTopLevelComma(body);

            long autoValue = -1; // 无赋值时从 0 递增
            for (int i = 0; i < parts.Count; i++)
            {
                string raw = parts[i];
                string s = raw.Trim();
                if (string.IsNullOrEmpty(s)) continue;

                s = s.Trim().TrimEnd(';').Trim();
                if (string.IsNullOrEmpty(s)) continue;

                string ident;
                string val = null;

                int eq = s.IndexOf('=');
                if (eq >= 0)
                {
                    ident = s.Substring(0, eq).Trim();
                    val = s.Substring(eq + 1).Trim();
                }
                else
                {
                    ident = s;
                }

                if (!Regex.IsMatch(ident, @"^[A-Za-z_]\w*$")) continue;

                long? parsed = null;
                if (val != null)
                {
                    parsed = TryParseEnumValue(val);
                    if (parsed.HasValue) autoValue = parsed.Value;
                }
                else
                {
                    autoValue++;
                    parsed = autoValue;
                    val = null;
                }

                items.Add(new CEnumItem(ident, val, parsed));
            }

            return new CEnum(name, items);
        }

        private static long? TryParseEnumValue(string expr)
        {
            expr = expr.Trim();

            // 十六进制
            if (expr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                long v;
                if (long.TryParse(expr.Substring(2),
                    System.Globalization.NumberStyles.AllowHexSpecifier,
                    null, out v))
                    return v;
            }

            // 字符字面量：'A'、'\n'
            if (expr.Length >= 3 && expr[0] == '\'' && expr[expr.Length - 1] == '\'')
            {
                int? ch = ParseCCharLiteral(expr);
                if (ch.HasValue) return (long)ch.Value;
            }

            // 十进制（含正负号）
            long d;
            if (long.TryParse(expr, out d)) return d;

            // 简单一元 +/-
            if ((expr.StartsWith("+") || expr.StartsWith("-")) && long.TryParse(expr, out d)) return d;

            // 更复杂表达式（位运算、宏）返回 null，让 RawValue 保留
            return null;
        }

        private static int? ParseCCharLiteral(string s)
        {
            if (s.Length < 3 || s[0] != '\'' || s[s.Length - 1] != '\'') return null;
            string inner = s.Substring(1, s.Length - 2);
            if (inner.Length == 1) return (int)inner[0];
            if (inner.Length == 2 && inner[0] == '\\')
            {
                char c = inner[1];
                switch (c)
                {
                    case 'n': return (int)'\n';
                    case 'r': return (int)'\r';
                    case 't': return (int)'\t';
                    case '\\': return (int)'\\';
                    case '\'': return (int)'\'';
                    case '\"': return (int)'\"';
                    case '0': return (int)'\0';
                    default: return null;
                }
            }
            return null;
        }

        // -------- 简单 #define 处理 --------
        private static List<CDefine> ExtractSimpleDefines(string codeNoComments)
        {
            List<CDefine> list = new List<CDefine>();
            using (StringReader sr = new StringReader(codeNoComments))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string ln = line.TrimEnd();

                    // 跳过函数式宏：#define NAME(x ...
                    if (FuncLikeDefineRegex.IsMatch(ln))
                        continue;

                    Match m = SimpleDefineRegex.Match(ln);
                    if (!m.Success) continue;

                    string name = m.Groups["name"].Value;
                    string value = m.Groups["value"].Value.Trim();

                    // 行尾可能还有多余注释（之前 StripComments 已去掉 // 与 /* */，此处通常干净）
                    if (IsSimpleDefineValue(value))
                        list.Add(new CDefine(name, value));
                }
            }
            return list;
        }

        private static bool IsSimpleDefineValue(string v)
        {
            v = v.Trim();

            // 字符字面量
            if (v.Length >= 3 && v[0] == '\'' && v[v.Length - 1] == '\'') return true;

            long tmp;
            if (v.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return long.TryParse(v.Substring(2),
                    System.Globalization.NumberStyles.AllowHexSpecifier, null, out tmp);

            if (long.TryParse(v, out tmp)) return true;
            if ((v.StartsWith("+") || v.StartsWith("-")) && long.TryParse(v, out tmp)) return true;

            // 单个标识符（允许后续一层替换）
            if (Regex.IsMatch(v, @"^[A-Za-z_]\w*$")) return true;

            return false;
        }

        // 用简单 define 做词法级替换（按 \bNAME\b 边界替换；支持一层间接）
        private static string ApplySimpleDefines(string codeNoComments, List<CDefine> defines)
        {
            if (defines == null || defines.Count == 0) return codeNoComments;

            Dictionary<string, string> map = new Dictionary<string, string>();

            // 第一轮：可直接用作值（数字/字符）的宏
            for (int i = 0; i < defines.Count; i++)
            {
                string v = defines[i].Value.Trim();
                if (v.Length >= 3 && v[0] == '\'' && v[v.Length - 1] == '\'')
                    map[defines[i].Name] = v;
                else
                {
                    long d;
                    if (v.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    {
                        if (long.TryParse(v.Substring(2),
                            System.Globalization.NumberStyles.AllowHexSpecifier, null, out d))
                            map[defines[i].Name] = "0x" + v.Substring(2);
                    }
                    else if (long.TryParse(v, out d) ||
                            ((v.StartsWith("+") || v.StartsWith("-")) && long.TryParse(v, out d)))
                    {
                        map[defines[i].Name] = v;
                    }
                }
            }

            // 第二轮：标识符引用（只做一层）
            for (int i = 0; i < defines.Count; i++)
            {
                string v = defines[i].Value.Trim();
                if (Regex.IsMatch(v, @"^[A-Za-z_]\w*$") && map.ContainsKey(v))
                {
                    map[defines[i].Name] = map[v];
                }
            }

            // 逐个 \bNAME\b 替换（避免替换到标识符子串）
            foreach (var kv in map)
            {
                string pattern = @"\b" + Regex.Escape(kv.Key) + @"\b";
                codeNoComments = Regex.Replace(codeNoComments, pattern, kv.Value);
            }

            return codeNoComments;
        }
    }

    // ======== 程序入口 / XML 保存 ========
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("用法：HeaderParse45.exe <header.h> [out.xml]");
                return;
            }

            ParseResult res = CHeaderParser.ParseFile(args[0]);

            // 控制台打印（可按需保留/删除）
            for (int i = 0; i < res.Defines.Count; i++)
            {
                var d = res.Defines[i];
                Console.WriteLine("#define " + d.Name + " " + d.Value);
            }
            Console.WriteLine();

            for (int i = 0; i < res.Structs.Count; i++)
            {
                CStruct s = res.Structs[i];
                Console.WriteLine("struct " + s.Name + " {");
                for (int j = 0; j < s.Fields.Count; j++)
                {
                    CField f = s.Fields[j];
                    string ptr = new string('*', f.PointerLevel);
                    string arr = string.IsNullOrEmpty(f.ArraySuffix) ? "" : (" " + f.ArraySuffix);
                    Console.WriteLine("  " + f.CType + " " + ptr + f.Name + arr + ";");
                }
                Console.WriteLine("};");
                Console.WriteLine();
            }

            for (int i = 0; i < res.Enums.Count; i++)
            {
                CEnum e = res.Enums[i];
                Console.WriteLine("enum " + e.Name + " {");
                for (int k = 0; k < e.Items.Count; k++)
                {
                    CEnumItem it = e.Items[k];
                    string valStr = "";
                    if (it.RawValue != null)
                        valStr = " = " + it.RawValue;
                    else if (it.ParsedValue.HasValue)
                        valStr = " /* = " + it.ParsedValue.Value + " */";
                    Console.WriteLine("  " + it.Name + valStr + ",");
                }
                Console.WriteLine("};");
                Console.WriteLine();
            }

            // 如传了 xml 路径，则保存
            if (args.Length >= 2)
            {
                try
                {
                    SaveAsXml(args[1], res);
                    Console.WriteLine("XML 已保存到: " + args[1]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("保存 XML 失败: " + ex.Message);
                }
            }
        }

        private static void SaveAsXml(string xmlPath, ParseResult res)
        {
            using (var xw = System.Xml.XmlWriter.Create(xmlPath, new System.Xml.XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.UTF8
            }))
            {
                xw.WriteStartDocument();
                xw.WriteStartElement("Header");

                // Defines
                xw.WriteStartElement("Defines");
                if (res.Defines != null)
                {
                    for (int i = 0; i < res.Defines.Count; i++)
                    {
                        var d = res.Defines[i];
                        xw.WriteStartElement("Define");
                        xw.WriteAttributeString("name", d.Name);
                        xw.WriteAttributeString("value", d.Value);
                        xw.WriteEndElement();
                    }
                }
                xw.WriteEndElement(); // </Defines>

                // Structs
                xw.WriteStartElement("Structs");
                for (int i = 0; i < res.Structs.Count; i++)
                {
                    var s = res.Structs[i];
                    xw.WriteStartElement("Struct");
                    xw.WriteAttributeString("name", s.Name);

                    for (int j = 0; j < s.Fields.Count; j++)
                    {
                        var f = s.Fields[j];
                        xw.WriteStartElement("Field");
                        xw.WriteAttributeString("type", f.CType);
                        xw.WriteAttributeString("name", f.Name);
                        xw.WriteAttributeString("pointerLevel", f.PointerLevel.ToString());
                        if (!string.IsNullOrEmpty(f.ArraySuffix))
                            xw.WriteAttributeString("array", f.ArraySuffix);
                        xw.WriteEndElement(); // Field
                    }

                    xw.WriteEndElement(); // Struct
                }
                xw.WriteEndElement(); // </Structs>

                // Enums
                xw.WriteStartElement("Enums");
                for (int i = 0; i < res.Enums.Count; i++)
                {
                    var e = res.Enums[i];
                    xw.WriteStartElement("Enum");
                    xw.WriteAttributeString("name", e.Name);

                    for (int k = 0; k < e.Items.Count; k++)
                    {
                        var it = e.Items[k];
                        xw.WriteStartElement("Item");
                        xw.WriteAttributeString("name", it.Name);
                        if (it.RawValue != null)
                            xw.WriteAttributeString("raw", it.RawValue);
                        if (it.ParsedValue.HasValue)
                            xw.WriteAttributeString("value", it.ParsedValue.Value.ToString());
                        xw.WriteEndElement(); // Item
                    }

                    xw.WriteEndElement(); // Enum
                }
                xw.WriteEndElement(); // </Enums>

                xw.WriteEndElement(); // </Header>
                xw.WriteEndDocument();
            }
        }
    }
}
