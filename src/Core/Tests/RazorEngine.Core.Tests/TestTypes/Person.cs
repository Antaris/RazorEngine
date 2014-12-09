namespace RazorEngine.Tests.TestTypes
{
    using System;

    /// <summary>
    /// Defines a person.
    /// </summary>
    [Serializable]
    public class Person
    {
        #region Properties
        /// <summary>
        /// Gets or sets the age.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the forename.
        /// </summary>
        public string Forename { get; set; }

        /// <summary>
        /// Gets or sets the surname.
        /// </summary>
        public string Surname { get; set; }
        #endregion
    }
}