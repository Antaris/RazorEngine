namespace RazorEngine.Templating
{
    using System.Collections.Generic;
    using System.Dynamic;

    /// <summary>
    /// Defines a dynamic view bag.
    /// </summary>
    public class DynamicViewBag : DynamicObject
    {
        #region Fields
        private readonly IDictionary<string, object> _dict = new Dictionary<string, object>();
        #endregion

        #region Methods
        /// <summary>
        /// Gets the set of dynamic member names.
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerable{String}"/>.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _dict.Keys;
        }

        /// <summary>
        /// Attempts to read a dynamic member from the object.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="result">The result instance.</param>
        /// <returns>True, always.</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_dict.ContainsKey(binder.Name))
                result = _dict[binder.Name];
            else
                result = null;

            return true;
        }

        /// <summary>
        /// Attempts to set a value on the object.
        /// </summary>
        /// <param name="binder">The binder.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True, always.</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_dict.ContainsKey(binder.Name))
                _dict[binder.Name] = value;
            else
                _dict.Add(binder.Name, value);

            return true;
        }
        #endregion
    }
}