using System;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(this string value) => string.IsNullOrWhiteSpace(value);
        public static bool IsNullOrEmpty(this string value) => string.IsNullOrEmpty(value);
        public static bool IsBlank(this string value) => string.IsNullOrWhiteSpace(value);
        public static string OrEmpty(this string value) => value ?? string.Empty;

        /// <summary>문자열이 지정한 길이보다 길면 앞부분만 반환합니다.</summary>
        public static string Shorten(this string value, int maxLength)
        {
            if (maxLength < 0)
                throw new ArgumentOutOfRangeException(nameof(maxLength));
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength);
        }

        /// <summary>startIndex부터 endIndex 직전까지 반환합니다. 음수 endIndex는 뒤에서부터 계산합니다.</summary>
        public static string Slice(this string value, int startIndex, int endIndex)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if ((uint)startIndex > (uint)value.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (endIndex < 0)
                endIndex += value.Length;
            if (endIndex < startIndex || endIndex > value.Length)
                throw new ArgumentOutOfRangeException(nameof(endIndex));

            return value.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>영문자, 숫자, 밑줄과 선택적으로 마침표만 남깁니다.</summary>
        public static string ConvertToAlphanumeric(this string input, bool allowPeriods = false)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            char[] buffer = new char[input.Length];
            int count = 0;
            for (int i = 0; i < input.Length; i++)
            {
                char character = input[i];
                bool allowed = char.IsLetterOrDigit(character)
                    || character == '_'
                    || allowPeriods && character == '.';
                if (!allowed)
                    continue;
                if (count == 0 && (char.IsDigit(character) || character == '.'))
                    continue;

                buffer[count++] = character;
            }

            while (count > 0 && buffer[count - 1] == '.')
                count--;

            return count == 0 ? string.Empty : new string(buffer, 0, count);
        }

        public static string RemoveAllSpaces(this string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace(" ", string.Empty);

        public static string RemoveAllWhitespace(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            char[] buffer = new char[value.Length];
            int count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                char character = value[i];
                if (!char.IsWhiteSpace(character))
                    buffer[count++] = character;
            }

            return count == value.Length ? value : new string(buffer, 0, count);
        }

        public static string RichColor(this string text, string color) => $"<color={color}>{text}</color>";
        public static string RichSize(this string text, int size) => $"<size={size}>{text}</size>";
        public static string RichBold(this string text) => $"<b>{text}</b>";
        public static string RichItalic(this string text) => $"<i>{text}</i>";
        public static string RichUnderline(this string text) => $"<u>{text}</u>";
        public static string RichStrikethrough(this string text) => $"<s>{text}</s>";
        public static string RichFont(this string text, string font) => $"<font={font}>{text}</font>";
        public static string RichAlign(this string text, string align) => $"<align={align}>{text}</align>";
        public static string RichGradient(this string text, string color1, string color2) =>
            $"<gradient={color1},{color2}>{text}</gradient>";
        public static string RichRotation(this string text, float angle) => $"<rotate={angle}>{text}</rotate>";
        public static string RichSpace(this string text, float space) => $"<space={space}>{text}</space>";
    }
}
