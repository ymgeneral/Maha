using Maha.JsonClient;
using Maha.JsonService;
using Maha.Microservices.DemoService.Base;
using Maha.Microservices.DemoService.Entity;
using System;
using System.Text;

namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 演示服务1
    /// </summary>
    public class Demo1
    {
        /// <summary>
        /// 输出两个参数的值
        /// </summary>
        /// <param name="param1">参数1</param>
        /// <param name="param2">参数2</param>
        /// <returns>返回参数1和参数2的信息</returns>
        public string Method1(string param1, string param2)
        {
            return $"param1: {param1}, param2: {param2}";
        }

        /// <summary>
        /// 不做任何事
        /// </summary>
        /// <param name="param1">参数1</param>
        public Order Method2(string param1)
        {
            // 服务内再调其他服务
            return JsonRpcRequest.Call<Order>("Demo.Demo1.Method3", new User() { UserName = param1});
            // Do nothing
        }

        /// <summary>
        /// 返回一个订单对象
        /// </summary>
        /// <param name="customer">顾客信息</param>
        /// <returns></returns>
        public Order Method3(Customer customer)
        {
            Order order = new Order
            {
                Customer = customer,
                OrderNumber = DateTime.Now.Ticks
            };
            return order;
        }

        /// <summary>
        /// 返回一个异常测试
        /// </summary>
        /// <exception cref="RpcException"></exception>
        public void RpcExceptionTest()
        {
            throw new RpcException("RpcExceptionContent");
        }

        /// <summary>
        /// 返回一个业务异常测试
        /// </summary>
        /// <exception cref="RpcException"></exception>
        public void SystemExceptionTest()
        {
            throw new ApplicationException("ApplicationExceptionContent");
        }

        /// <summary>
        /// 获取上下文
        /// </summary>
        /// <returns>用户信息</returns>
        public string GetContextData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("userid: {0}", JsonRpcDataContext.GetContextItem("userid")).AppendLine();
            sb.AppendFormat("companycode: {0}", JsonRpcDataContext.GetContextItem("companycode")).AppendLine();
            return sb.ToString();
        }
    }
}
