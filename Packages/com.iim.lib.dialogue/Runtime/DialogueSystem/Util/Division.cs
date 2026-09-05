using System;
using System.Globalization;

namespace DialogueSystem.Util
{
    public struct Division : IDialogueFunc
    {
        public const string KEY = "Division";
        private const float WHOLE_NUMBER_TOLERANCE = 0.0001f;
        private const float ZERO_TOLERANCE = 0.0000001f;

        public string Identifier => KEY;

        public string CollectInfo(string s = null)
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            var numbers = s.Replace(" ", string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (numbers.Length == 0 || !float.TryParse(numbers[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) return string.Empty;

            for (var i = 1; i < numbers.Length; i++)
            {
                if (!float.TryParse(numbers[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || MathF.Abs(value) < ZERO_TOLERANCE) return string.Empty;

                result /= value;
            }

            return MathF.Abs(result % 1f) < WHOLE_NUMBER_TOLERANCE ? ((int)result).ToString(CultureInfo.InvariantCulture) : result.ToString(CultureInfo.InvariantCulture);
        }
    }
}