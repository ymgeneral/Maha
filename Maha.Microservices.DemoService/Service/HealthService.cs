namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 健康检查服务
    /// </summary>
    public class HealthService
    {
        /// <summary>
        /// consul 心跳健康检查
        /// </summary>
        /// <returns></returns>
        public string check()
        {
            return "ok";
        }
    }
}
