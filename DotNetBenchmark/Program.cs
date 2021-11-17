using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace DotNetBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary1 = BenchmarkRunner.Run<BenchmarkDemo1>();
            //var summary2 = BenchmarkRunner.Run<BenchmarkDemo2>();
            Console.ReadLine();
        }
    }

    [MemoryDiagnoser, RankColumn]//分析内容使用并排序
    public class BenchmarkDemo1
    {
        [Params(true,false)]//Arguments can be combined with
        public bool AddExtra { get; set; }

        [Benchmark]
        [Arguments(1, 1)]
        [Arguments(1, 2)]
        public void Sleep(int a, int b)
        {
            if (AddExtra)
                Thread.Sleep(a + b + 3);
            else
                Thread.Sleep(a + b);
        }
    }

    public class BenchmarkDemo2
    {
        public IEnumerable<object[]> Numbers()
        {
            yield return new object[] { 1.0, 1.0 };
            yield return new object[] { 2.0, 2.0 };
        }

        public IEnumerable<object[]> NonPrimitive()
        {
            yield return new object[] { new Obj1 { Attr1 = "attr1-1" }, new Obj2 { Attr2 = "attr2-1" } };
            yield return new object[] { new Obj1 { Attr1 = "attr1-2" }, new Obj2 { Attr2 = "attr2-2" } };
        }

        [Benchmark]
        [ArgumentsSource(nameof(Numbers))]
        public void Plus(double x, double y)
        {
            var result = x + y;
        }

        [Benchmark]
        [ArgumentsSource(nameof(NonPrimitive))]
        public void Append(Obj1 obj1,Obj2 obj2)
        {
            var result = obj1.Attr1 + obj2.Attr2;
        }
    }

    #region Utils
    public class Obj1
    {
        public string Attr1 { get; set; }
    }

    public class Obj2
    {
        public string Attr2 { get; set; }
    }
    #endregion
}
