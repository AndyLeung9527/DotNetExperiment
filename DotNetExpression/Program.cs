using System.Linq.Expressions;

namespace DotNetExpression;

internal class Program
{
    //用法类似二叉树结构
    static void Main(string[] args)
    {
        Expression<Func<int, int, int>> expression1_0 = (x, y) => x * y + 1 + 2;
        // 基础应用，组装expression1_0
        {
            // 定义变量参数x,y
            var pX = Expression.Parameter(typeof(int), "x");
            var pY = Expression.Parameter(typeof(int), "y");
            // 定义常量1,2
            var c1 = Expression.Constant(1, typeof(int));
            var c2 = Expression.Constant(2, typeof(int));
            // 定义表达式x*y
            var multiplyPXPY = Expression.Multiply(pX, pY);
            // 定义表达式x*y+1
            var multiplyPXPYAddC1 = Expression.Add(multiplyPXPY, c1);
            // 定义表达式x*y+1+2
            var multiplyPXPYAddC1AddC2 = Expression.Add(multiplyPXPYAddC1, c2);

            // 定义最终的lambda表达式
            var expression1_1 = Expression.Lambda<Func<int, int, int>>(multiplyPXPYAddC1AddC2, pX, pY);
        }

        Expression<Func<People, bool>> expression2_0 = o => o.Id > 10 && o.Name.ToString().Equals("张三") && o.Age < 25;
        // 动态拼装，组装expression2_0
        {
            // 定义People类型的参数o
            var pO = Expression.Parameter(typeof(People), "o");

            // 获取People的Id属性
            var propId = typeof(People).GetProperty("Id")!;
            // 定义常量10
            var c10 = Expression.Constant(10, typeof(int));
            // 定义表达式c.Id>10
            var left = Expression.GreaterThan(Expression.Property(pO, propId), c10);

            // 获取People的Name属性
            var propName = typeof(People).GetProperty("Name")!;
            // ToString方法
            var methodToString = typeof(string).GetMethod("ToString", [])!;
            // 调用o.Name.ToString()方法
            var propNameCallToString = Expression.Call(Expression.Property(pO, propName), methodToString);
            // Equal方法
            var methodEqual = typeof(string).GetMethod("Equals", [typeof(string)])!;
            // 定义表达式o.Name.ToString().Equals("张三")
            var middle = Expression.Call(propNameCallToString, methodEqual, Expression.Constant("张三", typeof(string)));

            // 获取People的Age属性
            var propAge = typeof(People).GetProperty("Age")!;
            // 定义表达式o.Age < 25
            var right = Expression.LessThan(Expression.Property(pO, propAge), Expression.Constant(25, typeof(int)));

            // 定义表达式o.Id > 10 && o.Name.ToString().Equals("张三") && o.Age < 25
            var and = Expression.AndAlso(Expression.AndAlso(left, middle), right);

            //定义最终的lambda表达式
            var expression2_1 = Expression.Lambda<Func<People, bool>>(and, [pO]);
            //编译表达式
            var func = expression2_1.Compile();
            //调用表达式
            var result = func(new People { Id = 11, Name = "张三", Age = 20 });
            Console.WriteLine($"{result}");
        }
    }
}

internal class People
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}