using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maha.Spotted
{
    /// <summary>
    /// 数据超时管理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TimeOutManager<T>
    {
        public delegate void DataTimeOutHandle(object sender, List<T> e);
        public AutoResetEvent waitClose;
        public event DataTimeOutHandle EndTimeOut;
        private bool isStart = false;
        protected Dictionary<T, DateTime> dicData = new Dictionary<T, DateTime>();
        private Thread timeOutThread;
        /// <summary>
        /// 等待超时数据
        /// </summary>
        public Dictionary<T, DateTime> WaitData
        {
            get
            {
                lock (dicData)
                    return dicData;
            }
        }
        private int timeOut = 3600;
        /// <summary>
        /// 超时时间（单位秒）
        /// </summary>
        public int TimeOut
        {
            get { return timeOut; }
            set { timeOut = value; }
        }
        /// <summary>
        /// 添加数据，等待超时
        /// </summary>
        /// <param name="data"></param>
        public void AddData(T data)
        {
            Thread.Sleep(10);
            lock (dicData)
            {
                dicData[data] = DateTime.Now;
            }
        }
        public void AddData(T data, DateTime lastTime)
        {
            Thread.Sleep(10);
            lock (dicData)
            {
                dicData[data] = lastTime;
            }
        }
        public virtual void Remove(T data)
        {
            lock (dicData)
            {
                dicData.Remove(data);
            }
        }
        /// <summary>
        /// 开启超时监测
        /// </summary>
        public void Start()
        {
            if (isStart)
            {
                return;
            }
            waitClose = new AutoResetEvent(false);
            if (timeOutThread == null || timeOutThread.IsAlive == false)
            {
                timeOutThread = new Thread(Begin);
                timeOutThread.IsBackground = true;
                timeOutThread.Start();
            }
            isStart = true;
        }
        private void Begin()
        {
            while (true)
            {
                if (waitClose.WaitOne(TimeOut * 10))
                {
                    break;
                }
                lock (dicData)
                {
                    List<T> lst = new List<T>();
                    foreach (var item in dicData.Where(item => item.Key != null).ToList())
                    {
                        if (item.Value < DateTime.Now.AddSeconds(-TimeOut))
                        {
                            lst.Add(item.Key);
                            dicData.Remove(item.Key);
                        }
                    }
                    if (EndTimeOut != null && lst.Count > 0)
                    {
                       EndTimeOut(this, lst);
                    }
                }
            }
        }
        /// <summary>
        /// 关闭超时监测
        /// </summary>
        public void Shop()
        {
            waitClose.Set();
            isStart = false;
            timeOutThread = null;
        }
        public T GetTimeOutData()
        {
            return default(T);
        }

    }
}
