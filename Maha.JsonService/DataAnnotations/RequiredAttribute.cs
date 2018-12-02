using System;

namespace Maha.JsonService.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RequiredAttribute : ValidationAttribute
    {
        public RequiredAttribute()
            : base(() => DataAnnotationsResources.RequiredAttribute_ValidationError) { }

        public bool AllowEmptyStrings { get; set; }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return false;
            }

            var stringValue = value as string;
            if (stringValue != null && !AllowEmptyStrings)
            {
                return stringValue.Trim().Length != 0;
            }

            return true;
        }
    }
}
