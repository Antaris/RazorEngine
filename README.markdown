RazorEngine
===========

A templating engine built on Microsoft's Razor parsing engine, RazorEngine allows you to use Razor syntax to build dynamic templates:

    string template = "Hello @Model.Name, welcome to RazorEngine!";
    string result = Razor.Parse(template, new { Name = "World" });