using System.Net;

namespace Cortex.CMS.Pages {
    interface IPageRequest {
        string GetTitle(PageRequestClient client);
        string GetBody(PageRequestClient client);

        bool GetAccess(PageRequestClient client) => true;
        bool GetPage(PageRequestClient client) => true;

        void Evaluate(PageRequestClient client) {}
    }
}
