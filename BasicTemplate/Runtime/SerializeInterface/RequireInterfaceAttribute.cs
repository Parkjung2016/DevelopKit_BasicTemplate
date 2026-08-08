using System;
using UnityEngine;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>Object 필드에 지정한 인터페이스 구현만 할당할 수 있게 합니다.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class RequireInterfaceAttribute : PropertyAttribute
    {
        public RequireInterfaceAttribute(Type interfaceType)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException("RequireInterface requires an interface type.", nameof(interfaceType));

            InterfaceType = interfaceType;
        }

        public Type InterfaceType { get; }
    }
}