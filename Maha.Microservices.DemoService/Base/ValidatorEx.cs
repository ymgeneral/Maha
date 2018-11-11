using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Maha.Microservices.DemoService.Base
{
    /// <summary>
    /// 验证扩展类
    /// 将根据对 System.ComponentModel.DataAnnotations 声明的自定义属性进行验证
    /// </summary>
    public static class ValidatorEx
    {
        /// <summary>
        /// 验证器对象
        /// </summary>
        /// <param name="instance"></param>
        /// <exception cref="RpcException">抛出验证出现的错误</exception>
        public static void ValidateObject(object instance)
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(instance, new ValidationContext(instance, null, null), validationResults,true))
            {
                throw new RpcException(String.Join(Environment.NewLine, validationResults));
            }
        }
    }
}
