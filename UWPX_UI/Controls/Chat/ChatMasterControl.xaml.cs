﻿using UWPX_UI_Context.Classes.DataTemplates;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UWPX_UI.Controls.Chat
{
    public sealed partial class ChatMasterControl : UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public ChatDataTemplate Chat
        {
            get { return (ChatDataTemplate)GetValue(ChatProperty); }
            set { SetValue(ChatProperty, value); }
        }
        public static readonly DependencyProperty ChatProperty = DependencyProperty.Register(nameof(ChatDataTemplate), typeof(ChatDataTemplate), typeof(ChatMasterControl), new PropertyMetadata(null));

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public ChatMasterControl()
        {
            this.InitializeComponent();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


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
        #region --Presence--
        private void RequestPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RejectPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProbePresence_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
        private void UserControl_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {

        }

        private void Mute_tmfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveFromRoster_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteChat_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowInfo_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Join_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Leave_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Bookmark_tmfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MuteMUC_tmfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteMUC_mfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AccountActionAccept_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AccountActionRefuse_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void SlideListItem_sli_SwipeStatusChanged(Microsoft.Toolkit.Uwp.UI.Controls.SlidableListItem sender, Microsoft.Toolkit.Uwp.UI.Controls.SwipeStatusChangedEventArgs args)
        {

        }

        private void Muc_mfo_Opening(object sender, object e)
        {

        }

        private void ShowProfile_mfo_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
