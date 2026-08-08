namespace MusicPlatform.UI.Helpers;

public static class CoverHelper
{
    private static readonly (string From, string To)[] Palette =
    {
        ("#6C4DF6", "#2D1B69"),
        ("#E91E63", "#7B1230"),
        ("#00BCD4", "#04616E"),
        ("#F0AD4E", "#8A5C15"),
        ("#28A745", "#12551F"),
        ("#9C27B0", "#4A1256"),
        ("#F44336", "#7A1A12"),
        ("#3F51B5", "#1D2560")
    };

    public static (string From, string To) ColorFor(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Palette[0];

        var hash = text.Aggregate(0, (acc, c) => acc + c);
        return Palette[Math.Abs(hash) % Palette.Length];
    }

    public static string InitialsOf(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "?";

        var words = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return words.Length >= 2
            ? $"{words[0][0]}{words[1][0]}"
            : words[0].Length >= 2 ? words[0][..2] : words[0][..1];
    }

    public static string GradientStyle(string? text)
    {
        var (from, to) = ColorFor(text);
        return $"--lc-from:{from};--lc-to:{to}";
    }
}