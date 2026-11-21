using SwipeBook.Services;

namespace SwipeBook.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public string Genre { get; set; } = "";
    public string Description { get; set; } = "";

    public string Excerpt => BookExcerpts.Get(Id);

    // Ім'я файлу обкладинки в Resources\Images (без розширення)
    public string Cover => $"book_{Id}_cover";
}