﻿using System;
using System.Collections.ObjectModel;
using UWP_XMPP_Client.DataTemplates;
using UWP_XMPP_Client.Pages.SettingsPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWP_XMPP_Client.Pages
{
    public sealed partial class SettingsPage : Page
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private ObservableCollection<Setting> settings { get; set; }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Construktoren--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 03/09/2017 Created [Fabian Sauter]
        /// </history>
        public SettingsPage()
        {
            this.InitializeComponent();
            Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += AbstractBackRequestPage_BackRequested;
            loadSettingsCategories();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void loadSettingsCategories()
        {
            settings = new ObservableCollection<Setting>()
            {
                new Setting() {icon = "\xE13D", name = "Accounts", description = "Manage Accounts", page = typeof(AccountSettingsPage)},
                new Setting() {icon = "\xE771", name = "Personalize", description = "Background, Color", page = typeof(PersonalizeSettingsPage)},
                new Setting() {icon = "\xE12B", name = "Data", description = "Mobile Data, Wifi", page = typeof(DataSettingsPage)},
                new Setting() {icon = "\xE15F", name = "Chat", description = "Availability", page = typeof(ChatSettingsPage)},
                new Setting() {icon = "\xE71D", name = "Background Tasks", description = "Manage Tasks", page = typeof(BackgroundTasksSettingsPage)},
                new Setting() {icon = "\xE713", name = "Misc", description = "Everything Else", page = typeof(MiscSettingsPage)},
            };
        }

        private void navigateToPage(Type pageType)
        {
            (Window.Current.Content as Frame).Navigate(pageType);
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(e.ClickedItem != null && e.ClickedItem is Setting)
            {
                navigateToPage((e.ClickedItem as Setting).page);
            }            
        }

        private void AbstractBackRequestPage_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                return;
            }
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        #endregion
    }
}