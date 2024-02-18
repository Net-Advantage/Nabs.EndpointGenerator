# Developing Source Generators in .NET

These instructions will guide you through the process of creating a source generator in .NET.
The source generator we will create in this example will generate endpoints for each mediator request in an API project.
I hope you find the source generator useful for your projects and also learn something new about source generators.

## Prerequisites
You will need to have the `.NET Compiler Platform SDK` installed to run the source generators. 
Check in the `Visual Studio Installer` under the `Individual Components` tab.

## Add a Source Generator Project
1. Create a new project in Visual Studio.
1. Select `Class Library` and click `Next`.
1. Name the project. For example, `Nabs.EndpointGenerator`.
1. Select `.NET Standard 2.0` framework.
1. Click `Create`.

Add the following NuGet packages to the project:
- `Microsoft.CodeAnalysis.CSharp`
- `Microsoft.CodeAnalysis.Analyzers`

Add the `.cs` file for the source generator. For example, `RequestEndpointGenerator.cs`.

Update the `.csproj` file to include the following properties:

```xml
<PropertyGroup>
	<TargetFramework>netstandard2.0</TargetFramework>
	<Nullable>enable</Nullable>
	<ImplicitUsings>enable</ImplicitUsings>
	<langversion>latest</langversion>
	<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
</PropertyGroup>
```

The `langversion` property is set to `latest` to enable the latest C# features.
The `EnforceExtendedAnalyzerRules` is set to `true` to ensure warnings are generator for api we should not use in source generators.

## Implement the Source Generator

The source generator is a class that implements the `IIncrementGenerator` interface.
It also needs to be decorated with the `Generator` attribute.

```csharp
using Microsoft.CodeAnalysis;

namespace Nabs.EndpointGenerator;

[Generator]
public class RequestEndpointGenerator : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		
	}
}
```

This is the basic structure of an incremental source generator. 
The `Initialize` method is called when the generator is initialized. This is there you setup the behaviour of the generator.

# Reference the Source Generator

Create a new Api project to reference the source generator.

Add this reference to your Api project in the `.csproj` file:

```xml
<ItemGroup>
	<ProjectReference Include="..\..\Nabs.EndpointGenerator\Nabs.EndpointGenerator.csproj" 
						OutputItemType="Analyzer" 
						ReferenceOutputAssembly="false"/>
</ItemGroup>
```

This will reference the source generator in the project.
The OutputItemType is set to `Analyzer` to ensure the source generator is run during the build process.
The ReferenceOutputAssembly is set to `false` to ensure the source generator is not included in the build output.

Your should now be able to see the source generator under the `Analyzers` tab of your Api project dependencies in the `Solution Explorer`.

## Setup the Source Generator Pipeline

The source generator pipeline is setup in the `Initialize` method of the source generator.

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
	context.RegisterSourceOutput(context.Compilation.GetEntryPoint(context.CancellationToken), (context, cancellationToken) =>
	{
		var sourceBuilder = new StringBuilder();
		sourceBuilder.AppendLine("using System;");
		sourceBuilder.AppendLine("namespace Nabs.EndpointGenerator");
		sourceBuilder.AppendLine("{");
		sourceBuilder.AppendLine("	public class RequestEndpoint");
		sourceBuilder.AppendLine("	{");
		sourceBuilder.AppendLine("	}");
		sourceBuilder.AppendLine("}");

		var sourceText = SourceText.From(sourceBuilder.ToString(), Encoding.UTF8);
		context.AddSource("RequestEndpoint.cs", sourceText);
	});
}
```
