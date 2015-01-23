# Intellisense and ReSharper

It is often convenient to have your razor templates as file resources in your project and edit them 
like you would a normal ASP.NET MVC view as `*.cshtml` files and provide them to RazorEngine as a string. 
It is also nice when IntelliSense will give you a helping hand, 
but ReSharper/Visual Studio won't help you unless it can understand that the `Model` property exists on the `TemplateBase<T>` 
and what type the `Model` property is.

## Make `RazorEngine` known to Visual Studio

To get full intellisense you should use the `@inherits` directive like this:

```markup
@using RazorEngine
@using MyProject.Models
@inherits TemplateBase<MyModel>
<h1>Your Invoice @Model.InvoiceNumber</h1>
<p>The great stuff you bought was:</p>
<ul>
    @foreach(var line in Model.InvoiceLines)
    {
        <li>@line.Code - @line.Description for @line.Price</li>
    }
</ul>
<h3>Thanks for shopping at BuyMore</h3>
```

Please make sure the following is true:

 - The project references `RazorEngine`.

 - Your project output path is set to `bin\` instead of `bin\Debug\` and `bin\Release\`.
   
   > another possible solution is to copy `RazorEngine.dll` and `System.Web.Razor.dll` to `bin\`.


After this everything should work in the Visual Studio designer and you should have full intellisense for 
RazorEngine and your model-type.


## Custom base template class

If you cannot use the above solution you can get minimal intellisense by 
providing your own base class and using it with the `@inherits` directive.

Here is an example template file where `MyCustomizedTemplate<T>` derives from RazorEngine's `TemplateBase<T>`:

```markup
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
```

The custom class would look something like this:

```csharp
public class MyCustomizedTemplate<T> : TemplateBase<T>
{
	public new T Model 
	{
		get { return base.Model; }
		set { base.Model = value; }
	}

	public MyCustomizedTemplate()
	{
	}
}
```

The problem with this approach is that you only get intellisense for the `Model` property and not for other 
methods `TemplateBase<>` is providing for you (like `Include`).
You need to add all methods to your custom base class to get intellisense for them.

## References:

- http://stackoverflow.com/questions/4953330/razor-based-view-doesnt-see-referenced-assemblies
- https://github.com/Antaris/RazorEngine/issues/213
- http://stackoverflow.com/questions/26862336/how-to-make-intellisense-works-with-razorengine