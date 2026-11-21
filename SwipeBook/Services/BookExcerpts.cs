namespace SwipeBook.Services;

public static class BookExcerpts
{
    private static readonly Dictionary<int, string> Excerpts = new()
    {
       
    };

    public static string Get(int id) =>
        Excerpts.TryGetValue(id, out var text) ? text : "";
}