namespace SwipeBook.Services;

public static class GenreStyle
{
    private static readonly Dictionary<string, (string From, string To, string Emoji)> Styles = new()
    {
        ["Фентезі"] = ("#7E57C2", "#4527A0", "🐉"),
        ["Антиутопія"] = ("#607D8B", "#263238", "🏙️"),
        ["Притча"] = ("#F4A261", "#E76F51", "🌹"),
        ["Класика"] = ("#8D6E63", "#4E342E", "📜"),
        ["Детектив"] = ("#A63D40", "#5C1A1B", "🔍"),
        ["Фантастика"] = ("#26A69A", "#00695C", "🚀"),
        ["Підліткова проза"] = ("#EC407A", "#AD1457", "🎒"),
        ["Історичне"] = ("#C9A227", "#8D6E0F", "🏰"),
        ["Пригоди"] = ("#29B6F6", "#0277BD", "🧭"),
        ["Жахи"] = ("#455A64", "#000000", "🦇"),
        ["Науково-популярне"] = ("#66BB6A", "#2E7D32", "🔬"),
    };

    public static (Color From, Color To, string Emoji) Get(string genre)
    {
        var s = Styles.TryGetValue(genre, out var found) ? found : ("#9E9E9E", "#616161", "📚");
        return (Color.FromArgb(s.Item1), Color.FromArgb(s.Item2), s.Item3);
    }
}