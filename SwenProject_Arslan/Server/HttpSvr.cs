using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SwenProject_Arslan.Handlers;

namespace SwenProject_Arslan.Server
{
    /// <summary>This class implements a HTTP server with battle logic.</summary>
    public sealed class HttpSvr
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP listener instance.</summary>
        private TcpListener? _Listener;

        /// <summary>Queue for players waiting for a battle.</summary>
        private readonly Queue<TcpClient> waitingClients = new();

        /// <summary>Queue for request data associated with waiting clients.</summary>
        private readonly Queue<string> tempData = new();

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Is raised when incoming data is available.</summary>
        public event HttpSvrEventHandler? Incoming;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Gets if the server is available.</summary>
        public bool Active
        {
            get; private set;
        } = false;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Runs the server.</summary>
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
                Task.Run(() => HandleClient(client)); // Bearbeitung in eigenem Thread
            }
        }

        /// <summary>Handles an individual client connection.</summary>
        private void HandleClient(TcpClient client)
        {
            string data = string.Empty;
            byte[] buf = new byte[256];

            while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
            {
                int read = client.GetStream().Read(buf, 0, buf.Length);
                data += Encoding.ASCII.GetString(buf, 0, read);
            }

            Console.WriteLine($"Request received:\n{data}");

            // Wenn Anfrage Battle
            if (data.Contains("battle", StringComparison.OrdinalIgnoreCase))
            {
                lock (waitingClients)
                {
                    // Spieler in die Warteschlange 
                    waitingClients.Enqueue(client);
                    tempData.Enqueue(data);

                    if (waitingClients.Count >= 2)
                    {
                        // Zwei Spieler verfügbar: Battle starten
                        var player1 = waitingClients.Dequeue();
                        var player2 = waitingClients.Dequeue();
                        var data1 = tempData.Dequeue();
                        var data2 = tempData.Dequeue();

                        Console.WriteLine("Two players matched. Starting battle...");
                        HttpSvrEventArgs e1 = new HttpSvrEventArgs(player1, data1);
                        HttpSvrEventArgs e2 = new HttpSvrEventArgs(player2, data2);
                        
                        
                        //await BattleHandler.joinBattle(e1, e2);
                    }
                    else
                    {
                        Console.WriteLine("Player added to the battle queue. Waiting for an opponent...");
                    }
                }
            }
            else
            {
                // Andere Anfragen weiterleiten
                Incoming?.Invoke(this, new(client, data));
            }
        }

        /// <summary>Stops the server.</summary>
        public void Stop()
        {
            Active = false;
            _Listener?.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}
