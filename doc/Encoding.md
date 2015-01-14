# Encoding Values

## Encoding:
By default RazorEngine is configured to encode as Html. This sometimes this presents problems were certain characters are encoded as Html but what you want is to output them as-is. To output something in raw format use the `@Raw()` built-in method as shown in the following example:

```csharp
string template = "@Raw(Model.Data)";
var model = new { Data = "My raw double quotes appears here \"hello!\"" };

string result = Engine.Razor.RunCompile(template, "templateKey", null, model);
```

Which should result in:

> `My raw double quotes appears here "hello!"`