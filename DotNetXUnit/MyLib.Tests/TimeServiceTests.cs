using FluentAssertions;
using Microsoft.Extensions.Time.Testing;

namespace MyLib.Tests;

public class TimeServiceTests
{
    [Fact]
    public void ThrowArgumentException_WhenDateTimeOffsetIsInTheFuture()
    {
        //Arrange
        DateTimeOffset dateTimeOffset = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset dateTimeNow = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero);
        FakeTimeProvider timeProvider = new FakeTimeProvider(dateTimeNow);
        TimeService sut = new TimeService(timeProvider);
        //Act
        Action act = () => sut.GetMonths(dateTimeOffset);
        //Assert
        act.Should().Throw<ArgumentException>().WithMessage("The date must be in the past*");
    }
}
