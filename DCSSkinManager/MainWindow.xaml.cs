using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Documents;

namespace DCSSkinManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<UserFile> UserFiles { get; } = new ObservableCollection<UserFile>()
        {
            new UserFile(){Name = "TEST", Description = "TEST", Author = "THNDF", DownloadLink = "www.www.www", Date = "21.22.2222", Downloads = "0", Size = "5"}
        };
        public MainWindow()
        {
            InitializeComponent();
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
            DataContext = UserFiles;
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is UnitType craft)
            {
                var userFiles = new UserFiles(craft);
                DataLoader.LoadUserFiles(userFiles);
                UserFiles.Clear();
                userFiles.Files.ForEach(x=>UserFiles.Add(x));
            }
        }
    }
}
