namespace DotNetExpression;

using System.Linq.Expressions;

internal class Program
{
    //表达式树，用法类似二叉树结构，可以提供重用性
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

        Expression<Func<People, bool>> expression3_0 = o => !(o.Name.Equals("李四") && o.Age < 18 || o.Id == 1);
        // 表达式树的应用，按关键字拼装expression3_0
        {
            Expression<Func<People, bool>> exp = o => true;
            Console.WriteLine("输入名称，为空跳过");
            var name = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(name)) exp = exp.And(o => o.Name.Equals(name));
            Console.WriteLine("输入最大年纪，为空跳过");
            var strAge = Console.ReadLine();
            if (int.TryParse(strAge, out var age)) exp = exp.And(o => o.Age < age);
            Console.WriteLine("输入Id，为空跳过");
            var strId = Console.ReadLine();
            if (int.TryParse(strId, out var Id)) exp = exp.Or(o => o.Id == Id);
            exp = exp.Not();
        }
        // 另一种拼装方法
        {
            Expression<Func<People, bool>> lambda1 = x => x.Name.Equals("李四");
            Expression<Func<People, bool>> lambda2 = x => x.Age < 18;
            Expression<Func<People, bool>> lambda3 = x => x.Id == 1;
            Expression<Func<People, bool>> lambda4 = lambda1.And(lambda2).Or(lambda3).Not();
        }

        // 使用表达式做深拷贝(性能高)
        {
            var people = new People { Id = 1, Age = 18, Name = "王五" };
            var copy = ExpressionGenericMapper<People, PeopleCopy>.Map(people);
        }
    }
}

internal class People
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}

internal static class ExpressionExtend
{
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp1, Expression<Func<T, bool>> exp2)
    {
        var pNew = Expression.Parameter(typeof(T), "c");
        var visitor = new NewExpressionVisitor(pNew);

        var left = visitor.Replace(exp1.Body);
        var right = visitor.Replace(exp2.Body);
        var body = Expression.And(left, right);

        return Expression.Lambda<Func<T, bool>>(body, pNew);
    }

    public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> exp1, Expression<Func<T, bool>> exp2)
    {
        var pNew = Expression.Parameter(typeof(T), "c");
        var visitor = new NewExpressionVisitor(pNew);

        var left = visitor.Replace(exp1.Body);
        var right = visitor.Replace(exp2.Body);
        var body = Expression.Or(left, right);

        return Expression.Lambda<Func<T, bool>>(body, pNew);
    }

    public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> exp)
    {
        var pCandidate = exp.Parameters[0];
        var body = Expression.Not(exp.Body);

        return Expression.Lambda<Func<T, bool>>(body, pCandidate);
    }
}

internal class NewExpressionVisitor : ExpressionVisitor
{
    public ParameterExpression NewParameter { get; private set; }

    public NewExpressionVisitor(ParameterExpression param)
    {
        NewParameter = param;
    }

    public Expression Replace(Expression exp)
    {
        return Visit(exp);
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return NewParameter;
    }
}

internal class PeopleCopy
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Age { get; set; }
}

internal class ExpressionGenericMapper<TIn, TOut>
{
    static Func<TIn, TOut> s_mapFunc;
    static ExpressionGenericMapper()
    {
        var p = Expression.Parameter(typeof(TIn), "p");
        // 表达式目录树
        var memberBindings = new List<MemberBinding>();
        // 处理属性
        foreach (var item in typeof(TOut).GetProperties())
        {
            var propertyInfo = typeof(TIn).GetProperty(item.Name);
            if (propertyInfo == null) continue;
            var property = Expression.Property(p, propertyInfo);
            var memberBinding = Expression.Bind(item, property);
            memberBindings.Add(memberBinding);
        }
        // 处理字段
        foreach (var item in typeof(TOut).GetFields())
        {
            var fieldInfo = typeof(TIn).GetField(item.Name);
            if (fieldInfo == null) continue;
            var field = Expression.Field(p, fieldInfo);
            var memberBinding = Expression.Bind(item, field);
            memberBindings.Add(memberBinding);
        }
        // 组装转换过程
        var memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindings);
        var lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, p);
        //泛型缓存
        s_mapFunc = lambda.Compile();
    }
    public static TOut Map(TIn source) => s_mapFunc(source);
}