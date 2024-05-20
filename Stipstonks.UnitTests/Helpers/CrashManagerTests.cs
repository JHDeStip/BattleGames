using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Helpers;

[TestClass]
public class CrashManagerTests
{
    [DataTestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    public void Start_CorrectlyStarts(
        bool timerIsCancelled,
        bool otherOperationIsCancelled)
    {
        var fixture = FixtureFactory.Create();

        var applicationContext = fixture.Freeze<ApplicationContext>();
        applicationContext.HasCrashed = false;

        var firstTickTaskCompletionSource = new TaskCompletionSource<bool>();
        var secondTickTaskCompletionSource = new TaskCompletionSource<bool>();
        var firstDelayCompletionSource = new TaskCompletionSource();
        var secondDelayCompletionSource = new TaskCompletionSource();

        var mockPeriodicTimer = fixture.FreezeMock<IPeriodicTimer>();
        mockPeriodicTimer
            .SetupSequence(x => x.WaitForNextTickAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<bool>(firstTickTaskCompletionSource.Task))
            .Returns(new ValueTask<bool>(secondTickTaskCompletionSource.Task))
            .ReturnsAsync(() =>
            {
                throw new OperationCanceledException();
            });

        if (timerIsCancelled)
        {
            mockPeriodicTimer
                .Setup(x => x.WaitForNextTickAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
        }

        var mockPeriodicTimerFactory = fixture.FreezeMock<PeriodicTimerFactory>();
        mockPeriodicTimerFactory
            .Setup(x => x.Create(It.IsAny<TimeSpan>()))
            .Returns(mockPeriodicTimer.Object);

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var mockPriceCalculator = fixture.FreezeMock<PriceCalculator>();

        var mockDelayHelper = fixture.FreezeMock<DelayHelper>();
        mockDelayHelper
            .SetupSequence(x => x.Delay(
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()))
            .Returns(firstDelayCompletionSource.Task)
            .Returns(secondDelayCompletionSource.Task);

        if (otherOperationIsCancelled)
        {
            mockDelayHelper
                .Setup(x => x.Delay(
                    It.IsAny<TimeSpan>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());
        }

        var mockDataPersistenceHelper = fixture.FreezeMock<DataPersistenceHelper>();
        mockDataPersistenceHelper
            .Setup(x => x.SaveDataAsync())
            .Returns(Task.FromResult(ActionResult.FromSuccessState(true)));

        var nStonkMarketWillCrashCalls = 0;
        var nStonkMarketCrashEndedCalls = 0;

        var target = fixture.Create<CrashManager>();

        void VerifyNoOtherCalls()
        {
            mockPeriodicTimer.VerifyNoOtherCalls();
            mockPeriodicTimerFactory.VerifyNoOtherCalls();
            mockMessenger.VerifyNoOtherCalls();
            mockPriceCalculator.VerifyNoOtherCalls();
            mockDataPersistenceHelper.VerifyNoOtherCalls();
            mockDelayHelper.VerifyNoOtherCalls();
        }

        target.Start(
            () =>
            {
                ++nStonkMarketWillCrashCalls;
                return Task.CompletedTask;
            },
            () => ++nStonkMarketCrashEndedCalls);

        Assert.IsFalse(applicationContext.HasCrashed);
        Assert.AreEqual(0, nStonkMarketWillCrashCalls);
        Assert.AreEqual(0, nStonkMarketCrashEndedCalls);

        mockPeriodicTimerFactory.Verify(x => x.Create(applicationContext.Config.CrashInterval), Times.Once);
        mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Once);
        
        if (timerIsCancelled)
        {
            mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);
        }

        VerifyNoOtherCalls();

        firstTickTaskCompletionSource.SetResult(true);

        if (timerIsCancelled)
        {
            Assert.IsFalse(applicationContext.HasCrashed);
            Assert.AreEqual(0, nStonkMarketWillCrashCalls);
            Assert.AreEqual(0, nStonkMarketCrashEndedCalls);

            VerifyNoOtherCalls();
            return;
        }

        Assert.AreEqual(!otherOperationIsCancelled, applicationContext.HasCrashed);
        Assert.AreEqual(1, nStonkMarketWillCrashCalls);
        Assert.AreEqual(0, nStonkMarketCrashEndedCalls);

        mockPriceCalculator.Verify(
            x => x.Crash(
                applicationContext.Products,
                applicationContext.Config.MaxPriceDeviationFactor,
                applicationContext.Config.PriceResolutionInCents),
            Times.Once);

        mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Once());
        mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Once);
        mockDelayHelper.Verify(
            x => x.Delay(
                applicationContext.Config.CrashDuration,
                It.Is<CancellationToken>(y => y != CancellationToken.None)),
            Times.Once);

        if (otherOperationIsCancelled)
        {
            mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);
        }

        VerifyNoOtherCalls();

        firstDelayCompletionSource.SetResult();

        if (otherOperationIsCancelled)
        {
            Assert.IsFalse(applicationContext.HasCrashed);
            Assert.AreEqual(1, nStonkMarketWillCrashCalls);
            Assert.AreEqual(0, nStonkMarketCrashEndedCalls);

            VerifyNoOtherCalls();
            return;
        }

        Assert.IsFalse(applicationContext.HasCrashed);
        Assert.AreEqual(1, nStonkMarketWillCrashCalls);
        Assert.AreEqual(1, nStonkMarketCrashEndedCalls);

        mockPriceCalculator.Verify(x => x.ResetPricesAfterCrash(applicationContext.Products), Times.Once);
        mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Exactly(2));

        mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Exactly(2));

        VerifyNoOtherCalls();

        secondTickTaskCompletionSource.SetResult(true);

        Assert.IsTrue(applicationContext.HasCrashed);
        Assert.AreEqual(2, nStonkMarketWillCrashCalls);
        Assert.AreEqual(1, nStonkMarketCrashEndedCalls);

        mockPriceCalculator.Verify(
            x => x.Crash(
                applicationContext.Products,
                applicationContext.Config.MaxPriceDeviationFactor,
                applicationContext.Config.PriceResolutionInCents),
            Times.Exactly(2));

        mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Exactly(3));
        mockDataPersistenceHelper.Verify(x => x.SaveDataAsync(), Times.Exactly(2));
        mockDelayHelper.Verify(
            x => x.Delay(
                applicationContext.Config.CrashDuration,
                It.Is<CancellationToken>(y => y != CancellationToken.None)),
            Times.Exactly(2));

        VerifyNoOtherCalls();

        secondDelayCompletionSource.SetResult();

        Assert.IsFalse(applicationContext.HasCrashed);
        Assert.AreEqual(2, nStonkMarketWillCrashCalls);
        Assert.AreEqual(2, nStonkMarketCrashEndedCalls);

        mockPriceCalculator.Verify(x => x.ResetPricesAfterCrash(applicationContext.Products), Times.Exactly(2));
        mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Exactly(4));

        mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Exactly(3));
        mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);

        VerifyNoOtherCalls();
    }
}
