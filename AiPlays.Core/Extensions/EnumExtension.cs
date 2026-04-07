namespace AiPlays.Core.Extensions
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
