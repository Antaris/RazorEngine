namespace RazorEngine.Tests.TestTypes
{
    using System.Collections.Generic;
    using System.Dynamic;

    /// <summary>
    /// Test class.
    /// </summary>
    public class ValueObject : DynamicObject
    {
        #region Fields
        private readonly IDictionary<string, object> _values;
        #endregion

        #region Constructor
        /// <summary>
        /// Test class.
        /// </summary>
        public ValueObject(IDictionary<string, object> values)
        {
            _values = values;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Test class.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_values.ContainsKey(binder.Name))
            {
                result = _values[binder.Name];
                return true;
            }

            result = null;
            return false;
        }
        #endregion
    }
}
