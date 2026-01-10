using System;

namespace Skddkkkk.DevelopKit.BasicTemplate.Runtime
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ComponentOrderAttribute : Attribute
    {
        public int Order { get; }

        public ComponentOrderAttribute(int order)
        {
            Order = order;
        }
    }
}