using SwenProject_Arslan.Handlers;
using SwenProject_Arslan.Models;
using SwenProject_Arslan.Repositories;
using SwenProject_Arslan.Server;

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
            var dbHandler = new DbHandler("Host=localhost;Username=mtcg_user;Password=1234;Database=mtcg");
            var userService = new UserService(dbHandler);
            // Beispiel: Benutzer erstellen
            var newUser = new User ("mtcg_user", "mtcg-password");
            await userService.CreateUserAsync(newUser);

            // Alle Benutzer abfragen
            var users = await userService.GetAllUsersAsync();
            foreach (var user in users)
            {
                Console.WriteLine($"Username: {user.UserName}, Coins: {user.Coins}");
            }
            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming;
            //svr.Incoming *= (sender, e) => { Handler.HandleEvent(e); };

            svr.Run();
        }

        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.HandleEvent(e);
            /*
            Console.WriteLine(e.Method);
            Console.WriteLine(e.Path);
            Console.WriteLine();
            foreach(HttpHeader i in e.Headers)
            {
                Console.WriteLine(i.Name + ": " + i.Value);
            }
            Console.WriteLine();
            Console.WriteLine(e.Payload);

            e.Reply(HttpStatusCode.OK, "Yo Baby!");
            */
        }
    }
}