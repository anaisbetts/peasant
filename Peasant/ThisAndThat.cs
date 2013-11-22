using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Peasant
{
    public static class StringExtensions
    {
        public static string ToSHA1(this string This)
        {
            var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(This));

            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    public static class GitHubUrl
    {
        public static Tuple<string, string> NameWithOwner(string githubUrl)
        {
            var m = Regex.Match(githubUrl.ToLowerInvariant(), @"https://github.com/(\w+)/(\w+)");
            if (!m.Success) {
                return null;
            }

            return Tuple.Create(m.Groups[1].Value, m.Groups[2].Value);
        }
    }
}
