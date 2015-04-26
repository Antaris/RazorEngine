namespace RazorEngine.Templating
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;

    /// <summary>
    /// Defines a dynamic view bag.
    /// </summary>
    [Serializable]
    public class DynamicViewBag : DynamicObject
    {
        #region Fields
        private readonly IDictionary<string, object> _dict = 
            new System.Collections.Generic.Dictionary<string, object>();
        #endregion
        /// <summary>
        /// Create a new DynamicViewBag.
        /// </summary>
        public DynamicViewBag()
        {
        }

        /// <summary>
        /// Create a new DynamicViewBag by copying the given dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public DynamicViewBag(IDictionary<string, object> dictionary)
            : this()
        {
            AddDictionary(dictionary);
        }

        /// <summary>
        /// Create a copy of the given DynamicViewBag.
        /// </summary>
        /// <param name="viewbag"></param>
        public DynamicViewBag(DynamicViewBag viewbag)
            : this(viewbag._dict)
        {
        }


        #region Methods

        /// <summary>
        /// Add the given dictionary to the current DynamicViewBag
        /// </summary>
        /// <param name="valueDictionary"></param>
        [Obsolete("Use the generic AddDictionary overload instead")]
        public void AddDictionaryValues(System.Collections.IDictionary valueDictionary)
        {
            foreach (DictionaryEntry item in valueDictionary)
            {
                _dict.Add(item.Key.ToString(), item.Value);
            }
        }

        /// <summary>
        /// Adds the given dictionary to the current DynamicViewBag instance.
        /// </summary>
        /// <param name="dictionary"></param>
        public void AddDictionary(IDictionary<string, object> dictionary)
        {
            foreach (var item in dictionary)
            {
                _dict.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Add the given dictionary to the current DynamicViewBag
        /// </summary>
        /// <param name="valueDictionary"></param>
        [Obsolete("Use the generic AddDictionary overload instead")]
        public void AddDictionaryValuesEx(IDictionary<string, object> valueDictionary)
        {
            AddDictionary(valueDictionary);
        }

        /// <summary>
        /// Adds the given list by evaluating the given property name.
        /// </summary>
        /// <param name="valueList"></param>
        /// <param name="keyPropertyName"></param>
        [Obsolete("Use the generic AddDictionary or AddValue overload instead")]
        public void AddListValues(IList valueList, string keyPropertyName)
        {
            foreach (var item in valueList)
            {
                var t = item.GetType();
                var prop = t.GetProperty(keyPropertyName);
                if (prop == null)
                {
                    throw new InvalidOperationException(
                        string.Format("Property {0} was not found in {1}", keyPropertyName, t));
                }
                var indx = prop.GetValue(item, null);
                _dict.Add(indx.ToString(), item);
            }
        }

        /// <summary>
        /// Adds a single value.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public void AddValue(string propertyName, object value) 
        {
            _dict.Add(propertyName, value);
        }

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
