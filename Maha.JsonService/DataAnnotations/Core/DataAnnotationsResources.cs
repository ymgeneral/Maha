using System.Globalization;

namespace Maha.JsonService.DataAnnotations
{
    internal class DataAnnotationsResources
    {
        #region ValidationAttribute
        internal static string ValidationAttribute_ValidationError
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "验证错误");
            }
        }

        internal static string ValidationAttribute_Cannot_Set_ErrorMessage_And_Resource
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "设置无效，检查参数");
            }
        }

        internal static string ValidationAttribute_NeedBothResourceTypeAndResourceName
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "设置无效，需同时设置资源类型和资源名");
            }
        }

        internal static string ValidationAttribute_ResourceTypeDoesNotHaveProperty
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "设置无效，资源类型不包含属性");
            }
        }

        internal static string ValidationAttribute_ResourcePropertyNotStringType
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "设置无效，资源属性不是String类型");
            }
        }

        internal static string ValidationAttribute_IsValid_NotImplemented
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "IsValid 未实现");
            }
        }
        #endregion ValidationAttribute

        #region ValidationContext

        internal static string ValidationContextServiceContainer_ItemAlreadyExists
        {
            get
            {

                return string.Format(CultureInfo.CurrentCulture, "服务容器项已经存在");
            }
        }


        #endregion ValidationContext

        #region DisplayAttribute

        internal static string DisplayAttribute_PropertyNotSet
        {
            get
            {

                return string.Format(CultureInfo.CurrentCulture, "属性未设置");
            }
        }


        #endregion DisplayAttribute

        #region LocalizableString

        internal static string LocalizableString_LocalizationFailed
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "可本地化字符串定位失败");
            }
        }


        #endregion LocalizableString

        #region AttributeStore_Unknown_Property

        internal static string AttributeStore_Unknown_Property
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "未知属性");
            }
        }

        #endregion AttributeStore_Unknown_Property

        #region RangeAttribute

        internal static string RangeAttribute_ValidationError
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "范围特性，验证错误");
            }
        }

        internal static string RangeAttribute_MinGreaterThanMax
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "范围特性，最大值小于最小值");
            }
        }

        internal static string RangeAttribute_Must_Set_Min_And_Max
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "范围特性，必须设置最大值与最小值");
            }
        }

        internal static string RangeAttribute_Must_Set_Operand_Type
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "范围特性，必须设置操作数类型");
            }
        }

        internal static string RangeAttribute_ArbitraryTypeNotIComparable
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "范围特性，任意类型不可比较");
            }
        }

        #endregion RangeAttributes

        #region RequiredAttribute

        internal static string RequiredAttribute_ValidationError
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "非空验证错误");
            }
        }

        #endregion RequiredAttribute

        #region Validator
        internal static string Validator_InstanceMustMatchValidationContextInstance
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "验证程序实例必须匹配验证上下文实例");
            }
        }

        internal static string Validator_Property_Value_Wrong_Type
        {
            get
            {
                return string.Format(CultureInfo.CurrentCulture, "验证器属性值错误类型");
            }
        }
        #endregion Validator
    }
}
