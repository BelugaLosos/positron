using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators;
using PositronCodeGen.Processors;
using PositronCodeGen.Processors.Interface;
using System.Text;
using System.IO;

namespace PositronCodeGen
{
    [Generator]
    public class PositronCodeGen : ISourceGenerator
    {
        private const bool DO_DBG_LISTING = false;

        public void Execute(GeneratorExecutionContext context)
        {
            Compilation compiler = context.Compilation;
            StringBuilder sourceBuilder = new StringBuilder();

            TypesExtractor typesExtractor = new TypesExtractor();
            UsagesGenerator usagesGenerator = new UsagesGenerator();

            ITypeProcessor[] typeProcessors = new ITypeProcessor[] 
            {
                new RpcIntegrationProcessor(),
                new NetValueIntegrationProcessor()
            };

            usagesGenerator.GenerateUsages(sourceBuilder);

            foreach (ParsedTypeData type in typesExtractor.ExtractAllTypesFromAssembly(compiler, TypeKind.Class))
            {
                foreach (ITypeProcessor processor in typeProcessors)
                {
                    processor.Process(type, context, sourceBuilder);
                }
            }

            if(DO_DBG_LISTING) File.WriteAllText(@"C:/GenDBG/dbg.txt", sourceBuilder.ToString());
            context.AddSource("RpcLowLevelInteractors.gen.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
