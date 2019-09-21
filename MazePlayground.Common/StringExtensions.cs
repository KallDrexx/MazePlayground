using System.Text;

namespace MazePlayground.Common
{
    internal static class StringExtensions
    {
        internal static string AddSpacesBetweenUpperCaseLetters(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            var builder = new StringBuilder(str.Substring(0, 1));
            for (var x = 1; x < str.Length; x++)
            {
                if (!char.IsWhiteSpace(str[x - 1]) && !char.IsUpper(str[x - 1]) && char.IsUpper(str[x]))
                {
                    builder.Append(" ");
                }

                builder.Append(str[x]);
            }

            return builder.ToString();
        }
    }
}