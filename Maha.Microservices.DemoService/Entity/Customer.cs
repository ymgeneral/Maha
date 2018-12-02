using Maha.JsonService.DataAnnotations;

namespace Maha.Microservices.DemoService.Entity
{
    /// <summary>
    /// 顾客
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// 用户姓名
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }
    }
}
