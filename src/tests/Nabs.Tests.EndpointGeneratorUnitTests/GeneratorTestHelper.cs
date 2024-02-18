using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Nabs.Tests.EndpointGeneratorUnitTests;

public static class GeneratorTestHelper
{
	public static Compilation RunGenerator(string sourceCode, IIncrementalGenerator generator)
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
		var references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
		var compilation = CSharpCompilation.Create("TestCompilation", new[] { syntaxTree }, references);

		// Create an instance of the IncrementalGeneratorDriver
		var driver = CSharpGeneratorDriver.Create(generator);
		driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
			compilation, 
			out var outputCompilation, 
			out var diagnostics);

		Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)); // Ensure no errors

		return outputCompilation;
	}
}
