using Avalonia.Media;
using Avalonia.Themes.Fluent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace SCAuditStudio.Design
{
    public class AppTheme
    {
        public FluentThemeMode ThemeMode { get; set; }
        public FontFamily FontFamily { get; set; }
        public ObservableCollection<ContextBrush> Brushes { get; set; }

        /* COLOR ATTRIBUTES */
        public Color BackgroundColor { get; set; }
        public Color ForegroundColor { get; set; }
        public Color DetailColor { get; set; }
        public Color AccentColor { get; set; }
        public Color ControlOutlineColor { get; set; }
        public Color SelectedTextColor { get; set; }

        /* BRUSH ATTRIBUTES */
        public IBrush Background { get => new SolidColorBrush(BackgroundColor); }
        public IBrush Foreground { get => new SolidColorBrush(ForegroundColor); }
        public IBrush Detail { get => new SolidColorBrush(DetailColor); }
        public IBrush Accent { get => new SolidColorBrush(AccentColor); }
        public IBrush ControlOutline { get => new SolidColorBrush(ControlOutlineColor); }
        public IBrush SelectedText { get => new SolidColorBrush(SelectedTextColor); }

        /* STATIC ATTRIBUTES */
        public static AppTheme DefaultDark => new()
        {
            ThemeMode = FluentThemeMode.Dark,
            FontFamily = FontFamily.Parse("Segoe UI"),

            BackgroundColor = Color.Parse("#1F1F1F"),
            ForegroundColor = Color.Parse("#DFDFDF"),
            DetailColor = Color.Parse("#2E2E2E"),
            AccentColor = Color.Parse("#7160E8"),
            ControlOutlineColor = Color.Parse("#3D3D3D"),
            SelectedTextColor = Color.Parse("#DFDFDF"),

            Brushes = new()
            {
                new("Light Red", Color.Parse("#F55762"), Color.Parse("#1F1F1F")),
                new("Petite Orchid", Color.Parse("#D69D85"), Color.Parse("#1F1F1F")),
                new("Primrose", Color.Parse("#DBDC8B"), Color.Parse("#1F1F1F")),
                new("Shamrock", Color.Parse("#48C9A4"), Color.Parse("#1F1F1F")),
                new("Blizzard Blue", Color.Parse("#9CDBEB"), Color.Parse("#1F1F1F")),
                new("Wisteria", Color.Parse("#D8A0DF"), Color.Parse("#1F1F1F"))
            }
        };
        public static AppTheme DefaultLight => new()
        {
            ThemeMode = FluentThemeMode.Light,
            FontFamily = FontFamily.Parse("Segoe UI"),

            BackgroundColor = Color.Parse("#EAF8FF"),
            ForegroundColor = Color.Parse("#1E1E1E"),
            DetailColor = Color.Parse("#2E2E2E"),
            AccentColor = Color.Parse("#006CBE"),
            ControlOutlineColor = Color.Parse("#CCCEDB"),
            SelectedTextColor = Color.Parse("#EEEEF2"),

            Brushes = new()
            {
                new("Light Red", Color.Parse("#F55762"), Color.Parse("#EEEEF2")),
                new("Petite Orchid", Color.Parse("#D69D85"), Color.Parse("#EEEEF2")),
                new("Primrose", Color.Parse("#DBDC8B"), Color.Parse("#EEEEF2")),
                new("Shamrock", Color.Parse("#48C9A4"), Color.Parse("#EEEEF2")),
                new("Blizzard Blue", Color.Parse("#9CDBEB"), Color.Parse("#EEEEF2")),
                new("Wisteria", Color.Parse("#D8A0DF"), Color.Parse("#EEEEF2"))
            }
        };

        public AppTheme()
        {
            ThemeMode = FluentThemeMode.Dark;
            FontFamily = FontFamily.Default;
            Brushes = new() { ContextBrush.Clear };
        }

        public AppTheme(FontFamily fontFamily, Color background, Color accent)
        {
            FontFamily = fontFamily;
            Brushes = new() { ContextBrush.Clear };

            bool isDarkTheme = ColorGrayscale(background) < byte.MaxValue * 0.5f;
            ThemeMode = isDarkTheme ? FluentThemeMode.Dark : FluentThemeMode.Light;

            BackgroundColor = background;
            ForegroundColor = InvertColor(BackgroundColor);
            DetailColor = LerpColor(BackgroundColor, ForegroundColor, 0.15f);
            ControlOutlineColor = DetailColor;
            AccentColor = accent;
            SelectedTextColor = InvertColor(AccentColor);
        }

        /* COLOR OPERATIONS */
        public static float ColorGrayscale(Color a)
        {
            return (a.R + a.G + a.B) / 3f;
        }
        public static Color InvertColor(Color a, bool ignoreAlpha = true)
        {
            return new Color(ignoreAlpha ? a.A : (byte)(byte.MaxValue - a.A), (byte)(byte.MaxValue - a.R), (byte)(byte.MaxValue - a.G), (byte)(byte.MaxValue - a.B));
        }
        public static Color AddColor(Color a, Color b)
        {
            return new Color((byte)(a.A + b.A), (byte)(a.R + b.R), (byte)(a.G + b.G), (byte)(a.B + b.B));
        }
        public static Color SubtractColor(Color a, Color b)
        {
            return new Color((byte)(a.A - b.A), (byte)(a.R - b.R), (byte)(a.G - b.G), (byte)(a.B - b.B));
        }
        public static Color MultiplyColor(Color a, float t)
        {
            return new Color((byte)(a.A * t), (byte)(a.R * t), (byte)(a.G * t), (byte)(a.B * t));
        }
        public static Color LerpColor(Color a, Color b, float t)
        {
            return AddColor(a, MultiplyColor(SubtractColor(b, a), t));
        }

        public class ContextBrush
        {
            public string Name;
            public Color? Color;
            public Color? TextColor;

            public static ContextBrush Clear { get => new("Clear"); }
            public IBrush? Brush { get => Color == null ? null : new SolidColorBrush((Color)Color); }
            public IBrush? TextBrush { get => TextColor == null ? null : new SolidColorBrush((Color)TextColor); }

            public ContextBrush()
            {
                Name = "New ContextBrush";
                Color = new();
                TextColor = new();
            }

            public ContextBrush(string name)
            {
                Name = name;
            }

            public ContextBrush(string name, Color? color, Color? textColor)
            {
                Name = name;
                Color = color;
                TextColor = textColor;
            }
        }
    }
}
