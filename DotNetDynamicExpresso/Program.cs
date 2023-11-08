namespace DotNetDynamicExpresso;

using DynamicExpresso;

internal class Program
{
    static void Main(string[] args)
    {
        //基本调用
        {
            var interpreter = new Interpreter();
            var result = interpreter.Eval("8/2+2");
        }

        //使用变量或参数解析表达式并多次调用
        {
            var interpreter = new Interpreter().SetVariable("service", new ServiceExample());
            string expression = "x > 4 ? service.OneMethod() : service.AnotherMethod()";
            Lambda parsedExpression = interpreter.Parse(expression, new Parameter("x", typeof(int)));
            var result = parsedExpression.Invoke(5);
            result = parsedExpression.Invoke(5);
            result = parsedExpression.Invoke(5);
        }

        //为LINQ查询生成委托和lambda表达式
        {
            var whereFunction = new Interpreter().ParseAsDelegate<Func<int, bool>>("arg > 5");
            var prices = new[] { 5, 8, 6, 2 };
            var count = prices.Where(whereFunction).Count();
        }

        //通过泛型指定表达式返回值
        {
            var target = new Interpreter();
            double result = target.Eval<double>("Math.Pow(x,y) + 5", new Parameter("x", typeof(int), 10), new Parameter("y", typeof(double), 2));
        }

        //设置变量
        {
            var target = new Interpreter().SetVariable("myVar", 23);
            var result = target.Eval("myVar");
        }

        //设置委托
        {
            Func<double, double, double> pow = (x, y) => Math.Pow(x, y);
            var target = new Interpreter().SetFunction("pow", pow);
            var result = target.Eval("pow(3,2)");
        }

        //Paramers1
        {
            var target = new Interpreter();
            var parameters = new[]
            {
                new Parameter("x", 23),
                new Parameter("y", 7),
            };
            var result = target.Eval("x + y", parameters);
        }

        //Paramers2
        {
            var target = new Interpreter();
            var parameters = new[]
            {
                new Parameter("x", typeof(int)),
                new Parameter("y", typeof(int)),
            };
            var lambda = target.Parse("x + y", parameters);
            var result = lambda.Invoke(23, 7);
        }

        //隐式引用名为this的变量或参数
        {
            var target = new Interpreter();
            target.SetVariable("this", new Customer { Name = "John" });
            var result = target.Eval("this.Name");
            var result2 = target.Eval("Name");
        }

        //引用自定义类，可以调用类的方法、字段、属性和构造函数
        {
            var target = new Interpreter().Reference(typeof(Uri));
            var equal = target.Eval<Type>("typeof(Uri)") == typeof(Uri);
            var equal2 = target.Eval<string>("Uri.UriSchemeHttp") == Uri.UriSchemeHttp;
        }
        {
            var target = new Interpreter();
            var equal = new DateTime(2023, 11, 7) == target.Eval<DateTime>("new DateTime(2023,11,7)");
        }
        {
            var x = new int[] { 10, 30, 4 };
            var target = new Interpreter().Reference(typeof(System.Linq.Enumerable)).SetVariable("x", x);
            var count = target.Eval("x.Count()");
            var first = target.Eval("x[0]");
        }

        //Lambda表达式
        {
            var x = new string[] { "this", "is", "awesome" };
            var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
            var target = new Interpreter(options).SetVariable("x", x);
            var result = target.Eval<IEnumerable<string>>("x.Where(str => str.Length > 5).Select(str => str.ToUpper())");
        }
        {
            var options = InterpreterOptions.Default | InterpreterOptions.LambdaExpressions;
            var target = new Interpreter(options).SetVariable("increment", 3);
            var lambda = target.Eval<Func<int, string, string>>("(i,str) => str.ToUpper() + (i + increment)");
            var result = lambda.Invoke(5, "test");
        }

        //区分大小写（默认区分）
        {
            var target = new Interpreter(InterpreterOptions.DefaultCaseInsensitive);
            double x = 2;
            var parameters = new[] { new Parameter("x", x.GetType(), x) };
            var equal = x == target.Eval<double>("x", parameters);
            var equal2 = x == target.Eval<double>("X", parameters);
        }

        //获取表达式标识符
        {
            var target = new Interpreter();
            var detectedIdentifiers = target.DetectIdentifiers("x + y");
            var equal = new[] { "x", "y" }.SequenceEqual(detectedIdentifiers.UnknownIdentifiers.ToArray());
        }

        {
            var target = new Interpreter();
            target.SetDefaultNumberType(DefaultNumberType.Decimal);
            var equal = typeof(Decimal).IsInstanceOfType(target.Eval("45"));
            var equal2 = (10M / 3M).Equals(target.Eval("10/3"));
        }

        //禁止赋值运算符
        {
            var target = new Interpreter().EnableAssignment(AssignmentOperators.None);
            var customer = new Customer { Name = "John" };
            target.SetVariable("this", customer);
            //var result = target.Eval("Name = \"abc\"");//Error
        }

        //启用反射功能（默认不启用）
        {
            var target = new Interpreter().EnableReflection();
            var result = target.Eval("typeof(double).GetMethods()");
            var result2 = target.Eval("typeof(double).Assembly");
        }

        Console.ReadLine();
    }
}

public class ServiceExample
{
    int _x;
    public int OneMethod() => _x++;
    public int AnotherMethod() => _x++;
}

public class Customer
{
    public string Name { get; set; }
}