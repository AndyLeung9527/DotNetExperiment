namespace DotNetHarmony;

using DotNetClass;
using HarmonyLib;
using System.Diagnostics;
using System.Reflection;

internal class Program
{
    static void Main(string[] args)
    {
        InitHook();
        string info = new Person().GetInfo(1);
        Console.WriteLine($"{System.Environment.NewLine}---Main Part---{System.Environment.NewLine}{info}");
        Console.ReadLine();
    }

    static void InitHook()
    {
        Harmony.DEBUG = true;//开启Harmony调试日志
        var harmony = new Harmony("com.example.patch");
        harmony.PatchAll(Assembly.GetExecutingAssembly());//自动补丁
    }
}

[HarmonyPatch(typeof(Person), nameof(Person.GetInfo))]
class Patch
{
    /// <summary>
    /// 前置方法, 在原始方法调用前执行
    /// </summary>
    /// <param name="__instance">对象实例</param>
    /// <param name="____name">对象实例的_name字段</param>
    /// <param name="__result">作为原始方法的返回结果返回</param>
    /// <param name="id">原始方法的参数</param>
    /// <param name="__state">在Prefix和Postfix之间共享, 固定写法__state(out或者ref)</param>
    /// <returns>True:执行原始方法, false:跳过原始方法</returns>
    [HarmonyPrefix]
    static bool Prefix(Person __instance, ref string ____name, ref string __result, ref int id, out Stopwatch __state)
    {
        Console.WriteLine($"{System.Environment.NewLine}---Prefix Part---");

        __state = Stopwatch.StartNew();
        __instance.Age = id;
        ____name = "faker_name";
        if (id < 0)
        {
            __result = "Id is illegal";
            return false;
        }

        return true;
    }

    /// <summary>
    /// 后置方法, 在原始方式调用后执行
    /// </summary>
    /// <param name="__result">原始方法的返回结果</param>
    /// <param name="__state">在Prefix和Postfix之间共享, 固定写法__state</param>
    [HarmonyPostfix]
    static void Postfix(ref string __result, Stopwatch __state)
    {
        __state.Stop();
        Console.WriteLine($"{System.Environment.NewLine}--Postfix Part--{System.Environment.NewLine}Elapsed:{__state.Elapsed.TotalSeconds.ToString("0.00")}s");
        FileLog.Log(DateTime.Now.ToString());
    }
}