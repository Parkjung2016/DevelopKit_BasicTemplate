using System;
using System.Collections.Generic;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class EnumerableExtensions
    {
        /// <summary>모든 항목에 action을 실행합니다.</summary>
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (T item in sequence)
                action(item);
        }

        /// <summary>열거 가능한 항목에서 하나를 같은 확률로 선택합니다.</summary>
        public static T Random<T>(this IEnumerable<T> sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (sequence is IList<T> list)
            {
                if (list.Count == 0)
                    throw new InvalidOperationException("빈 컬렉션에서는 항목을 선택할 수 없습니다.");

                return list[UnityEngine.Random.Range(0, list.Count)];
            }

            using IEnumerator<T> enumerator = sequence.GetEnumerator();
            if (!enumerator.MoveNext())
                throw new InvalidOperationException("빈 컬렉션에서는 항목을 선택할 수 없습니다.");

            T selected = enumerator.Current;
            int count = 1;
            while (enumerator.MoveNext())
            {
                count++;
                if (UnityEngine.Random.Range(0, count) == 0)
                    selected = enumerator.Current;
            }

            return selected;
        }
    }
}