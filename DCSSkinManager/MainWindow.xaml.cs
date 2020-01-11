using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace DCSSkinManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public Holder FilesHolder = new Holder();
        public MainWindow()
        {
            InitializeComponent();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
        }

        public class Holder
        {
            public ObservableCollection<UserFile> UserFiles { get; } = new ObservableCollection<UserFile>()
            {
                new UserFile(){Name = "TEST", Description = "TEST", Author = "THNDF", DownloadLink = "www.www.www", Date = "21.22.2222", Downloads = "0", Size = "5"},
                new UserFile(){Name = "TEST2", Description = "TEST2", Author = "AAA", DownloadLink = "www.www.www", Date = "21.22.2222", Downloads = "0", Size = "5"},
                new UserFile(){Name = "TEST3", Description = "TEST3", Author = "CSDFSDF", DownloadLink = "www.www.www", Date = "21.22.2222", Downloads = "0", Size = "5"},
                new UserFile(){Name = "TEST4", Description = "TEST4", Author = "BBCC", DownloadLink = "www.www.www", Date = "21.22.2222", Downloads = "0", Size = "5"},
            };

            public void Load(UserFiles files)
            {
                UserFiles.Clear();
                files.Files.ForEach(UserFiles.Add);
            }
        }

        private void OnModuleButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is Button button && button.DataContext is UnitType craft)
            {
                var userFiles = new UserFiles(craft);
                DataLoader.LoadUserFiles(userFiles);
                FilesHolder.Load(userFiles);
            }
        }
    }
}
