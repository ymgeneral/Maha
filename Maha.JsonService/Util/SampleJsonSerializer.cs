using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Maha.JsonService.Util
{
    /// <summary>
    /// 简单Json序列器
    /// </summary>
    public class SampleJsonSerializer
    {
        StringBuilder sb = new StringBuilder();
        private JsonWriter jsonWriter;
        public string Serialize(object sampleObject)
        {
            if (sampleObject == null)
            {
                return "null";
            }

            if (jsonWriter == null)
            {
                jsonWriter = new JsonTextWriter(new StringWriter(sb));
                jsonWriter.Formatting = Formatting.Indented;
            }

            if (typeof(IDictionary).IsInstanceOfType(sampleObject))
            {
                jsonWriter.WriteStartObject();
                foreach (string key in ((IDictionary)sampleObject).Keys)
                {
                    jsonWriter.WritePropertyName(key);
                    //jsonWriter.WriteValue(Serialize(((IDictionary)sampleObject)[key]));
                    jsonWriter.WriteValue(((IDictionary)sampleObject)[key]);
                }
                jsonWriter.WriteEndObject();
            }
            else if (sampleObject.GetType().IsGenericType && sampleObject.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>)))
            {
                jsonWriter.WriteStartObject();
                foreach (string key in ((IDictionary<string, object>)sampleObject).Keys)
                {
                    jsonWriter.WritePropertyName(key);
                    jsonWriter.WriteValue(((IDictionary<string, object>)sampleObject)[key]);
                }
                jsonWriter.WriteEndObject();
            }
            else if (sampleObject is JObject)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("ate");
                jsonWriter.WriteRawValue(new JRaw(sampleObject.ToString()).ToString());
                jsonWriter.WriteEndObject();
            }
            else if ((sampleObject is IEnumerable) && !(sampleObject is string))
            {
                jsonWriter.WriteStartArray();
                foreach (object item in (IEnumerable)sampleObject)
                {
                    Serialize(item);
                }
                jsonWriter.WriteEndArray();
            }
            else if (typeof(KeyValuePair<string, object>).IsInstanceOfType(sampleObject))
            {
                KeyValuePair<string, object> pair = (KeyValuePair<string, object>)sampleObject;
                jsonWriter.WritePropertyName(pair.Key);
                jsonWriter.WriteValue(pair.Value);
            }
            else if (sampleObject.GetType().IsClass && !(sampleObject is string))
            {
                jsonWriter.WriteStartObject();

                var props = sampleObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var fields = sampleObject.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    string writePropName = field.Name;

                    var jsonPropertyAttributes = field.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                    if (jsonPropertyAttributes.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(((JsonPropertyAttribute)jsonPropertyAttributes[0])
                            .PropertyName))
                            writePropName = ((JsonPropertyAttribute)jsonPropertyAttributes[0]).PropertyName;

                    }

                    jsonWriter.WritePropertyName(writePropName);
                    var fieldValue = field.GetValue(sampleObject);
                    if (fieldValue == null)
                        jsonWriter.WriteValue((string)null);
                    else
                        Serialize(fieldValue);
                    string comment = AssemlbyCommentReader.GetFieldDescprtionComment(field);
                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        jsonWriter.WriteWhitespace("  ");
                        jsonWriter.WriteComment(comment);
                    }
                }

                foreach (var prop in props)
                {
                    #region 不需要序列化的字段

                    if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Length > 0)
                        continue;
                    if (prop.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Length > 0)
                        continue;

                    if (prop.Name == "CurrentPageTotalInfo"
                        || prop.Name == "AllPageTotalInfo"
                        || prop.Name == "recordsFiltered"
                        || prop.Name == "DataPermissionAuthKey"
                        || prop.Name == "PassDataPermissionAuth"
                        || prop.Name == "PassFilterDeletedStatus"
                        || prop.Name == "OnlyTopAndSelf"
                        || prop.Name == "LimitThisCompanyCode"
                        || prop.Name == "LimitThisCategories"
                        || prop.Name == "EnableSolrQuery"
                        || prop.Name == "draw"
                        || prop.Name == "SplitDBNodeNo"
                        || prop.Name == "SplitCode"
                        )
                        continue;


                    #endregion

                    string writePropName = prop.Name;

                    var jsonPropertyAttributes = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                    if (jsonPropertyAttributes.Length > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(((JsonPropertyAttribute)jsonPropertyAttributes[0])
                            .PropertyName))
                            writePropName = ((JsonPropertyAttribute)jsonPropertyAttributes[0]).PropertyName;

                    }


                    var propValue = prop.GetValue(sampleObject, null);

                    if (propValue == null)
                    {
                        jsonWriter.WritePropertyName(writePropName);
                        jsonWriter.WriteValue((string)null);
                    }
                    else
                    {
                        if (propValue is JToken)
                        {
                            jsonWriter.WritePropertyName(writePropName);
                            jsonWriter.WriteRawValue(propValue.ToString());
                        }
                        else
                        {
                            jsonWriter.WritePropertyName(writePropName);
                            Serialize(propValue);
                        }
                    }


                    string comment = AssemlbyCommentReader.GetPropertyDescprtionComment(prop);

                    if (string.IsNullOrWhiteSpace(comment))
                    {
                        if (prop.Name == "SortFields")
                            comment = "排序方式，格式如： InDate DESC，可不填，保持默认排序即可";
                        else if (prop.Name == "PageIndex")
                            comment = "分页返回结果，指定页号，以0为起始数字，表示第1页";
                        else if (prop.Name == "PageSize")
                            comment = "分页返回结果，每页记录数";
                    }

                    //注释中显示该属性的数据类型
                    //comment = string.Concat(comment, " [", AssemlbyCommentReader.GetCSharpRepresentation(prop.PropertyType), "]");

                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        jsonWriter.WriteWhitespace("  ");
                        jsonWriter.WriteComment(comment);
                    }

                }

                jsonWriter.WriteEndObject();
            }
            else if (sampleObject.GetType().IsGenericType &&
                       sampleObject.GetType().GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                jsonWriter.WriteValue(sampleObject);
            }
            else
            {
                jsonWriter.WriteValue(sampleObject);

            }

            return sb.ToString();
            //|| sampleObject is string
            //|| sampleObject is double
            //|| sampleObject is decimal
            //|| sampleObject is Byte
            //|| sampleObject is Int16
            //|| sampleObject is Int32
            //|| sampleObject is Int64)
        }
    }
}
