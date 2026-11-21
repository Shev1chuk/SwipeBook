using System.Text.Json;
using System.Text.Json.Serialization;
using SwipeBook.Models;

namespace SwipeBook.Services;

public static class SwipeStorage
{
    private class SwipeData
    {
        public List<int> Liked { get; set; } = new();
        public List<int> Disliked { get; set; } = new();
        public List<BookStatusEntry> Statuses { get; set; } = new();
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static readonly string DataFilePath =
        System.IO.Path.Combine(FileSystem.AppDataDirectory, "swipes.json");

    // ---------- Лайки й дизлайки ----------
    public static List<int> LoadLiked() => Read().Liked;
    public static List<int> LoadDisliked() => Read().Disliked;

    public static void Save(IEnumerable<int> liked, IEnumerable<int> disliked)
    {
        var data = Read();
        data.Liked = liked.Distinct().ToList();
        data.Disliked = disliked.Distinct().ToList();
        Write(data);
    }

    // Скидає лише свайпи. Статуси читання (хочу/читаю/прочитано) залишаються
    public static void Clear()
    {
        var data = Read();
        data.Liked.Clear();
        data.Disliked.Clear();
        Write(data);
    }

    // ---------- Статуси читання ----------
    public static List<BookStatusEntry> LoadStatuses() => Read().Statuses;

    public static ReadingStatus GetStatus(int bookId) =>
        Read().Statuses.FirstOrDefault(s => s.BookId == bookId)?.Status ?? ReadingStatus.None;

    public static void SetStatus(int bookId, ReadingStatus status)
    {
        var data = Read();
        var existing = data.Statuses.FirstOrDefault(s => s.BookId == bookId);

        if (status == ReadingStatus.None)
        {
            if (existing != null) data.Statuses.Remove(existing);
        }
        else if (existing == null)
        {
            data.Statuses.Add(new BookStatusEntry { BookId = bookId, Status = status, Date = DateTime.Now });
        }
        else if (existing.Status != status)
        {
            existing.Status = status;
            existing.Date = DateTime.Now;
        }

        Write(data);
    }

    // ---------- Робота з файлом ----------
    private static void Write(SwipeData data)
    {
        try
        {
            File.WriteAllText(DataFilePath, JsonSerializer.Serialize(data, JsonOptions));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SwipeBook] Помилка збереження: {ex.Message}");
        }
    }

    private static SwipeData Read()
    {
        try
        {
            if (!File.Exists(DataFilePath)) return new SwipeData();
            var json = File.ReadAllText(DataFilePath);
            return JsonSerializer.Deserialize<SwipeData>(json, JsonOptions) ?? new SwipeData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SwipeBook] Помилка читання: {ex.Message}");
            return new SwipeData();
        }
    }
}