using System;



namespace FHTW.Swen1.Swamp
{
    public interface IHandler
    {
        public bool Handle(HttpSvrEventArgs e); // Idee ist beim Aufrufen zu schauen ob mans verarbeiten kann oder nicht (false wenn nicht)
    }
}
