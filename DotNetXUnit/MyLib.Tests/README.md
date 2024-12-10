### 1. 使用Stryker执行变异测试(Mutation Test)

变异测试思路：对代码中的逻辑进行变异，如==改为!=，若测试不通过则说明覆盖了(Killed)，否则没覆盖(survived)

* 安装Strykey工具：`dotnet tool install -g dotnet-stryker`

* 在单元测试项目根目录下新建一个文件名为stryker-config.json

  ```json
  {
      "stryker-config": {
          "project": "MyLib.csproj",
          "reporters": [ "cleartexttree" ],
          "thresholds": {
              "high": 80,
              "low": 60,
              "break": 50
          },
          "mutate": [ "!Program.cs" ]
      }
  }
  ```

  project：被测试的项目

  reporters：生成报告形式

  thresholds：覆盖率阈值

  mutate：需要变异测试的类，!号表示无需变异测试

* 在单元测试项目中执行`dotnet stryker`，等待测试结果

### 2.使用Github Actions执行CICD

只要根目录创建`.github\workflows\dotnet.yml`文件即可，可用的steps列表[Marketplace](https://github.com/marketplace?type=actions)，也可以自己开发

### 3.单元测试原则

* 自动的、可重复
* 容易运行、快速执行
* 结果稳定，完全与外界隔离
* 容易阅读，期望和问题都容易被看懂
* 测试是可信的

示例1：

```c#
// Bad, 单元测试代码里不应该涉及业务逻辑
User user1 = new User(2600);
User user2 = new User(2800);
User user3 = new User(3000);
int actualTotal = sut.Calc(user1, user2, user3);
int expectedTotal = user1.Salary + user2.Salary + user3.Salary;// 业务逻辑
totalSalary.Should().Be(expectedTotal);
```

```c#
// Good
User user1 = new User(2600);
User user2 = new User(2800);
User user3 = new User(3000);
int totalSalary = sut.Calc(user1, user2, user3);
totalSalary.Should().Be(8400);
```

示例2：

```c#
// Bad, 单元测试里尽量不要使用for、if等逻辑语句
Student student1 = new Student("Abc");
Student student2 = new Student("Def");
Student student3 = new Student("Ghi");
User[] actualUsers = sut.Map(student1, student2, student3);
foreach(User actualUser in actualUsers){
    actualUser.Name.Should().NotNull();
}
```

```c#
// Good
Student student1 = new Student("Abc");
Student student2 = new Student("Def");
Student student3 = new Student("Ghi");
User[] actualUsers = sut.Map(student1, student2, student3);
actualUsers[0].Name.Should().NotNull();
actualUsers[1].Name.Should().NotNull();
actualUsers[2].Name.Should().NotNull();
```

示例3：

```c#
// Bad, 单元测试里不应该涉及业务逻辑
User user = sut.Create("Abc","123456");
user.PasswordHash.Should().Be(hash.Calc("123456"));// 业务逻辑
```

```c#
//Good
User user = sut.Create("Abc","123456");
user.PasswordHash.Should().Be("e10adc3949ba59abbe56e057f20f883e");
```

示例4：

```c#
// Bad, 单元测试里不应该互相依赖, 不要封装逻辑(准备性、收尾性的工作可以封装, 封装到公共类)
[Test]
public void CreateAnalyzer_GoodNameAndBadNameUsage(){
    logan = new LogAnalyzer();
    logan.Initialize();
    bool Valid = logan.IsValid("abc");
    Assert.That(valid, Is.False);
    CreateAnalyzer_GoodFileName_ReturnsTrue();// 调用了另一个单元测试
}
[Test]
public void CreateAnalyzer_GoodFileName_ReturnsTrue(){
    bool valid = logan.IsValid("abcdefg");
    Assert.That(valid, Is.True);
}
```

示例5：

```c#
// Bad, 多个测试用例, 改用InlineData
[Test]
public void Test1(){
    Assert.AreEqual(3, Sum(1, 2));
    Assert.AreEqual(7, Sum(5, 2));
    Assert.AreEqual(0, Sum(1, -1));
}
```

```c#
// Good
[Test]
[InlineData(3, 1, 2)]
[InlineData(7, 5, 2)]
[InlineData(0, 1, -1)]
public void Test1(int result, int i1, int i2){
    Assert.AreEqual(result, Sum(i1, i2));
}
```

### 4.集成测试

项目内部使用真实代码测试，依赖的外部服务(比如http, grpc等)使用Mock

### 5.E2E测试

使用真实代码测试，可以使用Playwright工具模拟浏览器点击测试
