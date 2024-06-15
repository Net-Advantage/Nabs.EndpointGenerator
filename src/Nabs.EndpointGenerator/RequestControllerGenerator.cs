using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nabs.EndpointGenerator.Helpers;
using System.Text;

namespace Nabs.EndpointGenerator;

[Generator]
public class RequestControllerGenerator : IIncrementalGenerator
{
    private readonly InitializeHelpers _initializeHelpers = new();

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                _initializeHelpers.Predicate,
                _initializeHelpers.Transform);

        var compilationAndClasses = classes.Combine(compilationProvider);

        context.RegisterSourceOutput(compilationAndClasses, SourceOutputAction);

        context.RegisterPostInitializationOutput(PostInitializationHelpers.SourceOutputAction);
    }

    private void SourceOutputAction(
        SourceProductionContext context,
        (ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation) data)
    {
        var semanticModel = data.compilation.GetSemanticModel(data.classDeclarationSyntax.SyntaxTree);

        var assemblySymbol = GetAssemblyToScan(
            _initializeHelpers.RequestEndpointControllerAttribute,
            semanticModel);

        if (assemblySymbol is null)
        {
            return;
        }

        var actionMethods = new StringBuilder();
        if (assemblySymbol != null)
        {
            var namespaces = GetAllNamespaces(assemblySymbol.GlobalNamespace);
            foreach (var namespaceSymbol in namespaces)
            {
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

        var source = $@"
using System;
using Microsoft.AspNetCore.Mvc;

namespace {namespaceName}
{{
    public partial class {className}
    {{
        public {className}(MediatR.IMediator mediator)
        {{
        	Mediator = mediator;
        }}

        public MediatR.IMediator Mediator {{ get; }}

        {actionMethods}
    }}
}}
";

        context.AddSource($"{className}.g.cs", source);
        //context.AddSource($"Test.g.cs", $@"// Generated ");
    }

    private static string CreateActionMethod(
        SourceProductionContext ctx,
        INamedTypeSymbol namedTypeSymbol)
    {
        _ = ctx;

        var className = namedTypeSymbol.Name;
        var fullyQualifiedName = GetFullyQualifiedName(namedTypeSymbol);
        var variableName = "request";
        var responseType = GetResponseType(namedTypeSymbol);
        var routeTemplate = ExtractTemplateFromHttpAttributeArguments(namedTypeSymbol);

        //TODO: get the correct http method attribute for the right verb.

        var httpMethodAttribute = "HttpGet";

        var sourceText = $@"
        [{httpMethodAttribute}(""{routeTemplate}"")]
        public async Task<{responseType}> {className}Action([FromRoute]{fullyQualifiedName} {variableName})
        {{
            var response = await Mediator.Send({variableName});
            return response;
        }}
";

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

    public static string ExtractTemplateFromHttpAttributeArguments(INamedTypeSymbol namedTypeSymbol)
    {
        // Iterate through the attributes of the class
        foreach (var attributeData in namedTypeSymbol.GetAttributes())
        {
            // Check if the attribute is the one you're interested in (RequestEndpoint in this case)
            var a = attributeData.AttributeClass!.ToDisplayString();
            if (attributeData.AttributeClass!.ToDisplayString().EndsWith(".HttpGetEndpointAttribute"))
            {
                // Extract the constructor arguments
                // Assuming the first argument is the HttpVerb and the second is the endpoint template
                if (attributeData.ConstructorArguments.Length >= 1)
                {
                    var endpointTemplate = attributeData.ConstructorArguments[0].Value!.ToString();

                    // Use the extracted information as needed
                    // For example, log the information, store it, or use it to generate code
                    return endpointTemplate;
                }
            }
        }

        return "";
    }

    private static IAssemblySymbol? GetAssemblyToScan(AttributeSyntax attribute, SemanticModel semanticModel)
    {
        string name;
        AssemblyIdentity? assemblyIdentity = null;

        if (attribute is not null && attribute.ArgumentList is not null)
        {

            var genericName = attribute.Name as GenericNameSyntax;
            if (genericName is not null)
            {
                var typeArgument = genericName.TypeArgumentList.Arguments.FirstOrDefault();
                if (typeArgument is not null)
                {
                    var typeSymbol = semanticModel.GetTypeInfo(typeArgument).Type;
                    if (typeSymbol is not null)
                    {
                        name = typeSymbol.ContainingAssembly.ToDisplayString();
                        assemblyIdentity = semanticModel.Compilation.ReferencedAssemblyNames
                            .FirstOrDefault(a => a.ToString() == name);
                    }
                }
            }
            else
            {
                if (attribute.ArgumentList.Arguments.Count > 0)
                {
                    name = attribute.ArgumentList.Arguments
                        .First()
                        .Expression.ToString()
                        .Trim('"');

                    assemblyIdentity = semanticModel.Compilation.ReferencedAssemblyNames
                        .FirstOrDefault(a => a.Name == name);
                }
            }
        }

        if (assemblyIdentity is not null)
        {
            var result = semanticModel.Compilation.References
                .Select(semanticModel.Compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .FirstOrDefault(a => a.Identity.Equals(assemblyIdentity));
            return result;
        }

        return null;
    }
}
