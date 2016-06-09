using Microsoft.CodeAnalysis.Diagnostics;
using System;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NoImplicitToStringAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonStringConcatAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor diagDesc = 
            new DiagnosticDescriptor(
                    id: "MM123",
                    title: "non-string plus",
                    messageFormat: "",
                    category: "bla",
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: "non-string plus",
                    helpLinkUri: null,
                    customTags: new string[0]);

        private static DiagnosticDescriptor nonStringInInterpolation(NodeAndType n) =>
            new DiagnosticDescriptor(
                    id: "MM124",
                    title: "non-string in interpolation",
                    messageFormat: "",
                    category: "bla",
                    defaultSeverity: DiagnosticSeverity.Warning,
                    isEnabledByDefault: true,
                    description: "non-string in interpolation",
                    helpLinkUri: null,
                    customTags: new string[0]);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(diagDesc, nonStringInInterpolation(null));

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            // does not work, at least for "2+2" or "a" + "b"
            //context.RegisterSyntaxNodeAction(Process, SyntaxKind.PlusToken);

            context.RegisterSyntaxNodeAction(ProcessInterpolatedStringExpr, SyntaxKind.InterpolatedStringExpression);

            // works
            //context.RegisterSyntaxNodeAction(Process, SyntaxKind.LogicalAndExpression);

            // works
            //context.RegisterSemanticModelAction(Process);

            //context.RegisterSymbolAction(Process, SymbolKind.)

            // works at the method level, etc, not statement
            //context.RegisterCodeBlockAction(Process);

            // 'System.InvalidOperationException' with message 'Feature 'IOperation' is disabled.'.
            //context.RegisterOperationAction(Process, OperationKind.BinaryOperatorExpression);

            //context.RegisterSyntaxTreeAction(Process);
        }

        class NodeAndType
        {
            public string TypeInfo { get; }
            public SyntaxNode Node { get; }

            public NodeAndType(string TypeInfo, SyntaxNode Node)
            {
                this.TypeInfo = TypeInfo;
                this.Node = Node;
            }
        }

        static void ProcessInterpolatedStringExpr(SyntaxNodeAnalysisContext context)
        {
            Debugger.Launch();
            var offendingNodes = context.Node.ChildNodes().OfType<InterpolationSyntax>()
                .SelectMany(x =>
                {
                    var actualNode = x.ChildNodes().First();
                    var type = context.SemanticModel.GetTypeInfo(actualNode).ConvertedType.ToString();
                    if (!type.Contains("String"))
                        return new[] {
                            new NodeAndType(type, actualNode)
                        };
                    return new NodeAndType[0];
                });

            //foreach (var diag in offendingNodes)
            //{

            //    context.ReportDiagnostic(diag);
            //}
        }
    }
}
