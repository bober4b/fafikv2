using System.Text;

namespace Fafikv2.Services.OtherServices
{
    public class PolishLettersConverter
    {
        private static readonly Dictionary<char, char> PolishToEnglishMap = new()
        {
            { 'ą', 'a' }, { 'ć', 'c' }, { 'ę', 'e' }, { 'ł', 'l' }, { 'ń', 'n' },
            { 'ó', 'o' }, { 'ś', 's' }, { 'ź', 'z' }, { 'ż', 'z' },
            { 'Ą', 'A' }, { 'Ć', 'C' }, { 'Ę', 'E' }, { 'Ł', 'L' }, { 'Ń', 'N' },
            { 'Ó', 'O' }, { 'Ś', 'S' }, { 'Ź', 'Z' }, { 'Ż', 'Z' }
        };



        public static bool ContainsPolishChars(string input)
        {
            return input.Any(c => PolishToEnglishMap.ContainsKey(c));
        }

        public static string ReplacePolishChars(string input)
        {
            var sb = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                sb.Append(PolishToEnglishMap.TryGetValue(c, out var replacement) ? replacement : c);
            }
            return sb.ToString();
        }
    }
}
