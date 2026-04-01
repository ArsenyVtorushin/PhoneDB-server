using Newtonsoft.Json;
using Server.Models;
using Server.Repo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class HttpWebSocketServer
    {
        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Dictionary<string, Func<Dictionary<string, object>, object>> _handlers;

        public HttpWebSocketServer(int port = 9000)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://localhost:{port}/");
            _cancellationTokenSource = new CancellationTokenSource();

            _handlers = new Dictionary<string, Func<Dictionary<string, object>, object>>
            {
                ["GetPhones"] = _ => DatabaseControl.GetPhones(),
                ["GetCompanies"] = _ => DatabaseControl.GetCompanies(),
                ["AddPhone"] = parameters => AddPhoneHandler(parameters),
                ["UpdatePhone"] = parameters => UpdatePhoneHandler(parameters),
                ["DeletePhone"] = parameters => DeletePhoneHandler(parameters)
            };
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("HTTP WebSocket сервер запущен, ожидает подключения...");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var context = await _listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleWebSocket(context);
                    }
                    else
                    {
                        // Отправляем простой HTML для тестирования
                        SendHtmlPage(context.Response);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при обработке запроса: {ex.Message}");
                }
            }
        }

        private void SendHtmlPage(HttpListenerResponse response)
        {
            string html = @"
                <html>
                <head><title>WebSocket Test</title></head>
                <body>
                    <h1>WebSocket сервер работает</h1>
                    <p>Подключитесь через WebSocket к этому адресу.</p>
                    <script>
                        const ws = new WebSocket('ws://localhost:9000');
                        ws.onopen = function() {
                            console.log('Подключено к WebSocket');
                            ws.send(JSON.stringify({command: 'GetPhones'}));
                        };
                        ws.onmessage = function(event) {
                            console.log('Получено:', event.data);
                        };
                    </script>
                </body>
                </html>";

            byte[] buffer = Encoding.UTF8.GetBytes(html);
            response.ContentLength64 = buffer.Length;
            response.ContentType = "text/html";
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.Close();
        }

        private async Task HandleWebSocket(HttpListenerContext context)
        {
            WebSocket webSocket = null;
            try
            {
                WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                webSocket = webSocketContext.WebSocket;

                Console.WriteLine("WebSocket клиент подключился.");

                byte[] buffer = new byte[1024 * 4]; // 4KB buffer
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        try
                        {
                            var request = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);

                            if (request.ContainsKey("command"))
                            {
                                string command = request["command"].ToString();

                                if (_handlers.ContainsKey(command))
                                {
                                    var response = _handlers[command](request);

                                    string jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
                                    byte[] responseBytes = Encoding.UTF8.GetBytes(jsonResponse);

                                    await webSocket.SendAsync(
                                        new ArraySegment<byte>(responseBytes, 0, responseBytes.Length),
                                        WebSocketMessageType.Text,
                                        true,
                                        CancellationToken.None);
                                }
                                else
                                {
                                    var errorResponse = new { error = $"Неизвестная команда: {command}" };
                                    string errorJson = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
                                    byte[] errorBytes = Encoding.UTF8.GetBytes(errorJson);

                                    await webSocket.SendAsync(
                                        new ArraySegment<byte>(errorBytes, 0, errorBytes.Length),
                                        WebSocketMessageType.Text,
                                        true,
                                        CancellationToken.None);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorResponse = new { error = $"Ошибка обработки запроса: {ex.Message}" };
                            string errorJson = JsonConvert.SerializeObject(errorResponse, Formatting.Indented);
                            byte[] errorBytes = Encoding.UTF8.GetBytes(errorJson);

                            await webSocket.SendAsync(
                                new ArraySegment<byte>(errorBytes, 0, errorBytes.Length),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None);
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в обработке WebSocket: {ex.Message}");
            }
            finally
            {
                webSocket?.Dispose();
            }
        }

        private object AddPhoneHandler(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("data"))
            {
                var phoneData = parameters["data"] as Dictionary<string, object>;
                if (phoneData != null)
                {
                    var phone = new Phone
                    {
                        Title = phoneData["Title"].ToString(),
                        CompanyId = Convert.ToInt32(phoneData["CompanyId"]),
                        Price = Convert.ToDecimal(phoneData["Price"])
                    };

                    DatabaseControl.AddPhone(phone);
                    return new { success = true, message = "Телефон добавлен успешно" };
                }
            }
            return new { error = "Неверные данные для добавления телефона" };
        }

        private object UpdatePhoneHandler(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("data"))
            {
                var phoneData = parameters["data"] as Dictionary<string, object>;
                if (phoneData != null)
                {
                    var phone = new Phone
                    {
                        Id = Convert.ToInt32(phoneData["Id"]),
                        Title = phoneData["Title"].ToString(),
                        CompanyId = Convert.ToInt32(phoneData["CompanyId"]),
                        Price = Convert.ToDecimal(phoneData["Price"])
                    };

                    DatabaseControl.UpdatePhone(phone);
                    return new { success = true, message = "Телефон обновлен успешно" };
                }
            }
            return new { error = "Неверные данные для обновления телефона" };
        }

        private object DeletePhoneHandler(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("data"))
            {
                var phoneData = parameters["data"] as Dictionary<string, object>;
                if (phoneData != null)
                {
                    var phone = new Phone
                    {
                        Id = Convert.ToInt32(phoneData["Id"])
                    };

                    DatabaseControl.DeletePhone(phone);
                    return new { success = true, message = "Телефон удален успешно" };
                }
            }
            return new { error = "Неверные данные для удаления телефона" };
        }

        public async Task StopAsync()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
        }
    }
}
