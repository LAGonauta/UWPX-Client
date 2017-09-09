﻿using System;
using Data_Manager.Classes;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP_API.Classes;
using Data_Manager.Classes.Managers;

namespace UWP_XMPP_Client.Controls
{
    public sealed partial class AddChatContentDialog : ContentDialog
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public bool addToRooster;
        public bool requestSubscription;
        public string jabberId;
        public bool cancled;
        public XMPPClient client;
        private ObservableCollection<string> accounts;
        private ArrayList clients;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Construktoren--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 09/09/2017 Created [Fabian Sauter]
        /// </history>
        public AddChatContentDialog()
        {
            this.InitializeComponent();
            loadAccounts();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public void loadAccounts()
        {
            accounts = new ObservableCollection<string>();
            clients = ConnectionHandler.INSTANCE.getXMPPClients();
            foreach (XMPPClient c in clients)
            {
                accounts.Add(c.getSeverConnectionConfiguration().getIdAndDomain());
            }
        }

        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private async void add_btn_Click(object sender, RoutedEventArgs e)
        {
            if(account_cbx.SelectedIndex < 0 || account_cbx.SelectedIndex >= clients.Count)
            {
                MessageDialog messageDialog = new MessageDialog("Error")
                {
                   Content = "Please select a valid account!"
                };
                await messageDialog.ShowAsync();
            }
            else if(((string)account_cbx.SelectedItem).Equals(jabberId_tbx.Text))
            {
                MessageDialog messageDialog = new MessageDialog("Error")
                {
                    Content = "You can't start a chat with you selfe!"
                };
                await messageDialog.ShowAsync();
            }
            else if(Utils.isValidJabberId(jabberId_tbx.Text))
            {
                jabberId = jabberId_tbx.Text;
                client = (XMPPClient)clients[account_cbx.SelectedIndex];
                if (ChatManager.INSTANCE.doesChatExist(client, jabberId))
                {
                    MessageDialog messageDialog = new MessageDialog("Error")
                    {
                        Content = "Chat does already exist!"
                    };
                    await messageDialog.ShowAsync();
                }
                else
                {
                    addToRooster = (bool)rooster_cbx.IsChecked;
                    requestSubscription = (bool)subscription_cbx.IsChecked;
                    cancled = false;
                    Hide();
                }
            }
            else
            {
                MessageDialog messageDialog = new MessageDialog("Error")
                {
                    Content = "Invalid JabberID!"
                };
                await messageDialog.ShowAsync();
            }
        }

        private void cancle_btn_Click(object sender, RoutedEventArgs e)
        {
            cancled = true;
            Hide();
        }

        private void jabberId_tbx_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                e.Handled = true;
            }
        }

        #endregion
    }
}