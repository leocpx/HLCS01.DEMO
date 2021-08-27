using System;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace SDK
{
    public class Compiler
    {
        public static Assembly Compile(string code, MetadataReference[] references)
        {
            var compilation = GenerateCode(code,references);
            var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if(result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var asm = Assembly.Load(ms.ToArray());
                return asm;
            }
            return null;
        }
        public static Assembly Compile(string code)
        {
            var compilation = GenerateCode(code);
            var ms = new MemoryStream();
            var result = compilation.Emit(ms);
            if(result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var asm = Assembly.Load(ms.ToArray());
                return asm;
            }
            return null;
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            };

            return CSharpCompilation.Create("Hello",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private static CSharpCompilation GenerateCode(string sourceCode, MetadataReference[] references)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            //var references = new MetadataReference[]
            //{
            //    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
            //    MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
            //};

            return CSharpCompilation.Create("Hello",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }
    }

    public static class Extensions
    {
        public static IUserCode GetModule(this IUserCode[] modules,string moduleName)
        {
            for (int i = 0; i < modules.Length; i++)
            {
                if (modules[i].ControllerModuleName == moduleName)
                    return modules[i];
            }
            return null;
        }
    }
}
