using System.Collections.Generic;
using System.Data;

namespace Maha.Microservices.DemoService.Base
{
    /// <summary>
    /// 查询结果(泛型)
    /// 带有分页信息的查询结果Entity,所有带分页的查询Service返回类型都必须为此类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryResult<T> : QueryFilter
    {
        /// <summary>
        /// 记录总数
        /// </summary>
        public int RecordsTotal { get; set; }

        /// <summary>
        /// 数据集合
        /// </summary>
        public List<T> Data { get; set; }
    }

    /// <summary>
    /// 查询结果(非泛型)
    /// </summary>
    public class QueryResult : QueryFilter
    {
        /// <summary>
        /// 记录总数
        /// </summary>
        public int RecordsTotal { get; set; }

        /// <summary>
        /// 数据表
        /// </summary>
        public DataTable Data { get; set; }
    }
}
