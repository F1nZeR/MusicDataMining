using System.Linq;
using System.Text.RegularExpressions;

namespace Parser.Helpers
{
    public class StringUtilities
    {
        public static string[] GetWords(string input)
        {
            var matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = matches.Cast<Match>()
                .Where(m => !string.IsNullOrEmpty(m.Value))
                .Select(m => TrimSuffix(m.Value));

            return words.ToArray();
        }

        private static string TrimSuffix(string word)
        {
            var apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}