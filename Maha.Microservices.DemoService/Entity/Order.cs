using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Maha.Microservices.DemoService.Entity
{
    /// <summary>
    /// 订单
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 订单的总金额 OrderAmount_Field
        /// </summary>
        [Range(1, int.MaxValue)]
        [Bindable(BindableSupport.No)]
        public double OrderAmount_Field;

        /// <summary>
        /// 订单编号（系统自动生成）
        /// </summary>
        public long OrderNumber { get; set; }

        /// <summary>
        /// 创建日期（系统自动生成）
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// 订单的总金额
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "{0}(订单金额)不能为小于{1}。" )]
        public double OrderAmount { get; set; }

        /// <summary>
        /// 订单商品明细
        /// </summary>
        public List<OrderItem> Items { get; set; }

        /// <summary>
        /// 订单的用户信息
        /// </summary>
        [Required]
        public Customer Customer { get; set; }

        /// <summary>
        /// 送货地址
        /// </summary>
        [Required(AllowEmptyStrings = true)]
        public string ShippingAddress { get; set; }

        /// <summary>
        /// 订单的出货方式
        /// </summary>
        [Required]
        public ShippingMode ShippingMode { get; set; }
    }

    /// <summary>
    /// 订单商品
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public long ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 商品单价
        /// </summary>
        public double UnitPrice { get; set; }

        /// <summary>
        /// 商品数量
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// 商品单位
        /// </summary>
        public string UnitName { get; set; }
    }

    /// <summary>
    /// 出货方式
    /// </summary>
    public enum ShippingMode
    {
        /// <summary>
        /// 商家出货
        /// </summary>
        [Description("商家出货")]
        ShippingBySeller = 900,
        /// <summary>
        /// 供应商出货
        /// </summary>
        [Description("供应商出货")]
        ShippingByVendor = 901,
        /// <summary>
        /// 平台出货
        /// </summary>
        [Description("平台出货")]
        ShippingByPlatform = 902
    }
}