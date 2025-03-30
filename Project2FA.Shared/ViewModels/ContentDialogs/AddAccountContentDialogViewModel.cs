﻿using System.Threading.Tasks;
using UNOversal.Services.Dialogs;
using UNOversal.Services.Serialization;
using UNOversal.Logging;
using UNOversal.Services.File;
using Project2FA.Services.Parser;
using Project2FA.Services;
using System.Linq;
using System.Collections.ObjectModel;
using Project2FA.Repository.Models;
using Project2FA.Core.Utils;
using System.Collections.Generic;

#if WINDOWS_UWP
using Project2FA.UWP;
#else
using Project2FA.Uno;
using Microsoft.UI.Xaml.Data;
#endif

namespace Project2FA.ViewModels
{
#if !WINDOWS_UWP
    [Bindable]
#endif

    /// <summary>
    /// View model for adding an account countent dialog
    /// </summary>
    public class AddAccountContentDialogViewModel : AddAccountViewModelBase, IDialogInitialize
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public AddAccountContentDialogViewModel(
            ISerializationService serializationService,
            ILoggerFacade loggerFacade,
            IProject2FAParser project2FAParser): base()
        {
            SerializationService = serializationService;
            Logger = loggerFacade;
            Project2FAParser = project2FAParser;

            //ErrorsChanged += Validation_ErrorsChanged;


        }

        public void Initialize(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<List<KeyValuePair<string, string>>>("account", out var account))
            {
                ParseQRCode(account);
                PivotViewSelectionName = "NormalInputAccount";
            }
            OTPList.Clear();
            if (DataService.Instance.GlobalCategories != null && DataService.Instance.GlobalCategories.Count > 0)
            {
                GlobalTempCategories.Clear();
                for (int i = 0; i < DataService.Instance.GlobalCategories.Count; i++)
                {
                    GlobalTempCategories.Add((CategoryModel)DataService.Instance.GlobalCategories[i].Clone());
                }
            }
        }


#if WINDOWS_UWP
        public bool IsProVersion
        {
            get => SettingsService.Instance.IsProVersion;
        }
#endif
    }
}
