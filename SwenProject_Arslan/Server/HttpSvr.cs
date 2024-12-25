using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SwenProject_Arslan.Server
{
    /// <summary>This class implements a HTTP server.</summary>
    public sealed class HttpSvr
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                                  //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>TCP listener instance.</summary>
        private TcpListener? _Listener;  // hört auf einen bestimmten Port und meldet sich, falls was einkommt und
                                         // gibt einen TCP client zurück mit dem man stream bearbeiten und eine response zurückschicken kann



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public events                                                                                                    //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Is raised when incoming data is available.</summary>
        public event HttpSvrEventHandler? Incoming;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Gets if the server is available.</summary>
        public bool Active // lesen ob er läuft oder nicht
        {
            get; private set;
        } = false;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Runs the server.</summary>
        public void Run() 
        {
            if(Active) return; // damit nicht mehrmals gestartet werden kann, man könnte exception werfen

            Active = true; // jetzt active
            _Listener = new(IPAddress.Parse("127.0.0.1"), 10001); // auf welche ip adresse soll gehört werden und bei welchem host(localhost)
            _Listener.Start(); 

            byte[] buf = new byte[256]; // buffer damit im buffer der Stream verarbeitet werden kann den der listener zurückgibt

            while(Active)
            {
                TcpClient client = _Listener.AcceptTcpClient(); // solange der listener läuft, geht das,
                                                                // accepttcpclient hält den litener an bis eine tcp verbindung besteht
                string data = string.Empty; 
                
                while(client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data)) // mit der methode bekommt man die daten, die über tcp als stream hineinkommen
                                                                                           // DataAvailable wird aufgerufen und wenn nichts in data drinnen steht ist es das erste Mal, dass wir lesen,
                                                                                           // weil der initial false ist
                                                                                           // und sobald alle gelesen wird ist DataAvailable false und Schleife endet
                {
                    int n = client.GetStream().Read(buf, 0, buf.Length); // mit read lesen wir in einen buffer daten rein, übergeben buffer, den anfangspunkt wo man zu lesen beginnt (stream = haufen von bytes)
                                                                         // wir beginnen bei 0 zu lesen bis zum Ende des buffers
                    data += Encoding.ASCII.GetString(buf, 0, n); // wir encoden damit aus den Bytes ein String gemacht wird
                }

                Incoming?.Invoke(this, new(client, data)); // wir machen daraus HttpSvrEventArgs um sie "eleganter" darzustellen
                
            }
        }


        /// <summary>Stops the server.</summary>
        public void Stop()  // in unseren fall nicht benutzt, muss nicht gestoppt werden, weil wir den server eh nur für eine sachen verwenden?
        {
            Active = false;
        }
    }
}
