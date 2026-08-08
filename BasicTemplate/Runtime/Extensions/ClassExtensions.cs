using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    /// <summary>일반 C# 객체에 사용하는 확장 메서드입니다.</summary>
    public static class ClassExtensions
    {
        /// <summary>
        /// 직렬화 가능한 객체를 깊은 복사합니다. 큰 객체나 반복 호출에는 전용 복사 코드를 권장합니다.
        /// </summary>
        [Obsolete("이 메서드는 BinaryFormatter를 사용하므로 신뢰할 수 있는 런타임 객체에만 사용하세요.")]
        public static T DeepCopy<T>(this T value) where T : class
        {
            if (value == null)
                return null;

            Type type = value.GetType();
            if (!type.IsSerializable || typeof(ISerializable).IsAssignableFrom(type))
                return null;

#pragma warning disable SYSLIB0011
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, value);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
        }
    }
}
