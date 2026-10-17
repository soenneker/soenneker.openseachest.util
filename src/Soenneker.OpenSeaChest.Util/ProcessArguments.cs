using System;
using System.Collections.Generic;
using System.Text;
namespace Soenneker.OpenSeaChest.Util;

internal static class ProcessArguments
{
    internal static string Join(IReadOnlyList<string> arguments)
    {
        var result = new StringBuilder();
        foreach (string argument in arguments)
        {
            ArgumentNullException.ThrowIfNull(argument);
            if (argument.Contains('\0'))
                throw new ArgumentException("Process arguments cannot contain null characters.", nameof(arguments));
            if (result.Length != 0)
                result.Append(' ');
            result.Append('"');
            int backslashes = 0;
            foreach (char character in argument)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }
                result.Append('\\', character == '"' ? backslashes * 2 + 1 : backslashes);
                result.Append(character);
                backslashes = 0;
            }
            result.Append('\\', backslashes * 2);
            result.Append('"');
        }
        return result.ToString();
    }
}
