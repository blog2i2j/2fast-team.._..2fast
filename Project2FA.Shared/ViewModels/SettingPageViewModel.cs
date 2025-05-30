﻿using System;
using Windows.Storage;
using System.Windows.Input;
using Project2FA.Strings;
using UNOversal.Ioc;
using Project2FA.Repository.Models;
using System.Linq;
using Project2FA.Core.Services.NTP;
using System.Threading.Tasks;
using Project2FA.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UNOversal.Navigation;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Secrets;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.Utils;
using CommunityToolkit.Mvvm.Collections;
using Windows.System;
using UNOversal.Services.Logging;
using Windows.Globalization;
using System.IO;
using Windows.Storage.Streams;
using UNOversal.Services.Serialization;
using System.Collections.Generic;

#if WINDOWS_UWP
using Project2FA.UWP.Services;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Project2FA.UWP;
using Project2FA.UWP.Views;
using Project2FA.Services.Marketplace;
using CommunityToolkit.Labs.WinUI.MarkdownTextBlock;
using Windows.Services.Store;
using Windows.Security.Credentials;
using Windows.ApplicationModel.Core;
#else
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Project2FA.Uno.Views;
using Project2FA.UnoApp;
using WinUIWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Project2FA.ViewModels
{
    /// <summary>
    /// Navigation part from the settings page
    /// </summary>
#if !WINDOWS_UWP
    [Bindable]
#endif
    public class SettingPageViewModel : ObservableObject, IInitialize, IConfirmNavigationAsync
    {
        public SettingsPartViewModel SettingsPartViewModel { get; }
        public AboutPartViewModel AboutPartViewModel { get; }
        public DatafilePartViewModel DatafilePartViewModel { get; }
        public INavigationService NavigationService { get; }
        private ISerializationService SerializationService { get; }

        public ILoggingService LoggingService { get; }

#if WINDOWS_UWP
        public IPurchaseAddOnService PurchaseAddOnService { get; }
#endif
        public ICommand RateAppCommand { get; }
        public ICommand SendMailCommand { get; }
        public ICommand NavigateBackCommand { get; }
        public ICommand InAppPaymentCommand { get; }
        public ICommand SeeSourceCodeCommand { get; }
        public ICommand ManageSubscriptionsCommand { get; }
        public ICommand BackCommand { get; }

        private int _selectedItem;
        private bool _activatedProVersion;
        public SettingPageViewModel(IDialogService dialogService, 
            ISecretService secretService,
            INavigationService navigationService,
            ILoggingService loggingService,
            ISerializationService serializationService)
        {
            
#if WINDOWS_UWP
            IMarketplaceService marketplaceService = App.Current.Container.Resolve<IMarketplaceService>();
#endif
            SettingsPartViewModel = new SettingsPartViewModel(dialogService);
            DatafilePartViewModel = new DatafilePartViewModel(dialogService, secretService);
#if WINDOWS_UWP
            AboutPartViewModel  = new AboutPartViewModel(marketplaceService,loggingService,serializationService);
            PurchaseAddOnService = new PurchaseAddOnService(loggingService);
#else
            AboutPartViewModel  = new AboutPartViewModel(loggingService,serializationService);
#endif
            RateAppCommand = new RelayCommand(() =>
            {
                AboutPartViewModel.RateApp();
            });

            SeeSourceCodeCommand = new AsyncRelayCommand(async() => 
            {
                Uri uri = new Uri("https://github.com/2fast-team/2fast");
                await Launcher.LaunchUriAsync(uri);
            });

            BackCommand = new AsyncRelayCommand(async() =>
            {
                await NavigationService.GoBackAsync();
            });

#if WINDOWS_UWP
            InAppPaymentCommand = new AsyncRelayCommand(async() =>
            {
                IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
                var inAppPaymentDialog = new InAppPaymentContentDialog();
                var result = await dialogService.ShowDialogAsync(inAppPaymentDialog, new DialogParameters());
                if (result == ContentDialogResult.Primary)
                {
                    var selectedPurchaseItem = inAppPaymentDialog.ViewModel.Items.Where(x => x.IsChecked == true).FirstOrDefault();
                    PurchaseAddOnService.Initialize(selectedPurchaseItem.StoreId);
                    (bool isActive, StoreLicense storeLicense) = await PurchaseAddOnService.SetupPurchaseAddOnInfoAsync();
                    var purchaseInfo = await PurchaseAddOnService.PromptUserToPurchaseAsync();
                    
                    if (purchaseInfo)
                    {
                        (bool newisActive, StoreLicense newstoreLicense) = await PurchaseAddOnService.SetupPurchaseAddOnInfoAsync();
                        SettingsService.Instance.IsProVersion = true;
                        ActivatedProVersion = true;
                        SettingsService.Instance.PurchasedStoreId = newstoreLicense.SkuStoreId;
                        SettingsService.Instance.LastCheckedInPurchaseAddon = DateTimeOffset.Now;
                        SettingsService.Instance.NextCheckedInPurchaseAddon = newstoreLicense.ExpirationDate;
                    }
                }
            });

            ManageSubscriptionsCommand = new AsyncRelayCommand(async() => 
            {
                Uri uri = new Uri("https://account.microsoft.com/services");
                await Launcher.LaunchUriAsync(uri);
            });
#endif
            NavigationService = navigationService;
            LoggingService = loggingService;
            SerializationService = serializationService;
            NavigateBackCommand = new AsyncRelayCommand(NavigateBackCommandTask);
            //SendMailCommand = new AsyncRelayCommand(SendMail);
        }

        public void Initialize(INavigationParameters parameters)
        {
            if (parameters.TryGetValue<int>("PivotItem", out int selectedItem))
            {
                SelectedItem = selectedItem;
            }

            if (parameters.TryGetValue<bool>("OpenInAppPayments", out bool openInAppPayments))
            {
                if (openInAppPayments)
                {
                    InAppPaymentCommand.Execute(null);
                }
            }
        }

        private async Task NavigateBackCommandTask()
        {
            await NavigationService.GoBackAsync();
        }

        //private async Task SendMail()
        //{
        //    EmailMessage emailMessage = new EmailMessage();
        //    emailMessage.To.Add(new EmailRecipient("app-2fast@outlook.com"));
        //    emailMessage.Subject = "Support 2fast";

        //    await EmailManager.ShowComposeNewEmailAsync(emailMessage);
        //}

        public async Task<bool> CanNavigateAsync(INavigationParameters parameters)
        {
            IDialogService dialogService = App.Current.Container.Resolve<IDialogService>();
            return !await dialogService.IsDialogRunning();
        }

        public int SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public bool ActivatedProVersion { get => _activatedProVersion; set => SetProperty(ref _activatedProVersion, value); }

    }

    /// <summary>
    /// Main content part of the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class SettingsPartViewModel : ObservableObject
    {
        private SettingsService _settings;
        private IDialogService DialogService { get; }
        private bool _isWindowsHelloSupported;
        private bool _manualNTPServerConfiurationChecked;
        private bool _progressIsIndeterminate;
        private string _ntpServerStr;
        private bool _ntpServerEditValid;
        private bool _ntpServerEditException;

        public ICommand MakeFactoryResetCommand { get; }
        public ICommand SaveNTPServerAddressCommand { get; }
        public ICommand OpenLogCommand { get; }

        /// <summary>
        /// View Model constructor
        /// </summary>
        /// <param name="dialogService"></param>
        public SettingsPartViewModel(IDialogService dialogService)
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                _settings = SettingsService.Instance;
            DialogService = dialogService;
            MakeFactoryResetCommand = new RelayCommand(MakeFactoryReset);
            SaveNTPServerAddressCommand = new RelayCommand(() =>
            {
                SettingsService.Instance.NTPServerString = _ntpServerStr;
                NtpServerEditValid = false;
            });
            OpenLogCommand = new AsyncRelayCommand(OpenLogCommandTask);

#if WINDOWS_UWP
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CheckWindowsHelloIsSupported();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#else
            //App.ShellPageInstance.ViewModel.TabBarIsVisible = false;
#endif

#if WINDOWS_UWP
            // DEBUG MDM settings
            //if (System.Diagnostics.Debugger.IsAttached)
            //{
            //    if (!ApplicationData.Current.LocalSettings.Containers.ContainsKey(Constants.EnterpriseAppManagementContainer))
            //    {
            //        Windows.Storage.ApplicationDataContainer container =
            //        ApplicationData.Current.LocalSettings.CreateContainer(Constants.EnterpriseAppManagementContainer, Windows.Storage.ApplicationDataCreateDisposition.Always);
            //    }
            //    ApplicationData.Current.LocalSettings.Containers[Constants.EnterpriseAppManagementContainer].Values[nameof(UseHiddenTOTP)] = "true";
            //    ApplicationData.Current.LocalSettings.Containers[Constants.EnterpriseAppManagementContainer].Values[nameof(UseAutoLogout)] = "true";
            //    ApplicationData.Current.LocalSettings.Containers[Constants.EnterpriseAppManagementContainer].Values["AutoLogoutMinutes"] = "5";
            //    ApplicationData.Current.LocalSettings.Containers[Constants.EnterpriseAppManagementContainer].Values["LoginScreenWallpaper"] = string.Empty;
            //}

            // DEBUG languages
            //var resourceContext = Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView();
            //for (int i = 0; i < resourceContext.Languages.Count; i++)
            //{
            //    if (resourceContext.Languages[i].ToLower().StartsWith("fr"))
            //    {
            //        ApplicationLanguages.PrimaryLanguageOverride = "fr";
            //        Windows.ApplicationModel.Resources.Core.ResourceContext.GetForCurrentView().Reset();
            //        Windows.ApplicationModel.Resources.Core.ResourceContext.GetForViewIndependentUse().Reset();
            //    }
            //}
            //resourceContext.Languages
#endif
        }

        /// <summary>
        /// Make a factory reset of the app
        /// </summary>
        private async void MakeFactoryReset()
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Title = Resources.SettingsFactoryResetDialogTitle;
#if !WINDOWS_UWP
            dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
#if WINDOWS_UWP
            MarkdownTextBlock markdown = new MarkdownTextBlock();
            markdown.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = markdown;
#else
            TextBlock txt = new TextBlock();
            txt.Text = Resources.SettingsFactoryResetMessage;
            dialog.Content = txt;
#endif
            dialog.PrimaryButtonText = Resources.No;
            dialog.SecondaryButtonText = Resources.Yes;
            dialog.PrimaryButtonStyle = App.Current.Resources["AccentButtonStyle"] as Style;
            ContentDialogResult result = await DialogService.ShowDialogAsync(dialog, new DialogParameters());

            switch (result)
            {
                case ContentDialogResult.Secondary:
                    var secretHelper = App.Current.Container.Resolve<ISecretService>();
                    //delete password in the secret vault
                    secretHelper.Helper.RemoveSecret(Constants.ContainerName, SettingsService.Instance.DataFilePasswordHash);
                    //remove WebDAV login
                    secretHelper.Helper.RemoveSecret(Constants.ContainerName, "WDServerAddress");
                    secretHelper.Helper.RemoveSecret(Constants.ContainerName, "WDUsername");
                    secretHelper.Helper.RemoveSecret(Constants.ContainerName, "WDPassword");
                    secretHelper.Helper.RemoveSecret(Constants.ContainerName, Constants.ActivatedDatafileHashName);

                    // reset data and restart app
#if WINDOWS_UWP
                    await ApplicationData.Current.ClearAsync();
                    await CoreApplication.RequestRestartAsync("Factory reset");
#endif
#if __IOS__
                    App.Current.Exit();
#endif

#if ANDROID
                    App.Current.Exit();
#endif
                    break;
                case ContentDialogResult.Primary:
                    break;
                case ContentDialogResult.None:
                    break;
                default:
                    break;
            }
        }

        private async Task OpenLogCommandTask()
        {
            // TODO if nessesary
            //// Set the recommended app
            //var options = new Windows.System.LauncherOptions();
            //options.PreferredApplicationPackageFamilyName = "Contoso.URIApp_8wknc82po1e";
            //options.PreferredApplicationDisplayName = "Contoso URI Ap";
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Constants.LogName);
                // Launch the URI and pass in the recommended app
                // in case the user has no apps installed to handle the URI
                var success = await Launcher.LaunchFileAsync(file);
            }
            catch (Exception)
            {
                var dialog = new ContentDialog();
#if !WINDOWS_UWP
                dialog.XamlRoot = WinUIWindow.Current.Content.XamlRoot;
#endif
                dialog.Title = Resources.SettingsPageNoLogDialogTitle;
                dialog.Content = Resources.SettingsPageNoLogDialogContent;
                dialog.PrimaryButtonText = Resources.Confirm;

                await DialogService.ShowDialogAsync(dialog, new DialogParameters());
            }
        }

#region Theme
        public Theme Theme
        {
            get => _settings.AppTheme;
            set
            {
                if (_settings.AppTheme.Equals(value))
                {
                    return;
                }

                _settings.AppTheme = value;
//#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
//                DataService.Instance.ReloadAccountIconSVGs();
//#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public bool ThemeAsLight
        {
            get => Theme.Equals(Theme.Light);
            set
            {
                if (value)
                {
                    Theme = Theme.Light;
                }
            }
        }

        public bool ThemeAsDark
        {
            get => Theme.Equals(Theme.Dark);
            set
            {
                if (value)
                {
                    Theme = Theme.Dark;
                }
            }
        }

        public bool ThemeAsSystem
        {
            get => Theme.Equals(Theme.System);
            set
            {
                if (value)
                {
                    Theme = Theme.System;
                }
            }
        }
#endregion

        public bool IsWindowsHelloSupported
        {
            get => _isWindowsHelloSupported;
            set => SetProperty(ref _isWindowsHelloSupported, value);
        }

        public bool ActivateWindowsHello
        {
            get => _settings.ActivateWindowsHello;
            set
            {
                _settings.ActivateWindowsHello = value;
                OnPropertyChanged(nameof(ActivateWindowsHello));

            }
        }

        public bool UseHiddenTOTP
        {
            get => _settings.UseHiddenTOTP;
            set
            {
                _settings.UseHiddenTOTP = value;
                OnPropertyChanged(nameof(UseHiddenTOTP));
            }
        }

        public bool PrideMonthDesign
        {
            get => _settings.PrideMonthDesign;
            set
            {
                _settings.PrideMonthDesign = value;
                OnPropertyChanged(nameof(PrideMonthDesign));
            }
        }

        public string NTPServerStr
        {
            get => _settings.NTPServerString;
            set
            {
                if (value != _settings.NTPServerString)
                {
                    CheckNTPServer(value);
                }
                else
                {
                    NtpServerEditValid = false;
                    NtpServerEditException = false;
                }
            }
        }
        private async Task CheckNTPServer(string newAddress)
        {
            ProgressIsIndeterminate = true;
            try
            {
                NtpServerEditException = false;
                await App.Current.Container.Resolve<INetworkTimeService>().GetNetworkTimeAsync(newAddress);
            }
            catch (Exception)
            {
                NtpServerEditException = true;
            }
            finally
            {
                if (!NtpServerEditException)
                {
                    NtpServerEditValid = true;
                    _ntpServerStr = newAddress;
                }
                else
                {
                    NtpServerEditValid = false;
                }
                ProgressIsIndeterminate = false;
            }
        }

        public bool ProgressIsIndeterminate
        {
            get => _progressIsIndeterminate;

            set => SetProperty(ref _progressIsIndeterminate, value);
        }

        public bool PreferWindowsHelloLogin
        {
            get
            {
                switch (_settings.PreferWindowsHello)
                {
                    case BiometricPreferEnum.None:
                    case BiometricPreferEnum.No:
                        return false;
                    case BiometricPreferEnum.Prefer:
                        return true;
                    default:
                        return false;
                }
            }
            set
            {
                _settings.PreferWindowsHello = value ? BiometricPreferEnum.Prefer : BiometricPreferEnum.No;
                OnPropertyChanged(nameof(PreferWindowsHelloLogin));
            }
        }


#if WINDOWS_UWP
        private async Task CheckWindowsHelloIsSupported()
        {
            IsWindowsHelloSupported = await KeyCredentialManager.IsSupportedAsync();
            if (!IsWindowsHelloSupported)
            {
                ActivateWindowsHello = false;
            }
        }
#endif

        public bool UseHeaderBackButton
        {
            get => _settings.UseHeaderBackButton;
            set
            {
                if (_settings.UseHeaderBackButton != value)
                {
                    _settings.UseHeaderBackButton = value;
                    OnPropertyChanged(nameof(UseHeaderBackButton));
#if WINDOWS_UWP
                    App.ShellPageInstance.SetupBackButton();
#endif
                }
            }
        }

        public bool UseRoundCorner
        {
            get => _settings.UseRoundCorner;
            set
            {
                if (_settings.UseRoundCorner != value)
                {
                    if (value)
                    {
                        App.Current.Resources["ControlCornerRadius"] = new CornerRadius(4, 4, 4, 4);
                        App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(8, 8, 8, 8);
                        App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(3);
                        App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(1.5);
                        App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(16,16,16,16);
                    }
                    else
                    {
                        App.Current.Resources["ControlCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["ComboBoxItemCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["ComboBoxItemPillCornerRadius"] = new CornerRadius(0);
                        App.Current.Resources["TokenItemCornerRadius"] = new CornerRadius(0);
                    }
                    _settings.UseRoundCorner = value;
                    OnPropertyChanged(nameof(UseRoundCorner));
                }
            }
        }

        public bool ShowAvailableProFeatures
        {
            get => _settings.ShowAvailableProFeatures;
            set
            {
                _settings.ShowAvailableProFeatures = value;
                OnPropertyChanged(nameof(ShowAvailableProFeatures));
            }
        }

        public int ThemeIndex
        {
            get => _settings.AppTheme switch
            {
                Theme.Dark => 1,
                Theme.Light => 0,
                Theme.System => 2,
                _ => 2,
            };

            set
            {
                switch (value)
                {
                    case 0: ThemeAsLight = true; break;
                    case 1: ThemeAsDark = true; break;
                    case 2: ThemeAsSystem = true; break;
                }
            }
        }

        public int LoggingIndex
        {
            get => _settings.LoggingSetting switch
            {
                LoggingPreferEnum.Simple => 1,
                LoggingPreferEnum.None => 0,
                LoggingPreferEnum.Full => 2,
                _ => 1,
            };

            set
            {
                switch (value)
                {
                    case 0:
                        _settings.LoggingSetting = LoggingPreferEnum.None; break;
                    case 1:
                        _settings.LoggingSetting = LoggingPreferEnum.Simple; break;
                    case 2:
                        _settings.LoggingSetting = LoggingPreferEnum.Full; break;
                }
            }
        }

        public int SetAutoLogoutMinutes
        {
            get => _settings.AutoLogoutMinutes;
            set
            {
                if (_settings.AutoLogoutMinutes != value)
                {
                    _settings.AutoLogoutMinutes = value;
                    OnPropertyChanged(nameof(SetAutoLogoutMinutes));
                }
            }
        }

        public bool ManualNTPServerConfiurationChecked 
        {
            get => _manualNTPServerConfiurationChecked;
            set => SetProperty(ref _manualNTPServerConfiurationChecked, value);
        }
        public bool NtpServerEditValid
        {
            get => _ntpServerEditValid;
            set => SetProperty(ref _ntpServerEditValid, value);
        }
        public bool NtpServerEditException 
        {
            get => _ntpServerEditException;
            set => SetProperty(ref _ntpServerEditException, value);
        }

        public bool UseNTPServerCorrection
        {
            get => _settings.UseNTPServerCorrection;
            set
            {
                _settings.UseNTPServerCorrection = value;
                if (!value)
                {
                    ManualNTPServerConfiurationChecked = false;
                }
                OnPropertyChanged(nameof(UseNTPServerCorrection));
            }
        }

        public bool UseAutoLogout
        {
            get => _settings.UseAutoLogout;
            set
            {
                _settings.UseAutoLogout = value;
                OnPropertyChanged(nameof(UseAutoLogout));
                //OnPropertyChanged(nameof(AutoLogoutMinutesIsMDMManaged));
            }
        }
#if WINDOWS_UWP
        public bool IsProVersion 
        {
            get => _settings.IsProVersion;
        }

        public bool IsNoProVersion
        {
            get => !_settings.IsProVersion;
        }

        public bool UseProFeatures
        {
            get => _settings.UseProFeatures;
            set
            {
                _settings.UseProFeatures = value;
                OnPropertyChanged(nameof(UseProFeatures));
            }
        }
#endif

        #region MDMConfigs
        public bool IsMDMActive
        {
            get => _settings.AutoLogoutMinutesIsMDMManaged ||
                _settings.UseAutoLogoutIsMDMManaged ||
                _settings.ActivateWindowsHelloIsMDMManaged ||
                _settings.UseNTPServerCorrectionIsMDMManaged ||
                _settings.NTPServerStringIsMDMManaged ||
                _settings.UseHiddenTOTPIsMDMManaged;
        }
        public bool AutoLogoutMinutesIsMDMManaged
        {
            get => !_settings.AutoLogoutMinutesIsMDMManaged;
        }

        public bool UseAutoLogoutIsMDMManaged
        {
            get => !_settings.UseAutoLogoutIsMDMManaged;
        }

        public bool ActivateWindowsHelloIsMDMManaged
        {
            get => !_settings.ActivateWindowsHelloIsMDMManaged;
        }

        public bool UseNTPServerCorrectionIsMDMManaged
        {
            get => !_settings.UseNTPServerCorrectionIsMDMManaged;
        }

        public bool NTPServerStringIsMDMManaged
        {
            get => !_settings.NTPServerStringIsMDMManaged;
        }

        public bool UseHiddenTOTPIsMDMManaged
        {
            get => !_settings.UseHiddenTOTPIsMDMManaged;
        }
        #endregion
    }


    /// <summary>
    /// Datafile tab from the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class DatafilePartViewModel : ObservableObject
    {
        private IDialogService _dialogService { get; }

        public ICommand ChangeDatafilePasswordCommand { get; }
        //public ICommand EditDatafileCommand { get; }
        public ICommand DeleteDatafileCommand { get; }
        ISecretService SecretService { get; }

        private string _datafilePath;
        private string _datafileName;
        private bool _notifyPasswordChanged;

        /// <summary>
        /// Datafile part view model constructor
        /// </summary>
        public DatafilePartViewModel(IDialogService dialogService, ISecretService secretService)
        {
            _dialogService = dialogService;
            SecretService = secretService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            InitializeDataFileAttributes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            // open content dialog to change the password
#pragma warning disable AsyncFixer03 // Fire-and-forget async-void methods or delegates
            ChangeDatafilePasswordCommand = new RelayCommand(async() => {
                var dialog = new ChangeDatafilePasswordContentDialog();
                var param = new DialogParameters();
                param.Add("isInvalid", false);
                if (NotifyPasswordChanged)
                {
                    NotifyPasswordChanged = false;
                }
                var result = await _dialogService.ShowDialogAsync(dialog, param);
                if (dialog.ViewModel.PasswordChanged)
                {
                    NotifyPasswordChanged = true;
                }
            });
#pragma warning restore AsyncFixer03 // Fire-and-forget async-void methods or delegates

            //EditDatafileCommand = new DelegateCommand(EditDatafile);
        }

        /// <summary>
        /// Initialize the attributes for the datafile part
        /// </summary>
        /// <returns></returns>
        private async Task InitializeDataFileAttributes()
        {
            if (DataService.Instance.ActivatedDatafile != null)
            {
                DatafilePath = DataService.Instance.ActivatedDatafile.Path;
                DatafileName = DataService.Instance.ActivatedDatafile.Name;
            }
            else
            {
                DatafileName = SettingsService.Instance.DataFileName;
                DatafilePath = SettingsService.Instance.DataFileWebDAVEnabled ? SecretService.Helper.ReadSecret(Constants.ContainerName, "WDServerAddress") + SettingsService.Instance.DataFilePath : SettingsService.Instance.DataFilePath;
            }
        }

        public string DatafilePath { get => _datafilePath; set => SetProperty(ref _datafilePath, value); }
        public string DatafileName { get => _datafileName; set => SetProperty(ref _datafileName, value); }
        public bool NotifyPasswordChanged { get => _notifyPasswordChanged; set => SetProperty(ref _notifyPasswordChanged, value); }
    }

    /// <summary>
    /// About tab from the settings page
    /// </summary>

#if !WINDOWS_UWP
    [Bindable]
#endif
    public class AboutPartViewModel : ObservableObject
    {
#if WINDOWS_UWP
        private IMarketplaceService _marketplaceService { get; }
#endif
        private ILoggingService LoggingService { get; }
        private ISerializationService SerializationService { get; }

        public AboutPartViewModel(
#if WINDOWS_UWP
            IMarketplaceService marketplaceService,
#endif
            ILoggingService loggingService, 
            ISerializationService serializationService)
        {
#if WINDOWS_UWP
            _marketplaceService = marketplaceService;
#endif
            LoggingService = loggingService;
            SerializationService = serializationService;
            LoadDependencyList();
        }

        private async Task LoadDependencyList()
        {
            try
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/JSONs/Dependencies.json"));
                IRandomAccessStreamWithContentType randomStream = await file.OpenReadAsync();
                using StreamReader r = new StreamReader(randomStream.AsStreamForRead());
                var depList = SerializationService.Deserialize<List<DependencyModel>>(await r.ReadToEndAsync());
                foreach (var item in depList)
                {
                    if (item.CategoryUid == "Package")
                    {
                        item.Category = Resources.SettingsDependencyGroupPackages;
                    }
                    else
                    {
                        item.Category = Resources.SettingsDependencyGroupAssets;
                    }
                }
                var grouped = depList.OrderBy(g => g.Name).GroupBy(x => x.Category);
                var contactsSource = new ObservableGroupedCollection<string, DependencyModel>(grouped);
                DependencyCollectionViewSource.Source = contactsSource;
                DependencyCollectionViewSource.IsSourceGrouped = true;
            }
            catch (Exception exc)
            {
                await LoggingService.LogException(exc, SettingsService.Instance.LoggingSetting);
            }

        }
        public CollectionViewSource DependencyCollectionViewSource { get; } = new CollectionViewSource();
        public Uri Logo => Windows.ApplicationModel.Package.Current.Logo;

        public string DisplayName => Strings.Resources.ApplicationName;

        //public string Publisher => Windows.ApplicationModel.Package.Current.PublisherDisplayName;

        public string Version
        {
            get
            {
                var ver = Windows.ApplicationModel.Package.Current.Id.Version;
                return ver.Major.ToString() + "." + ver.Minor.ToString() + "." + ver.Build.ToString() + "." + ver.Revision.ToString();
            }
        }
        public async void GiveFeedback()
        {
#if WINDOWS_UWP
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
#endif
        }

        public Task RateApp()
        {
#if WINDOWS_UWP
            return _marketplaceService.LaunchAppReviewInStoreAsync();
#else
// TODO Android and IOS
            return Task.CompletedTask;
#endif
        }
    }
}
