using System.Collections.Specialized;
using System.Windows;
using TPC_GUI.ViewModels;

namespace TPC_GUI.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hacky way to do Listbox.ScrollIntoView
            ((INotifyCollectionChanged)StatusItems.Items).CollectionChanged += ListView_CollectionChanged;
        }

        private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                StatusItems.ScrollIntoView(e.NewItems[0]);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            viewModel.Dispose();
        }
    }
}
