using System.Collections.Generic;
using System.Text;

namespace Entropy.AIO.General
{
    class Optimizations
    {
        public static bool ContainsLoop<T>(List<T> list, T value)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static int IntParseFast(string value)
        {
            // An optimized int parse method.
            var result = 0;
            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < value.Length; i++)
            {
                result = 10 * result + (value[i] - 48);
            }

            return result;
        }

        // Priorize char > String
        public static string Replace(string inputString, char itemToReplace, char replaceWith)
        {
            var sb = new StringBuilder(inputString);
            sb.Replace(itemToReplace, replaceWith);
            return sb.ToString();
        }

        public static string Replace(string inputString, string itemToReplace, string replaceWith)
        {
            var sb = new StringBuilder(inputString);
            sb.Replace(itemToReplace, replaceWith);
            return sb.ToString();
        }
    }
}