using System.Threading;

namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 演示服务3
    /// </summary>
    public class Demo3
    {
        /// <summary>
        /// 根据输入参数设定的秒数，等待后返回
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public string WaitSenconds(int seconds)
        {
            Thread.Sleep(seconds * 1000);
            //throw new RpcException("test error");
            return $"Waited {seconds} seconds";
        }
    }
}