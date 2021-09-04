using System.Net;

namespace CortexCMS.Pages {
    interface IPageRequest {
        string GetTitle(PageRequestClient client);
        string GetBody(PageRequestClient client);

        bool GetAccess(PageRequestClient client) => true;

        void Evaluate(PageRequestClient client) {}
    }
}
