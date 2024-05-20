using AutoFixture;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.UnitTestsCommon;
using Stip.Stipstonks.Factories;
using Stip.Stipstonks.Helpers;
using Stip.Stipstonks.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.Stipstonks.UnitTests.Helpers
{
    [TestClass]
    public class PriceUpdateManagerTests
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

            var firstTickTaskCompletionSource = new TaskCompletionSource<bool>();
            var secondTickTaskCompletionSource = new TaskCompletionSource<bool>();

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

            if (otherOperationIsCancelled)
            {
                mockMessenger
                    .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<AnyToken>()))
                    .Throws(new OperationCanceledException());
            }

            var mockPriceCalculator = fixture.FreezeMock<PriceCalculator>();

            var target = fixture.Create<PriceUpdateManager>();

            void VerifyNoOtherCalls()
            {
                mockPeriodicTimer.VerifyNoOtherCalls();
                mockPeriodicTimerFactory.VerifyNoOtherCalls();
                mockMessenger.VerifyNoOtherCalls();
                mockPriceCalculator.VerifyNoOtherCalls();
            }

            target.Start();

            mockPeriodicTimerFactory.Verify(x => x.Create(applicationContext.Config.PriceUpdateInterval), Times.Once);
            mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Once);

            if (timerIsCancelled)
            {
                mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);
            }

            VerifyNoOtherCalls();

            firstTickTaskCompletionSource.SetResult(true);

            if (timerIsCancelled)
            {
                VerifyNoOtherCalls();
                return;
            }

            mockPriceCalculator.Verify(
                x => x.RecalculatePrices(
                    applicationContext.Products,
                    applicationContext.Config.MaxPriceDeviationFactor,
                    applicationContext.Config.PriceResolutionInCents),
                Times.Once);

            mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Once());

            if (otherOperationIsCancelled)
            {
                mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);
                VerifyNoOtherCalls();
                return;
            }

            mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Exactly(2));

            VerifyNoOtherCalls();

            secondTickTaskCompletionSource.SetResult(true);

            mockPriceCalculator.Verify(
                x => x.RecalculatePrices(
                    applicationContext.Products,
                    applicationContext.Config.MaxPriceDeviationFactor,
                    applicationContext.Config.PriceResolutionInCents),
                Times.Exactly(2));

            mockMessenger.VerifySend<PricesUpdatedMessage>(Times.Exactly(2));

            mockPeriodicTimer.Verify(x => x.WaitForNextTickAsync(It.Is<CancellationToken>(y => y != CancellationToken.None)), Times.Exactly(3));

            mockPeriodicTimer.Verify(x => x.Dispose(), Times.Once);

            VerifyNoOtherCalls();
        }
    }
}
