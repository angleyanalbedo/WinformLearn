using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
/*
 * TinyHost - 极简 C# HTTP 宿主框架 
 * 需要 System.Web.Extensions.dll 和 System.Runtime.Serialization.dll
 */
namespace TinyApi
{
    #region 宿主内核
    public class TinyHost
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
                var req = new Request(ctx.Request);
                object result = null;
                var handler = _routes.Match(req.Method, req.Path);
                if (handler != null)
                {
                    try { result = handler(req); }
                    catch (Exception ex) { result = Results.Json(new { error = ex.Message }, 500); }
                }
                else
                {
                    result = Results.Json(new { error = "not found" }, 404);
                }
                WriteResponse(ctx.Response, result);
                ctx.Response.Close();
            }
        }

        private static void WriteResponse(HttpListenerResponse res, object result)
        {
            string payload;
            int code;
            string contentType;

            JsonResult jr = result as JsonResult;
            if (jr != null)
            {
                payload = jr.Json;
                code = jr.Code;
                contentType = "application/json";
            }
            else if (result is string)
            {
                payload = (string)result;
                code = 200;
                contentType = "text/plain";
            }
            else
            {
                payload = new JavaScriptSerializer().Serialize(result);
                code = 200;
                contentType = "application/json";
            }

            res.StatusCode = code;
            res.ContentType = contentType;
            byte[] buf = Encoding.UTF8.GetBytes(payload);
            res.OutputStream.Write(buf, 0, buf.Length);
        }
    }
    #endregion

    #region Request 封装
    public class Request
    {
        private readonly HttpListenerRequest _raw;
        public string Method { get { return _raw.HttpMethod; } }
        public string Path { get { return _raw.Url.AbsolutePath; } }

        private NameValueCollection _query;
        public NameValueCollection Query
        {
            get { return _query ?? (_query = _raw.QueryString); }
        }

        public Request(HttpListenerRequest raw) { _raw = raw; }

        public T Body<T>()
        {
            using (var sr = new StreamReader(_raw.InputStream))
            {
                string json = sr.ReadToEnd();
                return new JavaScriptSerializer().Deserialize<T>(json);
            }
        }
    }
    #endregion

    #region 路由表
    public class RouteTable
    {
        private class Entry
        {
            public string Method;
            public string Path;
            public Func<Request, object> Handler;
            public Entry(string m, string p, Func<Request, object> h)
            {
                Method = m;
                Path = p;
                Handler = h;
            }
        }

        private readonly List<Entry> _list = new List<Entry>();

        public void Add(string method, string path, Func<Request, object> handler)
        {
            _list.Add(new Entry(method.ToUpper(), path.ToLower(), handler));
        }

        public Func<Request, object> Match(string method, string path)
        {
            string m = method.ToUpper();
            string p = path.ToLower();
            foreach (var e in _list)
            {
                if (e.Method == m && e.Path == p)
                    return e.Handler;
            }
            return null;
        }
    }
    #endregion

    #region 工具类

    #region 结果封装
    public static class Results
    {
        public static object Ok(object data) { return data; }
        public static JsonResult Json(object obj, int code = 200)
        {
            return new JsonResult(new JavaScriptSerializer().Serialize(obj), code);
        }
    }
    #endregion

    public class JsonResult
    {
        public string Json;
        public int Code;
        public JsonResult(string json, int code) { Json = json; Code = code; }
    }
    #endregion

}