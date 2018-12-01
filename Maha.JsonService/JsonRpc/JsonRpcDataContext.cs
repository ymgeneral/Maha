using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Maha.JsonService
{
    /// <summary>
    /// 基于线程的数据上下文
    /// </summary>
    public class JsonRpcDataContext
    {
        //线程Id对应的JsonRpc请求上下文键、值,key=当前线程Id为当前上下文Id,Value=JsonRpcRequestContext.上下文中定义的键、值
        private static ConcurrentDictionary<string, Dictionary<string, object>> Contexts { get; set; }

        //资源锁定器
        private static object contextLocker = new object();

        //TLS中的变量（用于多线程模式下，获取线程本地存储信息）
        private static ThreadLocal<string> local = new ThreadLocal<string>();

        /// <summary>
        /// 获取或初始化当前上下文(基于当前线程Id)
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetOrInitCurrentContext()
        {
            lock (contextLocker)
            {
                //当前线程Id作为上下文Id
                string contextId = GetContextId();
                if (Contexts == null)
                {
                    Contexts = new ConcurrentDictionary<string, Dictionary<string, object>>();
                }

                Dictionary<string, object> context = null;
                if (!Contexts.ContainsKey(contextId) || Contexts[contextId] == null)
                {
                    context = Contexts[contextId] = new Dictionary<string, object>();
                }
                else
                {
                    context = Contexts[contextId];
                }
                return context;
            }
        }
        /// <summary>
        /// 删除当前上下文项
        /// </summary>
        public static void RemoveContext()
        {
            string contextId = GetContextId();
            Dictionary<string, object> currentContext = null;
            Contexts.TryRemove(contextId, out currentContext);
            local.Values.Remove(local.Value);
        }

        /// <summary>
        /// 设置当前上下文内容项
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">值</param>
        public static void SetContextItem(string key, object val)
        {
            var context = GetOrInitCurrentContext();
            context[key] = val;
        }
        /// <summary>
        /// 获取当前上下文内容项
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>当前线程Id对应的值</returns>
        public static object GetContextItem(string key)
        {
            string contextId = GetContextId();
            if (Contexts != null && Contexts.ContainsKey(contextId) && Contexts[contextId] != null && Contexts[contextId].ContainsKey(key))
                return Contexts[contextId][key];
            else
                return null;
        }

        /// <summary>
        /// 获取当前上下文K,V键值
        /// </summary>
        /// <returns>当前线程Id对应的K,V值</returns>
        public static Dictionary<string, object> GetContextKV()
        {
            string contextId = GetContextId();
            if (Contexts != null && Contexts.ContainsKey(contextId) && Contexts[contextId] != null)
            {
                return Contexts[contextId];
            }
            return new Dictionary<string, object>();
        }
        /// <summary>
        /// 获取当前上下文内容项（转换为int）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">未找到项，则返回默认值</param>
        /// <returns>(int)值</returns>
        public static int GetContextItemInt(string key, int defaultValue)
        {
            object item = GetContextItem(key);
            if (item == null) return defaultValue;

            if (int.TryParse(item.ToString(), out int ret))
                return ret;
            else
                return defaultValue;
        }
        /// <summary>
        /// 获取当前上下文内容项（转换为String）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="defaultValue">未找到项，则返回默认值</param>
        /// <returns>(int)值</returns>
        public static string GetContextItemString(string key)
        {
            object item = GetContextItem(key);
            if (item == null) return null;
            return item.ToString();
        }

        /// <summary>
        /// 获取当前线程的上下文Id
        /// </summary>
        /// <returns></returns>
        private static string GetContextId()
        {
            if (local.Value == null)
            {
                var newTaskId = System.Threading.Tasks.Task.CurrentId.HasValue ? System.Threading.Tasks.Task.CurrentId.ToString() : Guid.NewGuid().ToString();
                local.Value = newTaskId;
                return newTaskId;
            }
            else
            {
                return local.Value;
            }
        }
    }
}
