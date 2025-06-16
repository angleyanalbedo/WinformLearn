using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;


// 实验室之前的代码使用的是while循环导致
namespace UdpServer
{
    public class UdpAsyncServer
    {
        private UdpClient udpServer = new UdpClient();
        private bool isRunning;
        private IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8080);
        private bool isHHighConcurrency = true; // 是否高并发模式

        // 启动UDP服务端
        public void Start(int port)
        {
            try
            {
                localEndPoint = new IPEndPoint(IPAddress.Any, port);
                udpServer = new UdpClient(localEndPoint);

                // 设置超时以避免阻塞
                udpServer.Client.ReceiveTimeout = 1000;

                isRunning = true;

                Console.WriteLine($"UDP server started on port {port}");

                // 开始接收循环
                Task.Run(() => ReceiveLoop());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting server: {ex.Message}");
            }
        }

        // 停止服务端
        public void Stop()
        {
            isRunning = false;
            udpServer?.Close();
            Console.WriteLine("UDP server stopped");
        }

        // 异步接收循环
        private async void ReceiveLoop()
        {
            while (isRunning)
            {
                try
                {
                    // 异步接收数据
                    var result = await udpServer.ReceiveAsync();
                    string receivedMessage = Encoding.ASCII.GetString(result.Buffer);
                    IPEndPoint clientEndPoint = result.RemoteEndPoint;

                    Console.WriteLine($"Received from {clientEndPoint}: {receivedMessage}");
                    // 如果接收到的消息是空的，跳过处理
                    if (string.IsNullOrEmpty(receivedMessage))
                    {
                        Console.WriteLine("Received empty message, skipping processing.");
                        continue;
                    }
                    if (isHHighConcurrency)
                    {
                        // 如果想要并发高一点
                        _ = Task.Run(() => ProcessAndReply(clientEndPoint, receivedMessage));
                    }
                    else
                    {
                        // 处理消息并回复（示例）
                        await ProcessAndReply(clientEndPoint, receivedMessage);
                    }
                    
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    // 超时是预期的，继续循环
                    continue;
                }
                catch (ObjectDisposedException)
                {
                    // 当UdpClient被关闭时抛出
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex.Message}");
                    break;
                }
            }
        }

        // 处理消息并回复
        private async Task ProcessAndReply(IPEndPoint clientEndPoint, string message)
        {
            //这里应该放消息的处理逻辑
            
            try
            {
                // 示例处理逻辑：将消息转为大写并回复
                string response = $"Server response: {message.ToUpper()}";
                byte[] responseData = Encoding.ASCII.GetBytes(response);

                // 异步发送回复
                await udpServer.SendAsync(responseData, responseData.Length, clientEndPoint);
                Console.WriteLine($"Sent response to {clientEndPoint}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reply error: {ex.Message}");
            }
        }
    }
}
