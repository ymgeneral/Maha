using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Maha.JsonService.Util
{
    /// <summary>
    /// 实体生成器
    /// </summary>
    public class CSharpEntityGenerator
    {
        public static bool IsServiceObjectType(Type type)
        {
            return type.Namespace != null
                   && !type.Namespace.StartsWith("System")
                   && !type.Namespace.StartsWith("Microsoft")
                   && !type.IsArray;
        }


        public Dictionary<Type, string> GenerateCodeForType(Type type)
        {
            Dictionary<Type, string> typesDict = new Dictionary<Type, string> { [type] = null };
            if (type.IsArray)
            {
                string arrTypeName = type.FullName.Remove(type.FullName.Length - 2);
                var arrType = Type.GetType(arrTypeName + "," + type.Assembly);
                type = arrType;
            }

            GenerateCodeLoop(type, typesDict);
            Dictionary<Type, string> newTypesDict = new Dictionary<Type, string> { };
            foreach (KeyValuePair<Type, string> pair in typesDict.Where(pair => IsServiceObjectType(pair.Key)))
            {
                newTypesDict.Add(pair.Key, pair.Value);
            }
            return newTypesDict;
        }

        private void GenerateCodeLoop(Type type, Dictionary<Type, string> typesDict)
        {
            if (type.IsEnum)
            {
                GenerateCodeEnum(type, typesDict);
                return;
            }

            StringBuilder code = new StringBuilder();
            typesDict[type] = null;

            if (!IsServiceObjectType(type) && !type.IsGenericType)
                return;

            List<MemberInfo> memberInfos = new List<MemberInfo>();
            memberInfos.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.Instance));
            memberInfos.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            if (IsServiceObjectType(type))
            {
                #region When Is ServiceObjectType

                code.AppendLine("public class " + AssemlbyCommentReader.GetCSharpRepresentation(type));
                code.AppendLine("{");

                for (var index = 0; index < memberInfos.Count; index++)
                {
                    var memberInfo = memberInfos[index];

                    #region write comment content

                    string writePropName = memberInfo.Name;

                    var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                    if (jsonPropertyAttributes.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(((JsonPropertyAttribute)jsonPropertyAttributes[0])
                            .PropertyName))
                            writePropName = ((JsonPropertyAttribute)jsonPropertyAttributes[0]).PropertyName;
                    }

                    string comment = AssemlbyCommentReader.GetMemberDescriptionComment(memberInfo);

                    if (string.IsNullOrWhiteSpace(comment))
                    {
                        if (memberInfo.Name == "SortFields")
                            comment = "排序方式，格式如： InDate DESC，可不填，保持默认排序即可";
                        else if (memberInfo.Name == "PageIndex")
                            comment = "分页返回结果，指定页号，以0为起始数字，表示第1页";
                        else if (memberInfo.Name == "PageSize")
                            comment = "分页返回结果，每页记录数";
                    }

                    if (string.IsNullOrWhiteSpace(comment))
                        comment = writePropName;


                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        code.AppendLine("   /// <summary>");
                        code.AppendLine("   /// " + comment.Replace("<", "&lt;").Replace(">", "&gt;"));
                        code.AppendLine("   /// </summary>");
                    }

                    #endregion

                    #region write custom attributes code

                    var allProperityAttributes = memberInfo.GetCustomAttributes(true);
                    var allProperityAttributesData = memberInfo.GetCustomAttributesData();
                    foreach (CustomAttributeData customAttributeData in allProperityAttributesData)
                    {
                        bool isHasAttributeArgs = customAttributeData.ConstructorArguments.Count > 0
                                                  || (customAttributeData.NamedArguments != null &&
                                                      customAttributeData.NamedArguments.Count > 0);

                        string attributeTypeName = AssemlbyCommentReader.GetCSharpRepresentation(
                            customAttributeData.Constructor.DeclaringType);
                        if (attributeTypeName.EndsWith("Attribute"))
                            attributeTypeName = attributeTypeName.Substring(0,
                                attributeTypeName.LastIndexOf("Attribute", StringComparison.Ordinal));
                        code.Append("   [").Append(attributeTypeName);
                        if (isHasAttributeArgs)
                            code.Append("(");
                        code.Append(string.Join(", ", Array.ConvertAll<CustomAttributeTypedArgument, string>(
                            customAttributeData.ConstructorArguments.ToArray(),
                            (customAttributeTypedArgument) => ObjectToSourceCode(customAttributeTypedArgument.Value,
                                customAttributeTypedArgument.ArgumentType))));

                        if (customAttributeData.NamedArguments != null &&
                            customAttributeData.NamedArguments.Count > 0)
                        {
                            if (customAttributeData.ConstructorArguments.Count > 0)
                                code.Append(", ");
                            code.Append(string.Join(", ", Array.ConvertAll<CustomAttributeNamedArgument, string>(
                                customAttributeData.NamedArguments.ToArray(),
                                (customAttributeNamedArgument) => string.Format("{0} = {1}",
                                    customAttributeNamedArgument.MemberInfo.Name,
                                    ObjectToSourceCode(customAttributeNamedArgument.TypedValue.Value,
                                        customAttributeNamedArgument.TypedValue.ArgumentType)))));
                        }
                        if (isHasAttributeArgs)
                            code.Append(")");
                        code.AppendLine("]");
                    }

                    #endregion

                    #region write body code

                    Type memberType = null;

                    if (memberInfo is PropertyInfo)
                    {
                        memberType = ((PropertyInfo)memberInfo).PropertyType;
                        code.AppendFormat("   public {0} {1} ",
                                AssemlbyCommentReader.GetCSharpRepresentation(memberType),
                                writePropName)
                            .Append("{ get; set; }")
                            .AppendLine();
                        if (index < memberInfos.Count - 1)
                            code.AppendLine();
                    }
                    else
                    {
                        memberType = ((FieldInfo)memberInfo).FieldType;
                        code.AppendFormat("   public {0} {1}; ",
                                AssemlbyCommentReader.GetCSharpRepresentation(memberType),
                                writePropName)
                            .AppendLine();
                        if (index < memberInfos.Count - 1)
                            code.AppendLine();
                    }

                    #endregion

                    var refTypes = GetRefTypes(memberType);
                    foreach (var refType in refTypes)
                    {
                        if (!typesDict.ContainsKey(refType))
                        {
                            GenerateCodeLoop(refType, typesDict);
                        }
                    }
                }
                code.AppendLine("}");
                typesDict[type] = code.ToString();

                #endregion
            }
            else
            {
                #region When not ServiceObjectType, only find reference types

                foreach (var memberInfo in memberInfos)
                {
                    Type memberType = null;

                    if (memberInfo is PropertyInfo)
                    {
                        memberType = ((PropertyInfo)memberInfo).PropertyType;
                    }
                    else
                    {
                        memberType = ((FieldInfo)memberInfo).FieldType;
                    }
                    var refTypes = GetRefTypes(memberType);

                    foreach (var refType in refTypes)
                    {
                        if (!typesDict.ContainsKey(refType))
                        {
                            GenerateCodeLoop(refType, typesDict);
                        }
                    }
                }

                #endregion
            }
        }

        private Type GetMemberInfoType(MemberInfo mi)
        {
            if (mi is PropertyInfo)
            {
                return ((PropertyInfo)mi).PropertyType;
            }
            else if (mi is FieldInfo)
                return ((FieldInfo)mi).FieldType;
            else
                return null;
        }

        private void GenerateCodeEnum(Type type, Dictionary<Type, string> typesDict)
        {
            if (!IsServiceObjectType(type) && !type.IsGenericType)
                return;

            StringBuilder code = new StringBuilder();
            code.AppendLine("public enum " + AssemlbyCommentReader.GetCSharpRepresentation(type));
            code.AppendLine("{");

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            for (var index = 0; index < fields.Length; index++)
            {
                FieldInfo fieldInfo = fields[index];
                string enumName = fieldInfo.Name;
                // Comment
                string comment = AssemlbyCommentReader.GetFieldDescprtionComment(fieldInfo);
                if (string.IsNullOrWhiteSpace(comment))
                    comment = enumName;
                // DescriptionAttribute
                object[] objs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                string description = null;
                if (objs != null && objs.Length > 0)
                {
                    DescriptionAttribute a = objs[0] as DescriptionAttribute;
                    if (a != null && a.Description != null)
                    {
                        description = a.Description;
                    }
                }

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    code.AppendLine("   /// <summary>");
                    code.AppendLine("   /// " + comment.Replace("<", "&lt;").Replace(">", "&gt;"));
                    code.AppendLine("   /// </summary>");
                }

                if (description != null)
                    code.AppendFormat("   [Description(\"{0}\")]", description).AppendLine();

                code.AppendFormat("   {0} = {1}", enumName, (int)fieldInfo.GetValue(null));
                if (index < fields.Length - 1)
                    code.Append(",");
                code.AppendLine();
            }
            code.AppendLine("}");

            typesDict[type] = code.ToString();
        }

        public static Type[] GetRefTypes(Type t)
        {
            if (t.IsArray)
            {
                string arrTypeName = t.FullName.Remove(t.FullName.Length - 2);
                var arrType = Type.GetType(arrTypeName + "," + t.Assembly);
                return new[] { arrType };
            }
            if (t.IsGenericType)
            {
                /*
                 * class A 
                 * {
                 *      Dictionary<List<User>, Order> u { get; set; }
                 * }
                 */
                Type[] ts = t.GetGenericArguments();
                return ts;
            }
            return new[] { t };
        }

        public string ObjectToSourceCode(object obj, Type t = null)
        {
            if (obj == null)
            {
                return "null";
            }

            if (t == null)
                t = obj.GetType();

            if (obj is CustomAttributeTypedArgument)
            {
                return ObjectToSourceCode(((CustomAttributeTypedArgument)obj).Value,
                    ((CustomAttributeTypedArgument)obj).ArgumentType);
            }
            else if (t.IsEnum)
            {
                return String.Format("{0}.{1}", AssemlbyCommentReader.GetCSharpRepresentation(t),
                    Enum.GetName(t, obj));
            }
            else if (t == typeof(int) || t == typeof(long)
                     || t == typeof(float) || t == typeof(double)
                     || t == typeof(decimal))
            {
                return obj.ToString();
            }
            else if (t == typeof(bool))
            {
                return obj.ToString().ToLower();
            }
            if (t == typeof(DateTime))
            {
                return "new DateTime(" + ((DateTime)obj).ToString("yyyy-M-d H:m:s.fff") + ")";
            }
            else if (t == typeof(TimeSpan))
            {
                return string.Format("TimeSpan.FromMilliseconds({0})", ((TimeSpan)obj).TotalMilliseconds);
            }
            else if (t == typeof(string))
            {
                return string.Format("\"{0}\"", obj);
            }
            else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var genericArgType = t.GetGenericArguments()[0];
                return ObjectToSourceCode(Convert.ChangeType(obj, genericArgType), genericArgType);
            }
            else if (t.IsArray)
            {
                string arrTypeName = t.FullName.Remove(t.FullName.Length - 2);
                var arrType = Type.GetType(arrTypeName + "," + t.Assembly);

                StringBuilder sbForArray = new StringBuilder();
                sbForArray.Append("new [] {");

                var arrObjects = obj as IEnumerable;
                int i = -1;
                foreach (var arrObject in arrObjects)
                {
                    i++;
                    if (i > 0)
                        sbForArray.Append(", ");

                    sbForArray.Append(ObjectToSourceCode(arrObject, arrType));
                }
                //sbForArray.Append(string.Join(", ",
                //    Array.ConvertAll<object, string>((obj as ArrayList).ToArray(), (input) => ObjectToSourceCode(input))));
                sbForArray.Append("}");
                return sbForArray.ToString();
            }
            else
            {
                return string.Format("/* NOT SUPPORT TYPE {0}: {0} */",
                    AssemlbyCommentReader.GetCSharpRepresentation(t), obj);
            }
        }
    }
}
