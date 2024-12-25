namespace SwenProject_Arslan.Server
{
    /// <summary>This class represents a HTTP header.</summary>
    public class HttpHeader
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                                     //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="header">Raw header string.</param>
        public HttpHeader(string header) // header relevant für Token based Security
        {
            Name = Value = string.Empty;

            try
            {
                int n = header.IndexOf(':'); // als Trennung
                Name = header.Substring(0, n).Trim(); // Name vor dem Doppelpunkt
                Value = header.Substring(n + 1).Trim(); // Value danach
            }
            catch(Exception) {}
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                                //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Gets the header name.</summary>
        public string Name
        {
            get; protected set;
        }


        /// <summary>Gets the header value.</summary>
        public string Value
        {
            get; protected set;
        }
    }
}
