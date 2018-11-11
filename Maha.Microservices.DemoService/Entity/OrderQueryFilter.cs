using Maha.Microservices.DemoService.Base;

namespace Maha.Microservices.DemoService.Entity
{
    /// <summary>
    /// 订单过虑条件
    /// </summary>
    public class OrderQueryFilter: QueryFilter
    {
        /// <summary>
        /// 订单编号
        /// </summary>
        public long? OrderNumber { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        public string CustomerName { get; set; }
    }
}