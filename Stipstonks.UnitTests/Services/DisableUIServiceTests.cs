using Microsoft.VisualStudio.TestTools.UnitTesting;
using Stip.Stipstonks;
using Stip.Stipstonks.Services;

namespace AodbManagementTool.UnitTests.Services
{
    [TestClass]
    public class DisableUIServiceTests
    {
        [TestMethod]
        public void Disable_DisablesUI()
        {
            var vm = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm);

            target.Disable();

            Assert.IsFalse(vm.UIEnabled);
        }

        [TestMethod]
        public void Dispose_EnablesUI()
        {
            var vm = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm);
            vm.UIEnabled = false;

            target.Dispose();

            Assert.IsTrue(vm.UIEnabled);
        }

        [TestMethod]
        public void Disable_KeepsUIDisabledWhenCalledMultipleTimes()
        {
            var vm = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm);

            target.Disable();
            target.Disable();

            Assert.IsFalse(vm.UIEnabled);
        }

        [TestMethod]
        public void Dispose_NeedsToBeCalledEqualAmountOfTimesAsDisableToEnableUI()
        {
            var vm = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm);

            target.Disable();
            target.Disable();
            target.Disable();

            target.Dispose();
            Assert.IsFalse(vm.UIEnabled);

            target.Dispose();
            Assert.IsFalse(vm.UIEnabled);

            target.Dispose();
            Assert.IsTrue(vm.UIEnabled);
        }

        [TestMethod]
        public void Dispose_CanBeCalledTooManyTimes()
        {
            var vm = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm);

            target.Disable();

            target.Dispose();
            target.Dispose();
            Assert.IsTrue(vm.UIEnabled);

            target.Disable();
            Assert.IsFalse(vm.UIEnabled);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void DisableAndDispose_OnlyAffectLastPushedViewModel(bool vm1UIEnabled)
        {
            var vm1 = new TestIUIEnabled
            {
                UIEnabled = vm1UIEnabled
            };

            var vm2 = new TestIUIEnabled();

            var target = new DisableUIService();
            target.PushViewModel(vm1);
            target.PushViewModel(vm2);

            target.Disable();
            Assert.AreEqual(vm1UIEnabled, vm1.UIEnabled);
            Assert.IsFalse(vm2.UIEnabled);

            target.Disable();
            Assert.AreEqual(vm1UIEnabled, vm1.UIEnabled);
            Assert.IsFalse(vm2.UIEnabled);

            target.Dispose();
            Assert.AreEqual(vm1UIEnabled, vm1.UIEnabled);
            Assert.IsFalse(vm2.UIEnabled);

            target.Dispose();
            Assert.AreEqual(vm1UIEnabled, vm1.UIEnabled);
            Assert.IsTrue(vm2.UIEnabled);
        }

        [TestMethod]
        public void Dispose_EnablesParentViewModelWhenChildViewModelIsPopped()
        {
            var vm1 = new TestIUIEnabled();
            var vm2 = new TestIUIEnabled();

            var target = new DisableUIService();

            target.PushViewModel(vm1);
            target.Disable();

            target.PushViewModel(vm2);
            target.Disable();

            target.Dispose();

            target.PopViewModel();

            target.Dispose();

            Assert.IsTrue(vm1.UIEnabled);
        }

        private class TestIUIEnabled : IUIEnabled
        {
            public bool UIEnabled { get; set; } = true;
        }
    }
}
