namespace DumpFile
{
    internal class Program
    {
        internal class Files
        {
            internal static string DumpFile(string file, out bool ok)
            {
                try
                {
                    var data = File.ReadAllText(Environment.ExpandEnvironmentVariables(file));
                    ok = true;
                    return data;
                }
                catch (Exception e)
                {
                    ok = false;
                    return e.Message;
                }
            }
            internal static string Hosts()
            {
                return DumpFile(@"%windir%\system32\drivers\etc\hosts", out var _);
            }
            internal static string IIS()
            {
                // >=IIS7
                var iis7 = DumpFile(@"%windir%\system32\inetsrv\config\ApplicationHost.config", out var ok);
                if (ok) return iis7;
                // IIS6
                var iis6 = DumpFile(@"%windir%\system32\inetsrv\MetaBase.xml", out ok);
                if (ok)
                {
                    return iis6;
                }
                else
                {
                    return iis7 + "\n" + iis6;
                }
            }
            internal static string Powershell()
            {
                return DumpFile(@"%appdata%\Microsoft\Windows\PowerShell\PSReadline\ConsoleHost_history.txt", out var _);
            }

            internal static string SSH()
            {
                string config = DumpFile(@"%userprofile%\.ssh\config", out var ok1);
                string known_hosts = DumpFile(@"%userprofile%\.ssh\known_hosts", out var ok2);
                string id_rsa = DumpFile(@"%userprofile%\.ssh\id_rsa", out var ok3);
                string id_rsa_pub = DumpFile(@"%userprofile%\.ssh\id_rsa.pub", out var ok4);
                return config + "\n" + known_hosts + "\n" + id_rsa + "\n" + id_rsa_pub;
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("----- Hosts File -----");
            Console.WriteLine(Files.Hosts());
            Console.WriteLine("----- SSH Config -----");
            Console.WriteLine(Files.SSH());
        }
    }
}
