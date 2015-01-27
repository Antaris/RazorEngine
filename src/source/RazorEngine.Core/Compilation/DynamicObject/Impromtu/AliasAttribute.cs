using System;

namespace RazorEngine.Compilation.ImpromptuInterface
{
    /// <summary>
    /// Alias to swap method/property/event call name invoked on original
    /// </summary>
    [AttributeUsage(System.AttributeTargets.Method
                    | System.AttributeTargets.Property
                    | System.AttributeTargets.Event)]
    public class AliasAttribute : Attribute
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AliasAttribute(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return _name; }

        }
    }
}