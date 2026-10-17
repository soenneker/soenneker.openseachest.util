using System;
using System.Collections.Generic;
using Soenneker.Utils.PooledStringBuilders;
namespace Soenneker.OpenSeaChest.Util;

internal static class ProcessArguments
{
    internal static string Join(IReadOnlyList<string> arguments)
    {
        using var result = new PooledStringBuilder();
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
