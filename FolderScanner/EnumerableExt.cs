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

        public static IEnumerable<T> Traverse<T>(
          this IEnumerable<T> items,
          Func<T, IEnumerable<T>> childSelector)
        {
            Stack<T> stack = new Stack<T>(items);
            while (stack.Any<T>())
            {
                T next = stack.Pop();
                yield return next;
                IEnumerable<T> objs = childSelector(next);
                if (objs != null)
                {
                    foreach (T obj in objs)
                        stack.Push(obj);
                    next = default(T);
                }
            }
        }
    }
}
