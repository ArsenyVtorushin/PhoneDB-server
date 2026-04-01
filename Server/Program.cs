namespace Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var listener = new Listener();
            await listener.RunAsync();
        }
    }
}
