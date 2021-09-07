using System.Net;
using System.Collections.Generic;

namespace Cortex.CMS.API {
    interface IAPIRequest {
        object Evaluate(HttpListenerContext context, string method, string body);
    }
}
