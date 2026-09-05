using System;
using System.Globalization;

namespace IIMLib.Dialogue.Parsing.BuiltIn
{
    internal static class NumericExpressionParser
    {
        public static bool TrySum(ReadOnlySpan<char> expression, out double result)
        {
            result = 0d;
            if (IsWhiteSpace(expression))
                return true;

            var cursor = 0;
            while (true)
            {
                if (!TryReadOperand(expression, '+', ref cursor, out var operand, out var hasMore))
                    return false;

                result += operand;
                if (!hasMore)
                    return true;
            }
        }

        public static bool TryMultiply(ReadOnlySpan<char> expression, out double result)
        {
            if (IsWhiteSpace(expression))
            {
                result = 0d;
                return true;
            }

            result = 1d;
            var cursor = 0;
            while (true)
            {
                if (!TryReadOperand(expression, '*', ref cursor, out var operand, out var hasMore))
                    return false;

                result *= operand;
                if (!hasMore)
                    return true;
            }
        }

        public static bool TryDivide(
            ReadOnlySpan<char> expression,
            out double result,
            out bool divisionByZero)
        {
            divisionByZero = false;

            if (IsWhiteSpace(expression))
            {
                result = 0d;
                return true;
            }

            var cursor = 0;
            if (!TryReadOperand(expression, '/', ref cursor, out result, out var hasMore))
                return false;

            while (hasMore)
            {
                if (!TryReadOperand(expression, '/', ref cursor, out var divisor, out hasMore))
                    return false;

                if (divisor == 0d)
                {
                    divisionByZero = true;
                    return false;
                }

                result /= divisor;
            }

            return true;
        }

        private static bool TryReadOperand(
            ReadOnlySpan<char> expression,
            char separator,
            ref int cursor,
            out double operand,
            out bool hasMore)
        {
            var separatorIndex = FindSeparator(expression, separator, cursor);
            var end = separatorIndex >= 0 ? separatorIndex : expression.Length;
            var start = cursor;

            while (start < end && char.IsWhiteSpace(expression[start]))
                start++;

            while (end > start && char.IsWhiteSpace(expression[end - 1]))
                end--;

            if (start >= end ||
                !double.TryParse(
                    expression.Slice(start, end - start),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out operand))
            {
                operand = 0d;
                hasMore = false;
                return false;
            }

            hasMore = separatorIndex >= 0;
            cursor = hasMore ? separatorIndex + 1 : expression.Length;
            return true;
        }

        private static int FindSeparator(
            ReadOnlySpan<char> expression,
            char separator,
            int start)
        {
            var tokenStart = start;
            while (tokenStart < expression.Length && char.IsWhiteSpace(expression[tokenStart]))
                tokenStart++;

            for (var i = start; i < expression.Length; i++)
            {
                if (expression[i] != separator)
                    continue;

                if (separator == '+' && IsNumberSign(expression, tokenStart, i))
                    continue;

                return i;
            }

            return -1;
        }

        private static bool IsNumberSign(
            ReadOnlySpan<char> expression,
            int tokenStart,
            int signIndex)
        {
            if (signIndex == tokenStart)
                return true;

            var previous = signIndex - 1;
            while (previous >= tokenStart && char.IsWhiteSpace(expression[previous]))
                previous--;

            return previous >= tokenStart &&
                   (expression[previous] == 'e' || expression[previous] == 'E');
        }

        private static bool IsWhiteSpace(ReadOnlySpan<char> value)
        {
            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                    return false;
            }

            return true;
        }
    }
}
