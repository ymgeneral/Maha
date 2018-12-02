using System;
using System.Globalization;
using System.Reflection;

namespace Maha.JsonService.DataAnnotations
{
    public class ValidationAttribute : Attribute
    {
        #region Member Fields

        private string _errorMessage;
        private Func<string> _errorMessageResourceAccessor;
        private string _errorMessageResourceName;
        private Type _errorMessageResourceType;
        private string _defaultErrorMessage;

        private volatile bool _hasBaseIsValid;

        #endregion

        #region All Constructors

        /// <summary>
        /// Default constructor for any validation attribute.
        /// </summary>
        /// <remarks>This constructor chooses a very generic validation error message.
        /// Developers subclassing ValidationAttribute should use other constructors
        /// or supply a better message.
        /// </remarks>
        protected ValidationAttribute()
            : this(() => DataAnnotationsResources.ValidationAttribute_ValidationError)
        {
        }

        /// <summary>
        /// Constructor that accepts a fixed validation error message.
        /// </summary>
        /// <param name="errorMessage">A non-localized error message to use in <see cref="ErrorMessageString"/>.</param>
        protected ValidationAttribute(string errorMessage)
            : this(() => errorMessage)
        {
        }

        /// <summary>
        /// Allows for providing a resource accessor function that will be used by the <see cref="ErrorMessageString"/>
        /// property to retrieve the error message.  An example would be to have something like
        /// CustomAttribute() : base( () =&gt; MyResources.MyErrorMessage ) {}.
        /// </summary>
        /// <param name="errorMessageAccessor">The <see cref="Func{T}"/> that will return an error message.</param>
        protected ValidationAttribute(Func<string> errorMessageAccessor)
        {
            // If null, will later be exposed as lack of error message to be able to construct accessor
            this._errorMessageResourceAccessor = errorMessageAccessor;
        }

        #endregion

        #region Internal Properties
        /// <summary>
        /// Gets or sets and the default error message string.
        /// This message will be used if the user has not set <see cref="ErrorMessage"/>
        /// or the <see cref="ErrorMessageResourceType"/> and <see cref="ErrorMessageResourceName"/> pair.
        /// This property was added after the public contract for DataAnnotations was created.
        /// It was added to fix DevDiv issue 468241.
        /// It is internal to avoid changing the DataAnnotations contract.
        /// </summary>
        internal string DefaultErrorMessage
        {
            get
            {
                return this._defaultErrorMessage;
            }
            set
            {
                this._defaultErrorMessage = value;
                this._errorMessageResourceAccessor = null;
                this.CustomErrorMessageSet = true;
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the localized error message string, coming either from <see cref="ErrorMessage"/>, or from evaluating the 
        /// <see cref="ErrorMessageResourceType"/> and <see cref="ErrorMessageResourceName"/> pair.
        /// </summary>
        protected string ErrorMessageString
        {
            get
            {
                this.SetupResourceAccessor();
                return this._errorMessageResourceAccessor();
            }
        }

        /// <summary>
        /// A flag indicating whether a developer has customized the attribute's error message by setting any one of 
        /// ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType or DefaultErrorMessage.
        /// </summary>
        internal bool CustomErrorMessageSet
        {
            get;
            private set;
        }

        /// <summary>
        /// A flag indicating that the attribute requires a non-null <see cref=System.ComponentModel.DataAnnotations.ValidationContext /> to perform validation.
        /// Base class returns false. Override in child classes as appropriate.
        /// </summary>
        public virtual bool RequiresValidationContext
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets and explicit error message string.
        /// </summary>
        /// <value>
        /// This property is intended to be used for non-localizable error messages.  Use
        /// <see cref="ErrorMessageResourceType"/> and <see cref="ErrorMessageResourceName"/> for localizable error messages.
        /// </value>
        public string ErrorMessage
        {
            get
            {
                // DevDiv: 468241
                // If _errorMessage is not set, return the default. This is done to preserve
                // behavior prior to the fix where ErrorMessage showed the non-null message to use.
                return this._errorMessage ?? this._defaultErrorMessage;
            }
            set
            {
                this._errorMessage = value;
                this._errorMessageResourceAccessor = null;
                this.CustomErrorMessageSet = true;

                // DevDiv: 468241
                // Explicitly setting ErrorMessage also sets DefaultErrorMessage if null.
                // This prevents subsequent read of ErrorMessage from returning default.
                if (value == null)
                {
                    this._defaultErrorMessage = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the resource name (property name) to use as the key for lookups on the resource type.
        /// </summary>
        /// <value>
        /// Use this property to set the name of the property within <see cref="ErrorMessageResourceType"/>
        /// that will provide a localized error message.  Use <see cref="ErrorMessage"/> for non-localized error messages.
        /// </value>
        public string ErrorMessageResourceName
        {
            get
            {
                return this._errorMessageResourceName;
            }
            set
            {
                this._errorMessageResourceName = value;
                this._errorMessageResourceAccessor = null;
                this.CustomErrorMessageSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the resource type to use for error message lookups.
        /// </summary>
        /// <value>
        /// Use this property only in conjunction with <see cref="ErrorMessageResourceName"/>.  They are
        /// used together to retrieve localized error messages at runtime.
        /// <para>Use <see cref="ErrorMessage"/> instead of this pair if error messages are not localized.
        /// </para>
        /// </value>
        public Type ErrorMessageResourceType
        {
            get
            {
                return this._errorMessageResourceType;
            }
            set
            {
                this._errorMessageResourceType = value;
                this._errorMessageResourceAccessor = null;
                this.CustomErrorMessageSet = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the configuration of this attribute and sets up the appropriate error string accessor.
        /// This method bypasses all verification once the ResourceAccessor has been set.
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is malformed.</exception>
        private void SetupResourceAccessor()
        {
            if (this._errorMessageResourceAccessor == null)
            {
                string localErrorMessage = this.ErrorMessage;
                bool resourceNameSet = !string.IsNullOrEmpty(this._errorMessageResourceName);
                bool errorMessageSet = !string.IsNullOrEmpty(this._errorMessage);
                bool resourceTypeSet = this._errorMessageResourceType != null;
                bool defaultMessageSet = !string.IsNullOrEmpty(this._defaultErrorMessage);

                // The following combinations are illegal and throw InvalidOperationException:
                //   1) Both ErrorMessage and ErrorMessageResourceName are set, or
                //   2) None of ErrorMessage, ErrorMessageReourceName, and DefaultErrorMessage are set.
                if ((resourceNameSet && errorMessageSet) || !(resourceNameSet || errorMessageSet || defaultMessageSet))
                {
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_Cannot_Set_ErrorMessage_And_Resource);
                }

                // Must set both or neither of ErrorMessageResourceType and ErrorMessageResourceName
                if (resourceTypeSet != resourceNameSet)
                {
                    throw new InvalidOperationException(DataAnnotationsResources.ValidationAttribute_NeedBothResourceTypeAndResourceName);
                }

                // If set resource type (and we know resource name too), then go setup the accessor
                if (resourceNameSet)
                {
                    this.SetResourceAccessorByPropertyLookup();
                }
                else
                {
                    // Here if not using resource type/name -- the accessor is just the error message string,
                    // which we know is not empty to have gotten this far.
                    this._errorMessageResourceAccessor = delegate
                    {
                        // We captured error message to local in case it changes before accessor runs
                        return localErrorMessage;
                    };
                }
            }
        }

        private void SetResourceAccessorByPropertyLookup()
        {
            if (this._errorMessageResourceType != null && !string.IsNullOrEmpty(this._errorMessageResourceName))
            {
                var property = this._errorMessageResourceType.GetProperty(this._errorMessageResourceName, BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                if (property != null)
                {
                    MethodInfo propertyGetter = property.GetGetMethod(true /*nonPublic*/);
                    // We only support internal and public properties
                    if (propertyGetter == null || (!propertyGetter.IsAssembly && !propertyGetter.IsPublic))
                    {
                        // Set the property to null so the exception is thrown as if the property wasn't found
                        property = null;
                    }
                }
                if (property == null)
                {
                    throw new InvalidOperationException(
                        String.Format(
                        CultureInfo.CurrentCulture,
                        DataAnnotationsResources.ValidationAttribute_ResourceTypeDoesNotHaveProperty,
                        this._errorMessageResourceType.FullName,
                        this._errorMessageResourceName));
                }
                if (property.PropertyType != typeof(string))
                {
                    throw new InvalidOperationException(
                        String.Format(
                        CultureInfo.CurrentCulture,
                        DataAnnotationsResources.ValidationAttribute_ResourcePropertyNotStringType,
                        property.Name,
                        this._errorMessageResourceType.FullName));
                }

                this._errorMessageResourceAccessor = delegate
                {
                    return (string)property.GetValue(null, null);
                };
            }
            else
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationAttribute_NeedBothResourceTypeAndResourceName));
            }
        }

        #endregion

        #region Protected & Public Methods

        public virtual string FormatErrorMessage(string name)
        {
            return String.Format(CultureInfo.CurrentCulture, this.ErrorMessageString, name);
        }

        public virtual bool IsValid(object value)
        {
            if (!this._hasBaseIsValid)
            {
                // track that this method overload has not been overridden.
                this._hasBaseIsValid = true;
            }

            // call overridden method.
            return this.IsValid(value, null) == null;
        }

        protected virtual ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (this._hasBaseIsValid)
            {
                // this means neither of the IsValid methods has been overriden, throw.
                throw new NotImplementedException(DataAnnotationsResources.ValidationAttribute_IsValid_NotImplemented);
            }

            ValidationResult result = ValidationResult.Success;

            // call overridden method.
            if (!this.IsValid(value))
            {
                string[] memberNames = validationContext.MemberName != null ? new string[] { validationContext.MemberName } : null;
                result = new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName), memberNames);
            }

            return result;
        }

        public ValidationResult GetValidationResult(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }

            ValidationResult result = this.IsValid(value, validationContext);

            // If validation fails, we want to ensure we have a ValidationResult that guarantees it has an ErrorMessage
            if (result != null)
            {
                bool hasErrorMessage = (result != null) ? !string.IsNullOrEmpty(result.ErrorMessage) : false;
                if (!hasErrorMessage)
                {
                    string errorMessage = this.FormatErrorMessage(validationContext.DisplayName);
                    result = new ValidationResult(errorMessage, result.MemberNames);
                }
            }
            return result;
        }

        public void Validate(object value, string name)
        {
            if (!this.IsValid(value))
            {
                throw new ValidationException(this.FormatErrorMessage(name), this, value);
            }
        }

        public void Validate(object value, ValidationContext validationContext)
        {
            if (validationContext == null)
            {
                throw new ArgumentNullException("validationContext");
            }

            ValidationResult result = this.GetValidationResult(value, validationContext);

            if (result != null)
            {
                // Convenience -- if implementation did not fill in an error message,
                throw new ValidationException(result, this, value);
            }
        }
        #endregion
    }
}
