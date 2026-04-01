using Server;
using System.Threading.Tasks;

internal class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpWebSocketServer(9000);

        var serverTask = server.StartAsync();

        Console.WriteLine("Нажмите любую клавишу для остановки сервера...");
        Console.ReadKey();

        await server.StopAsync();
    }
}