using System.Security.Cryptography;

namespace PollSurveyBuilder.Application.Common
{
    /// <summary>
    /// Generates short, URL-safe codes like "7fGh2" for poll links (/poll/7fGh2).
    /// Uses a crypto RNG so codes aren't guessable/enumerable, and excludes visually
    /// ambiguous characters (0/O, 1/l/I) to keep codes easy to read off a QR scan or screen-share.
    /// </summary>
    public static class ShortCodeGenerator
    {
        private const string Alphabet = "23456789abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ";

        public static string Generate(int length = 6)
        {
            Span<byte> buffer = stackalloc byte[length];
            RandomNumberGenerator.Fill(buffer);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = Alphabet[buffer[i] % Alphabet.Length];
            }
            return new string(chars);
        }
    }
}
