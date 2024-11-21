using System;

namespace FHTW.Swen1.Swamp
{
    public class TestHandler : Handler, IHandler
    {
        public override bool Handle(HttpSvrEventArgs e)
        {
            if (e.Path.StartsWith("/test", StringComparison.OrdinalIgnoreCase))
            {

                if (e.Method == "GET")
                {
                    e.Reply(HttpStatusCode.OK, "TestHandler: GET request received for /test");
                }
                else if (e.Method == "POST")
                {
                    e.Reply(HttpStatusCode.OK, "TestHandler: POST request received for /test");
                }
                else
                {
                    e.Reply(HttpStatusCode.NOT_FOUND, "TestHandler: Unsupported HTTP method");
                }

                return true; // Anfrage wurde verarbeitet
            }

            return false; // Anfrage an den nächsten Handler weitergeben
        }
    }
}