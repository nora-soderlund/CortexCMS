namespace CortexCMS.Pages.Public {
    class Index : IPage {
        public string Path => "/index";

        public string Title => "Project Cortex";
        public string Description => "Project Cortex is a community project replicating the modern Habbo Hotel client using modern technologies like HTML5.";
        public string Image => "";

        public string GetBody() {
            return "hi world";
        }
    }
}
