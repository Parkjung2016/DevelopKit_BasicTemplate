using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
#endif

namespace PJDev.DevelopKit.BasicTemplate.Runtime
{
    public abstract class ConditionalVisibilityAttribute : PropertyAttribute
    {
        protected ConditionalVisibilityAttribute(string conditionMember)
        {
            ConditionMember = conditionMember;
        }

        protected ConditionalVisibilityAttribute(string conditionMember, object expectedValue)
        {
            ConditionMember = conditionMember;
            ExpectedValue = expectedValue;
            HasExpectedValue = true;
        }

        public string ConditionMember { get; }
        public object ExpectedValue { get; }
        public bool HasExpectedValue { get; }
        public abstract bool ShowWhenConditionMatches { get; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class ShowIfAttribute : ConditionalVisibilityAttribute
    {
        public ShowIfAttribute(string conditionMember) : base(conditionMember)
        {
        }

        public ShowIfAttribute(string conditionMember, object expectedValue)
            : base(conditionMember, expectedValue)
        {
        }

        public override bool ShowWhenConditionMatches => true;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class HideIfAttribute : ConditionalVisibilityAttribute
    {
        public HideIfAttribute(string conditionMember) : base(conditionMember)
        {
        }

        public HideIfAttribute(string conditionMember, object expectedValue)
            : base(conditionMember, expectedValue)
        {
        }

        public override bool ShowWhenConditionMatches => false;
    }

    public abstract class ConditionalEnableAttribute : PropertyAttribute
    {
        protected ConditionalEnableAttribute(string conditionMember)
        {
            ConditionMember = conditionMember;
        }

        protected ConditionalEnableAttribute(string conditionMember, object expectedValue)
        {
            ConditionMember = conditionMember;
            ExpectedValue = expectedValue;
            HasExpectedValue = true;
        }

        public string ConditionMember { get; }
        public object ExpectedValue { get; }
        public bool HasExpectedValue { get; }
        public abstract bool EnableWhenConditionMatches { get; }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class EnableIfAttribute : ConditionalEnableAttribute
    {
        public EnableIfAttribute(string conditionMember) : base(conditionMember)
        {
        }

        public EnableIfAttribute(string conditionMember, object expectedValue)
            : base(conditionMember, expectedValue)
        {
        }

        public override bool EnableWhenConditionMatches => true;
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public sealed class DisableIfAttribute : ConditionalEnableAttribute
    {
        public DisableIfAttribute(string conditionMember) : base(conditionMember)
        {
        }

        public DisableIfAttribute(string conditionMember, object expectedValue)
            : base(conditionMember, expectedValue)
        {
        }

        public override bool EnableWhenConditionMatches => false;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ConditionalVisibilityAttribute), true)]
    public sealed class ConditionalVisibilityAttributeDrawer : PropertyDrawer
    {
        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<MemberKey, MemberInfo> MemberCache = new();
        private static readonly HashSet<MemberKey> MissingMemberCache = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            VisibilityResult result = EvaluateVisibility(property);
            if (result.IsValid && !result.IsVisible)
                return -EditorGUIUtility.standardVerticalSpacing;

            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            if (result.IsValid)
                return propertyHeight;

            return propertyHeight
                   + EditorGUIUtility.standardVerticalSpacing
                   + GetHelpBoxHeight(result.ErrorMessage);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            VisibilityResult result = EvaluateVisibility(property);
            if (result.IsValid && !result.IsVisible)
                return;

            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            Rect propertyRect = new(position.x, position.y, position.width, propertyHeight);
            EditorGUI.PropertyField(propertyRect, property, label, true);

            if (result.IsValid)
                return;

            Rect helpRect = new(
                position.x,
                propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                GetHelpBoxHeight(result.ErrorMessage));
            EditorGUI.HelpBox(helpRect, result.ErrorMessage, MessageType.Error);
        }

        private VisibilityResult EvaluateVisibility(SerializedProperty property)
        {
            var visibilityAttribute = (ConditionalVisibilityAttribute)attribute;
            if (!TryEvaluateAnyResult(
                    property,
                    visibilityAttribute.ConditionMember,
                    visibilityAttribute.ExpectedValue,
                    visibilityAttribute.HasExpectedValue,
                    visibilityAttribute.ShowWhenConditionMatches,
                    out bool anyVisible,
                    out string error))
            {
                return VisibilityResult.Invalid(error);
            }

            return anyVisible ? VisibilityResult.Visible : VisibilityResult.Hidden;
        }

        internal static bool TryEvaluateAnyResult(
            SerializedProperty property,
            string condition,
            object expectedValue,
            bool hasExpectedValue,
            bool positiveResult,
            out bool anyResult,
            out string error)
        {
            anyResult = false;
            error = null;
            if (string.IsNullOrWhiteSpace(condition))
            {
                error = "The conditional member or expression is empty.";
                return false;
            }

            UnityEngine.Object[] targets = property.serializedObject.targetObjects;
            if (targets == null || targets.Length == 0)
            {
                anyResult = true;
                return true;
            }

            bool isExpression = condition.StartsWith("@", StringComparison.Ordinal);
            string expression = isExpression ? condition.Substring(1) : null;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!TryGetPropertyParent(targets[i], property.propertyPath, out object parent, out error))
                    return false;

                bool conditionMatches;
                if (isExpression)
                {
                    var parser = new ConditionalExpressionParser(parent, expression);
                    if (!parser.TryEvaluate(out object expressionValue, out error))
                        return false;

                    conditionMatches = IsTruthy(expressionValue);
                }
                else
                {
                    if (!TryGetConditionValue(parent, condition, out object conditionValue, out error))
                        return false;

                    conditionMatches = hasExpectedValue
                        ? ValuesEqual(conditionValue, expectedValue)
                        : IsTruthy(conditionValue);
                }

                anyResult |= conditionMatches == positiveResult;
            }

            return true;
        }
        internal static bool TryGetConditionValue(
            object source,
            string memberPath,
            out object value,
            out string error)
        {
            value = source;
            error = null;
            string[] pathParts = memberPath.Split('.');

            for (int i = 0; i < pathParts.Length; i++)
            {
                if (value == null)
                {
                    error = $"A null object was encountered while evaluating '{memberPath}'.";
                    return false;
                }

                string memberName = NormalizeMemberName(pathParts[i]);
                Type sourceType = value.GetType();
                if (!TryFindMember(sourceType, memberName, out MemberInfo member))
                {
                    error = $"Field, property, or method '{memberName}' was not found on '{sourceType.Name}'.";
                    return false;
                }

                try
                {
                    value = GetMemberValue(member, value);
                }
                catch (Exception exception)
                {
                    Exception cause = exception is TargetInvocationException { InnerException: not null }
                        ? exception.InnerException
                        : exception;
                    error = $"Failed to evaluate '{memberPath}': {cause.Message}";
                    return false;
                }
            }

            return true;
        }

        private static bool TryFindMember(Type type, string memberName, out MemberInfo member)
        {
            var key = new MemberKey(type, memberName);
            if (MemberCache.TryGetValue(key, out member))
                return true;

            if (MissingMemberCache.Contains(key))
                return false;

            for (Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(memberName, MemberFlags | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    member = field;
                    MemberCache[key] = member;
                    return true;
                }

                PropertyInfo property = current.GetProperty(memberName, MemberFlags | BindingFlags.DeclaredOnly);
                if (property?.GetMethod != null && property.GetIndexParameters().Length == 0)
                {
                    member = property;
                    MemberCache[key] = member;
                    return true;
                }

                MethodInfo method = current.GetMethod(
                    memberName,
                    MemberFlags | BindingFlags.DeclaredOnly,
                    null,
                    Type.EmptyTypes,
                    null);
                if (method != null && method.ReturnType != typeof(void) && !method.ContainsGenericParameters)
                {
                    member = method;
                    MemberCache[key] = member;
                    return true;
                }
            }

            member = null;
            MissingMemberCache.Add(key);
            return false;
        }

        private static object GetMemberValue(MemberInfo member, object source)
        {
            return member switch
            {
                FieldInfo field => field.GetValue(field.IsStatic ? null : source),
                PropertyInfo property => property.GetValue(
                    property.GetMethod != null && property.GetMethod.IsStatic ? null : source),
                MethodInfo method => method.Invoke(method.IsStatic ? null : source, null),
                _ => null
            };
        }

        private static bool TryGetPropertyParent(
            object target,
            string propertyPath,
            out object parent,
            out string error)
        {
            parent = target;
            error = null;
            string normalizedPath = propertyPath.Replace(".Array.data[", "[");
            string[] pathParts = normalizedPath.Split('.');

            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                if (!TryGetPathPartValue(parent, pathParts[i], out parent))
                {
                    error = $"The parent object of '{propertyPath}' could not be resolved.";
                    return false;
                }
            }

            if (parent != null)
                return true;

            error = $"The parent object of '{propertyPath}' is null.";
            return false;
        }

        private static bool TryGetPathPartValue(object source, string pathPart, out object value)
        {
            value = source;
            if (source == null)
                return false;

            int bracketIndex = pathPart.IndexOf('[');
            string memberName = bracketIndex >= 0 ? pathPart.Substring(0, bracketIndex) : pathPart;
            if (!TryFindMember(source.GetType(), memberName, out MemberInfo member))
                return false;

            value = GetMemberValue(member, source);
            if (bracketIndex < 0)
                return true;

            int endBracketIndex = pathPart.IndexOf(']', bracketIndex + 1);
            if (endBracketIndex < 0
                || !int.TryParse(
                    pathPart.Substring(bracketIndex + 1, endBracketIndex - bracketIndex - 1),
                    out int elementIndex)
                || value is not IList list
                || elementIndex < 0
                || elementIndex >= list.Count)
            {
                return false;
            }

            value = list[elementIndex];
            return true;
        }

        internal static bool ValuesEqual(object current, object expected)
        {
            bool currentIsNull = IsNullValue(current);
            bool expectedIsNull = IsNullValue(expected);
            if (currentIsNull || expectedIsNull)
                return currentIsNull && expectedIsNull;

            Type currentType = current.GetType();
            if (currentType.IsEnum)
            {
                if (expected is string enumName)
                    return string.Equals(current.ToString(), enumName, StringComparison.OrdinalIgnoreCase);

                try
                {
                    object converted = Enum.ToObject(
                        currentType,
                        Convert.ChangeType(expected, Enum.GetUnderlyingType(currentType), CultureInfo.InvariantCulture));
                    return current.Equals(converted);
                }
                catch
                {
                    return false;
                }
            }

            if (IsNumeric(current) && IsNumeric(expected))
            {
                try
                {
                    return Convert.ToDecimal(current, CultureInfo.InvariantCulture)
                           == Convert.ToDecimal(expected, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return false;
                }
            }

            if (currentType != expected.GetType())
            {
                try
                {
                    expected = Convert.ChangeType(expected, currentType, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return false;
                }
            }

            return Equals(current, expected);
        }

        private static bool IsNullValue(object value)
        {
            return value == null || value is UnityEngine.Object unityObject && unityObject == null;
        }

        internal static bool IsTruthy(object value)
        {
            if (value == null)
                return false;

            if (value is bool boolean)
                return boolean;

            if (value is UnityEngine.Object unityObject)
                return unityObject != null;

            if (value.GetType().IsEnum)
                return Convert.ToInt64(value, CultureInfo.InvariantCulture) != 0L;

            if (IsNumeric(value))
                return Convert.ToDecimal(value, CultureInfo.InvariantCulture) != decimal.Zero;

            return true;
        }

        private static bool IsNumeric(object value)
        {
            return Type.GetTypeCode(value.GetType()) is
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64
                or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double
                or TypeCode.Single;
        }

        private static string NormalizeMemberName(string memberName)
        {
            string normalized = memberName.Trim();
            return normalized.EndsWith("()", StringComparison.Ordinal)
                ? normalized.Substring(0, normalized.Length - 2)
                : normalized;
        }

        internal static float GetHelpBoxHeight(string message)
        {
            return Mathf.Max(
                EditorGUIUtility.singleLineHeight * 2f,
                EditorStyles.helpBox.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth));
        }

        private readonly struct MemberKey : IEquatable<MemberKey>
        {
            public MemberKey(Type type, string name)
            {
                Type = type;
                Name = name;
            }

            private Type Type { get; }
            private string Name { get; }

            public bool Equals(MemberKey other) => Type == other.Type && Name == other.Name;
            public override bool Equals(object obj) => obj is MemberKey other && Equals(other);
            public override int GetHashCode() => (Type != null ? Type.GetHashCode() : 0) * 397 ^ Name.GetHashCode();
        }

        private readonly struct VisibilityResult
        {
            private VisibilityResult(bool isValid, bool isVisible, string errorMessage)
            {
                IsValid = isValid;
                IsVisible = isVisible;
                ErrorMessage = errorMessage;
            }

            public bool IsValid { get; }
            public bool IsVisible { get; }
            public string ErrorMessage { get; }

            public static VisibilityResult Visible => new(true, true, null);
            public static VisibilityResult Hidden => new(true, false, null);
            public static VisibilityResult Invalid(string message) => new(false, true, message);
        }
    }

    [CustomPropertyDrawer(typeof(ConditionalEnableAttribute), true)]
    public sealed class ConditionalEnableAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (TryEvaluate(property, out _, out string error))
                return EditorGUI.GetPropertyHeight(property, label, true);

            return EditorGUI.GetPropertyHeight(property, label, true)
                   + EditorGUIUtility.standardVerticalSpacing
                   + ConditionalVisibilityAttributeDrawer.GetHelpBoxHeight(error);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isValid = TryEvaluate(property, out bool isEnabled, out string error);
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            Rect propertyRect = new(position.x, position.y, position.width, propertyHeight);
            using (new EditorGUI.DisabledScope(isValid && !isEnabled))
                EditorGUI.PropertyField(propertyRect, property, label, true);

            if (isValid)
                return;

            Rect helpRect = new(
                position.x,
                propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                ConditionalVisibilityAttributeDrawer.GetHelpBoxHeight(error));
            EditorGUI.HelpBox(helpRect, error, MessageType.Error);
        }

        private bool TryEvaluate(SerializedProperty property, out bool isEnabled, out string error)
        {
            var enableAttribute = (ConditionalEnableAttribute)attribute;
            return ConditionalVisibilityAttributeDrawer.TryEvaluateAnyResult(
                property,
                enableAttribute.ConditionMember,
                enableAttribute.ExpectedValue,
                enableAttribute.HasExpectedValue,
                enableAttribute.EnableWhenConditionMatches,
                out isEnabled,
                out error);
        }
    }

    internal sealed class ConditionalExpressionParser
    {
        private static readonly Dictionary<string, Type> EnumTypeCache = new(StringComparer.Ordinal);
        private static readonly HashSet<string> MissingEnumTypeCache = new(StringComparer.Ordinal);

        private readonly object context;
        private readonly string expression;
        private int index;

        public ConditionalExpressionParser(object context, string expression)
        {
            this.context = context;
            this.expression = expression ?? string.Empty;
        }

        public bool TryEvaluate(out object value, out string error)
        {
            value = null;
            error = null;
            try
            {
                value = ParseOr();
                SkipWhitespace();
                if (index != expression.Length)
                    throw Error($"Unexpected token '{expression[index]}'.");
                return true;
            }
            catch (ExpressionParseException exception)
            {
                error = $"Invalid conditional expression at {exception.Position}: {exception.Message}";
                return false;
            }
            catch (Exception exception)
            {
                error = $"Conditional expression failed: {exception.Message}";
                return false;
            }
        }

        private object ParseOr()
        {
            object left = ParseAnd();
            while (Match("||"))
            {
                object right = ParseAnd();
                left = ConditionalVisibilityAttributeDrawer.IsTruthy(left)
                       || ConditionalVisibilityAttributeDrawer.IsTruthy(right);
            }
            return left;
        }

        private object ParseAnd()
        {
            object left = ParseEquality();
            while (Match("&&"))
            {
                object right = ParseEquality();
                left = ConditionalVisibilityAttributeDrawer.IsTruthy(left)
                       && ConditionalVisibilityAttributeDrawer.IsTruthy(right);
            }
            return left;
        }

        private object ParseEquality()
        {
            object left = ParseRelational();
            while (true)
            {
                if (Match("=="))
                    left = ConditionalVisibilityAttributeDrawer.ValuesEqual(left, ParseRelational());
                else if (Match("!="))
                    left = !ConditionalVisibilityAttributeDrawer.ValuesEqual(left, ParseRelational());
                else
                    return left;
            }
        }

        private object ParseRelational()
        {
            object left = ParseUnary();
            while (true)
            {
                if (Match(">="))
                    left = Compare(left, ParseUnary()) >= 0;
                else if (Match("<="))
                    left = Compare(left, ParseUnary()) <= 0;
                else if (Match(">"))
                    left = Compare(left, ParseUnary()) > 0;
                else if (Match("<"))
                    left = Compare(left, ParseUnary()) < 0;
                else
                    return left;
            }
        }

        private object ParseUnary()
        {
            SkipWhitespace();
            if (Peek("!") && !Peek("!="))
            {
                index++;
                return !ConditionalVisibilityAttributeDrawer.IsTruthy(ParseUnary());
            }

            return ParsePrimary();
        }

        private object ParsePrimary()
        {
            SkipWhitespace();
            if (Match("("))
            {
                object value = ParseOr();
                Require(")");
                return value;
            }

            if (index >= expression.Length)
                throw Error("Expected a value.");

            char current = expression[index];
            if (current == '"' || current == '\'')
                return ParseString();

            if (char.IsDigit(current) || current == '-' && index + 1 < expression.Length && char.IsDigit(expression[index + 1]))
                return ParseNumber();

            string identifier = ParseIdentifier();
            if (string.Equals(identifier, "true", StringComparison.OrdinalIgnoreCase))
                return true;
            if (string.Equals(identifier, "false", StringComparison.OrdinalIgnoreCase))
                return false;
            if (string.Equals(identifier, "null", StringComparison.OrdinalIgnoreCase))
                return null;

            SkipWhitespace();
            bool isMethodCall = Match("(");
            if (isMethodCall)
                Require(")");
            return ResolveIdentifier(identifier + (isMethodCall ? "()" : string.Empty));
        }

        private object ResolveIdentifier(string identifier)
        {
            if (identifier == "this")
                return context;

            string memberPath = identifier.StartsWith("this.", StringComparison.Ordinal)
                ? identifier.Substring(5)
                : identifier;
            if (ConditionalVisibilityAttributeDrawer.TryGetConditionValue(
                    context,
                    memberPath,
                    out object value,
                    out _))
            {
                return value;
            }

            if (TryResolveEnumValue(identifier, out value))
                return value;

            throw Error($"Member or enum value '{identifier}' was not found.");
        }

        private bool TryResolveEnumValue(string identifier, out object value)
        {
            value = null;
            int valueSeparator = identifier.LastIndexOf('.');
            if (valueSeparator <= 0 || valueSeparator >= identifier.Length - 1)
                return false;

            string typeName = identifier.Substring(0, valueSeparator);
            string enumValueName = identifier.Substring(valueSeparator + 1);
            Type enumType = FindEnumType(typeName);
            if (enumType == null || !Enum.IsDefined(enumType, enumValueName))
                return false;

            value = Enum.Parse(enumType, enumValueName);
            return true;
        }

        private Type FindEnumType(string typeName)
        {
            string cacheKey = $"{context?.GetType().Assembly.FullName}|{typeName}";
            if (EnumTypeCache.TryGetValue(cacheKey, out Type cached))
                return cached;
            if (MissingEnumTypeCache.Contains(cacheKey))
                return null;

            Type found = FindEnumType(context?.GetType().Assembly, typeName);
            if (found == null)
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length && found == null; i++)
                    found = FindEnumType(assemblies[i], typeName);
            }

            if (found != null)
                EnumTypeCache[cacheKey] = found;
            else
                MissingEnumTypeCache.Add(cacheKey);
            return found;
        }

        private static Type FindEnumType(Assembly assembly, string typeName)
        {
            if (assembly == null)
                return null;

            Type exact = assembly.GetType(typeName, false);
            if (exact?.IsEnum == true)
                return exact;

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types;
            }

            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                if (type?.IsEnum == true
                    && (type.Name == typeName || type.FullName == typeName))
                {
                    return type;
                }
            }

            return null;
        }

        private int Compare(object left, object right)
        {
            if (left == null || right == null)
                throw Error("Relational comparisons cannot use null.");

            if (IsNumeric(left) && IsNumeric(right))
            {
                decimal leftNumber = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                decimal rightNumber = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                return leftNumber.CompareTo(rightNumber);
            }

            if (left is string leftString && right is string rightString)
                return string.Compare(leftString, rightString, StringComparison.Ordinal);

            if (left is IComparable comparable)
            {
                try
                {
                    object converted = right.GetType() == left.GetType()
                        ? right
                        : Convert.ChangeType(right, left.GetType(), CultureInfo.InvariantCulture);
                    return comparable.CompareTo(converted);
                }
                catch
                {
                    throw Error($"Values of type '{left.GetType().Name}' and '{right.GetType().Name}' cannot be compared.");
                }
            }

            throw Error($"Type '{left.GetType().Name}' does not support relational comparisons.");
        }

        private string ParseIdentifier()
        {
            SkipWhitespace();
            int start = index;
            while (index < expression.Length)
            {
                char character = expression[index];
                if (!char.IsLetterOrDigit(character) && character != '_' && character != '.')
                    break;
                index++;
            }

            if (index == start)
                throw Error("Expected an identifier.");
            return expression.Substring(start, index - start);
        }

        private string ParseString()
        {
            char quote = expression[index++];
            var value = new System.Text.StringBuilder();
            while (index < expression.Length)
            {
                char character = expression[index++];
                if (character == quote)
                    return value.ToString();

                if (character == '\\' && index < expression.Length)
                {
                    char escaped = expression[index++];
                    value.Append(escaped switch
                    {
                        'n' => '\n',
                        'r' => '\r',
                        't' => '\t',
                        _ => escaped
                    });
                }
                else
                {
                    value.Append(character);
                }
            }

            throw Error("Unterminated string literal.");
        }

        private object ParseNumber()
        {
            int start = index;
            if (expression[index] == '-')
                index++;

            bool isFloatingPoint = false;
            while (index < expression.Length)
            {
                char character = expression[index];
                if (char.IsDigit(character))
                {
                    index++;
                    continue;
                }

                if (character == '.' || character == 'e' || character == 'E')
                {
                    isFloatingPoint = true;
                    index++;
                    if ((character == 'e' || character == 'E')
                        && index < expression.Length
                        && (expression[index] == '+' || expression[index] == '-'))
                    {
                        index++;
                    }
                    continue;
                }

                break;
            }

            string token = expression.Substring(start, index - start);
            if (!isFloatingPoint
                && long.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out long integer))
            {
                return integer;
            }

            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double floatingPoint))
                return floatingPoint;
            throw Error($"Invalid number '{token}'.");
        }

        private static bool IsNumeric(object value)
        {
            return value != null && Type.GetTypeCode(value.GetType()) is
                TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64
                or TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double
                or TypeCode.Single;
        }

        private bool Match(string token)
        {
            SkipWhitespace();
            if (!Peek(token))
                return false;
            index += token.Length;
            return true;
        }

        private bool Peek(string token)
        {
            return index + token.Length <= expression.Length
                   && string.CompareOrdinal(expression, index, token, 0, token.Length) == 0;
        }

        private void Require(string token)
        {
            if (!Match(token))
                throw Error($"Expected '{token}'.");
        }

        private void SkipWhitespace()
        {
            while (index < expression.Length && char.IsWhiteSpace(expression[index]))
                index++;
        }

        private ExpressionParseException Error(string message) => new(message, index);

        private sealed class ExpressionParseException : Exception
        {
            public ExpressionParseException(string message, int position) : base(message)
            {
                Position = position;
            }

            public int Position { get; }
        }
    }
#endif
}