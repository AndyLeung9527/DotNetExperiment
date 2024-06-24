using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotNetHarmony;

/// <summary>
/// II注入，改变正在运行的应用程序的行为（旧版.net使用FastWin32注入工具，目前未找到适配的注入工具）
/// </summary>
public class Patcher
{
    //FastWin32要求函数签名必须是static int MethodName(string)
    public static int DoPatching(string msg)
    {
        var harmony = new Harmony("com.example.patch");
        harmony.PatchAll(Assembly.GetExecutingAssembly());//自动补丁
        return 1;
    }
}

[HarmonyPatch("Program", "GetNumber")]
class InjectingPatch
{
    /// <summary>
    /// 前置方法, 在原始方法调用前执行
    /// </summary>
    /// <param name="__result">作为原始方法的返回结果返回</param>
    /// <returns>True:执行原始方法, false:跳过原始方法</returns>
    [HarmonyPrefix]
    static bool Prefix(ref string __result)
    {
        __result = "1";
        return false;
    }

    /// <summary>
    /// 后置方法, 在原始方式调用后执行
    /// </summary>
    /// <param name="__result">原始方法的返回结果</param>
    [HarmonyPostfix]
    static void Postfix(ref string __result)
    {

    }
}