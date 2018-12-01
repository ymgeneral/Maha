using Maha.JsonService.JsonRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;

namespace Maha.JsonService
{
    /// <summary>
    /// 服务注册器
    /// </summary>
    public class JsonRpcRegister
    {
        /// <summary>
        /// jsonRpc 实例集合
        /// </summary>
        private static readonly List<object> jsonRpcInstances = new List<object>();

        /// <summary>
        /// 加载锁定器
        /// </summary>
        private static readonly object loadLocker = new object();

        /// <summary>
        /// jsonRpc配置文件路径
        /// </summary>
        private static readonly string jsonRpcConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jsonrpc.config");

        /// <summary>
        /// 是否加载完成配置
        /// </summary>
        private static bool isLoadedConfig = false;

        /// <summary>
        /// 锁定加载程序集
        /// </summary>
        public static void LoadFromConfig()
        {
            lock (loadLocker)
            {
                if (isLoadedConfig)
                    return;
                if (File.Exists(jsonRpcConfigPath))
                {
                    DoLoadFromConfig();
                }
                isLoadedConfig = true;
            }
        }

        /// <summary>
        /// 遍历jsonrpc.config加载程序集
        /// </summary>
        private static void DoLoadFromConfig()
        {
            XmlDocument jsonRpcDoc = new XmlDocument();
            jsonRpcDoc.Load(jsonRpcConfigPath);

            foreach (XmlNode node in jsonRpcDoc["jsonrpc"]["serviceAssemblies"].ChildNodes)
            {
                if (!node.Name.Equals("add", StringComparison.InvariantCultureIgnoreCase))
                    continue;
                try
                {
                    RegisterAssembly(node);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("注册程序集 " + node.Attributes["assembly"].Value + " 错误并跳过, " + ex.ToString());
                }
                isLoadedConfig = true;
            }
        }

        /// <summary>
        /// 注册程序集
        /// </summary>
        /// <param name="node"></param>
        private static void RegisterAssembly(XmlNode node)
        {
            //程序集名称
            var assemblyName = node.Attributes["assembly"]?.Value;
            //程序集命名空间
            var assemblyNameSpace = node.Attributes["namespace"]?.Value;
            //域名
            var domain = node.Attributes["domain"]?.Value;
            //方法访问权限
            var methodMode = node.Attributes["methodMode"]?.Value;
            //子域名截止符
            var subdomainTrimChars = node.Attributes["subdomainTrimChars"]?.Value;

            var assem = Assembly.Load(assemblyName);
            var typesWithHandlers = assem.GetTypes().Where(p => p.IsPublic && p.IsClass);
            foreach (Type handler in typesWithHandlers)
            {
                try
                {
                    //在当前程序集中，匹配方法域
                    if (!string.IsNullOrWhiteSpace(assemblyNameSpace) && !string.Equals(handler.Namespace, assemblyNameSpace) && !handler.Namespace.StartsWith(assemblyNameSpace + "."))
                    {
                        continue;
                    }
                    RegisterType(handler, assem, methodMode, domain, subdomainTrimChars);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("注册类型 " + handler.FullName + " 错误并跳过, " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// 注册服务方法
        /// </summary>
        /// <param name="handler">程序集成员</param>
        /// <param name="assem">程序集</param>
        /// <param name="methodMode">方法权限类型</param>
        /// <param name="domain">域名</param>
        /// <param name="subdomainTrimChars">服务截止符</param>
        private static void RegisterType(Type handler, Assembly assem, string methodMode, string domain, string subdomainTrimChars = null)
        {
            string sessionId = Handler.DefaultSessionId;
            var regMethod = typeof(Handler).GetMethod("Register");

            var typeHandler = handler;
            var instance = assem.CreateInstance(handler.FullName);
            List<MethodInfo> methods = new List<MethodInfo>();

            //是否具有方法特性
            bool isMethodModeByAttribute = methodMode != null && "attribute".Equals(methodMode, StringComparison.InvariantCultureIgnoreCase);
            if (isMethodModeByAttribute)
            {
                //获取所有公开的方法
                methods = typeHandler.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(JsonRpcMethodAttribute), false).Length > 0).ToList();
            }
            else
            {
                methods = typeHandler.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToList();
            }

            if (methods.Any())
            {
                jsonRpcInstances.Add(instance);
            }

            foreach (var method in methods)
            {
                //判定为属性
                if (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")) continue;

                //请求服务方法的参数名->类型
                Dictionary<string, Type> paramsTypes = new Dictionary<string, Type>();
                //请求服务方法的参数名->可选参数的默认值
                Dictionary<string, object> paramsDefaultValues = new Dictionary<string, object>();

                var methodParams = method.GetParameters();
                for (int i = 0; i < methodParams.Length; i++)
                {
                    //添加参数以参数类型
                    paramsTypes.Add(methodParams[i].Name, methodParams[i].ParameterType);

                    //如果参数是可选的，则将默认值添加到默认值字典中
                    if (methodParams[i].IsOptional)
                        paramsDefaultValues.Add(methodParams[i].Name, methodParams[i].DefaultValue);
                }

                //返回值类型
                var returnType = method.ReturnType;
                paramsTypes.Add("returns", returnType);
                //标记为JsonRpc服务中的方法
                if (isMethodModeByAttribute)
                {
                    var attrs = method.GetCustomAttributes(typeof(JsonRpcMethodAttribute), false);
                    foreach (JsonRpcMethodAttribute handlerAttribute in attrs)
                    {
                        //拼接完整的方法名
                        //默认方法名为:{域名}.{子域名}.{方法名}；子域名默认为类名，允许自定义结尾符，比如DemoService（subdomainTrimChars="Service"）
                        //<add assembly="Maha.Microservices.DemoService" domain="Demo" methodMode="allPublic" subdomainTrimChars="Service" />
                        string methodName = string.Empty;
                        if (string.IsNullOrWhiteSpace(handlerAttribute.JsonMethodName))
                        {
                            string subdomain = handler.Name;
                            if (!string.IsNullOrWhiteSpace(subdomainTrimChars))
                            {
                                //截取掉尾部自定义结尾符
                                if (subdomain.EndsWith(subdomainTrimChars))
                                    subdomain = subdomain.Substring(0, subdomain.LastIndexOf(subdomainTrimChars, StringComparison.Ordinal));
                            }
                            //拼接域名
                            if (!string.IsNullOrWhiteSpace(domain))
                            {
                                methodName += domain + ".";
                            }
                            //拼接子域名
                            if (!string.IsNullOrWhiteSpace(subdomain))
                            {
                                methodName += subdomain + ".";
                            }
                            //拼接方法名
                            methodName += method.Name;
                        }
                        else
                        {
                            methodName = handlerAttribute.JsonMethodName == string.Empty ? method.Name : handlerAttribute.JsonMethodName;
                        }

                        //通过参数与实例构建委托方法，将创建的委托添加到Handler的所有委托集中
                        var newDelegate = Delegate.CreateDelegate(Expression.GetDelegateType(paramsTypes.Values.ToArray()),instance , method);
                        var handlerSession = Handler.GetSessionHandler(sessionId);
                        //调用Handler.Register方法
                        regMethod.Invoke(handlerSession, new object[] { methodName, newDelegate });

                        try
                        {
                            //将方法信息添加到MetaData中
                            handlerSession.MetaData.AddService(methodName, paramsTypes, paramsDefaultValues);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine("Register method " + methodName + " error and skiped, " + ex.ToString());
                        }
                    }
                }
                else
                {
                    string methodName = string.Empty;
                    string subdomain = handler.Name;
                    if (!string.IsNullOrWhiteSpace(subdomainTrimChars))
                    {
                        //截取掉尾部自定义结尾符
                        if (subdomain.EndsWith(subdomainTrimChars))
                            subdomain = subdomain.Substring(0, subdomain.LastIndexOf(subdomainTrimChars, StringComparison.Ordinal));
                    }
                    //拼接域名
                    if (!string.IsNullOrWhiteSpace(domain))
                    {
                        methodName += domain + ".";
                    }
                    //拼接子域名
                    if (!string.IsNullOrWhiteSpace(subdomain))
                    {
                        methodName += subdomain + ".";
                    }
                    //拼接方法名
                    methodName += method.Name;

                    //通过参数与实例构建委托方法，将创建的委托添加到Handler的所有委托集中
                    var newDelegate = Delegate.CreateDelegate(Expression.GetDelegateType(paramsTypes.Values.ToArray()), instance, method);
                    var handlerSession = Handler.GetSessionHandler(sessionId);
                    //调用Handler.Register方法
                    regMethod.Invoke(handlerSession, new object[] { methodName, newDelegate });

                    try
                    {
                        //将方法信息添加到MetaData中
                        handlerSession.MetaData.AddService(methodName, paramsTypes, paramsDefaultValues);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Register method " + methodName + " error and skiped, " + ex.ToString());
                    }
                }
            }
        }
    }
}
