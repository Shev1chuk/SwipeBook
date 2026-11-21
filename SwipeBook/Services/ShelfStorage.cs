using System.Text.Json;
using SwipeBook.Models;

namespace SwipeBook.Services;

public static class ShelfStorage
{
    private const string Key = "shelves";

    public static List<Shelf> Load()
    {
        var json = Preferences.Default.Get(Key, "");
        if (string.IsNullOrEmpty(json)) return new();
        try { return JsonSerializer.Deserialize<List<Shelf>>(json) ?? new(); }
        catch { return new(); }
    }

    private static void Save(List<Shelf> shelves) =>
        Preferences.Default.Set(Key, JsonSerializer.Serialize(shelves));

    // Якщо полиця з такою назвою вже є, повертає її
    public static Shelf Create(string name)
    {
        var shelves = Load();
        var existing = shelves.FirstOrDefault(s =>
            string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
        if (existing != null) return existing;

        var shelf = new Shelf { Name = name };
        shelves.Add(shelf);
        Save(shelves);
        return shelf;
    }

    public static void AddBook(string shelfId, int bookId)
    {
        var shelves = Load();
        var shelf = shelves.FirstOrDefault(s => s.Id == shelfId);
        if (shelf == null || shelf.BookIds.Contains(bookId)) return;
        shelf.BookIds.Add(bookId);
        Save(shelves);
    }

    // Додає книжку, а якщо вона вже на полиці, прибирає
    public static void Toggle(string shelfId, int bookId)
    {
        var shelves = Load();
        var shelf = shelves.FirstOrDefault(s => s.Id == shelfId);
        if (shelf == null) return;
        if (!shelf.BookIds.Remove(bookId)) shelf.BookIds.Add(bookId);
        Save(shelves);
    }

    public static void Delete(string shelfId)
    {
        var shelves = Load();
        shelves.RemoveAll(s => s.Id == shelfId);
        Save(shelves);
    }
}