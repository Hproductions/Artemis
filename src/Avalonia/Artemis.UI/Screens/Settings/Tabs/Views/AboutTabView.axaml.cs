using Artemis.UI.Screens.Settings.Tabs.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Settings.Tabs.Views
{
    public partial class AboutTabView : ReactiveUserControl<AboutTabViewModel>
    {
        public AboutTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}