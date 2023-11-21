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
            BardClient[] clients = new BardClient[25];

                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i] = new BardClient(reader);
                    clients[i].Connect();
            
                }
          
                server.SendToAll("Hello, world!");

        
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Send("Hello, server!");

                }
      
                for (int i = 0; i < clients.Length; i++)
                {
                    clients[i].Send("Bye, server!");
               
                    clients[i].Disconnect();
                }
            
               
                Debug.Log("Only " + server.clients.Count + " peeps remain.",LogSource.Server);
                while (true)
                {
                    await Task.Delay(1000   );
                 
                }

            }).GetAwaiter().GetResult();
        }
    }
}