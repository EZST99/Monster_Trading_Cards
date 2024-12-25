using System.Reflection;
using SwenProject_Arslan.Interfaces;

namespace SwenProject_Arslan.Server
{
    public abstract class Handler: IHandler
    {
        private static List<IHandler>? _Handlers = null; // Liste von Handlern, über die man durchiterieren kann und schauen, ob es einer verarbeiten kann

        
        private static List<IHandler> _GetHandlers() // hier wird die Liste initialisiert (erst wenns gebraucht wird)
        {
            List<IHandler> rval = new();

            foreach(Type i in Assembly.GetExecutingAssembly().GetTypes()
                              .Where(m => m.IsAssignableTo(typeof(IHandler)) && (!m.IsAbstract))) // Reflection, Code des Programs wird in laufzeit ausgewertet
                                                                                                  // das Assembly (mein Program) das gerade läuft (GetExecutingAssembly)
                                                                                                  // ich kriege ein Assembly Object zurück, hole die Types als Array raus
                                                                                                  // und filtere mit Linq die raus die iHandler sind und nicht abstakt
                                                                                                  // bruh wtf
            {
                IHandler? h = (IHandler?) Activator.CreateInstance(i); // Vom Typeobject kann eine Instanz erstellt werden, indem der Konstruktor aufgerufen wird, und wird auf iHandler gecasted
                if(h != null)
                {
                    rval.Add(h); // nur wenns nicht null ist wirds in die Liste der Handler reingegeben
                }
            }

            return rval;
        }


        public static void HandleEvent(HttpSvrEventArgs e)
        {
            _Handlers ??= _GetHandlers(); // if Handlers = null Handlers = gethandlers

            foreach(IHandler i in _Handlers) 
            {
                if(i.Handle(e)) return; // kannst du das handlen? Wenn ja handlet Handler und ich bin fertig
            }
            e.Reply(HttpStatusCode.BAD_REQUEST);
        }



        public abstract bool Handle(HttpSvrEventArgs e);
    }
}
