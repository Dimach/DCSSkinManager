using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MahApps.Metro;
using System.Windows.Input;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using DCSSkinManager.Annotations;

namespace DCSSkinManager
{
    public class SimpleCommand : ICommand
    {
        public Predicate<object> CanExecuteDelegate { get; set; }
        public Action<object> ExecuteDelegate { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate(parameter);
            return true; // if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
                ExecuteDelegate(parameter);
        }
    }

    public class AccentColorMenuData
    {
        public string Name { get; set; }
        public Brush BorderColorBrush { get; set; }
        public Brush ColorBrush { get; set; }

        private ICommand changeAccentCommand;

        public ICommand ChangeAccentCommand
        {
            get
            {
                return this.changeAccentCommand ?? (changeAccentCommand = new SimpleCommand
                           {CanExecuteDelegate = x => true, ExecuteDelegate = x => this.DoChangeTheme(x)});
            }
        }

        protected virtual void DoChangeTheme(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var accent = ThemeManager.GetAccent(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, accent, theme.Item1);
        }
    }

    public class AppThemeMenuData : AccentColorMenuData
    {
        protected override void DoChangeTheme(object sender)
        {
            var theme = ThemeManager.DetectAppStyle(Application.Current);
            var appTheme = ThemeManager.GetAppTheme(this.Name);
            ThemeManager.ChangeAppStyle(Application.Current, theme.Item2, appTheme);
        }
    }

    public class MainWindowViewModel
    {
        private ICommand _moduleClickCommand;

        public ObservableCollection<UserFile> UserFiles { get; } = new ObservableCollection<UserFile>()
        {
            new UserFile(UnitType.KA50)
            {
                Name = "TEST", Description = "TEST", Author = "THNDF", DownloadLink = "www.www.www",
                Date = "21.22.2222", Downloads = "0", Size = "5"
            },
            new UserFile(UnitType.F14B)
            {
                Name = "TEST2", Description = "TEST2", Author = "AAA", DownloadLink = "www.www.www",
                Date = "21.22.2222", Downloads = "0", Size = "5"
            },
            new UserFile(UnitType.MI8MTV2)
            {
                Name = "TEST3", Description = "TEST3", Author = "CSDFSDF", DownloadLink = "www.www.www",
                Date = "21.22.2222", Downloads = "0", Size = "5"
            },
            new UserFile(UnitType.F16C)
            {
                Name = "TEST4", Description = "TEST4", Author = "BBCC", DownloadLink = "www.www.www",
                Date = "21.22.2222", Downloads = "0", Size = "5"
            },
        };

        public ICommand ModuleClickCommand => _moduleClickCommand ?? (_moduleClickCommand = new SimpleCommand()
        {
            CanExecuteDelegate = o => true,
            ExecuteDelegate = OnModuleButtonClick
        });

        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        private DataLoader dataLoader { get; }
        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            this.dataLoader = new DataLoader($"test");
            // create accent color menu items for the demo
            this.AccentColors = ThemeManager.Accents
                .Select(a => new AccentColorMenuData()
                    {Name = a.Name, ColorBrush = a.Resources["AccentColorBrush"] as Brush})
                .ToList();

            // create metro theme color menu items for the demo
            this.AppThemes = ThemeManager.AppThemes
                .Select(a => new AppThemeMenuData()
                {
                    Name = a.Name, BorderColorBrush = a.Resources["BlackColorBrush"] as Brush,
                    ColorBrush = a.Resources["WhiteColorBrush"] as Brush
                })
                .ToList();
        }

        private void OnModuleButtonClick(object sender)
        {
            if (sender is UnitType craft)
            {
                Load(this.dataLoader.LoadUserFiles(craft));
            }
        }
        public void Load(UserFiles files)
        {
            UserFiles.Clear();
            files.Files.ForEach(UserFiles.Add);
        }
    }
}