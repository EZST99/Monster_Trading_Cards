using System.Net.Sockets;
using System.Text;

namespace SwenProject_Arslan.Server
{
    /// <summary>This class defines event arguments for the <see cref="HttpSvrEventHandler"/> event handler.</summary>
    public class HttpSvrEventArgs: EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // protected members                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>TCP client.</summary>
        protected TcpClient _Client;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="client">TCP client.</param>
        /// <param name="plainMessage">Plain HTTP message.</param>
        public HttpSvrEventArgs(TcpClient client, string plainMessage) // Client erstellt vom Listener und der Request den wi gelesen haben als blanken Text
        {
            _Client = client;

            PlainMessage = plainMessage;
            Payload = string.Empty; // da nicht gewiss ist, dass eine Payload drinnen ist (das was im http String nach der Leerzeile steht)

            string[] lines = plainMessage.Replace("\r\n", "\n").Split('\n'); // spliten damit wir die einzelnen Zeilen bekommen als Array
            bool inheaders = true; // Hilfsvariable ob wir in den Headers (vor der Leerzeile) 
            List<HttpHeader> headers = new(); // Liste von http Headern, weil wir die als soolche übergeben wollen

            for(int i = 0; i < lines.Length; i++)  // alle Zeilen durchgehen
            {
                if(i == 0) // wenn wir das erste Mal in der Schleife sind, sind wir z.B.: bei GET bla bla
                {
                    string[] inc = lines[0].Split(' ');
                    Method = inc[0]; // der erste Teil die Methode (z.B.: Get)
                    Path = inc[1]; // der zweite Teil der Pfad 
                    continue;
                }

                if(inheaders)
                {
                    if(string.IsNullOrWhiteSpace(lines[i])) // falls wir NULL oder Whitespaces haben sind wir nicht mehr in den Headers
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new(lines[i])); } // Header hinzugefügt
                }
                else
                {
                    if(!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; } // Zeilemunbruch da wir alle davor herausgenommen werden
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray(); 
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Gets the plain message.</summary>
        public string PlainMessage
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP method.</summary>
        public virtual string Method
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP path.</summary>
        public virtual string Path
        {
            get; protected set;
        } = string.Empty;


        /// <summary>Gets the HTTP headers.</summary>
        public virtual HttpHeader[] Headers
        {
            get; protected set;
        } = Array.Empty<HttpHeader>();


        /// <summary>Gets the payload.</summary>
        public virtual string Payload
        {
            get; protected set;
        } = string.Empty;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Replies the request</summary>
        /// <param name="status">HTTP Status code.</param>
        /// <param name="msg">Reply body.</param>
        public void Reply(int status, string? body = null) // wenn ein Header verarbeiten kann Antworter das, listen around minute 35
        {
            string data;

            switch(status)
            {
                case 200:
                    data = "HTTP/1.1 200 OK\n"; break;
                case 400:
                    data = "HTTP/1.1 400 Bad Request\n"; break;
                case 404:
                    data = "HTTP/1.1 404 Not found\n"; break;
                default:
                    data = $"HTTP/1.1 {status} Status unknown\n"; break;
            }

            if(string.IsNullOrEmpty(body)) 
            {
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";
            if(!string.IsNullOrEmpty(body)) { data += body; }

            byte[] buf = Encoding.ASCII.GetBytes(data); // andersherum umwandeln wieder in Bytearray
            _Client.GetStream().Write(buf, 0, buf.Length); // wird in Stream geschrieben
            _Client.Close();
            _Client.Dispose();
        }
    }
}
