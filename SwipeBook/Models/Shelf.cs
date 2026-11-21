namespace SwipeBook.Models;

public class Shelf
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public List<int> BookIds { get; set; } = new();
}

// Для відображення на сторінці полиць
public class ShelfView
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string CountText { get; set; } = "";
    public List<BookListItem> Books { get; set; } = new();
}