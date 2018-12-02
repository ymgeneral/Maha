using Maha.JsonService.DataAnnotations;
using Maha.Microservices.DemoService.Base;
using Maha.Microservices.DemoService.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Maha.Microservices.DemoService
{
    /// <summary>
    /// 订单服务
    /// </summary>
    public class OrderService
    {
        private static readonly List<Order> orders = new List<Order>();
        private static long orderNumberLast;

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="order">订单信息</param>
        /// <returns></returns>
        public long Create(Order order)
        {
            CheckInput(order);
            order.OrderNumber = orderNumberLast++;
            orders.Add(order);
            return orderNumberLast;
        }

        private void CheckInput(Order order)
        {
            Validator.ValidateObject(order, new ValidationContext(order, null, null), true);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(order, new ValidationContext(order, null, null), validationResults, true))
            {
                throw new RpcException(string.Join(Environment.NewLine, validationResults));
            }
        }

        /// <summary>
        /// 获取订单
        /// </summary>
        /// <param name="orderNumber">订单编号</param>
        /// <returns></returns>
        public Order GetByOrderNumber(long orderNumber)
        {
            return orders.Find(it => it.OrderNumber == orderNumber);
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="queryFilter"></param>
        /// <returns></returns>
        public List<Order> QueryOrders(OrderQueryFilter queryFilter)
        {
            ValidatorEx.ValidateObject(queryFilter);
            return orders.Where( it => 
                (queryFilter.OrderNumber == null || queryFilter.OrderNumber == 0 || it.OrderNumber == queryFilter.OrderNumber) &&
                (string.IsNullOrEmpty(queryFilter.CustomerName) || (it.Customer != null && it.Customer.Name == queryFilter.CustomerName))
            ).ToList();
        }

        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="newOrder">新订单</param>
        /// <exception cref="RpcException">找不到订单</exception>
        public void Update(Order newOrder)
        {
            var oldIndex = orders.FindIndex(it => it.OrderNumber == newOrder.OrderNumber);
            var oldOrder = orders[oldIndex];
            if (oldOrder == null)
                throw new RpcException($"找不到订单：{newOrder.OrderNumber}");
            orders[oldIndex] = newOrder;
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="orderNumber">订单编号</param>
        public void Delete(long orderNumber)
        {
            orders.RemoveAll(it => it.OrderNumber == orderNumber);
        }
    }
}