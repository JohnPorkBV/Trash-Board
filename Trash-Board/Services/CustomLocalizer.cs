using Microsoft.Extensions.Localization;
using System.Globalization;

namespace TrashBoard.Services
{
    public enum LanguageMode
    {
        CultureBased,    // e.g. en-US, fr-FR
        Emoji,
        Morse,
        Braille
    }

    public class CustomLocalizer<T>
    {


        private readonly IStringLocalizer<T> _localizer;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string CurrentCulture => CultureInfo.CurrentUICulture.Name;

        public LanguageMode CurrentMode { get; set; } = LanguageMode.CultureBased;

        public CustomLocalizer(IStringLocalizer<T> localizer, IHttpContextAccessor httpContextAccessor)
        {
            _localizer = localizer;
            _httpContextAccessor = httpContextAccessor;

        }
        public void SetMode(LanguageMode mode)
        {
            CurrentMode = mode;
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
                    _ => value
                };
            }
        }

        // Sample conversions:
        private string ToEmoji(string text) =>
            string.Concat(text.Select(c => char.IsLetter(c) ? $"{char.ToUpper(c)}️⃣" : c.ToString()));

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
