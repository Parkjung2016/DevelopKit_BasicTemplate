using System;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>ComponentManager의 초기화, AfterInitialize, Update 실행 순서를 지정합니다.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class ComponentOrderAttribute : Attribute
    {
        public ComponentOrderAttribute(int order) => Order = order;

        public int Order { get; }
    }
}
