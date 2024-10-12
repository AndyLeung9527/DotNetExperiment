using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetRoslyn.Analyzer.Demo
{
    /// <summary>
    /// C#的代码修补程序，共享
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DotNetRoslynAnalyzerDemoCodeFixProvider)), Shared]
    public class DotNetRoslynAnalyzerDemoCodeFixProvider : CodeFixProvider
    {
        // 指定代码修补程序能修复的诊断ID
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(DotNetRoslynAnalyzerDemoAnalyzer.DiagnosticId); }
        }

        // 提供"Fix All"功能，允许用户一次修复所有类似问题
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            // 批量修复多个问题
            return WellKnownFixAllProviders.BatchFixer;
        }

        // 当发现诊断问题时，注册代码修补操作
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            // 获取语法树的根节点
            var diagnostic = context.Diagnostics.First();
            // 获取当前诊断问题
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            // 在语法树中找到对应的问题类型声明
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            // 注册一个修补操作，当用户点击修补时执行MakeUppercaseAsync方法
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: CodeFixResources.CodeFixTitle,
                    createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
                    equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
                diagnostic);
        }

        // 实际执行的修补逻辑，将类名转换为大写
        private async Task<Solution> MakeUppercaseAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.
            // 获取类的标识符，即类名
            var identifierToken = typeDecl.Identifier;
            // 将类名转为大写
            var newName = identifierToken.Text.ToUpperInvariant();

            // Get the symbol representing the type to be renamed.
            // 获取语义模型，用来理解代码中的符号和上下文
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            // 获取类的符号信息
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDecl, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            // 获取原始的解决方案，包含项目的所有代码和引用
            var originalSolution = document.Project.Solution;
            // 获取重命名操作的设置
            var optionSet = originalSolution.Workspace.Options;
            //调用Renamer.RenameSymbolAsync将类名以及所有引用的地方改为大写
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            // 返回包含更新后类名的解决方案
            return newSolution;
        }
    }
}
