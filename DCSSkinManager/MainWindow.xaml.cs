using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace DCSSkinManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public UserFiles UserFiles = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is UnitType craft)
            {
                UserFiles = new UserFiles(craft);
                DataLoader.LoadUserFiles(UserFiles);
            }
        }
    }
}
