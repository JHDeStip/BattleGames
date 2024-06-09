using AutoFixture;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Helpers;
using System;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Helpers;

[TestClass]
public class StonkMarketManagerTests
{
    [TestMethod]
    public async Task Start_CorrectlyStarts()
    {
        var fixture = FixtureFactory.Create();

        var mockPriceUpdateManager = fixture.FreezeMock<PriceUpdateManager>();
        mockPriceUpdateManager
            .Setup(x => x.StopAsync())
            .Returns(Task.CompletedTask);

        Func<Task> passedStonkMarketWillCrashFunc = null;
        Action passedStonkMarketCrashEndedAction = null;
        var mockCrashMAnager = fixture.FreezeMock<CrashManager>();
        mockCrashMAnager
            .Setup(x => x.Start(
                It.IsAny<Func<Task>>(),
                It.IsAny<Action>()))
            .Callback<Func<Task>, Action>((x, y) =>
            {
                passedStonkMarketWillCrashFunc = x;
                passedStonkMarketCrashEndedAction = y;
            });

        var target = fixture.Create<StonkMarketManager>();

        void VerifyNoOtherCalls()
        {
            mockPriceUpdateManager.VerifyNoOtherCalls();
            mockCrashMAnager.VerifyNoOtherCalls();
        }

        target.Start();

        mockPriceUpdateManager.Verify(x => x.Start(), Times.Once);

        mockCrashMAnager.Verify(
            x => x.Start(
                It.IsAny<Func<Task>>(),
                It.IsAny<Action>()),
            Times.Once);

        VerifyNoOtherCalls();

        if (passedStonkMarketWillCrashFunc is not null)
        {
            await passedStonkMarketWillCrashFunc();
        }
        
        mockPriceUpdateManager.Verify(x => x.StopAsync(), Times.Once);

        VerifyNoOtherCalls();

        passedStonkMarketCrashEndedAction?.Invoke();

        mockPriceUpdateManager.Verify(x => x.Start(), Times.Exactly(2));
    }

    [TestMethod]
    public void Start_DoesNotStartWhenAlreadyStarted()
    {
        var fixture = FixtureFactory.Create();

        var mockPriceUpdateManager = fixture.FreezeMock<PriceUpdateManager>();

        var mockCrashMAnager = fixture.FreezeMock<CrashManager>();

        var target = fixture.Create<StonkMarketManager>();

        void VerifyNoOtherCalls()
        {
            mockPriceUpdateManager.VerifyNoOtherCalls();
            mockCrashMAnager.VerifyNoOtherCalls();
        }

        target.Start();

        mockPriceUpdateManager.Verify(x => x.Start(), Times.Once);

        mockCrashMAnager.Verify(
            x => x.Start(
                It.IsAny<Func<Task>>(),
                It.IsAny<Action>()),
            Times.Once);

        VerifyNoOtherCalls();

        target.Start();

        VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task StopAsync_CorrectlyStopsWhenStopped()
    {
        var fixture = FixtureFactory.Create();

        var mockPriceUpdateManager = fixture.FreezeMock<PriceUpdateManager>();

        var mockCrashMAnager = fixture.FreezeMock<CrashManager>();

        var target = fixture.Create<StonkMarketManager>();

        void VerifyNoOtherCalls()
        {
            mockPriceUpdateManager.VerifyNoOtherCalls();
            mockCrashMAnager.VerifyNoOtherCalls();
        }

        target.Start();

        mockPriceUpdateManager.Verify(x => x.Start(), Times.Once);

        mockCrashMAnager.Verify(
            x => x.Start(
                It.IsAny<Func<Task>>(),
                It.IsAny<Action>()),
            Times.Once);

        VerifyNoOtherCalls();

        await target.StopAsync();

        mockPriceUpdateManager.Verify(x => x.StopAsync(), Times.Once);
        mockCrashMAnager.Verify(x => x.StopAsync(), Times.Once);

        VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task StopAsync_DoesNotStopWhenNotStarted()
    {
        var fixture = FixtureFactory.Create();

        var mockPriceUpdateManager = fixture.FreezeMock<PriceUpdateManager>();

        var mockCrashMAnager = fixture.FreezeMock<CrashManager>();

        var target = fixture.Create<StonkMarketManager>();

        await target.StopAsync();

        mockPriceUpdateManager.VerifyNoOtherCalls();
        mockCrashMAnager.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task StopAsync_DoesNotStopWHenStartedAndThenStopped()
    {
        var fixture = FixtureFactory.Create();

        var mockPriceUpdateManager = fixture.FreezeMock<PriceUpdateManager>();

        var mockCrashMAnager = fixture.FreezeMock<CrashManager>();

        var target = fixture.Create<StonkMarketManager>();

        void VerifyNoOtherCalls()
        {
            mockPriceUpdateManager.VerifyNoOtherCalls();
            mockCrashMAnager.VerifyNoOtherCalls();
        }

        target.Start();

        mockPriceUpdateManager.Verify(x => x.Start(), Times.Once);

        mockCrashMAnager.Verify(
            x => x.Start(
                It.IsAny<Func<Task>>(),
                It.IsAny<Action>()),
            Times.Once);

        VerifyNoOtherCalls();

        await target.StopAsync();

        mockPriceUpdateManager.Verify(x => x.StopAsync(), Times.Once);
        mockCrashMAnager.Verify(x => x.StopAsync(), Times.Once);

        VerifyNoOtherCalls();

        await target.StopAsync();

        VerifyNoOtherCalls();
    }
}
