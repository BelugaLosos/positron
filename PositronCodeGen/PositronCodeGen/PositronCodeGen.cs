using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using PositronCodeGen.Extractors;
using PositronCodeGen.Extractors.Data;
using PositronCodeGen.Generators;
using PositronCodeGen.Processors;
using PositronCodeGen.Processors.Interface;
using System.Text;

namespace PositronCodeGen
{
    [Generator]
    public class PositronCodeGen : ISourceGenerator
    {
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

            context.AddSource("RpcLowLevelInteractors.gen.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context) { }
    }
}
