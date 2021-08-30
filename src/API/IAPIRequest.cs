using System.Net;
using System.Collections.Generic;

namespace CortexCMS.API {
    interface IAPIRequest {
        object Evaluate(HttpListenerContext context, string method, string body);
    }
}
