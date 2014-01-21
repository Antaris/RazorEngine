namespace RazorEngine.Templating
{
    using System;
    using System.Collections;
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

        /// <summary>
        /// Initialises a new instance of <see cref="DynamicViewBag"/>
        /// </summary>
        /// <param name="viewbag">The parent view bag.</param>
        public DynamicViewBag(DynamicViewBag viewbag = null)
        {
            if (viewbag != null)
            {
                // Add the viewbag to the current dictionary.
                foreach (var pair in viewbag._dict)
                    _dict.Add(pair);
            }
        }

        #region DynamicObject Overrides

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

        #region Helper Methods

        /// <summary>
        /// Set a value in this instance of DynamicViewBag.
        /// </summary>
        /// <param name="propertyName">
        /// The property name through which this value can be get/set.
        /// </param>
        /// <param name="value">
        /// The value that will be assigned to this property name.
        /// </param>
        public void SetValue(string propertyName, object value)
        {
            if (propertyName == null)
                throw new ArgumentNullException("The propertyName parameter may not be NULL.");

            _dict[propertyName] = value;
        }
        
        /// <summary>
        /// Add a value to this instance of DynamicViewBag.
        /// </summary>
        /// <param name="propertyName">
        /// The property name through which this value can be get/set.
        /// </param>
        /// <param name="value">
        /// The value that will be assigned to this property name.
        /// </param>
        public void AddValue(string propertyName, object value)
        {
            if (propertyName == null)
                throw new ArgumentNullException("The propertyName parameter may not be NULL.");

            if (_dict.ContainsKey(propertyName) == true)
                throw new ArgumentException("Attempt to add duplicate value for the '" + propertyName + "' property.");

            _dict.Add(propertyName, value);
        }

        /// <summary>
        /// Adds values from the specified valueList to this instance of DynamicViewBag.
        /// </summary>
        /// <param name="valueList">
        /// A list of objects.  Each must have a public property of keyPropertyName.
        /// </param>
        /// <param name="keyPropertyName">
        /// The property name that will be retrieved for each object in the specified valueList
        /// and used as the key (property name) for the ViewBag.  This property must be of type string.
        /// </param>
        public void AddListValues(IList valueList, string keyPropertyName)
        {
            foreach (object value in valueList)
            {
                if (value == null)
                    throw new ArgumentNullException("Invalid NULL value in initializer list.");

                Type type = value.GetType();
                object objKey = type.GetProperty(keyPropertyName);

                if (objKey.GetType() != typeof(string))
                    throw new ArgumentNullException("The keyPropertyName property must be of type string.");

                string strKey = (string)objKey;

                if (_dict.ContainsKey(strKey) == true)
                    throw new ArgumentException("Attempt to add duplicate value for the '" + strKey + "' property.");

                _dict.Add(strKey, value);
            }
        }

        /// <summary>
        /// Adds values from the specified valueDictionary to this instance of DynamicViewBag.
        /// </summary>
        /// <param name="valueDictionary">
        /// A dictionary of objects.  The Key of each item in the dictionary will be used
        /// as the key (property name) for the ViewBag.
        /// </param>
        public void AddDictionaryValues(IDictionary valueDictionary)
        {
            foreach (object objKey in valueDictionary.Keys)
            {
                if (objKey.GetType() != typeof(string))
                    throw new ArgumentNullException("The Key in valueDictionary must be of type string.");

                string strKey = (string)objKey;

                if (_dict.ContainsKey(strKey) == true)
                    throw new ArgumentException("Attempt to add duplicate value for the '" + strKey + "' property.");

                object value = valueDictionary[strKey];

                _dict.Add(strKey, value);
            }
        }

        /// <summary>
        /// Adds values from the specified valueDictionary to this instance of DynamicViewBag.
        /// </summary>
        /// <param name="valueDictionary">
        /// A generic dictionary of {string, object} objects.  The Key of each item in the 
        /// dictionary will be used as the key (property name) for the ViewBag.
        /// </param>
        /// <remarks>
        /// This method was intentionally not overloaded from AddDictionaryValues due to an ambiguous 
        /// signature when the caller passes in a Dictionary&lt;string, object&gt; as the valueDictionary.
        /// This is because the Dictionary&lt;TK, TV&gt;() class implements both IDictionary and IDictionary&lt;TK, TV&gt;.
        /// A Dictionary&lt;string, ???&gt; (any other type than object) will resolve to AddDictionaryValues.
        /// This is specifically for a generic List&lt;string, object&gt;, which does not resolve to
        /// an IDictionary interface.
        /// </remarks>
        public void AddDictionaryValuesEx(IDictionary<string, object> valueDictionary)
        {
            foreach (string strKey in valueDictionary.Keys)
            {
                if (_dict.ContainsKey(strKey) == true)
                    throw new ArgumentException("Attempt to add duplicate value for the '" + strKey + "' property.");

                object value = valueDictionary[strKey];

                _dict.Add(strKey, value);
            }
        }

        #endregion
    }
}
