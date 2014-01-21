namespace RazorEngine.Compilation
{
    using System;
    using System.Diagnostics;
    using System.Dynamic;

    /// <summary>
    /// Defines a dynamic object.
    /// </summary>
    internal class RazorDynamicObject : DynamicObject
    {
        #region Properties
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        public object Model { get; set; }

        /// <summary>
        /// Gets or sets whether to allow missing properties on dynamic members.
        /// </summary>
        public bool AllowMissingPropertiesOnDynamic { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the value of the specified member.
        /// </summary>
        /// <param name="binder">The current binder.</param>
        /// <param name="result">The member result.</param>
        /// <returns>True.</returns>
        [DebuggerStepThrough]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
                throw new ArgumentNullException("binder");

            var dynamicObject = Model as RazorDynamicObject;
            if (dynamicObject != null)
                return dynamicObject.TryGetMember(binder, out result);

            Type modelType = Model.GetType();
            var prop = modelType.GetProperty(binder.Name);
            if (prop == null)
            {
                if (!AllowMissingPropertiesOnDynamic)
                {
                    result = null;
                    return false;
                }

                result = new RazorDynamicObject() { AllowMissingPropertiesOnDynamic = AllowMissingPropertiesOnDynamic, Model = new object() };
                return true;
            }

            object value = prop.GetValue(Model, null);
            if (value == null)
            {
                result = value;
                return true;
            }

            Type valueType = value.GetType();

            result = (CompilerServicesUtility.IsAnonymousType(valueType))
                         ? new RazorDynamicObject { Model = value }
                         : value;
            return true;
        }

        /// <summary>
        /// Returns the string representation of the current instance.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return "";
        }
        #endregion
    }
}