using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Nabs.EndpointGenerator.Helpers;
using System.Text;

namespace Nabs.EndpointGenerator;

[Generator]
public class RequestControllerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                InitializeHelpers.Predicate,
                InitializeHelpers.Transform);

        var compilationAndClasses = classes.Combine(compilationProvider);

        context.RegisterSourceOutput(compilationAndClasses, SourceOutputAction);

        context.RegisterPostInitializationOutput(PostInitializationHelpers.SourceOutputAction);
    }

    private void SourceOutputAction(
        SourceProductionContext context,
        (ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation) data)
    {
        var semanticModel = data.compilation.GetSemanticModel(data.classDeclarationSyntax.SyntaxTree);

        var requestEndpointControllerAttribute = data.classDeclarationSyntax.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .First(InitializeHelpers.IsGenerateEndpointsAttribute);

        var assemblySymbol = GetAssemblyToScan(
            requestEndpointControllerAttribute,
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
                    var actionMethod = CreateActionMethod(context, data.classDeclarationSyntax, namedTypeSymbol);
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
        private readonly MediatR.IMediator _mediator;

        public {className}(MediatR.IMediator mediator)
        {{
        	_mediator = mediator;
        }}
        
        {actionMethods}
    }}
}}
";

        context.AddSource($"{className}.g.cs", source);
    }

    private static string CreateActionMethod(
        SourceProductionContext ctx,
        ClassDeclarationSyntax classDeclarationSyntax,
        INamedTypeSymbol namedTypeSymbol)
    {
        _ = ctx;

        var attributeData = namedTypeSymbol.GetAttributeByName("Nabs.EndpointGenerator.Abstractions", "HttpEndpointAttribute");

        if(attributeData is null)
        {
            return string.Empty;
        }

        var sourceText = attributeData!.AttributeClass!.Name switch
        {
            "HttpGetEndpointAttribute" => namedTypeSymbol.BuildHttpGetActionMethod(classDeclarationSyntax, attributeData),
            "HttpPostEndpointAttribute" => namedTypeSymbol.BuildHttpPostActionMethod(classDeclarationSyntax, attributeData),
            "HttpPushEndpointAttribute" => namedTypeSymbol.BuildHttpPushActionMethod(classDeclarationSyntax, attributeData),
            "HttpDeleteEndpointAttribute" => namedTypeSymbol.BuildHttpDeleteActionMethod(classDeclarationSyntax, attributeData),
            _ => string.Empty
        };

        return sourceText;
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

    private static IAssemblySymbol? GetAssemblyToScan(AttributeSyntax attribute, SemanticModel semanticModel)
    {
        var referencesAssemblyNames = semanticModel.Compilation.ReferencedAssemblyNames;

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
                        assemblyIdentity = referencesAssemblyNames
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

                    assemblyIdentity = referencesAssemblyNames
                        .FirstOrDefault(a => a.Name == name);
                }
            }
        }

        if (assemblyIdentity is not null)
        {
            var assemblySymbols = semanticModel.Compilation.References
                .Select(semanticModel.Compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>();
            var result = assemblySymbols
                .FirstOrDefault(a => a.Identity.Equals(assemblyIdentity));
            return result;
        }

        return null;
    }
}
