using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Maha.JsonService.DataAnnotations
{
    internal class LocalizableString
    {
        #region Member fields

        private string _propertyName;
        private string _propertyValue;
        private Type _resourceType;

        private Func<string> _cachedResult;

        #endregion

        #region All Constructors

        public LocalizableString(string propertyName)
        {
            this._propertyName = propertyName;
        }

        #endregion

        #region Properties

        public string Value
        {
            get
            {
                return this._propertyValue;
            }
            set
            {
                if (this._propertyValue != value)
                {
                    this.ClearCache();
                    this._propertyValue = value;
                }
            }
        }

        public Type ResourceType
        {
            get
            {
                return this._resourceType;
            }
            set
            {
                if (this._resourceType != value)
                {
                    this.ClearCache();
                    this._resourceType = value;
                }
            }
        }

        #endregion

        #region Methods

        private void ClearCache()
        {
            this._cachedResult = null;
        }

        public string GetLocalizableValue()
        {
            if (this._cachedResult == null)
            {
                if (this._propertyValue == null || this._resourceType == null)
                {
                    this._cachedResult = () => this._propertyValue;
                }
                else
                {
                    PropertyInfo property = this._resourceType.GetProperty(this._propertyValue);

                    bool badlyConfigured = false;

                    if (!this._resourceType.IsVisible || property == null || property.PropertyType != typeof(string))
                    {
                        badlyConfigured = true;
                    }
                    else
                    {
                        MethodInfo getter = property.GetGetMethod();

                        if (getter == null || !(getter.IsPublic && getter.IsStatic))
                        {
                            badlyConfigured = true;
                        }
                    }

                    if (badlyConfigured)
                    {
                        string exceptionMessage = String.Format(CultureInfo.CurrentCulture,
#if SYSTEM_WEB
                            SR.GetString(SR.LocalizableString_LocalizationFailed),
#else
                            DataAnnotationsResources.LocalizableString_LocalizationFailed,
#endif
                            this._propertyName, this._resourceType.FullName, this._propertyValue);
                        this._cachedResult = () => { throw new InvalidOperationException(exceptionMessage); };
                    }
                    else
                    {
                        this._cachedResult = () => (string)property.GetValue(null, null);
                    }
                }
            }
            return this._cachedResult();
        }

        #endregion
    }
}
