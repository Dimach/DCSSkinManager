using System;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using SevenZip;

namespace DCSSkinManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            DataContext = new MainWindowViewModel(DialogCoordinator.Instance);
            InitializeComponent();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            SevenZipBase.SetLibraryPath($@"{AppDomain.CurrentDomain.BaseDirectory}\7z.dll");
        }

        private void UserFileMenuControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (sender is HamburgerMenu menu)
            {
                menu.IsPaneOpen = false;
            }
        }
    }
}
