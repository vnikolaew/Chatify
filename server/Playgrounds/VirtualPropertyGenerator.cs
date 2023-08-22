using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Playgrounds;

[Generator]
public class VirtualPropertyGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // Get the syntax receiver
        if ( context.SyntaxReceiver is not SyntaxReceiver receiver )
            return;

        // Load the template class
        var templateClass = context.Compilation.GetTypeByMetadataName(
            "Playgrounds.Person");
        if ( templateClass == null )
            return;

        // Generate the virtual class for each candidate
        foreach ( var candidate in receiver.Candidates )
        {
            var model = context.Compilation.GetSemanticModel(candidate.SyntaxTree);
            var symbol = ModelExtensions.GetDeclaredSymbol(model, candidate);

            // Create the new class with virtual properties
            var newClass = GenerateVirtualClass(symbol);

            // Add the generated class to the compilation
            var newClassSyntax = newClass.NormalizeWhitespace();
            context.AddSource($"{symbol.Name}_Virtual.cs",
                SourceText.From(newClassSyntax.ToFullString(), Encoding.UTF8));
        }
    }

    private ClassDeclarationSyntax GenerateVirtualClass(ISymbol symbol)
    {
        var originalClass = symbol as INamedTypeSymbol;
        var newClassName = originalClass.Name + "_Virtual";

        // Create the new class declaration
        var newClass = SyntaxFactory.ClassDeclaration(newClassName)
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
            .AddMembers(GenerateVirtualProperties(originalClass));

        return newClass;
    }

    private MemberDeclarationSyntax[] GenerateVirtualProperties(INamedTypeSymbol symbol)
    {
        var properties = symbol.GetMembers().OfType<IPropertySymbol>()
            .Where(prop => prop.DeclaredAccessibility == Accessibility.Public && !prop.IsStatic)
            .Select(prop =>
            {
                var propertyName = prop.Name + "_Virtual";
                var propertyType = prop.Type;
                var virtualProperty = SyntaxFactory
                    .PropertyDeclaration(SyntaxFactory.ParseTypeName(propertyType.ToString()), propertyName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                    .WithAccessorList(
                        SyntaxFactory.AccessorList(
                            SyntaxFactory.List(new[]
                            {
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                                SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                            })
                        )
                    );

                return virtualProperty;
            });

        return properties.ToArray();
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> Candidates { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if ( syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                 classDeclarationSyntax.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)) )
            {
                Candidates.Add(classDeclarationSyntax);
            }
        }
    }
}