using SwenProject_Arslan.Server;

namespace SwenProject_Arslan.Interfaces
{
    public interface IHandler
    {
        public bool Handle(HttpSvrEventArgs e); // Idee ist beim Aufrufen zu schauen ob mans verarbeiten kann oder nicht (false wenn nicht)
    }
}
