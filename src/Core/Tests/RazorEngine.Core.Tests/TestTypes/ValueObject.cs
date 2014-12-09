namespace RazorEngine.Tests.TestTypes
{
    using System.Collections.Generic;
    using System.Dynamic;

    public class ValueObject : DynamicObject
    {
        #region Fields
        private readonly IDictionary<string, object> _values;
        #endregion

        #region Constructor
        public ValueObject(IDictionary<string, object> values)
        {
            _values = values;
        }
        #endregion

        #region Methods
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
