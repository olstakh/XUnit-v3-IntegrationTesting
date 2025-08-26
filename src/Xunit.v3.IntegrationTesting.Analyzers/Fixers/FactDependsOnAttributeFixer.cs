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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FactDependsOnAttributeFixer)), Shared]
public class FactDependsOnAttributeFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(AttributeUsageDescriptors.UseFactDependsOnAttribute.Id);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id != AttributeUsageDescriptors.UseFactDependsOnAttribute.Id)
            {
                continue;
            }
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var node = root.FindNode(diagnosticSpan);
            if (node is not AttributeSyntax factAttribute)
                return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    "Replace [Fact] with [FactDependsOn]",
                    ct => ReplaceFactWithFactDependsOnAsync(context.Document, factAttribute, ct),
                    nameof(FactDependsOnAttributeFixer)),
                diagnostic);
        }
    }

    private async Task<Document> ReplaceFactWithFactDependsOnAsync(Document document, AttributeSyntax factAttribute, CancellationToken cancellationToken)
    {
        var factDependsOn = factAttribute.WithName(SyntaxFactory.IdentifierName("FactDependsOn"));
        var root = await document.GetSyntaxRootAsync(cancellationToken);
        if (root == null)
            return document;
        var newRoot = root.ReplaceNode(factAttribute, factDependsOn);
        return document.WithSyntaxRoot(newRoot);
    }
}
