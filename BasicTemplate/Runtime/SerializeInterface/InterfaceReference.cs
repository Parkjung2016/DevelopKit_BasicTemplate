using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    [Serializable]
    public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
    {
        [SerializeField, HideInInspector] TObject underlyingValue;

        public TInterface Value
        {
            get
            {
                Object unityObject = underlyingValue;
                if (unityObject == null)
                    return null;

                if (underlyingValue is TInterface @interface)
                    return @interface;

                throw new InvalidOperationException(
                    $"{underlyingValue} needs to implement interface {nameof(TInterface)}.");
            }
            set
            {
                if (value == null)
                {
                    underlyingValue = null;
                    return;
                }

                if (value is TObject newValue)
                {
                    underlyingValue = newValue;
                    return;
                }

                throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty);
            }
        }

        public TObject UnderlyingValue
        {
            get => underlyingValue;
            set => underlyingValue = value;
        }

        public InterfaceReference()
        {
        }

        public InterfaceReference(TObject target) => underlyingValue = target;

        public InterfaceReference(TInterface @interface) => underlyingValue = @interface as TObject;

        public static implicit operator TInterface(InterfaceReference<TInterface, TObject> obj) => obj.Value;
    }

    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class
    {
    }
}