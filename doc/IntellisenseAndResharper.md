# Intellisence and ReSharper

It is often convenient to have your razor templates as file resources in your project and edit them like you would a normal ASP.NET MVC view as *.cshtml files and provide them to RazorEngine as a string.  It is also nice when IntelliSense will give you a helping hand, but ReSharper won't help you unless it can understand that the `Model` property exists on the `TemplateBase<T>` and what type the `Model` property is.

Here is an example template file where `MyCustomizedTemplate<T>` derives from RazorEngine's `TemplateBase<T>`:

    @using MyProject.Templates
    @using MyProject.Templates.Models
    @inherits MyCustomizedTemplate<InvoiceModel>
    <h1>Your Invoice @Model.InvoiceNumber</h1>
    <p>The great stuff you bought was:</p>
    <ul>
        @foreach(var line in Model.InvoiceLines)
        {
            <li>@line.Code - @line.Description for @line.Price</li>
        }
    </ul>
    <h3>Thanks for shopping at BuyMore</h3>


The custom class would look something like this:

	public class MyCustomizedTemplate<T> : TemplateBase<T>
	{
		public T Model { get; set; }

		public MyCustomizedTemplate()
		{
		}
	}
