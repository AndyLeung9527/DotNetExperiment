using AutoFixture.Xunit2;
using FakeItEasy;
using FluentAssertions;

namespace MyLib.Tests;

//安装nuget: AutoFixture.Xunit2, AutoFixture.AutoFakeItEasy, FluentAssertions
public class MyServiceTests_AutoFake
{
    [Theory]
    [AutoFakeItEasy]
    public async Task ChangeAsync_Successfully([Frozen] IMyRepository myRepository, MyService sut, string id, CancellationToken cancellationToken)
    {
        //Arrange
        object o = new object();
        A.CallTo(() => myRepository.FindAsync(id, cancellationToken)).Returns(o);//为Fake类的FindAsync方法设置模拟实现
        //Act
        bool ret = await sut.ChangeAsync(id, cancellationToken);
        //Assert
        ret.Should().BeTrue();
        A.CallTo(() => myRepository.UpdateAsync(A<object>.That.Matches(p => p.ToString() == o.ToString()), cancellationToken)).MustHaveHappenedOnceExactly();//断言Fake类UpdateAsync方法执行了一次, 并且第一个参数ToString()等于o.ToString()
    }

    [Theory]
    [AutoFakeItEasy]
    public async Task ChangeAsync_WhenTheIdIsEmpty([Frozen] IMyRepository myRepository, MyService sut, string id, CancellationToken cancellationToken)
    {
        //Arrange
        object o = new object();
        A.CallTo(() => myRepository.FindAsync(id, cancellationToken)).Returns(o);
        //Act
        Func<Task<bool>> func = async () => await sut.ChangeAsync(null, cancellationToken);
        //Assert
        await func.Should().ThrowAsync<Exception>().WithMessage("ID cannot be empty*");//断言ChangeAsync抛出异常
        A.CallTo(() => myRepository.UpdateAsync(A<object>.Ignored, cancellationToken)).MustNotHaveHappened();//断言Fake类UpdateAsync方法不能被执行
    }
}
