using System.Net;

namespace CortexCMS.Pages {
    interface IPageRequest {
        string GetTitle(HttpListenerContext context);
        string GetBody(HttpListenerContext context);
    }
}
