using System;
using System.Globalization;

namespace DialogueSystem.Util
{
    public struct Multiply : IDialogueFunc
    {
        public const string KEY = "Multiply";

        public string Identifier => KEY;

        public string CollectInfo(string s = null)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;

            var numbers = s.Replace(" ", string.Empty).Split('*', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length == 0 || !float.TryParse(numbers[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return string.Empty;

            for (var i = 1; i < numbers.Length; i++)
            {
                if (!float.TryParse(numbers[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    return string.Empty;

                result *= value;
            }

            return result % 1f == 0f ? ((int)result).ToString(CultureInfo.InvariantCulture) : result.ToString(CultureInfo.InvariantCulture);
        }
    }
}