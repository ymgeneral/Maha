using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;

namespace Maha.JsonService.DataAnnotations
{
    public sealed class ValidationContext : IServiceProvider
    {
        #region Member Fields

        private Func<Type, object> _serviceProvider;
        private object _objectInstance;
        private string _memberName;
        private string _displayName;
        private Dictionary<object, object> _items;

        #endregion

        #region Constructors

        public ValidationContext(object instance)
            : this(instance, null, null) { }


        public ValidationContext(object instance, IDictionary<object, object> items)
            : this(instance, null, items) { }

        public ValidationContext(object instance, IServiceProvider serviceProvider, IDictionary<object, object> items)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (serviceProvider != null)
            {
                this.InitializeServiceProvider(serviceType => serviceProvider.GetService(serviceType));
            }

            IServiceContainer container = serviceProvider as IServiceContainer;
            if (container != null)
            {
                this._serviceContainer = new ValidationContextServiceContainer(container);
            }
            else
            {
                this._serviceContainer = new ValidationContextServiceContainer();
            }

            if (items != null)
            {
                this._items = new Dictionary<object, object>(items);
            }
            else
            {
                this._items = new Dictionary<object, object>();
            }

            this._objectInstance = instance;
        }

        #endregion

        #region Properties

        public object ObjectInstance
        {
            get
            {
                return this._objectInstance;
            }
        }

        public Type ObjectType
        {
            get
            {
                return this.ObjectInstance.GetType();
            }
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayName))
                {
                    this._displayName = this.GetDisplayName();

                    if (string.IsNullOrEmpty(this._displayName))
                    {
                        this._displayName = this.MemberName;

                        if (string.IsNullOrEmpty(this._displayName))
                        {
                            this._displayName = this.ObjectType.Name;
                        }
                    }
                }
                return this._displayName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                this._displayName = value;
            }
        }

        public string MemberName
        {
            get
            {
                return this._memberName;
            }
            set
            {
                this._memberName = value;
            }
        }

        public IDictionary<object, object> Items
        {
            get
            {
                return this._items;
            }
        }

        #endregion

        #region Methods

        private string GetDisplayName()
        {
            string displayName = null;
            ValidationAttributeStore store = ValidationAttributeStore.Instance;
            DisplayAttribute displayAttribute = null;

            if (string.IsNullOrEmpty(this._memberName))
            {
                displayAttribute = store.GetTypeDisplayAttribute(this);
            }
            else if (store.IsPropertyContext(this))
            {
                displayAttribute = store.GetPropertyDisplayAttribute(this);
            }

            if (displayAttribute != null)
            {
                displayName = displayAttribute.GetName();
            }

            return displayName ?? this.MemberName;
        }

        public void InitializeServiceProvider(Func<Type, object> serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        #endregion

        #region IServiceProvider Members

        public object GetService(Type serviceType)
        {
            object service = null;

            if (this._serviceContainer != null)
            {
                service = this._serviceContainer.GetService(serviceType);
            }

            if (service == null && this._serviceProvider != null)
            {
                service = this._serviceProvider(serviceType);
            }
            return service;
        }

        #endregion

        #region Service Container

        private IServiceContainer _serviceContainer;


        public IServiceContainer ServiceContainer
        {
            get
            {
                if (this._serviceContainer == null)
                {
                    this._serviceContainer = new ValidationContextServiceContainer();
                }

                return this._serviceContainer;
            }
        }


        private class ValidationContextServiceContainer : IServiceContainer
        {
            #region Member Fields

            private IServiceContainer _parentContainer;
            private Dictionary<Type, object> _services = new Dictionary<Type, object>();
            private readonly object _lock = new object();

            #endregion

            #region Constructors

            internal ValidationContextServiceContainer() { }


            internal ValidationContextServiceContainer(IServiceContainer parentContainer)
            {
                this._parentContainer = parentContainer;
            }

            #endregion

            #region IServiceContainer Members

            public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
                if (promote && this._parentContainer != null)
                {
                    this._parentContainer.AddService(serviceType, callback, promote);
                }
                else
                {
                    lock (this._lock)
                    {
                        if (this._services.ContainsKey(serviceType))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationContextServiceContainer_ItemAlreadyExists, serviceType), "serviceType");
                        }

                        this._services.Add(serviceType, callback);
                    }
                }
            }

            public void AddService(Type serviceType, ServiceCreatorCallback callback)
            {
                this.AddService(serviceType, callback, true);
            }

            public void AddService(Type serviceType, object serviceInstance, bool promote)
            {
                if (promote && this._parentContainer != null)
                {
                    this._parentContainer.AddService(serviceType, serviceInstance, promote);
                }
                else
                {
                    lock (this._lock)
                    {
                        if (this._services.ContainsKey(serviceType))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.ValidationContextServiceContainer_ItemAlreadyExists, serviceType), "serviceType");
                        }

                        this._services.Add(serviceType, serviceInstance);
                    }
                }
            }

            public void AddService(Type serviceType, object serviceInstance)
            {
                this.AddService(serviceType, serviceInstance, true);
            }

            public void RemoveService(Type serviceType, bool promote)
            {
                lock (this._lock)
                {
                    if (this._services.ContainsKey(serviceType))
                    {
                        this._services.Remove(serviceType);
                    }
                }

                if (promote && this._parentContainer != null)
                {
                    this._parentContainer.RemoveService(serviceType);
                }
            }

            public void RemoveService(Type serviceType)
            {
                this.RemoveService(serviceType, true);
            }

            #endregion

            #region IServiceProvider Members

            public object GetService(Type serviceType)
            {
                if (serviceType == null)
                {
                    throw new ArgumentNullException("serviceType");
                }

                object service = null;
                this._services.TryGetValue(serviceType, out service);

                if (service == null && this._parentContainer != null)
                {
                    service = this._parentContainer.GetService(serviceType);
                }

                ServiceCreatorCallback callback = service as ServiceCreatorCallback;

                if (callback != null)
                {
                    service = callback(this, serviceType);
                }

                return service;
            }
            #endregion
        }
        #endregion
    }
}
