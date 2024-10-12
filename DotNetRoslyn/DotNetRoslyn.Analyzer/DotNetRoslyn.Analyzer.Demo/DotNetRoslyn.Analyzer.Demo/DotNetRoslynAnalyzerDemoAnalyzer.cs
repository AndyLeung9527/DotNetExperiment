using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DotNetRoslyn.Analyzer.Demo
{
    /// <summary>
    /// C#的诊断分析器
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DotNetRoslynAnalyzerDemoAnalyzer : DiagnosticAnalyzer
    {
        // 诊断ID，用来标识分析器
        public const string DiagnosticId = "BadWayOfCreatingImmutableArray";
        // 分析器标题
        private static readonly string Title = "Bad Way Of Creating Immutable Array";
        // 分析器发现问题时显示的提示内容
        private static readonly string MessageFormat = "Bad Way Of Creating Immutable Array";
        // 对问题的详细描述
        private static readonly string Description = "Bad Way Of Creating Immutable Array";
        // 问题的分类
        private static readonly string Category = "Immutable arrays";
        // 定义诊断规则，参数分别是诊断ID、标题、提示内容、分类，严重性、默认启用、问题描述
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);
        // 分析器支持的所有规则列表
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        // 分析器的初始方法，主要是注册具体的分析动作
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        // 实际的分析逻辑
        private static void Analyze(SyntaxNodeAnalysisContext context)
        {
            // 获取当前节点
            var node = (InvocationExpressionSyntax)context.Node;
            // 根据ImmutableArray<int>.Empty.Add(1);找到这个特点
            // 小于一个参数跳过
            if (node.ArgumentList.Arguments.Count != 1) return;
            //一般从右往左去找
            // 无法将表达式转换为成员、方法、属性的去掉
            if (!(node.Expression is MemberAccessExpressionSyntax addAccess)) return;
            // 判断方法名是否Add
            if (addAccess.Name.Identifier.Text != "Add") return;
            // 获取左边一个的成员、方法、属性
            if (!(addAccess.Expression is MemberAccessExpressionSyntax emptyAccess)) return;
            // 判断是不是Empty，不是就直接返回
            if (emptyAccess.Name.Identifier.Text != "Empty") return;
            // 判断是不是GenericNameSyntax类型
            if (!(emptyAccess.Expression is GenericNameSyntax ImmutableArrayAccess)) return;
            // 判断是不是有一个泛型的类型
            if (ImmutableArrayAccess.TypeArgumentList.Arguments.Count != 1) return;
            // 判断是不是ImmutableArray
            if (ImmutableArrayAccess.Identifier.Text != "ImmutableArray") return;
            // 创建提示消息
            context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation()));
        }
    }
}
