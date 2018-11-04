using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Maha.JsonService 
{
    public class JsonRpcDataContext
    {
        private const string ContextIdKey = "JsonRpcDataContext-main-task-id";
        private static ConcurrentDictionary<string, Dictionary<string, object>> Contexts { get; set; }
        private static object contextLocker = new object();

        //TLS中的变量（用于多线程模式下，获取线程本地存储信息）
        private static ThreadLocal<string> local = new ThreadLocal<string>();


        public static Dictionary<string, object> Start()
        {
            lock (contextLocker)
            {
                string contextId = GetCurrentContextId();
                if (Contexts == null)
                {
                    Contexts = new ConcurrentDictionary<string, Dictionary<string, object>>();
                }

                Dictionary<string, object> context;
                if (!Contexts.ContainsKey(contextId) || Contexts[contextId] == null)
                    context = Contexts[contextId] = new Dictionary<string, object>();
                else
                    context = Contexts[contextId];

                return context;
            }
        }

        public static void SetContextItem(string key, object val)
        {
            var context = Start();
            context[key] = val;
        }

        private static string GetCurrentContextId()
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

        public static void RemoveContext()
        {
            string contextId = GetCurrentContextId();
            Dictionary<string, object> currentContext;
            Contexts.TryRemove(contextId, out currentContext);
            local.Values.Remove(local.Value);
        }

        public static object GetContextItem(string key)
        {
            string contextId = GetCurrentContextId();
            if (Contexts != null && Contexts.ContainsKey(contextId) && Contexts[contextId] != null && Contexts[contextId].ContainsKey(key))
                return Contexts[contextId][key];
            else
                return null;
        }

        public static Dictionary<string, object> GetCurrentContextDict()
        {
            string contextId = GetCurrentContextId();
            if (Contexts != null && Contexts.ContainsKey(contextId) && Contexts[contextId] != null)
            {
                return Contexts[contextId];
            }
            return new Dictionary<string, object>();
        }

        public static int GetContextItemInt(string key, int defaultValue)
        {
            object orgValue = GetContextItem(key);
            if (orgValue == null) return defaultValue;

            string stringValue = GetContextItem(key) as string;
            if (int.TryParse(stringValue, out int ret))
                return ret;
            else
                return defaultValue;
        }

        public static string GetContextItemString(string key)
        {
            object orgValue = GetContextItem(key);
            if (orgValue == null)  return null;
            return GetContextItem(key).ToString();
        }
    }
}
