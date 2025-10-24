using TinyApi;
using TinyApiNA;

namespace SimpleHttpServer
{
    #region 示例模型
    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
    #endregion
    class Program
    {
        static void Main()
        {
            //var host = new TinyApiNA.TinyHost("http://localhost:5000/");
            //host.Get("/", req => "Hello TinyHost");
            //host.Get("/hello", req => "Hello " + (req.Query["name"] ?? "world"));
            //host.Post("/user", req =>
            //{
            //    // 手动把 json 转成 User
            //    var u = JsonHelper.FromJson<User>(req.BodyText);
            //    return new { msg = "got user", u.Name, u.Age };
            //});
            //host.Run();

            var api = new TinyApi.TinyHost("http://localhost:5000/");
            api.Get("/", req => "Hello TinyHost");
            api.Get("/hello", req => "Hello " + (req.Query["name"] ?? "world"));
            api.Post("/user", req => Results.Ok(req.Body<User>()));
            api.Run();
        }
    }
}