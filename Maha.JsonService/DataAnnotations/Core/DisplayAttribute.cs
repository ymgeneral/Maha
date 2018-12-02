using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Maha.JsonService.DataAnnotations
{
    public sealed class DisplayAttribute : Attribute
    {
        #region Member Fields

        private Type _resourceType;
        private LocalizableString _shortName = new LocalizableString("ShortName");
        private LocalizableString _name = new LocalizableString("Name");
        private LocalizableString _description = new LocalizableString("Description");
        private LocalizableString _prompt = new LocalizableString("Prompt");
        private LocalizableString _groupName = new LocalizableString("GroupName");
        private bool? _autoGenerateField;
        private bool? _autoGenerateFilter;
        private int? _order;

        #endregion

        #region Properties

        public string ShortName
        {
            get
            {
                return this._shortName.Value;
            }
            set
            {
                if (this._shortName.Value != value)
                {
                    this._shortName.Value = value;
                }
            }
        }

        public string Name
        {
            get
            {
                return this._name.Value;
            }
            set
            {
                if (this._name.Value != value)
                {
                    this._name.Value = value;
                }
            }
        }

        public string Description
        {
            get
            {
                return this._description.Value;
            }
            set
            {
                if (this._description.Value != value)
                {
                    this._description.Value = value;
                }
            }
        }

        public string Prompt
        {
            get
            {
                return this._prompt.Value;
            }
            set
            {
                if (this._prompt.Value != value)
                {
                    this._prompt.Value = value;
                }
            }
        }

        public string GroupName
        {
            get
            {
                return this._groupName.Value;
            }
            set
            {
                if (this._groupName.Value != value)
                {
                    this._groupName.Value = value;
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
                    this._resourceType = value;

                    this._shortName.ResourceType = value;
                    this._name.ResourceType = value;
                    this._description.ResourceType = value;
                    this._prompt.ResourceType = value;
                    this._groupName.ResourceType = value;
                }
            }
        }

        public bool AutoGenerateField
        {
            get
            {
                if (!this._autoGenerateField.HasValue)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "AutoGenerateField", "GetAutoGenerateField"));
                }

                return this._autoGenerateField.Value;
            }
            set
            {
                this._autoGenerateField = value;
            }
        }

        public bool AutoGenerateFilter
        {
            get
            {
                if (!this._autoGenerateFilter.HasValue)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "AutoGenerateFilter", "GetAutoGenerateFilter"));
                }

                return this._autoGenerateFilter.Value;
            }
            set
            {
                this._autoGenerateFilter = value;
            }
        }

        public int Order
        {
            get
            {
                if (!this._order.HasValue)
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.DisplayAttribute_PropertyNotSet, "Order", "GetOrder"));
                }

                return this._order.Value;
            }
            set
            {
                this._order = value;
            }
        }

        #endregion

        #region Methods

        public string GetShortName()
        {
            return this._shortName.GetLocalizableValue() ?? this.GetName();
        }
        
        public string GetName()
        {
            return this._name.GetLocalizableValue();
        }
        
        public string GetDescription()
        {
            return this._description.GetLocalizableValue();
        }
       
        public string GetPrompt()
        {
            return this._prompt.GetLocalizableValue();
        }
       
        public string GetGroupName()
        {
            return this._groupName.GetLocalizableValue();
        }

        public bool? GetAutoGenerateField()
        {
            return this._autoGenerateField;
        }

        public bool? GetAutoGenerateFilter()
        {
            return this._autoGenerateFilter;
        }

        public int? GetOrder()
        {
            return this._order;
        }

        #endregion
    }
}
