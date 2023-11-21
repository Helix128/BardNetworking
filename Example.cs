using BardNetworking.Components;

namespace BardNetworking
{
    internal class Example
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                Console.WriteLine("Bard Networking Example");
                PacketReader reader = new PacketReader();
                BardServer server = new BardServer(reader);
                server.Start();
                BardClient[] clients = new BardClient[64];
                await Task.Delay(25);
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i] = new BardClient(reader);
                    await clients[i].Connect();

                }

                server.SendToAll("hi!");
                await Task.Delay(5);

                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Send("Dear Karthus, \r\nI hope this finds you well.We seem to have found ourselves in a dire situation at the bottom lane. Consider casting your ultimate ability to assist us as I do believe Lee Sin has come to dive our tower.  \r\nsincerely,your bottom lane");
                    await Task.Delay(55);
                }

                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Send("bye,serv!");
                    await Task.Delay(55);
                    clients[i].Disconnect();
                }

                await Task.Delay(4000);
                Debug.Log("Only " + server.clients.Count + " peeps remain.", LogSource.Server);
                while (true)
                {
                    await Task.Delay(1000);

                }

            }).GetAwaiter().GetResult();
        }
    }
}