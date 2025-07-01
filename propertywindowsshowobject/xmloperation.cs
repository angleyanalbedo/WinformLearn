using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XmlOperation
{
    
    public class Decl
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }

    
    public class Struct
    {
        public string Name { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public List<Decl> Members { get; set; } = new List<Decl>();
    }

    public class Enum
    {
        public string Name { get; set; } = "";
        public Hashtable Values { get; set; } = new Hashtable();
    }

    public class Varabile
    {
        public string Name { get; set; } = "";
        public Struct _struct { get; set; } = new Struct() { Name = "None" };

        public string Address { get; set; } = "";
    }


    public class XmlToHashtableConverter
    {
        public static (Hashtable structs, Hashtable enums, Hashtable macros) ConvertXmlToHashtables(string xmlString)
        {
            Hashtable structs = new Hashtable();
            Hashtable enums = new Hashtable();
            Hashtable macros = new Hashtable();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            // 处理结构体
            XmlNodeList structNodes = xmlDoc.SelectNodes("//struct") ?? throw new InvalidOperationException("No struct nodes found.");
            foreach (XmlNode structNode in structNodes)
            {
                // Updated code to handle potential null references for structNode.Attributes["name"]  
                string structName = structNode.Attributes?["name"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.");
                ArrayList members = new ArrayList();
                XmlNodeList xmlNodes = structNode.SelectNodes("member") ?? throw new InvalidOperationException($"No member nodes found in struct '{structName}'.");
                foreach (XmlNode memberNode in xmlNodes)
                {
                    members.Add(new Hashtable
                    {
                        {"name", memberNode.Attributes ?["name"] ?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.")},
                        {"type", memberNode.Attributes ?["type"] ?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.")}
                    });
                }

                structs[structName] = members;
            }

            // 处理枚举
            XmlNodeList enumNodes = xmlDoc.SelectNodes("//enum") ?? throw new InvalidOperationException("No struct nodes found.");
            foreach (XmlNode enumNode in enumNodes)
            {
                string enumName = enumNode.Attributes?["name"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.");
                Hashtable enumValues = new Hashtable();

                foreach (XmlNode valueNode in enumNode.SelectNodes("value") ?? throw new InvalidOperationException("No struct nodes found."))
                {
                    enumValues[valueNode.Attributes?["name"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.")] =
                        valueNode.Attributes?["value"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.");
                }

                enums[enumName] = enumValues;
            }

            // 处理宏定义
            XmlNodeList macroNodes = xmlDoc.SelectNodes("//macro") ?? throw new InvalidOperationException("No struct nodes found.");
            foreach (XmlNode macroNode in macroNodes)
            {
                macros[macroNode.Attributes?["name"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.")] =
                    macroNode.Attributes?["value"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.");
            }

            return (structs, enums, macros);
        }

        public static void RunExe(string exePtah, string args = "")
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = exePtah;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(output))
                Console.WriteLine("Output: " + output);

            if (!string.IsNullOrEmpty(error))
                Console.WriteLine("Error: " + error);
        }

        public static List<Struct> GetStructs(Hashtable structs)
        {
            List<Struct> structList = new List<Struct>();
            foreach (DictionaryEntry entry in structs)
            {
                List<Decl> membersList = new List<Decl>();
                if (entry.Value is ArrayList members)
                {
                    foreach (Hashtable member in members)
                    {
                        if (member["name"] == null || member["type"] == null)
                        {
                            throw new InvalidOperationException("Struct member 'name' or 'type' attribute is missing.");
                        }
                        else
                        {
                            membersList.Add(new Decl
                            {
                                Name = (string)member["name"],
                                Type = (string)member["type"],
                            });
                        }

                    }
                }

                Struct s = new Struct
                {
                    Name = (string)entry.Key,
                    Members = membersList
                };
                structList.Add(s);
            }
            return structList;
        }

        public static Dictionary<string, int> GetStructsSize(string xmlString)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            Dictionary<string, int> structsSizes = new Dictionary<string, int>();
            // 处理结构体
            XmlNodeList structNodes = xmlDoc.SelectNodes("//struct-size") ?? throw new InvalidOperationException("No struct nodes found.");
            foreach (XmlNode structNode in structNodes)
            {
                // Updated code to handle potential null references for structNode.Attributes["name"]
                string structName = structNode.Attributes?["name"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing.");
                int size = int.Parse(structNode.Attributes?["size"]?.Value ?? throw new InvalidOperationException("Struct node 'name' attribute is missing."));
                structsSizes[structName] = size;
            }
            return structsSizes;
        }

        public static List<Enum> GetEnums(Hashtable enums)
        {
            List<Enum> enumList = new List<Enum>();
            foreach (DictionaryEntry entry in enums)
            {
                if (entry.Key is string key && entry.Value is Hashtable value) // Ensure non-null and correct type
                {
                    Enum e = new Enum
                    {
                        Name = key,
                        Values = value
                    };
                    enumList.Add(e);
                }
            }
            return enumList;
        }
    }

}

