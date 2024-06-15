using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Nabs.EndpointGenerator;
using Nabs.EndpointGenerator.Abstractions;
using System.Reflection;
using Xunit.Abstractions;

namespace Nabs.Tests.EndpointGeneratorUnitTests
{
    public class RequestSourceGeneratorUnitTest
    {
        private readonly ITestOutputHelper _output;

        public RequestSourceGeneratorUnitTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [ResourceFileData(".Templates.RequestEndpoints.cstemplate")]
        public void OutputTest(string templateSourceCode)
        {
            var personDomainAssembly = Assembly.GetAssembly(typeof(Nabs.Samples.BusinessLogic.PersonDomain.Root))!;
            var personDomainAssemblyReference = MetadataReference.CreateFromFile(personDomainAssembly.Location);

            var companyDomainAssembly = Assembly.GetAssembly(typeof(Nabs.Samples.BusinessLogic.CompanyDomain.Root))!;
            var companyDomainAssemblyReference = MetadataReference.CreateFromFile(companyDomainAssembly.Location);

            var assemblyAbstract = Assembly.GetAssembly(typeof(HttpGetEndpointAttribute))!;
            var assemblyAbstractReference = MetadataReference.CreateFromFile(assemblyAbstract.Location);

            // Parse the source code into a syntax tree
            SyntaxTree sourceCodeSyntaxTree = CSharpSyntaxTree.ParseText(templateSourceCode);

            CSharpCompilation compilation = CSharpCompilation.Create(personDomainAssembly.FullName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location), // Reference to System.Private.CoreLib
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), // Reference to System.Console, if needed
                    personDomainAssemblyReference,
                    companyDomainAssemblyReference,
                    assemblyAbstractReference)
                .AddSyntaxTrees(sourceCodeSyntaxTree);

            var generator = new RequestControllerGenerator();
            CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var diagnostics);

            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning))
            {
                foreach (var diagnostic in diagnostics)
                {
                    _output.WriteLine(diagnostic.ToString());
                }
                return;
            }

            _output.WriteLine($"SyntaxTrees: {outputCompilation.SyntaxTrees.Count()}");
            foreach (var syntaxTree in outputCompilation.SyntaxTrees)
            {
                _output.WriteLine(syntaxTree.ToString());
            }
        }

    }
}