﻿using Microsoft.UI.Xaml.Controls;
using Project2FA.Repository.Models;
using Project2FA.Services;
using Project2FA.Services.Enums;
using Project2FA.ViewModels;
using System;
using System.Linq;
using UNOversal.Extensions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Project2FA.UWP.Views
{
    public sealed partial class ImportBackupContentDialog : ContentDialog
    {
        public ImportBackupContentDialogViewModel ViewModel => DataContext as ImportBackupContentDialogViewModel;
        public ImportBackupContentDialog()
        {
            this.InitializeComponent();
            this.Loaded += ImportAccountContentDialog_Loaded;

            // register the change of the input
            MainPivot.RegisterPropertyChangedCallback(TagProperty, PivotItemChangedCallback);
        }

        private void PivotItemChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == Pivot.TagProperty)
            {
                if (((Pivot)sender).Tag is string tag)
                {
                    if (tag == "PI_ImportAccountList")
                    {
                        RemovePivotItems(PI_ImportAccountList);
                        tag = string.Empty;
                    }
                }
            }
        }

        private void ImportAccountContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            switch (SettingsService.Instance.AppTheme)
            {
                case Theme.System:
                    if (RequestedTheme != SettingsService.Instance.OriginalAppTheme.ToElementTheme())
                    {
                        RequestedTheme = SettingsService.Instance.OriginalAppTheme.ToElementTheme();
                    }
                    break;
                case Theme.Dark:
                    if (RequestedTheme != ElementTheme.Dark)
                    {
                        RequestedTheme = ElementTheme.Dark;
                    }
                    break;
                case Theme.Light:
                    if (RequestedTheme != ElementTheme.Light)
                    {
                        RequestedTheme = ElementTheme.Light;
                    }
                    break;
                default:
                    break;
            }

            // remove PivotItems
            RemovePivotItems(null);

            // initialize the camera control
            ViewModel.MediaPlayerElementControl = CameraPlayerElement;
        }

        private void RemovePivotItems(UIElement element)
        {
            switch (element)
            {
                case PivotItem item when item.Name == nameof(PI_ImportBackupFile):
                    if (!MainPivot.Items.Contains(PI_ImportBackupFile))
                    {
                        MainPivot.Items.Add(PI_ImportBackupFile);
                    }
                    if (MainPivot.Items.Contains(PI_ImportBackupCamera))
                    {
                        MainPivot.Items.Remove(PI_ImportBackupCamera);
                    }
                    if (MainPivot.Items.Contains(PI_ImportAccountList))
                    {
                        MainPivot.Items.Remove(PI_ImportAccountList);
                    }
                    ViewModel.LastPivotItemName = nameof(PI_ImportBackupFile);
                    break;
                case PivotItem item when item.Name == nameof(PI_ImportBackupCamera):
                    if (MainPivot.Items.Contains(PI_ImportBackupFile))
                    {
                        MainPivot.Items.Remove(PI_ImportBackupFile);
                    }
                    if (!MainPivot.Items.Contains(PI_ImportBackupCamera))
                    {
                        MainPivot.Items.Add(PI_ImportBackupCamera);
                    }
                    if (MainPivot.Items.Contains(PI_ImportAccountList))
                    {
                        MainPivot.Items.Remove(PI_ImportAccountList);
                    }
                    ViewModel.LastPivotItemName = nameof(PI_ImportBackupCamera);
                    break;
                case PivotItem item when item.Name == nameof(PI_ImportAccountList):
                    if (MainPivot.Items.Contains(PI_ImportBackupFile) && ViewModel.LastPivotItemName != "PI_ImportBackupFile")
                    {
                        MainPivot.Items.Remove(PI_ImportBackupFile);
                    }
                    if (MainPivot.Items.Contains(PI_ImportBackupCamera))
                    {
                        MainPivot.Items.Remove(PI_ImportBackupCamera);
                    }
                    if (!MainPivot.Items.Contains(PI_ImportAccountList))
                    {
                        MainPivot.Items.Add(PI_ImportAccountList);
                    }
                    // Check if the last pivot item was the camera scan
                    if (ViewModel.LastPivotItemName == "PI_ImportBackupCamera")
                    {
                        ViewModel.SelectedPivotIndex = 1;
                        ViewModel.SetPrimaryBTNStatus();
                    }
                    ViewModel.LastPivotItemName = nameof(PI_ImportAccountList);
                    break;
                default:
                    if (MainPivot.Items.Contains(PI_ImportBackupFile))
                    {
                        MainPivot.Items.Remove(PI_ImportBackupFile);
                    }
                    if (MainPivot.Items.Contains(PI_ImportBackupCamera))
                    {
                        MainPivot.Items.Remove(PI_ImportBackupCamera);
                    }
                    if (MainPivot.Items.Contains(PI_ImportAccountList))
                    {
                        MainPivot.Items.Remove(PI_ImportAccountList);
                    }
                    ViewModel.LastPivotItemName = string.Empty;
                    break;
            }
        }

        private void HLBTN_QRCodeInfo(object sender, RoutedEventArgs e)
        {
            TeachingTip teachingTip = new TeachingTip
            {
                Target = sender as FrameworkElement,
                MaxWidth = 400,
                Subtitle = Strings.Resources.AddAccountCodeContentDialogQRCodeHelp,
                IsLightDismissEnabled = true,
                BorderBrush = new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"]),
                IsOpen = true
            };
            RootGrid.Children.Add(teachingTip);
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TwoFACodeModel model)
            {
                if (model.IsEnabled)
                {
                    model.IsChecked = !model.IsChecked;
                    ViewModel.SetPrimaryBTNStatus();
                }
            }
        }

        private void BTN_QRCodeCameraScan_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == nameof(BTN_QRCodeCameraScan))
            {
                RemovePivotItems(PI_ImportBackupCamera);
                if (MainPivot.Items.Count > 1)
                {
                    ViewModel.SelectedPivotIndex = 1;
                }
            }
        }

        private async void BTN_FileImport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Name == nameof(BTN_FileImport))
            {
                RemovePivotItems(PI_ImportBackupFile);
                var file = await ViewModel.FileImportCommandTask();
                if (file != null && MainPivot.Items.Count > 1)
                {
                    ViewModel.SelectedPivotIndex = 1;
                }
                else
                {
                    // remove PivotItems
                    RemovePivotItems(null);
                }
            }
        }

        private async void BTN_ConfirmFileImport_Click(object sender, RoutedEventArgs e)
        {
            var successful = await ViewModel.ConfirmImportTask();
            if (successful)
            {
                RemovePivotItems(PI_ImportAccountList);
                if (MainPivot.Items.Count > 2)
                {
                    ViewModel.SelectedPivotIndex = 2;
                }
                ViewModel.SetPrimaryBTNStatus();
            }
            else
            {
                RemovePivotItems(PI_ImportBackupFile);
            }
        }



        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SetPrimaryBTNStatus();
        }
    }
}
