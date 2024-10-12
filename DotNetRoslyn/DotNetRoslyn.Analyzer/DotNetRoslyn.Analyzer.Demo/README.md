| 项目名                               | 描述                                                         |
| ------------------------------------ | ------------------------------------------------------------ |
| DotNetRoslyn.Analyzer.Demo           | 分析器（Analyzer），负责定义代码分析器                       |
| DotNetRoslyn.Analyzer.Demo.CodeFixes | 代码修补程序，用于修复由分析器检测到的问题                   |
| DotNetRoslyn.Analyzer.Demo.Package   | 用于生成分析器和代码修补程序的Nuget包                        |
| DotNetRoslyn.Analyzer.Demo.Test      | 分析器和代码修补程序的单元测试                               |
| DotNetRoslyn.Analyzer.Demo.Vsix      | 默认的启动程序，将分析器和代码修补程序集成到Visual Studio中，使其载IDE中运行和生效 |

分析器编写可根据对实际代码进行分析得出，Visual Studio->视图->其他窗口->Syntax Visualizer，定位到行或高亮选择需要分析的代码。实际是对一颗语法树进行分析，可以结合当前例子理解。把DotNetRoslyn.Analyzer.Demo.Vsix设为启动项目，然后在启动后的vs中打开需要分析的项目。

如何安装：

DotNetRoslyn.Analyzer.Demo.Package中生成后会发布为nuget包，在需要分析的项目中安装后即可