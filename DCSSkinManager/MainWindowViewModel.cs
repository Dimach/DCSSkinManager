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
using System.Windows.Media.Imaging;
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
        private ICommand _checkedCommand;
        private ICommand _uncheckedCommand;
        public ObservableCollection<HamburgerMenuGlyphItem> TestCollection { get; } = new ObservableCollection<HamburgerMenuGlyphItem>()
        {
            new HamburgerMenuGlyphItem()
            {
                Glyph = "Icons/SU33.png",
                Label = "TEST S33"
            }
        };
        public ObservableCollection<UserFileData> UserFiles { get; } = new ObservableCollection<UserFileData>() { };
        public ICommand UncheckedCommand => _uncheckedCommand ?? (_uncheckedCommand = new SimpleCommand()
        {
            CanExecuteDelegate = o => true,
            ExecuteDelegate = o =>
            {

            }
        });
        public ICommand CheckedCommand => _checkedCommand ?? (_checkedCommand = new SimpleCommand()
        {
            CanExecuteDelegate = o => true,
            ExecuteDelegate = o =>
            {

            }
        });
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
        private async void OnModuleButtonClick(object sender)
        {
            if (sender is UnitType craft)
            {
                Load(await this.dataLoader.LoadUserFiles(craft, CancellationToken.None));
            }
        }
        public void Load(UserFiles files)
        {
            UserFiles.Clear();
            files.Files.ForEach(x=>UserFiles.Add(new UserFileData(x, dataLoader)));
        }
    }

    public class UserFileData
    {
        private readonly DataLoader _dataLoader;
        private readonly string[] imageLinks;
        public UserFileData(UserFile file, DataLoader dataLoader)
        {
            _dataLoader = dataLoader;
            Name = file.Name;
            Description = file.Description;
            Author = file.Author;
            Date = file.Date;
            Size = file.Size;
            Downloads = file.Downloads;
            IsDownloaded = file.Preview.Length % 2 == 0;//file.Downloaded;
            imageLinks = file.Preview;

            DownloadImages();
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Date { get; set; }
        public string Size { get; set; }
        public string Downloads { get; set; }
        public bool IsDownloaded { get; set; }

        public BitmapImage MainImage => _dataLoader.GetPreviewImage(imageLinks[0], CancellationToken.None).Result;
        public ObservableCollection<BitmapImage> AllImages => new ObservableCollection<BitmapImage>(imageLinks.Select(x=>_dataLoader.GetPreviewImage(x, CancellationToken.None).Result).ToList());
        public async void DownloadImages()
        {
            if(imageLinks.Length == 0)
                return;
            // Task.WhenAll(imageLinks.Select(x => _dataLoader.GetPreviewImage(x, CancellationToken.None)).ToArray());
        }
    }
}