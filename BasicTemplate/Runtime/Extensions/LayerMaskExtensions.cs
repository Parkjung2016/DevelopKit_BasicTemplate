using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public static class LayerMaskExtensions
    {
        /// <summary>
        /// 지정한 레이어만 포함하는 <see cref="LayerMask"/>를 생성합니다.
        /// </summary>
        /// <param name="layer">마스크에 포함할 레이어 인덱스입니다.</param>
        /// <returns><paramref name="layer"/>만 포함하는 LayerMask를 반환합니다.</returns>
        /// <example>
        /// <code>
        /// LayerMask playerOnlyMask = playerLayer.CreateFromLayer();
        /// </code>
        /// </example>
        public static LayerMask CreateFromLayer(this int layer) => 1 << layer;
        
        /// <summary>
        /// 주어진 레이어 번호가 LayerMask에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="mask">확인할 LayerMask입니다.</param>
        /// <param name="layerNumber">LayerMask에 포함되어 있는지 확인할 레이어 번호입니다.</param>
        /// <returns>레이어 번호가 LayerMask에 포함되어 있으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool Contains(this LayerMask mask, int layerNumber)
        {
            return mask == (mask | (1 << layerNumber));
        }
    }
}