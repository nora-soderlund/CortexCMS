namespace CortexCMS.Pages {
    interface IPage {

        string Path { get; }

        string Title { get; }
        string Description { get; }
        string Image { get; }
        
        string GetHead() {
            return
                $"<title>{Title}</title>" +
                $"<meta property='og:title' content='{Title}'>" +
                $"<meta property='og:description' content='{Description}'>" +
                $"<meta property='og:image' content='{Image}'>";
        }

        string GetBody();
    }
}
