using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Server.Repo;

namespace Server
{
    public class Listener
    {
        public async Task RunAsync(CancellationToken token = default)
        {
            TcpListener server = new TcpListener(IPAddress.Any, 8080);
            server.Start();
            Console.WriteLine("Сервер запущен, ожидает подключения...");

            while (!token.IsCancellationRequested)
            {
                var client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Клиент подключился.");
                _ = HandleClientAsync(client);
            }
        }

        public async Task HandleClientAsync(TcpClient client)
        {
            using (client)
            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Получено сообщение: {message}");

                if (message == "GetPhones")
                {
                    var phones = DatabaseControl.GetPhones();
                    string json = JsonConvert.SerializeObject(phones, Newtonsoft.Json.Formatting.Indented);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Ответ клиенту отправлен.");
                }
                else if (message == "GetCompanies")
                {
                    var companies = DatabaseControl.GetCompanies();
                    string json = JsonConvert.SerializeObject(companies, Newtonsoft.Json.Formatting.Indented);
                    byte[] responseBytes = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    Console.WriteLine("Ответ клиенту отправлен.");
                }
            }
        }
    }
}
