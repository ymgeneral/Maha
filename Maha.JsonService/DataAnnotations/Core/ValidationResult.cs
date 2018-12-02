using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Maha.JsonService.DataAnnotations
{
    public class ValidationResult
    {
        #region Member Fields

        private IEnumerable<string> _memberNames;
        private string _errorMessage;

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "We want this to be readonly since we're just returning null")]
        public static readonly ValidationResult Success;

        #endregion

        #region All Constructors

        public ValidationResult(string errorMessage)
            : this(errorMessage, null)
        {
        }

        public ValidationResult(string errorMessage, IEnumerable<string> memberNames)
        {
            this._errorMessage = errorMessage;
            this._memberNames = memberNames ?? new string[0];
        }


        protected ValidationResult(ValidationResult validationResult)
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException("validationResult");
            }

            this._errorMessage = validationResult._errorMessage;
            this._memberNames = validationResult._memberNames;
        }
        #endregion

        #region Properties

        public IEnumerable<string> MemberNames
        {
            get
            {
                return this._memberNames;
            }
        }


        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }
            set
            {
                this._errorMessage = value;
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return this.ErrorMessage ?? base.ToString();
        }

        #endregion Methods
    }
}
