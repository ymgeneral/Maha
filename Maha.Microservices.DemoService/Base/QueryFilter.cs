using System.ComponentModel.DataAnnotations;

namespace Maha.Microservices.DemoService.Base
{
    /// <summary>
    /// 查询过滤器
    /// </summary>
    public class QueryFilter
    {
        /// <summary>
        /// 查询过滤器构造器
        /// </summary>
        public QueryFilter()
        {
            this.PageIndex = 1;
            this.PageSize = 10;
        }

        /// <summary>
        /// 页码
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageIndex { get; set; }

        /// <summary>
        /// 页数
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageSize { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortFields { get; set; }
    }
}
