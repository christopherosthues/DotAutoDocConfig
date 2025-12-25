using System.Collections.Generic;

namespace DotAutoDocConfig.SourceGenerator.Extensions;

internal static class FileNameExtensions
{
    extension(string fileName)
    {
        public string EnsureUniqueFileName(string ext, ISet<string> usedNames)
        {
            // TODO: check for comment at the top of the file if it exists already
            string candidate = fileName + ext;
            int i = 2;
            while (!usedNames.Add(candidate))
            {
                candidate = fileName + "-" + i + ext;
                i++;
            }

            return candidate;
        }
    }
}
