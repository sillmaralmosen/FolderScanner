using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FolderScanner
{
    public static class EnumerableExt
    {
        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            if (input == null)
                return;
            foreach (T obj in input)
                action(obj);
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> input, Func<T, Task> action)
        {
            if (input == null)
                return;
            foreach (T obj in input)
                await action(obj);
        }
    }
}
