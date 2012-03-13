using System.Web.Razor.Parser.SyntaxTree;

namespace RazorEngine.Hosts.Console
{
    using System;
    using System.Linq;

    using Compilation;
    using Templating;

    class Program
    {
        static void Main(string[] args)
        {
            CompilerServiceBuilder.SetCompilerServiceFactory(new DefaultCompilerServiceFactory());

            using (var service = new TemplateService())
            {
                const string template = "<h1>Age: @Model.Age</h1>";
                var expected = Enumerable.Range(1, 10).Select(i => string.Format("<h1>Age: {0}</h1>", i)).ToList();
                var templates = Enumerable.Repeat(template, 10).ToList();
                var models = Enumerable.Range(1, 10).Select(i => new Person { Age = i });

                var results = service.ParseMany(templates, models, null, null, true).ToList();

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(templates[i]);
                    Console.WriteLine(expected[i]);
                    Console.WriteLine(results[i]);
                }
            }

            Console.ReadKey();
        }
    }

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
