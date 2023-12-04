using System;
using System.Windows;
using Client.ViewModels;

namespace Client.Views {
    public partial class MainView : Window {
        public MainView() {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
