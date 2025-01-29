using SwenProject_Arslan.Handlers;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Server;
using SwenProject_Arslan.Handlers.DbHandlers;

namespace SwenProject_Arslan
{
    /// <summary>This class contains the main entry point of the application.</summary>
    internal class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // entry point                                                                                                      //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Application entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static async Task Main(string[] args)
        {
            DbHandlerFactory.Initialize("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg");
            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming;
            svr.Run();
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
        }
    }
}