using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>Unity Object가 구현한 인터페이스를 Inspector에서 직렬화합니다.</summary>
    [Serializable]
    public class InterfaceReference<TInterface, TObject>
        where TInterface : class
        where TObject : Object
    {
        [SerializeField, HideInInspector] private TObject underlyingValue;

        public InterfaceReference()
        {
        }

        public InterfaceReference(TObject target)
        {
            UnderlyingValue = target;
        }

        public InterfaceReference(TInterface value)
        {
            Value = value;
        }

        public bool IsAssigned => underlyingValue != null;

        public TInterface Value
        {
            get
            {
                if (underlyingValue == null)
                    return null;

                if (underlyingValue is TInterface value)
                    return value;

                throw CreateTypeException(underlyingValue);
            }
            set
            {
                if (value == null)
                {
                    underlyingValue = null;
                    return;
                }

                if (value is not TObject target)
                    throw new ArgumentException(
                        $"{value.GetType().Name} cannot be stored as {typeof(TObject).Name}.",
                        nameof(value));

                Validate(target);
                underlyingValue = target;
            }
        }

        public TObject UnderlyingValue
        {
            get => underlyingValue;
            set
            {
                Validate(value);
                underlyingValue = value;
            }
        }

        public bool TryGetValue(out TInterface value)
        {
            value = underlyingValue as TInterface;
            return value != null;
        }

        public static implicit operator TInterface(InterfaceReference<TInterface, TObject> reference) =>
            reference?.Value;

        private static void Validate(TObject value)
        {
            if (value != null && value is not TInterface)
                throw CreateTypeException(value);
        }

        private static InvalidOperationException CreateTypeException(Object value) =>
            new($"{value.name} must implement {typeof(TInterface).Name}.");
    }

    [Serializable]
    public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object>
        where TInterface : class
    {
        public InterfaceReference()
        {
        }

        public InterfaceReference(Object target) : base(target)
        {
        }

        public InterfaceReference(TInterface value) : base(value)
        {
        }
    }
}