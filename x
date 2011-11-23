[1mdiff --git a/src/Core/RazorEngine.Core/Compilation/DirectCompilerServiceBase.cs b/src/Core/RazorEngine.Core/Compilation/DirectCompilerServiceBase.cs[m
[1mindex c1ac608..829347c 100644[m
[1m--- a/src/Core/RazorEngine.Core/Compilation/DirectCompilerServiceBase.cs[m
[1m+++ b/src/Core/RazorEngine.Core/Compilation/DirectCompilerServiceBase.cs[m
[36m@@ -53,8 +53,6 @@[m
             var compileUnit = GetCodeCompileUnit(context.ClassName, context.TemplateContent, context.Namespaces,[m
                                                  context.TemplateType, context.ModelType);[m
 [m
[31m-            Inspect(compileUnit);[m
[31m-[m
             var @params = new CompilerParameters[m
             {[m
                 GenerateInMemory = true,[m
