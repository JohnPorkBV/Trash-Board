using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;

namespace TrashBoard.Services
{
    public enum LanguageMode
    {
        CultureBased,    // e.g. en-US, fr-FR
        Emoji,
        Morse,
        Braille,
        Minecraft
    }

    public class CustomLocalizer<T>
    {

        private readonly IJSRuntime _js;
        private bool _initialized;
        private readonly IStringLocalizer<T> _localizer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string CurrentCulture => CultureInfo.CurrentUICulture.Name;

        public LanguageMode CurrentMode { get; set; } = LanguageMode.CultureBased;

        public CustomLocalizer(IStringLocalizer<T> localizer, IJSRuntime js)
        {
            _localizer = localizer;
            _js = js;
        }
        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                var modeStr = await _js.InvokeAsync<string>("localStorage.getItem", "customLanguageMode");
                if (Enum.TryParse<LanguageMode>(modeStr, true, out var mode))
                {
                    CurrentMode = mode;
                }
            }
            catch
            {
                // Fail silently if interop not available (e.g. during prerender)
            }
        }

        public async void SetMode(LanguageMode mode)
        {
            CurrentMode = mode;

            try
            {
                if (mode == LanguageMode.CultureBased)
                {
                    await _js.InvokeVoidAsync("localStorage.removeItem", "customLanguageMode");
                }
                else
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", "customLanguageMode", mode.ToString().ToLowerInvariant());

                }
            }
            catch
            {
                // Ignore interop failures (e.g. server-side)
            }
        }



        public string this[string key]
        {
            get
            {
                var value = _localizer[key];

                return CurrentMode switch
                {
                    LanguageMode.Emoji => ToEmoji(value),
                    LanguageMode.Morse => ToMorse(value),
                    LanguageMode.Braille => ToBraille(value),
                    LanguageMode.Minecraft => value,
                    _ => value
                };
            }
        }

        // Sample conversions:
        private static readonly List<string> MemeEmojis = new()
        {
            "😂", "😎", "🔥", "💀", "🤡", "👻", "🍕", "🐸", "💩", "🦄",
            "🚀", "🥶", "🥵", "😱", "😈", "🫠", "🥸", "🗿", "🦍", "👽",
            "🧠", "👀", "😵", "🤯", "🫥", "😹"
        };

        private static readonly Random Random = new();

        private string ToEmoji(string text)
        {
            return string.Concat(text.Select(c =>
                char.IsLetter(c)
                    ? MemeEmojis[Random.Next(MemeEmojis.Count)]
                    : c.ToString()
            ));
        }

        private string ToMorse(string text)
        {
            var morseMap = new Dictionary<char, string>
            {
                ['A'] = ".-",
                ['B'] = "-...",
                ['C'] = "-.-.",
                ['D'] = "-..",
                ['E'] = ".",
                ['F'] = "..-.",
                ['G'] = "--.",
                ['H'] = "....",
                ['I'] = "..",
                ['J'] = ".---",
                ['K'] = "-.-",
                ['L'] = ".-..",
                ['M'] = "--",
                ['N'] = "-.",
                ['O'] = "---",
                ['P'] = ".--.",
                ['Q'] = "--.-",
                ['R'] = ".-.",
                ['S'] = "...",
                ['T'] = "-",
                ['U'] = "..-",
                ['V'] = "...-",
                ['W'] = ".--",
                ['X'] = "-..-",
                ['Y'] = "-.--",
                ['Z'] = "--..",
                [' '] = " / "
            };

            return string.Join(" ", text.ToUpper().Select(c => morseMap.GetValueOrDefault(c, "?")));
        }

        private string ToBraille(string text)
        {
            var brailleMap = new Dictionary<char, string>
            {
                ['A'] = "⠁",
                ['B'] = "⠃",
                ['C'] = "⠉",
                ['D'] = "⠙",
                ['E'] = "⠑",
                ['F'] = "⠋",
                ['G'] = "⠛",
                ['H'] = "⠓",
                ['I'] = "⠊",
                ['J'] = "⠚",
                ['K'] = "⠅",
                ['L'] = "⠇",
                ['M'] = "⠍",
                ['N'] = "⠝",
                ['O'] = "⠕",
                ['P'] = "⠏",
                ['Q'] = "⠟",
                ['R'] = "⠗",
                ['S'] = "⠎",
                ['T'] = "⠞",
                ['U'] = "⠥",
                ['V'] = "⠧",
                ['W'] = "⠺",
                ['X'] = "⠭",
                ['Y'] = "⠽",
                ['Z'] = "⠵",
                [' '] = "⠀"
            };

            return string.Concat(text.ToUpper().Select(c => brailleMap.GetValueOrDefault(c, c.ToString())));
        }

    }
}
