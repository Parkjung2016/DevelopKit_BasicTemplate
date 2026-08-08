using System;
using System.Collections.Generic;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>컬렉션의 상태 확인과 내용 교체에 사용하는 기본 확장 메서드입니다.</summary>
    public static class ListExtensions
    {
        /// <summary>컬렉션이 null이거나 항목이 없는지 확인합니다.</summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) =>
            collection == null || collection.Count == 0;

        /// <summary>열거 가능한 항목을 새 List로 복사합니다.</summary>
        public static List<T> Clone<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return new List<T>(source);
        }

        /// <summary>두 인덱스의 항목을 서로 교환합니다.</summary>
        public static void Swap<T>(this IList<T> list, int firstIndex, int secondIndex)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (firstIndex == secondIndex)
                return;

            (list[firstIndex], list[secondIndex]) = (list[secondIndex], list[firstIndex]);
        }

        /// <summary>기존 List 인스턴스는 유지하고 내용을 새 항목으로 교체합니다.</summary>
        public static void ReplaceWith<T>(this List<T> list, IEnumerable<T> items)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            list.Clear();
            list.AddRange(items);
        }

        /// <summary>기존 이름을 사용하는 코드에서 List 내용을 새 항목으로 교체합니다.</summary>
        public static void RefreshWith<T>(this List<T> list, IEnumerable<T> items) =>
            list.ReplaceWith(items);

        /// <summary>Fisher-Yates 방식으로 List의 항목 순서를 섞습니다.</summary>
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            for (int remaining = list.Count; remaining > 1; remaining--)
            {
                int index = UnityEngine.Random.Range(0, remaining);
                int last = remaining - 1;
                (list[index], list[last]) = (list[last], list[index]);
            }

            return list;
        }
    }
}