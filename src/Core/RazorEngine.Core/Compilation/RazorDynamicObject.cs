//-----------------------------------------------------------------------------
// <copyright file="RazorDynamicObject.cs" company="RazorEngine">
//     Copyright (c) Matthew Abbott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------------
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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the value of the specified member.
        /// </summary>
        /// <param name="binder">The current binder.</param>
        /// <param name="result">The member result.</param>
        /// <returns>
        /// True or false 
        /// </returns>
        [DebuggerStepThrough]
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }

            var dynamicObject = this.Model as RazorDynamicObject;
            if (dynamicObject != null)
            {
                return dynamicObject.TryGetMember(binder, out result);
            }

            Type modelType = this.Model.GetType();
            var prop = modelType.GetProperty(binder.Name);
            if (prop == null)
            {
                result = null;
                return false;
            }

            object value = prop.GetValue(this.Model, null);
            if (value == null)
            {
                result = null;
                return true;
            }

            Type valueType = value.GetType();

            result = CompilerServicesUtility.IsAnonymousType(valueType)
                         ? new RazorDynamicObject { Model = value }
                         : value;

            return true;
        }
        #endregion
    }
}