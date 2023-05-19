using System;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Services
{
    public class DisableUIService :
        IInjectable,
        IDisposable
    {
        private readonly Stack<KeyValuePair<IUIEnabled, int>> _viewModels = new();

        public virtual IDisposable Disable()
        {
            lock (_viewModels)
            {
                if (_viewModels.Any())
                {
                    var vm = _viewModels.Pop();
                    vm.Key.UIEnabled = false;
                    _viewModels.Push(new(vm.Key, vm.Value + 1));
                }
            }

            return this;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            lock (_viewModels)
            {
                if (_viewModels.Any())
                {
                    var vm = _viewModels.Pop();

                    if (vm.Value <= 1)
                    {
                        vm.Key.UIEnabled = true;
                    }
                    
                    _viewModels.Push(new(vm.Key, Math.Max(0, vm.Value - 1)));
                }
            }
        }

        public virtual void PushViewModel(IUIEnabled vm)
        {
            lock (_viewModels)
            {
                _viewModels.Push(new(vm, 0));
            }
        }

        public virtual void PopViewModel()
        {
            lock (_viewModels)
            {
                _viewModels.Pop();
            }
        }
    }
}
