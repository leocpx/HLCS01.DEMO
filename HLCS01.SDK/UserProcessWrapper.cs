using HLCS01.SDK;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using Microsoft.CSharp.RuntimeBinder;
using SDK;
using System.Windows;

namespace HLCS01.SDK
{

    [MessagePack.MessagePackObject]
    public class UserProcessWrapper
    {
        #region -- PROPERTIES --

        #region -- PUBLIC --

        public static string UserProcessModulePath = Directory.GetCurrentDirectory() + @"\UserProcessModules";

        [MessagePack.Key(0)]
        public string UserProcessName { get; set; }

        [MessagePack.Key(1)]
        public List<string> ImportedModuleNames { get; set; } = new List<string>();

        [MessagePack.IgnoreMember]
        public Func<IUserCode[], bool> ExecuteCode;

        [MessagePack.Key(2)]
        public string ExecuteSourceCode { get; set; } =
            "using System.Collections.Generic;\n" +
            "using System.IO;\n" +
            "using System.Reflection;\n" +
            "using System;\n" +
            "using Prism.Events;\n"+
            "using Microsoft.CSharp;\n" +
            "using SDK;\n"+
            //"using Microsoft.CodeAnalysis;\n" +
            //"using Microsoft.CodeAnalysis.CSharp;\n" +
            "using System.Runtime.CompilerServices;\n" +
            "using System.Linq.Expressions;\n" +
            "using Microsoft.CSharp.RuntimeBinder;\n"+
            "using System.Linq;\n\n" +
            "public class ExecutionClass\n{" +
            "\n" +
                "    public bool ExecuteUserCode(IUserCode[] _modules, IEventAggregator _eventAggregator)\n" +
                "    {\n" +
                "        // return true to finish module execution\n"+
                "        return true;\n" +
                "    }\n" +
            "}\n";
        #endregion

        #region -- PRIVATE --
        private List<IUserCode> ImportedModules = new List<IUserCode>();
        private IUserCode[] _importedModules => ImportedModules.ToArray();
        private IEventAggregator _eventAggregator { get; set; }
        private object _compiledObject { get; set; }
        #endregion

        #endregion

        #region -- CONSTRUCTOR --
        public UserProcessWrapper()
        {
        }

        public bool ExecuteUserCode(object[] _modules)
        {
            return true;
        }

        public void SetEventAggregator(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OnProvideModuleInstance>().Subscribe(
                m =>
                {
                    ImportedModules.Add(m);
                });
            LoadImportedModules();
            ExecuteCode = GetCompiledExecutionCode();
        }

        public UserProcessWrapper(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OnProvideModuleInstance>().Subscribe(
                m =>
                {
                    ImportedModules.Add(m);
                });
            LoadImportedModules();
            ExecuteCode = GetCompiledExecutionCode();
        }

        #endregion

        #region -- METHODS --

        #region -- PUBLIC --

        public void CompileExecutionCode()
        {
            ExecuteCode = GetCompiledExecutionCode();

        }
        public bool ExecuteUserCode()
        {
            return ExecuteCode != null ? ExecuteCode(_importedModules) : true;
        }
        public void LoadImportedModules()
        {
            _eventAggregator.GetEvent<OnRequestModuleInstance>().Publish();
        }
        #endregion

        #region -- PRIVATE --
        private Func<IUserCode[], bool> GetCompiledExecutionCode()
        {
            var asm = Compiler.Compile(ExecuteSourceCode);
            var exportedType = asm.GetExportedTypes().FirstOrDefault();
            if (exportedType == null) return null;

            _compiledObject=Activator.CreateInstance(exportedType);

            var _compiledMethod = _compiledObject.GetType().GetMethod("ExecuteUserCode");

            return (o) => (bool)_compiledMethod.Invoke(_compiledObject, new object[] { o, _eventAggregator });
        }
        #endregion

        #endregion
    }

    public class Compiler
    {
        public static Assembly Compile(string code, MetadataReference[] references)
        {
            var compilation = GenerateCode(code, references);
            var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (result.Success)
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
            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var asm = Assembly.Load(ms.ToArray());
                return asm;
            }
            else
            {
                Console.WriteLine(result.Diagnostics[0].Descriptor);
                string error = "";

                result.Diagnostics.ToList().ForEach( e => error += e + "\n");
                throw new Exception($"{error}");
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
                //MetadataReference.CreateFromFile("Microsoft.CodeAnalysis.CSharp.dll"),
                //MetadataReference.CreateFromFile("Microsoft.CSharp.dll"),
                MetadataReference.CreateFromFile("System.Collections.Immutable.dll"),
                MetadataReference.CreateFromFile("System.Linq.Expressions.dll"),
                MetadataReference.CreateFromFile("Prism.dll"),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.CSharpCodeProvider).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(UserProcessWrapper).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IUserCode).Assembly.Location),
            };

            return CSharpCompilation.Create("Hello",
                new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Debug,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private static CSharpCompilation GenerateCode(string sourceCode, MetadataReference[] references)
        {
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);

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
}
