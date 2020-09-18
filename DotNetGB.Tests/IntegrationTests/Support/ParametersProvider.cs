using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetGB.Tests.IntegrationTests.Support
{
    public static class ParametersProvider
    {
        private static readonly string[] EXCLUDES =
        {
            "-mgb.gb",
            "-sgb.gb",
            "-sgb2.gb",
            "-S.gb",
            "-A.gb",
        };

        public static ICollection<object[]> GetParameters(string dirName) 
            => GetParameters(dirName, EXCLUDES, false);

        public static ICollection<object[]> GetParameters(string dirName, bool recurse) 
            => GetParameters(dirName, EXCLUDES, recurse);

        public static ICollection<object[]> GetParameters(string dirName, IList<string> excludes, bool recurse) 
            => new DirectoryInfo(Path.Combine("Resources", dirName))
                .EnumerateFiles("*.gb", recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(f => !excludes.Any(p => f.Name.EndsWith(p)))
                .Select(p => (new object[] { Path.GetRelativePath(Directory.GetCurrentDirectory(), p.FullName), p }))
                .ToList();
    }
}
