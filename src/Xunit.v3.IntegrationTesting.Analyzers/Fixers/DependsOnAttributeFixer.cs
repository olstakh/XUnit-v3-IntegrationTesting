using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;

namespace Xunit.v3.IntegrationTesting.Analyzers.Fixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DependsOnAttributeFixer)), Shared]
public class DependsOnAttributeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(AttributeUsageDescriptors.UseFactDependsOnAttribute.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
            return;

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken);
        if (semanticModel == null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id != AttributeUsageDescriptors.UseFactDependsOnAttribute.Id)
            {
                continue;
            }
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            if (node is not AttributeSyntax attribute)
                return;

            var attrType = semanticModel.GetTypeInfo(attribute).Type;
            var theorySymbol = semanticModel.Compilation.GetTypeByMetadataName("Xunit.TheoryAttribute");
            var isTheory = theorySymbol != null && SymbolEqualityComparer.Default.Equals(attrType, theorySymbol);

            var replacementName = isTheory ? "TheoryDependsOn" : "FactDependsOn";
            var title = isTheory ? "Replace [Theory] with [TheoryDependsOn]" : "Replace [Fact] with [FactDependsOn]";

            context.RegisterCodeFix(
                CodeAction.Create(
                    title,
                    ct => ReplaceAttributeAsync(context.Document, attribute, replacementName, ct),
                    nameof(DependsOnAttributeFixer)),
                diagnostic);
        }
    }

    private async Task<Document> ReplaceAttributeAsync(Document document, AttributeSyntax attribute, string replacementName, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null)
            return document;

        var replacement = attribute.WithName(SyntaxFactory.IdentifierName(replacementName));

        SyntaxNode newRoot = root.ReplaceNode(attribute, replacement);

        return document.WithSyntaxRoot(newRoot);
    }
}
