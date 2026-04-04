using System;
using System.Collections.Generic;
using System.Text;

namespace AiPlays.Brain.Extensions
{
    public static class EnumExtension
    {
        public static T ToEnum<T> (this string value) where T : struct, Enum
        {
            if (Enum.TryParse<T>(value, true, out var result))
            {
                return result;
            }

            return default (T);
        }
    }
}
