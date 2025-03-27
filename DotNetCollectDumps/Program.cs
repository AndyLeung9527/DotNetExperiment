using System.Collections;
using System.Diagnostics;

namespace DotNetCollectDumps;

internal class Program
{
    static void Main(string[] args)
    {
        if (!"1".Equals(Environment.GetEnvironmentVariable("DOTNET_DbgEnableMiniDump")))
        {
            SetEnvironmentVariables();

            // 设置完环境变量后，重启进程
            RestartApp();
        }

        foreach (DictionaryEntry environmentVariable in Environment.GetEnvironmentVariables())
        {
            if (environmentVariable.Key.ToString()?.StartsWith("DOTNET_") == false) continue;
            Console.WriteLine($"{environmentVariable.Key}={environmentVariable.Value}");
        }

        throw new Exception("Crash");
    }

    static void SetEnvironmentVariables()
    {
        var guid = Guid.NewGuid().ToString("N");

        // 启用核心转储生成
        Environment.SetEnvironmentVariable("DOTNET_DbgEnableMiniDump", "1", EnvironmentVariableTarget.Process);

        // 设置核心转储类型
        // 1： Mini，小型转储，其中包含模块列表、线程列表、异常信息和所有堆栈
        // 2：Heap，大型且相对全面的转储，其中包含模块列表、线程列表、所有堆栈、异常信息、句柄信息和除映射图像以外的所有内存
        // 3：Triage，与Mini相同，但会删除个人用户信息，如路径和密码
        // 4：Full，最大的转储，包含所有内存（包括模块映像）
        Environment.SetEnvironmentVariable("DOTNET_DbgMiniDumpType", "1", EnvironmentVariableTarget.Process);

        // 写入转储的文件路径，确保运行dotnet进程的用户对指定目录具有写入权限
        // 路径动态填充:
        // 1.%%,单个%字符
        // 2.%p,进程ID
        // 3.%e,进程的可执行文件名
        // 4.%h,主机名
        // 5.%t,时间戳(s)
        Environment.SetEnvironmentVariable("DOTNET_DbgMiniDumpName", $@"D:%t_{guid}.dmp", EnvironmentVariableTarget.Process);

        // 启用转储进程的诊断日志记录
        Environment.SetEnvironmentVariable("DOTNET_CreateDumpDiagnostics", "1", EnvironmentVariableTarget.Process);

        // 运行时会生成JSON格式的故障报表，其中包括有关故障应用程序的线程和堆栈帧的信息
        Environment.SetEnvironmentVariable("DOTNET_EnableCrashReport", "1", EnvironmentVariableTarget.Process);

        // 启用转储进程的详细诊断日志记录
        Environment.SetEnvironmentVariable("DOTNET_CreateDumpVerboseDiagnostics", "1", EnvironmentVariableTarget.Process);

        // 应写入诊断消息的文件路径
        Environment.SetEnvironmentVariable("DOTNET_CreateDumpLogToFile", $@"D:%t_{guid}.txt", EnvironmentVariableTarget.Process);
    }

    static void RestartApp()
    {
        string exePath = Process.GetCurrentProcess().MainModule?.FileName!;
        Process.Start(exePath);
        Environment.Exit(0);
    }
}
