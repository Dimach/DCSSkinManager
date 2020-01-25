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
using System.Windows.Threading;
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

        public ObservableCollection<HamburgerMenuGlyphItem> TestCollection { get; } =
            new ObservableCollection<HamburgerMenuGlyphItem>()
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
                _dataLoader.DeleteFile((o as UserFileData)?.UserFile, CancellationToken.None);
            }
        });

        public ICommand CheckedCommand => _checkedCommand ?? (_checkedCommand = new SimpleCommand()
        {
            CanExecuteDelegate = o => true,
            ExecuteDelegate = o =>
            {
                _dataLoader.InstallFile((o as UserFileData)?.UserFile, CancellationToken.None);
            }
        });

        public ICommand ModuleClickCommand => _moduleClickCommand ?? (_moduleClickCommand = new SimpleCommand()
        {
            CanExecuteDelegate = o => true,
            ExecuteDelegate = OnModuleButtonClick
        });

        public List<AccentColorMenuData> AccentColors { get; set; }
        public List<AppThemeMenuData> AppThemes { get; set; }
        private DataLoader _dataLoader { get; }

        public MainWindowViewModel(IDialogCoordinator dialogCoordinator)
        {
            this._dataLoader = new DataLoader(@"C:\Users\Professional\Saved Games\DCS.openbeta");
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
                Load(await this._dataLoader.LoadUserFiles(craft, CancellationToken.None));
            }
        }

        public void Load(UserFiles files)
        {
            UserFiles.Clear();
            //UserFiles.Add(new UserFileData(files.Files[0], dataLoader));
            files.Files
                .Take(20).ToList()
                .ForEach(x=>UserFiles.Add(new UserFileData(x, _dataLoader)));
        }
    }

    public class UserFileData : INotifyPropertyChanged
    {
        private readonly DataLoader _dataLoader;
        private BitmapImage _mainImage;

        public UserFileData(UserFile file, DataLoader dataLoader)
        {
            _dataLoader = dataLoader;
            UserFile = file;

            var path = $"Samples/not_loaded_sample.gif";
            var uri = new Uri(path, UriKind.Relative);
            var image = new BitmapImage(uri);
            MainImage = image;
            AllImages = new ObservableCollection<Image>();
            for (int i = 0; i < UserFile.Preview.Length; i++)
            {
                AllImages.Add(new Image() { Source = image });
            }

            DownloadImages();
        }
        public UserFile UserFile { get; }
        public string Name => UserFile.Name;
        public string Description => UserFile.Description;
        public string Author => UserFile.Author;
        public string Date => UserFile.Date;
        public string Size => UserFile.Size;
        public string Downloads => UserFile.Downloads;
        public bool IsDownloaded => UserFile.Downloaded;

        public BitmapImage MainImage
        {
            get => _mainImage;
            private set
            {
                _mainImage = value;
                OnPropertyChanged(nameof(MainImage));
            }
        }

        public ObservableCollection<Image> AllImages { get; }

        public void DownloadImages()
        {
            if (UserFile.Preview.Length == 0)
                return;
            AllImages.Clear();
            LoadAndSetImage(UserFile.Preview[0], path =>
            {
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    var image = new BitmapImage(new Uri(path, UriKind.Absolute));
                    MainImage = image;
                    AllImages.Add(new Image(){Source = MainImage});
                });
            });
            //for (var index = 1; index < _userFile.Preview.Length; index++)
            //{
            //    string s = _userFile.Preview[index];
            //    var localIndex = index;
            //    LoadAndSetImage(s, path =>
            //    {
            //        var bitmapImage = new BitmapImage(new Uri(path, UriKind.Absolute));
            //        AllImages.Add(new Image() { Source = bitmapImage });
            //    });
            //}
        }

        private async void LoadAndSetImage(string imageLink, Action<string> setAction)
        {
            var image = await _dataLoader.GetPreviewImage(imageLink, CancellationToken.None);
            setAction(image);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}