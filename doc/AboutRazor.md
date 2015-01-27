# About Razor and its syntax

## About Razor
The Razor parser was introduced as part of the [ASP.NET](http://www.asp.net) MVC and WebPages release by Microsoft. The Razor parser itself is designed to process a stream of characters to generate a C# or VB class which can be compiled.

For an overview of how the Razor parser works under the hood, please visit [Andrew Nurse's blog](http://vibrantcode.com) for some in-depth articles.

## Razor Syntax
The Razor syntax is designed to be a clean but robust syntax for merging both code and markup into a single language. Primarily Razor was designed for Html-like languages, but future editions may take advantage of the existing `MarkupParser` abstraction to deliver alternative markup languages (possibly BBCode, Latex, Markdown, etc.). An example Razor template could look like:

```markup
<div>Hello @Model.Name, you are @Model.GetAge() years old.</div>
```

This template is transformed into the body of a method, the `Execute` method, which could look something like this:

```csharp
public void Execute()
{
    WriteLiteral("<div>Hello ");
    Write(Model.Name);
    WriteLiteral(", you are ");
    Write(Model.GetAge());
    WriteLiteral(" years old.</div>");
}
```

This mixture of code and markup allows for quite a declarative syntax where markup becomes a first-class feature alongside the code.  Here is slightly more complex template:

```markup
<ul>
    @foreach (Person p in Model.Persons) {
        <li>@p.name</li>
    }
</ul>
```

Razor understands the code language (in this case, C#) because it operates dual parsers (a code parser and a markup parser). Much like the markup parser is geared up to understand Html, the code parser (in this example), is designed to understand C#.

For an overview of the Razor syntax, please view [ScottGu's article: Introducing “Razor” – a new view engine for ASP.NET](http://weblogs.asp.net/scottgu/archive/2010/07/02/introducing-razor.aspx)

## Razor vs. MVC vs. WebPages vs. RazorEngine
There is often a confusion about where Razor sits in this set of technologies. Essentially Razor is the parsing framework that does the work to take your text template and convert it into a compilable class. In terms of MVC and WebPages, they both utilise this parsing engine to convert text templates (view/page files) into executable classes (views/pages). Often we are asked questions such as "Where is @Html, @Url", etc. These are not features provided by Razor itself, but implementation details of the MVC and WebPages frameworks.

RazorEngine is another consumer framework of the Razor parser. We wrap up the instantiation of the Razor parser and provide a common framework for using runtime template processing.
