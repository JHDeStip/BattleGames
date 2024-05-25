using AutoFixture;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Stip.BattleGames.Common.Messages;
using Stip.BattleGames.Common.Windows;
using Stip.BattleGames.UnitTestsCommon;
using System.Threading;
using System.Threading.Tasks;

namespace Stip.BattleGames.Common.UnitTests.Windows;

[TestClass]
public class ChartWindowViewModelBaseTests
{
    public class TestChartWindowViewModel(
        IMessenger _messenger)
        : ChartWindowViewModelBase(
            _messenger) { }

    [TestMethod]
    public async Task ActivateAsync_CorrectlyActivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var target = fixture.Create<TestChartWindowViewModel>();

        await target.ActivateAsync(CancellationToken.None);

        mockMessenger.VerifyRegister(target, Times.Once());

        mockMessenger.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task DeactivateAsync_CorrectlyDeactivates()
    {
        var fixture = FixtureFactory.Create();

        var mockMessenger = fixture.FreezeMock<IMessenger>();

        var target = fixture.Create<TestChartWindowViewModel>();

        await target.DeactivateAsync(default);

        mockMessenger.Verify(x => x.UnregisterAll(target), Times.Once);

        mockMessenger.VerifyNoOtherCalls();
    }

    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task CanDeactivateAsync_ReturnsCorrectly(
        bool closeAsyncCalled)
    {
        var fixture = FixtureFactory.Create();

        var target = fixture.Create<TestChartWindowViewModel>();

        if (closeAsyncCalled)
        {
            await target.CloseAsync(CancellationToken.None);
        }

        var actual = await target.CanDeactivateAsync(CancellationToken.None);

        Assert.AreEqual(closeAsyncCalled, actual);
    }

    [DataTestMethod]
    [DataRow(WindowState.Normal)]
    [DataRow(WindowState.Minimized)]
    [DataRow(WindowState.Maximized)]
    public void Receive_ToggleChartWindowStateMessage_CorrectlyHandlesMessage(
        WindowState initialWindowState)
    {
        var fixture = FixtureFactory.Create();

        var target = fixture
            .Build<TestChartWindowViewModel>()
            .With(x => x.WindowState, initialWindowState)
            .Create();

        target.Receive(new ToggleChartWindowStateMessage());

        Assert.AreEqual(WindowState.FullScreen, target.WindowState);

        target.Receive(new ToggleChartWindowStateMessage());

        Assert.AreEqual(initialWindowState, target.WindowState);
    }
}
