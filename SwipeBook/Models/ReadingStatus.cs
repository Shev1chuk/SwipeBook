namespace SwipeBook.Models;

public enum ReadingStatus
{
    None,
    WantToRead,
    Reading,
    Done
}

public class BookStatusEntry
{
    public int BookId { get; set; }
    public ReadingStatus Status { get; set; }
    public DateTime Date { get; set; }
}

public static class StatusText
{
    public static string ToText(ReadingStatus status) => status switch
    {
        ReadingStatus.WantToRead => "📌 Хочу прочитати",
        ReadingStatus.Reading => "📖 Читаю",
        ReadingStatus.Done => "✅ Прочитано",
        _ => ""
    };
}