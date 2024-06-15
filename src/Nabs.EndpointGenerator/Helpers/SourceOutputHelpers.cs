namespace Nabs.EndpointGenerator.Helpers;

internal static class SourceOutputHelpers
{
    internal static void SourceOutputAction(
        SourceProductionContext context,
        (ClassDeclarationSyntax classDeclarationSyntax, Compilation compilation) data)
    {
        var semanticModel = data.compilation.GetSemanticModel(data.classDeclarationSyntax.SyntaxTree);
        var assemblySymbol = GetAssembly(data.classDeclarationSyntax, semanticModel);

        if (assemblySymbol is null)
        {
            return;
        }

        var actionMethods = assemblySymbol.BuildActionMethods(data.classDeclarationSyntax);

        var now = DateTime.Now;
        var className = data.classDeclarationSyntax.Identifier.Text;
        var namespaceName = data.classDeclarationSyntax.GetNamespace();

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

    private static IAssemblySymbol? GetAssembly(
        ClassDeclarationSyntax classDeclarationSyntax, 
        SemanticModel semanticModel)
    {
        var referencesAssemblyNames = semanticModel.Compilation.ReferencedAssemblyNames;

        var attribute = classDeclarationSyntax.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .First(AttributeHelpers.IsGenerateEndpointsAttribute);

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
