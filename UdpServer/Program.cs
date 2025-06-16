namespace UdpServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UdpAsyncServer udpAsyncServer = new UdpAsyncServer();
            udpAsyncServer.Start(8080);
            Console.WriteLine("Press any key to stop the server...");
            //这个有个缺点会错误的按下键导致关闭服务器
            Console.ReadKey();
            udpAsyncServer.Stop();
            Console.WriteLine("Server stopped. Press any key to exit.");

        }
    }
}
