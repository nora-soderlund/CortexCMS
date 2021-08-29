using System.Net;
using System.Collections.Generic;

namespace CortexCMS.API {
    interface IAPIRequest {
        Dictionary<string, object> Handle(HttpListenerContext context, string method);
    }
}
