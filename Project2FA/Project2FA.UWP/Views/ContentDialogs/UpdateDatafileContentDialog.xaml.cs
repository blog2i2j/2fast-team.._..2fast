﻿using Project2FA.Services;
using Project2FA.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Project2FA.UWP.Views
{
    public sealed partial class UpdateDatafileContentDialog : ContentDialog
    {
        public UpdateDatafileContentDialogViewModel ViewModel => DataContext as UpdateDatafileContentDialogViewModel;   

        /// <summary>
        /// Constructor
        /// </summary>
        public UpdateDatafileContentDialog()
        {
            this.InitializeComponent();
            SetPivotItem();
        }

        private void SetPivotItem()
        {
            if(SettingsService.Instance.DataFileWebDAVEnabled)
            {
                MainPivot.Items.Remove(FolderPivotItem);
            }
            else
            {
                MainPivot.Items.Remove(WebDAVPivotItem);
                MainPivot.Items.Remove(WebDAVFolderPivotItem);
            }
        }

        /// <summary>
        /// Handle click on submit button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //first check if the given password matches with the datafile
            if (await ViewModel.TestPassword())
            {
                ViewModel.UpdateLocalFileSettings();
                Hide();
            }
            else
            {
                args.Cancel = true;
            }
        }

        private async void BTN_ChangeDatafile_Click(object sender, RoutedEventArgs e)
        {
            var result = await ViewModel.SetLocalFile(true);
            if (result)
            {
                PB_LocalPassword.Focus(FocusState.Programmatic);
            }

        }
    }
}
