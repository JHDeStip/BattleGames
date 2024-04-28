using Stip.Stipstonks.Services;
using Moq;
using CommunityToolkit.Mvvm.Messaging;
using System;

namespace Stip.Stipstonks.UnitTests
{
    [TypeMatcher]
    public sealed class AnyToken : ITypeMatcher, IEquatable<AnyToken>
    {
        public bool Matches(Type typeArgument) => true;
        public bool Equals(AnyToken? other) => throw new NotImplementedException();
    }

    public static class MockExtensions
    {
        public static void VerifyUIDisabledAndEnabledAtLeastOnce(this Mock<DisableUIService> mock)
        {
            mock.Verify(x => x.Disable());
            mock.Verify(x => x.Dispose());

            mock.VerifyNoOtherCalls();
        }

        public static void VerifyRegister<TMessage>(this Mock<IMessenger> mock, IRecipient<TMessage> target, Times times) where TMessage : class
            => mock.Verify(x => x.Register(target, It.IsAny<AnyToken>(), It.IsAny<MessageHandler<IRecipient<TMessage>, TMessage>>()), times);

        public static void VerifySend<TMessage>(this Mock<IMessenger> mock, Times times) where TMessage : class
            => mock.Verify(x => x.Send(It.IsAny<TMessage>(), It.IsAny<AnyToken>()), times);
    }
}
