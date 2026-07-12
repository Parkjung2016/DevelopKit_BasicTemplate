using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class BoundsExtensions
    {
        /// <summary>
        /// 두 Bounds를 모두 포함하는 새로운 Bounds를 반환합니다.
        /// 메서드 체이닝 방식으로 사용할 수 있습니다.
        /// </summary>
        /// <param name="bounds">기준이 되는 Bounds입니다.</param>
        /// <param name="other">포함할 Bounds입니다.</param>
        /// <returns>
        /// <paramref name="bounds"/>와 <paramref name="other"/>를 모두 포함하는 새로운 Bounds를 반환합니다.
        /// </returns>
        /// <example>
        /// <code>
        /// Bounds combined = rendererA.bounds
        ///     .ExpandToInclude(rendererB.bounds)
        ///     .ExpandToInclude(rendererC.bounds);
        /// </code>
        /// </example>
        public static Bounds ExpandToInclude(this Bounds bounds, Bounds other)
        {
            bounds.Encapsulate(other);
            return bounds;
        }
    }
}