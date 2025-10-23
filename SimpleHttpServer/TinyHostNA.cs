using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/*
 * TinyHost - 极简 C# HTTP 宿主框架 (适用于 .NET Framework 4.5 及以上版本) 
 * 不使用任何第三方库 甚至不需要 System.Web.Extensions 和System.Runtime.Serialization
 */
namespace TinyApiNA
{


    #region 宿主内核
    class TinyHost
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly RouteTable _routes = new RouteTable();

        public TinyHost(params string[] prefixes)
        {
            foreach (var p in prefixes) _listener.Prefixes.Add(p);
            _listener.Start();
        }

        public void Get(string path, Func<Request, object> handler) { _routes.Add("GET", path, handler); }
        public void Post(string path, Func<Request, object> handler) { _routes.Add("POST", path, handler); }

        public void Run()
        {
            Console.WriteLine("TinyHost running on " + string.Join(", ", _listener.Prefixes));
            while (true)
            {
                var ctx = _listener.GetContext();
                object result = null;
                try
                {
                    var req = new Request(ctx.Request);
                    var handler = _routes.Match(req.Method, req.Path);
                    result = handler == null
                        ? new JsonResult { Code = 404, Json = "{\"error\":\"not found\"}" }
                        : handler(req);
                }
                catch (Exception ex)
                {
                    result = new JsonResult { Code = 500, Json = "{\"error\":\"" + ex.Message + "\"}" };
                }
                WriteResponse(ctx.Response, result);
                ctx.Response.Close();
            }
        }

        private static void WriteResponse(HttpListenerResponse res, object result)
        {
            var jr = result as JsonResult;
            if (jr != null)
            {
                res.StatusCode = jr.Code;
                res.ContentType = "application/json";
                byte[] buf = Encoding.UTF8.GetBytes(jr.Json);
                res.OutputStream.Write(buf, 0, buf.Length);
                return;
            }
            // 纯文本
            res.StatusCode = 200;
            res.ContentType = "text/plain";
            byte[] buf2 = Encoding.UTF8.GetBytes(result.ToString());
            res.OutputStream.Write(buf2, 0, buf2.Length);
        }
    }
    #endregion

    #region 请求封装
    class Request
    {
        private readonly HttpListenerRequest _raw;
        public string Method { get { return _raw.HttpMethod; } }
        public string Path { get { return _raw.Url.AbsolutePath; } }
        private Dictionary<string, string> _query;
        public Dictionary<string, string> Query
        {
            get
            {
                if (_query == null)
                {
                    _query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string key in _raw.QueryString.Keys)
                        if (key != null) _query[key] = _raw.QueryString[key];
                }
                return _query;
            }
        }
        public Request(HttpListenerRequest raw) { _raw = raw; }

        private string _bodyText;
        public string BodyText
        {
            get
            {
                if (_bodyText == null)
                    using (var sr = new StreamReader(_raw.InputStream))
                        _bodyText = sr.ReadToEnd();
                return _bodyText;
            }
        }
    }
    #endregion

    #region 路由表
    class RouteTable
    {
        private readonly List<Entry> _list = new List<Entry>();
        private class Entry
        {
            public string Method; public string Path; public Func<Request, object> Handler;
            public Entry(string m, string p, Func<Request, object> h) { Method = m; Path = p; Handler = h; }
        }
        public void Add(string method, string path, Func<Request, object> handler)
        {
            _list.Add(new Entry(method.ToUpper(), path.ToLower(), handler));
        }
        public Func<Request, object> Match(string method, string path)
        {
            string m = method.ToUpper(), p = path.ToLower();
            foreach (var e in _list)
                if (e.Method == m && e.Path == p) return e.Handler;
            return null;
        }
    }
    #endregion

    #region 极简 JSON 工具
    class JsonHelper
    {
        // 只演示反序列化，够用即可
        private static readonly Regex _nameRx = new Regex("\"([^\"]+)\"");
        private static readonly Regex _valueRx = new Regex("\"([^\"]*)\""); // 字符串值
        public static T FromJson<T>(string json) where T : new()
        {
            T obj = new T();
            var type = typeof(T);
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            // 最简实现：只支持 {"Name":"xx","Age":20} 这种格式
            string[] parts = json.Trim('{', '}').Split(',');
            foreach (var p in parts)
            {
                var nv = p.Split(':');
                if (nv.Length != 2) continue;
                string n = _nameRx.Match(nv[0]).Groups[1].Value;
                string v = nv[1].Trim();
                dict[n] = v;
            }
            foreach (var pi in type.GetProperties())
            {
                if (!dict.ContainsKey(pi.Name)) continue;
                string raw = dict[pi.Name].Trim('"');
                if (pi.PropertyType == typeof(string))
                    pi.SetValue(obj, raw, null);
                else if (pi.PropertyType == typeof(int))
                    pi.SetValue(obj, int.Parse(raw), null);
            }
            return obj;
        }
    }
    #endregion

    #region 返回对象
    class JsonResult
    {
        public int Code = 200;
        public string Json = "{}";
    }
    #endregion
}

