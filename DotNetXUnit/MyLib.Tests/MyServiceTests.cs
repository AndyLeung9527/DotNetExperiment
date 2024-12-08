namespace MyLib.Tests;

public class MyServiceTests
{
    [Fact]
    public async Task ChangeAsync_输入存在id_修改实体成功()
    {
        //Arrange
        var id = "10001";
        //Act
        FakeMyRepository fakeMyRepository = new FakeMyRepository(id);
        MyService sut = new MyService(fakeMyRepository);//MyService依赖注入的IMyRepository, 需要重写一个假的实现类(FakeMyRepository)来替代，不能使用真正的实现类
        bool expectedResult = await sut.ChangeAsync(id, CancellationToken.None);
        //Assert
        Assert.True(expectedResult);
        Assert.Equal(1, fakeMyRepository.UpdateAsyncInvokedCounter);//确保Repository的UpdateAsync方法一定执行了一次
    }

    [Fact]
    public async Task ChangeAsync_输入不存在id_修改实体失败()
    {
        //Arrange
        var correctId = "10001";
        var errorId = "10002";
        //Act
        FakeMyRepository fakeMyRepository = new FakeMyRepository(correctId);
        MyService sut = new MyService(fakeMyRepository);//MyService依赖注入的IMyRepository, 需要重写一个假的实现类(FakeMyRepository)来替代，不能使用真正的实现类
        bool expectedResult = await sut.ChangeAsync(errorId, CancellationToken.None);
        //Assert
        Assert.False(expectedResult);
        Assert.Equal(0, fakeMyRepository.UpdateAsyncInvokedCounter);//确保Repository的UpdateAsync方法没执行过
    }

    public class FakeMyRepository : IMyRepository
    {
        private readonly string _id;

        public int UpdateAsyncInvokedCounter { get; private set; }

        public FakeMyRepository(string id)
        {
            _id = id;
        }

        public Task<object?> FindAsync(string id, CancellationToken cancellationToken)
        {
            if (_id == id) return Task.FromResult<object?>(new());
            else return Task.FromResult<object?>(null);
        }

        public Task UpdateAsync(object obj, CancellationToken cancellationToken)
        {
            UpdateAsyncInvokedCounter++;//UpdateAsync调用次数
            return Task.CompletedTask;
        }
    }
}
