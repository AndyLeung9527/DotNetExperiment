namespace MyLib.Tests;

//测试项目命名规范: 被测试项目名+.Tests
//单元测试只专注于一个类或方法, 测试类命名规范: 被测试类名+Tests
public class MyClassTests
{
    //TestCase(测试用例), 命名规范: 给定什么条件_期望结果
    //没有参数使用Fact特性标注
    [Fact]
    public void 两个整型相加_计算结果正确()
    {
        //Arrange(准备测试数据)
        int i = 5;
        int j = 6;
        //Act(执行被测试的方法)
        MyClass sut = new MyClass();//被测试的类名一般叫sut(system under test)
        int actualResult = sut.Add(i, j);
        //Assert(断言)
        int expectedResult = 11;
        Assert.Equal(expectedResult, actualResult);
    }

    //有参数使用Theory特性标注, 配合InlineData特性提供测试数据使用
    [Theory]
    [InlineData(5, 6, 0)]
    [InlineData(10, 6, 1)]
    public void 两个整型相除_计算结果正确(int i, int j, int expectedResult)
    {
        //Act
        MyClass sut = new MyClass();//被测试的类名一般叫sut(system under test)
        int actualResult = sut.Divide(i, j);
        //Assert
        Assert.Equal(expectedResult, actualResult);
    }
}
