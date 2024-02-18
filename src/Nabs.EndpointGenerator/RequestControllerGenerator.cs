using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nabs.EndpointGenerator.Abstractions;
using System.Text;
using System.Text.Json;

namespace Nabs.EndpointGenerator;

[Generator]
public class RequestControllerGenerator : IIncrementalGenerator
{
	public void Initialize(
		IncrementalGeneratorInitializationContext context)
	{
		var compilationProvider = context.CompilationProvider;
		var classes = context.SyntaxProvider
			.CreateSyntaxProvider(Predicate, Transform);

		var compilationAndClasses = classes.Combine(compilationProvider);

		context.RegisterSourceOutput(compilationAndClasses, SourceOutputAction);
	}

	private static bool Predicate(
		SyntaxNode node,
		CancellationToken cancellationToken)
	{
		if (node is ClassDeclarationSyntax classDeclaration)
		{
			var hasAttribute = classDeclaration.AttributeLists
				.SelectMany(attrList => attrList.Attributes)
				.Any(attr => attr.Name.ToString().EndsWith("RequestEndpointController"));

			return hasAttribute;
		}
		return false;
	}

	private static ClassDeclarationSyntax Transform(
		GeneratorSyntaxContext ctx,
		CancellationToken cancellationToken)
	{
		return (ClassDeclarationSyntax)ctx.Node;
	}

	private static void SourceOutputAction(
		SourceProductionContext context,
		(ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation) data)
	{
		var requestEndpointAttribute = data.classDeclarationSyntax.AttributeLists
			.SelectMany(attrList => attrList.Attributes)
			.FirstOrDefault(attr => attr.Name.ToString().EndsWith("RequestEndpointController"));


		var referenceTypeArgument = requestEndpointAttribute.ArgumentList!
			.Arguments.FirstOrDefault()!
			.Expression.ToString()
			.Trim('"');

		var referencedAssemblyIdentity = data.compilation.ReferencedAssemblyNames
			.FirstOrDefault(a => a.Name == referenceTypeArgument);

		if (referencedAssemblyIdentity is null)
		{
			return;
		}

		var assemblySymbol = data.compilation.References
				.Select(data.compilation.GetAssemblyOrModuleSymbol)
				.OfType<IAssemblySymbol>()
				.FirstOrDefault(a => a.Identity.Equals(referencedAssemblyIdentity));

		var actionMethods = new StringBuilder();

		if (assemblySymbol != null)
		{
			var namespaces = GetAllNamespaces(assemblySymbol.GlobalNamespace);
			foreach (var namespaceSymbol in namespaces)
			{
				// Now you have the namespaces and can work with types within them
				var namedTypeSymbols = GetPublicClassesImplementingIRequest(namespaceSymbol);
				foreach (var namedTypeSymbol in namedTypeSymbols)
				{
					var actionMethod = CreateActionMethod(context, namedTypeSymbol);
					actionMethods.AppendLine(actionMethod);
				}
			}
		}

			

		var now = DateTime.Now;
		var className = data.classDeclarationSyntax.Identifier.Text;
		var namespaceName = GetNamespace(data.classDeclarationSyntax);

		var sourceText = $$"""
			// Generated @ {{now}}
			using System;
			using Microsoft.AspNetCore.Mvc;
			
			namespace {{namespaceName}}
			{
				public partial class {{className}}
				{
					public {{className}}(MediatR.IMediator mediator)
					{
						Mediator = mediator;
					}

					public MediatR.IMediator Mediator { get; }
					
			{{actionMethods}}
				}
			}
			""";

		context.AddSource($"{className}.g.cs", sourceText);
	}

	private static string CreateActionMethod(
		SourceProductionContext ctx,
		INamedTypeSymbol namedTypeSymbol)
	{
		_ = ctx;

		var className = namedTypeSymbol.Name;
		var fullyQualifiedName = GetFullyQualifiedName(namedTypeSymbol);
		var variableName = JsonNamingPolicy.CamelCase.ConvertName(className);
		var responseType = GetResponseType(namedTypeSymbol);
		var (httpVerb, routeTemplate) = ExtractRequestEndpointAttributeArguments(namedTypeSymbol);

		;

		var httpMethodAttribute = httpVerb switch
		{
			HttpVerb.Get => "HttpGet",
			HttpVerb.Post => "HttpPost",
			HttpVerb.Put => "HttpPut",
			HttpVerb.Delete => "HttpDelete",
			_ => "HttpGet"
		};

		var sourceText = $$"""
					[{{httpMethodAttribute}}(HttpVerb.{{httpVerb}}, "{{routeTemplate}}")]
					public async Task<{{responseType}}> {{className}}Action({{fullyQualifiedName}} {{variableName}})
					{
						var response = await Mediator.Send({{variableName}});
						return response;
					}
			""";

		return sourceText;
	}

	private static string GetResponseType(INamedTypeSymbol namedTypeSymbol)
	{
		foreach (var iRequestInterfaceType in namedTypeSymbol.AllInterfaces)
		{
			if (iRequestInterfaceType.IsGenericType && iRequestInterfaceType.ConstructedFrom.Name == "IRequest")
			{
				var typeArgument = iRequestInterfaceType.TypeArguments[0];
				return GetFullyQualifiedName(typeArgument);
			}
		}

		return "object";
	}

	private static IEnumerable<INamespaceSymbol> GetAllNamespaces(INamespaceSymbol root)
	{
		var stack = new Stack<INamespaceSymbol>();
		stack.Push(root);

		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;

			foreach (var ns in current.GetNamespaceMembers())
			{
				stack.Push(ns);
			}
		}
	}

	private static IEnumerable<INamedTypeSymbol> GetPublicClassesImplementingIRequest(INamespaceSymbol namespaceSymbol)
	{
		var allTypes = namespaceSymbol.GetTypeMembers();
		var publicClasses = allTypes
			.Where(t => t.DeclaredAccessibility == Accessibility.Public && t.TypeKind == TypeKind.Class);

		var classesImplementingIRequest = publicClasses.Where(c => ImplementsIRequestInterface(c));

		return classesImplementingIRequest;
	}

	private static bool ImplementsIRequestInterface(INamedTypeSymbol classSymbol)
	{
		foreach (var iRequestInterfaceTypes in classSymbol.AllInterfaces)
		{
			if (iRequestInterfaceTypes.IsGenericType && iRequestInterfaceTypes.ConstructedFrom.Name == "IRequest")
			{
				return true;
			}
		}

		return false;
	}

	private static string GetFullyQualifiedName(ITypeSymbol typeSymbol)
	{
		if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
		{
			// For nested types, recursively get the containing type's full name
			var containingType = namedTypeSymbol.ContainingType;
			if (containingType != null)
			{
				return GetFullyQualifiedName(containingType) + "+" + namedTypeSymbol.MetadataName;
			}

			// Combine the namespace and type name
			var namespaceName = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
				? string.Empty
				: namedTypeSymbol.ContainingNamespace.ToDisplayString() + ".";

			return namespaceName + namedTypeSymbol.MetadataName;
		}

		// Fallback for non-named types (e.g., array types, pointer types, etc.)
		return typeSymbol.ToDisplayString();
	}

	private static string GetNamespace(ClassDeclarationSyntax classDeclaration)
	{
		// Check for a regular NamespaceDeclarationSyntax ancestor
		var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
		if (namespaceDeclaration != null)
		{
			return namespaceDeclaration.Name.ToString();
		}

		// If not found, check for a FileScopedNamespaceDeclarationSyntax ancestor
		var fileScopedNamespace = classDeclaration.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>();
		if (fileScopedNamespace != null)
		{
			return fileScopedNamespace.Name.ToString();
		}

		// Handle cases where the class might not be in a namespace
		// This could be a top-level class, a class within a script, etc.
		return string.Empty; // Or any default namespace indication as per your use case
	}

	public static (HttpVerb HttpVerb, string RouteTemplate) ExtractRequestEndpointAttributeArguments(INamedTypeSymbol namedTypeSymbol)
	{
		var a = namedTypeSymbol.DeclaringSyntaxReferences;


		// Iterate through the attributes of the class
		foreach (var attributeData in namedTypeSymbol.GetAttributes())
		{
			// Check if the attribute is the one you're interested in (RequestEndpoint in this case)
			if (attributeData.AttributeClass!.ToDisplayString().EndsWith(".RequestEndpointAttribute"))
			{
				// Extract the constructor arguments
				// Assuming the first argument is the HttpVerb and the second is the endpoint template
				if (attributeData.ConstructorArguments.Length >= 2)
				{
					var httpVerb = Enum.IsDefined(typeof(HttpVerb), attributeData.ConstructorArguments[0].Value);
					var endpointTemplate = attributeData.ConstructorArguments[1].Value; // "person/{PersonId}" in your case

					// Use the extracted information as needed
					// For example, log the information, store it, or use it to generate code
					//return (httpVerb, endpointTemplate.ToString());
				}
			}
		}

		return (HttpVerb.Get, "");
	}
}
