using System.Text;

namespace TrashBoard.Services
{
    public static class CSVParser
    {
        public static string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            values.Add(current.ToString());
            return values.ToArray();
        }

        public static (DateTime start, DateTime? end) ParseDutchDateRange(string input)
        {
            var today = DateTime.Today;
            var year = today.Year;

            input = input.Replace("–", "-").Replace("—", "-").Replace("+", ",");
            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

            DateTime? firstStart = null, lastEnd = null;

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                var range = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);

                if (range.Length == 1)
                {
                    var singleDate = ParseDayMonth(range[0], year);
                    firstStart ??= singleDate;
                    lastEnd = singleDate;
                }
                else if (range.Length == 2)
                {
                    var startDate = ParseDayMonth(range[0], year);
                    var endDate = ParseDayMonth(range[1], year, startDate.Month);
                    firstStart ??= startDate;
                    lastEnd = endDate;
                }
            }

            return (firstStart ?? today, lastEnd != firstStart ? lastEnd : null);
        }

        private static DateTime ParseDayMonth(string input, int year, int? fallbackMonth = null)
        {
            var parts = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var day = int.Parse(parts[0]);
                var month = ParseDutchMonth(parts[1]);
                return new DateTime(year, month, day);
            }
            else if (parts.Length == 1 && fallbackMonth.HasValue)
            {
                var day = int.Parse(parts[0]);
                return new DateTime(year, fallbackMonth.Value, day);
            }

            throw new FormatException($"Invalid date format: '{input}'");
        }

        private static int ParseDutchMonth(string month)
        {
            return month.ToLower() switch
            {
                "januari" => 1,
                "februari" => 2,
                "maart" => 3,
                "april" => 4,
                "mei" => 5,
                "juni" => 6,
                "juli" => 7,
                "augustus" or "aug" => 8,
                "september" or "sept" => 9,
                "oktober" or "okt" => 10,
                "november" => 11,
                "december" or "dec" => 12,
                _ => throw new FormatException($"Onbekende maand: '{month}'")
            };
        }
    }
}
