using Maha.Microservices.DemoService.Base;

namespace Maha.Microservices.DemoService.Entity
{
    /// <summary>
    /// 学生过滤条件
    /// </summary>
    public class StudentQueryFilter: QueryFilter
    {
        /// <summary>
        /// Id
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 学生姓名
        /// </summary>
        public string Name { get; set; }
    }
}
