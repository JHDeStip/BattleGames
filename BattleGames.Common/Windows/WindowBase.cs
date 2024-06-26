﻿using Avalonia.Controls;

namespace Stip.BattleGames.Common.Windows;

public abstract class WindowBase<T> : Window where T : ViewModelBase
{
    public T ViewModel => (T)DataContext;

    protected WindowBase(T viewModel)
    {
        DataContext = viewModel;
        viewModel.Window = this;
    }
}
