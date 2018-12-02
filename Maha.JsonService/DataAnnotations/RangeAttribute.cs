using System;
using System.ComponentModel;
using System.Globalization;

namespace Maha.JsonService.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class RangeAttribute: ValidationAttribute
    {
        public object Minimum { get; private set; }

        public object Maximum { get; private set; }

        public Type OperandType { get; private set; }

        private Func<object, object> Conversion { get; set; }

        public RangeAttribute(int minimum, int maximum)
            : this()
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.OperandType = typeof(int);
        }

        public RangeAttribute(double minimum, double maximum)
            : this()
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
            this.OperandType = typeof(double);
        }

        public RangeAttribute(Type type, string minimum, string maximum)
            : this()
        {
            this.OperandType = type;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        private RangeAttribute()
            : base(() => DataAnnotationsResources.RangeAttribute_ValidationError)
        {
        }

        private void Initialize(IComparable minimum, IComparable maximum, Func<object, object> conversion)
        {
            if (minimum.CompareTo(maximum) > 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.RangeAttribute_MinGreaterThanMax, maximum, minimum));
            }

            this.Minimum = minimum;
            this.Maximum = maximum;
            this.Conversion = conversion;
        }

        public override bool IsValid(object value)
        {
            // Validate our properties and create the conversion function
            this.SetupConversion();

            // Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
            if (value == null)
            {
                return true;
            }
            string s = value as string;
            if (s != null && String.IsNullOrEmpty(s))
            {
                return true;
            }

            object convertedValue = null;

            try
            {
                convertedValue = this.Conversion(value);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (InvalidCastException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }

            IComparable min = (IComparable)this.Minimum;
            IComparable max = (IComparable)this.Maximum;
            return min.CompareTo(convertedValue) <= 0 && max.CompareTo(convertedValue) >= 0;
        }

        public override string FormatErrorMessage(string name)
        {
            this.SetupConversion();

            return String.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, this.Minimum, this.Maximum);
        }

        private void SetupConversion()
        {
            if (this.Conversion == null)
            {
                object minimum = this.Minimum;
                object maximum = this.Maximum;

                if (minimum == null || maximum == null)
                {
                    throw new InvalidOperationException(DataAnnotationsResources.RangeAttribute_Must_Set_Min_And_Max);
                }

                // Careful here -- OperandType could be int or double if they used the long form of the ctor.
                // But the min and max would still be strings.  Do use the type of the min/max operands to condition
                // the following code.
                Type operandType = minimum.GetType();

                if (operandType == typeof(int))
                {
                    this.Initialize((int)minimum, (int)maximum, v => Convert.ToInt32(v, CultureInfo.InvariantCulture));
                }
                else if (operandType == typeof(double))
                {
                    this.Initialize((double)minimum, (double)maximum, v => Convert.ToDouble(v, CultureInfo.InvariantCulture));
                }
                else
                {
                    Type type = this.OperandType;
                    if (type == null)
                    {
                        throw new InvalidOperationException(DataAnnotationsResources.RangeAttribute_Must_Set_Operand_Type);
                    }
                    Type comparableType = typeof(IComparable);
                    if (!comparableType.IsAssignableFrom(type))
                    {
                        throw new InvalidOperationException(
                            String.Format( CultureInfo.CurrentCulture,
                                DataAnnotationsResources.RangeAttribute_ArbitraryTypeNotIComparable,
                                type.FullName,
                                comparableType.FullName));
                    }

                    TypeConverter converter = TypeDescriptor.GetConverter(type);
                    IComparable min = (IComparable)converter.ConvertFromString((string)minimum);
                    IComparable max = (IComparable)converter.ConvertFromString((string)maximum);

                    Func<object, object> conversion = value => (value != null && value.GetType() == type) ? value : converter.ConvertFrom(value);

                    this.Initialize(min, max, conversion);
                }
            }
        }
    }
}
