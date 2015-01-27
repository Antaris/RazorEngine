using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorEngine.Compilation.ImpromptuInterface.Dynamic
{
    class ImpromptuForwarderAddRemove
    {
        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static ImpromptuForwarderAddRemove operator +(ImpromptuForwarderAddRemove left, object right)
        {
            left.Delegate = right;
            left.IsAdding = true;

            return left;
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static ImpromptuForwarderAddRemove operator -(ImpromptuForwarderAddRemove left, object right)
        {
            left.Delegate = right;
            left.IsAdding = false;

            return left;
        }

        /// <summary>
        /// Gets or sets the delegate.
        /// </summary>
        /// <value>The delegate.</value>
        public object Delegate { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is adding.
        /// </summary>
        /// <value><c>true</c> if this instance is adding; otherwise, <c>false</c>.</value>
        public bool IsAdding { get; protected set; }

    }
}
