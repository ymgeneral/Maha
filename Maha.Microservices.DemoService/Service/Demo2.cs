using System;
using System.Collections.Generic;

namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 演示服务2
    /// </summary>
    public class Demo2
    {
        /// <summary>
        /// 获取一个 User
        /// </summary>
        /// <returns>用户信息</returns>
        public User GetUser()
        {
            return new User
            {
                EditDate = new DateTime(2016, 6, 1, 14, 1, 2, 124),
                InDate = new DateTime(2016, 6, 1, 14, 1, 2, 124)
            };
        }

        /// <summary>
        /// 复制一个 User
        /// </summary>
        /// <param name="user">复制源 User</param>
        /// <returns>复制后的 User信息</returns>
        public User CopyUser(User user)
        {
            return user;
        }

        #region internal input\output class

        /// <summary>
        /// 临时的 UserBase 结构
        /// </summary>
        public class UserBase
        {
            /// <summary>
            /// 用户名
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 创建日期
            /// </summary>
            public DateTime? InDate { get; set; }

            /// <summary>
            /// Dict Property Test
            /// </summary>
            public Dictionary<string, double> Dict { get; set; }
        }

        /// <summary>
        /// 临时的 User 结构
        /// </summary>
        public class User : UserBase
        {
            /// <summary>
            /// 修改日期
            /// </summary>
            public DateTime? EditDate { get; set; }

            /// <summary>
            /// 父级用户
            /// </summary>
            public User ParentUser { get; set; }
        }
        #endregion
    }
}