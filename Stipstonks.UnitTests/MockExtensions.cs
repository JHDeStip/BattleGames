using System;
using System.Threading;
using System.Threading.Tasks;
using Stip.Stipstonks.Services;
using Caliburn.Micro;
using Moq;

namespace Stip.Stipstonks.UnitTests
{
    public static class MockExtensions
    {
        public static void VerifyUIDisabledAndEnabledAtLeastOnce(this Mock<DisableUIService> mock)
        {
            mock.Verify(x => x.Disable());
            mock.Verify(x => x.Dispose());

            mock.VerifyNoOtherCalls();
        }

        public static void VerifySubscribeOnce(this Mock<IEventAggregator> mock, object instance)
        {
            mock.Verify(x => x.Subscribe(instance, It.IsAny<Func<Func<Task>, Task>>()), Times.Once);

            mock.VerifyNoOtherCalls();
        }

        public static void VerifyPublishOnCurrentThreadAsync<T>(this Mock<IEventAggregator> mock, T message, Func<Times> times)
            => mock.Verify(x => x.PublishAsync(message, It.IsAny<Func<Func<Task>, Task>>(), It.IsAny<CancellationToken>()), times);

        public static void VerifyPublishOnCurrentThreadAsyncAny<T>(this Mock<IEventAggregator> mock, Func<Times> times)
            => VerifyPublishOnCurrentThreadAsyncAny<T>(mock, times());

        public static void VerifyPublishOnCurrentThreadAsyncAny<T>(this Mock<IEventAggregator> mock, Times times)
            => mock.Verify(x => x.PublishAsync(It.IsAny<T>(), It.IsAny<Func<Func<Task>, Task>>(), It.IsAny<CancellationToken>()), times);
    }
}
