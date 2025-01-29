using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SwenProject_Arslan.Handlers;

namespace SwenProject_Arslan.Server
{
    public sealed class HttpSvr
    {
        private TcpListener? _Listener;

        public event HttpSvrEventHandler? Incoming;

        private readonly ConcurrentQueue<TcpClient> _WaitingClients = new();
        private readonly ConcurrentQueue<string> _DataTemp = new();

        public bool Active { get; private set; } = false;

        public void Run()
        {
            if (Active) return;

            Active = true;
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10001);
            _Listener.Start();

            Console.WriteLine("Server is running and waiting for connections...");

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();
                Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            Console.WriteLine("New client connected.");
            byte[] buf = new byte[256];
            string data = string.Empty;

            while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
            {
                int bytesRead = await client.GetStream().ReadAsync(buf, 0, buf.Length);
                data += Encoding.UTF8.GetString(buf, 0, bytesRead);
            }
            
            Console.WriteLine($"Request received:\n{data}");

            if (data.Contains("battle"))
            {
                if (_WaitingClients.Contains(client))
                {
                    // Methode schreiben
                }
                else
                {
                    _WaitingClients.Enqueue(client);
                    _DataTemp.Enqueue(data);

                    if (_WaitingClients.Count == 2)
                    {
                        if (_WaitingClients.TryDequeue(out var client1) &&
                            _WaitingClients.TryDequeue(out var client2) &&
                            _DataTemp.TryDequeue(out var dataTempClient1) &&
                            _DataTemp.TryDequeue(out var dataTempClient2))
                        {
                            HttpSvrEventArgs e1 = new HttpSvrEventArgs(client1, dataTempClient1);
                            HttpSvrEventArgs e2 = new HttpSvrEventArgs(client2, dataTempClient2);
                            
                            await BattleHandler.HandleBattleRequest(e1, e2);
                        }
                    }
                }
            }
            else
            {
                Incoming?.Invoke(this, new(client, data));
            }
        }

       /* private async void HandleClient(TcpClient client)
        {
            NetworkStream? stream = null;
            try
            {
                Console.WriteLine("New client connected.");

                stream = client.GetStream();
                string data = string.Empty;
                byte[] buf = new byte[256];

                // Lesen der Anfrage
                while (client.Connected && stream.DataAvailable || string.IsNullOrWhiteSpace(data))
                {
                    int read = await stream.ReadAsync(buf, 0, buf.Length);
                    if (read == 0) break; // Client hat die Verbindung geschlossen
                    data += Encoding.ASCII.GetString(buf, 0, read);
                }

                if (string.IsNullOrWhiteSpace(data))
                {
                    Console.WriteLine("No data received. Closing connection.");
                    return;
                }

                Console.WriteLine($"Request received:\n{data}");

                // EventArgs erstellen und weiterleiten
                HttpSvrEventArgs eventArgs = new(client, data);
                Incoming?.Invoke(this, eventArgs);

                // Sicherstellen, dass die Antwort verarbeitet wurde
                Console.WriteLine("Request handled successfully.");
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"Client already disposed: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleClient: {ex.Message}");
            }
        }*/



        public void Stop()
        {
            Active = false;
            _Listener?.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
