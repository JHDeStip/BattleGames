using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Stip.Stipstonks.Dialogs
{
    // TODO: In general for dialogs: Find a way to disable the minimize, maximize and close buttons. This is not properly implemented yet on Avalonia's side.
    // TODO: Add error and info icons.
    public partial class BasicDialog : Window
    {
        private const double ScreenHeightToMaxHeightRatio = 4 / 5.0;

        public BasicDialogButton Result { get; private set; }

        private bool _canClose;

        public BasicDialog()
            => InitializeComponent();

        public BasicDialog(
            string title,
            string message,
            params BasicDialogButton[] basicDialogButtons)
        {
            SizeChanged += (_, _) =>
            {
                var screen = Screens.ScreenFromVisual(this);
                if (screen is not null)
                {
                    MaxHeight = screen.WorkingArea.Height / screen.Scaling * ScreenHeightToMaxHeightRatio;
                }
            };

            Closing += (_, e) => e.Cancel = !_canClose;
            Deactivated += (_, _) => WindowState = WindowState.Normal;

            InitializeComponent();

            Title = title;

            MessageTextBlock.Text = message;

            var buttonsToAdd = basicDialogButtons
                .OrderBy(x => x)
                .Distinct()
                .Select(CreateButton)
                .ToList();

            var defaultButton = buttonsToAdd.FirstOrDefault();
            if (defaultButton != null)
            {
                defaultButton.IsDefault = true;
            }

            ButtonsPanel.Children.AddRange(buttonsToAdd);
        }

        private Button CreateButton(BasicDialogButton basicDialogButton)
        {
            var button = new Button
            {
                Content = GetBasicDialogButtonText(basicDialogButton)
            };

            button.Click += (_, _) =>
            {
                Result = basicDialogButton;
                _canClose = true;
                Close();
            };

            return button;
        }

        private static string GetBasicDialogButtonText(BasicDialogButton basicDialogButton)
            => basicDialogButton switch
            {
                BasicDialogButton.Close => UIStrings.Global_Close,
                BasicDialogButton.Yes => UIStrings.Global_Yes,
                BasicDialogButton.No => UIStrings.Global_No,
                _ => string.Empty
            };
    }
}
