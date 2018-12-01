using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Maha.JsonService.Util
{
    /// <summary>
    /// 程序集Xml注释读取类
    /// </summary>
    public class AssemlbyCommentReader
    {
        public static string GetCSharpRepresentation(Type t)
        {
            if (t.IsGenericType)
            {
                var genericArgs = t.GetGenericArguments().ToList();
                return GetCSharpRepresentation(t, true, genericArgs);
            }
            return t.Name;
        }

        public static string GetCSharpRepresentation(Type t, bool trimArgCount)
        {
            if (t.IsGenericType)
            {
                var genericArgs = t.GetGenericArguments().ToList();
                return GetCSharpRepresentation(t, trimArgCount, genericArgs);
            }
            return t.Name;
        }

        public static string GetCSharpRepresentation(Type t, bool trimArgCount, List<Type> availableArguments)
        {
            if (t.IsGenericType)
            {
                string value = t.Name;
                if (trimArgCount && value.IndexOf("`") > -1)
                {
                    value = value.Substring(0, value.IndexOf("`"));
                }

                if (t.DeclaringType != null)
                {
                    // This is a nested type, build the nesting type first
                    value = GetCSharpRepresentation(t.DeclaringType, trimArgCount, availableArguments) + "+" + value;
                }

                // Build the type arguments (if any)
                string argString = "";
                var thisTypeArgs = t.GetGenericArguments();
                for (int i = 0; i < thisTypeArgs.Length && availableArguments.Count > 0; i++)
                {
                    if (i != 0) argString += ", ";

                    argString += GetCSharpRepresentation(availableArguments[0], trimArgCount);
                    availableArguments.RemoveAt(0);
                }

                // If there are type arguments, add them with < >
                if (argString.Length > 0)
                {
                    value += "<" + argString + ">";
                }

                return value;
            }
            return t.Name;
        }

        public static string GetMemberDescriptionComment(MemberInfo mi)
        {
            if (mi is PropertyInfo)
            {
                return GetPropertyDescprtionComment((PropertyInfo)mi);
            }
            else if (mi is FieldInfo)
            {
                return GetFieldDescprtionComment((FieldInfo)mi);
            }
            else if (mi is MethodInfo)
            {
                var methodComment = GetMethodComment((MethodInfo) mi);
                return GetMethodDescprtionComment(methodComment);
            }
            else
            {
                throw new NotSupportedException(mi.GetType().ToString());
            }
        }

        #region method comments

        public static XmlNode GetMethodComment(MethodInfo mi)
        {
            string methodSign = "M:" + ToTypeNameComment(mi.DeclaringType) + "." + mi.Name;
            methodSign = methodSign.Replace("+", ".");

            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "\\bin\\" +
                             mi.DeclaringType.Assembly.GetName().Name + ".xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(xmlPath))
                return null;

            xmlDoc.Load(xmlPath);

            foreach (XmlNode memberNode in xmlDoc["doc"]["members"].ChildNodes)
            {
                if (memberNode.Attributes["name"].Value == methodSign ||
                    memberNode.Attributes["name"].Value.StartsWith(methodSign + "("))
                {
                    return memberNode.Clone();
                }
            }
            return null;
        }

        public static string GetMethodDescprtionComment(MethodInfo mi)
        {
            var methodComment = GetMethodComment(mi);
            return GetMethodDescprtionComment(methodComment);
        }
        public static string GetMethodDescprtionComment(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return string.Empty;

            var xmlElement = xmlNode["summary"];
            if (xmlElement != null) return xmlElement.InnerText.Trim();

            return string.Empty;
        }

        public static string GetMethodReturnsComment(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return string.Empty;

            var xmlElement = xmlNode["returns"];
            if (xmlElement != null) return xmlElement.InnerText;

            return string.Empty;
        }

        public static string GetMethodParameterComment(XmlNode xmlNode, string paramName)
        {
            if (xmlNode == null)
                return string.Empty;

            foreach (XmlNode childNode in xmlNode.ChildNodes)
            {
                if (childNode.Attributes != null &&
                    (childNode.Name == "param" && childNode.Attributes["name"].Value == paramName))
                    return childNode.InnerText;
            }
            return string.Empty;
        }

        #endregion

        #region property comments

        public static string ToTypeNameComment(Type type)
        {
            return (type.Namespace + "." + type.Name).Trim('.');
        }

        public static XmlNode GetPropertyComment(PropertyInfo pi)
        {
            string propSign = "P:" + ToTypeNameComment(pi.DeclaringType) + "." + pi.Name;
            propSign = propSign.Replace("+", ".");

            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "\\bin\\" +
                             pi.DeclaringType.Assembly.GetName().Name + ".xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(xmlPath))
                return null;

            xmlDoc.Load(xmlPath);

            foreach (XmlNode memberNode in xmlDoc["doc"]["members"].ChildNodes)
            {
                if (memberNode.Attributes == null)
                    continue;

                if (memberNode.Attributes["name"].Value == propSign ||
                    memberNode.Attributes["name"].Value.StartsWith(propSign + "("))
                {
                    return memberNode.Clone();
                }
            }
            return null;
        }

        public static string GetPropertyDescprtionComment(PropertyInfo pi)
        {
            var xmlNode = GetPropertyComment(pi);
            if (xmlNode == null)
                return string.Empty;

            var xmlElement = xmlNode["summary"];
            if (xmlElement != null) return xmlElement.InnerText.Trim();

            return string.Empty;
        }

        #endregion

        #region field comments

        public static XmlNode GetFieldComment(FieldInfo fi)
        {
            string fieldSign = "F:" + ToTypeNameComment(fi.DeclaringType) + "." + fi.Name;
            fieldSign = fieldSign.Replace("+", ".");

            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "\\bin\\" +
                             fi.DeclaringType.Assembly.GetName().Name + ".xml";
            XmlDocument xmlDoc = new XmlDocument();
            if (!File.Exists(xmlPath))
                return null;

            xmlDoc.Load(xmlPath);

            foreach (XmlNode memberNode in xmlDoc["doc"]["members"].ChildNodes)
            {
                if (memberNode.Attributes["name"].Value == fieldSign ||
                    memberNode.Attributes["name"].Value.StartsWith(fieldSign + "("))
                {
                    return memberNode.Clone();
                }
            }
            return null;
        }

        public static string GetFieldDescprtionComment(FieldInfo fi)
        {
            var xmlNode = GetFieldComment(fi);
            if (xmlNode == null)
                return string.Empty;

            var xmlElement = xmlNode["summary"];
            if (xmlElement != null) return xmlElement.InnerText.Trim();

            return string.Empty;
        }
        #endregion
    }
}