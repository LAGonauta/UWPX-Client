﻿using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XMPP_API.Classes;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI;
using UWP_XMPP_Client.Classes;
using Data_Manager2.Classes.DBTables;
using Data_Manager2.Classes.DBManager;
using UWP_XMPP_Client.Dialogs;
using UWP_XMPP_Client.Pages;
using UWP_XMPP_Client.Classes.Events;
using Data_Manager2.Classes;
using Microsoft.Toolkit.Uwp.UI.Controls;
using XMPP_API.Classes.Network.XML.Messages.XEP_0249;
using XMPP_API.Classes.Network.XML.Messages;
using Data_Manager2.Classes.Events;
using XMPP_API.Classes.Network.XML.Messages.XEP_0048;
using System.Collections.Generic;
using UWP_XMPP_Client.DataTemplates;
using Logging;

namespace UWP_XMPP_Client.Controls.Chat
{
    public sealed partial class ChatMasterControl : UserControl
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        public ChatTemplate ChatTemp
        {
            get { return (ChatTemplate)GetValue(ChatTempProperty); }
            set
            {
                ChatTemplate cur = (ChatTemplate)GetValue(ChatTempProperty);
                if (value != cur)
                {
                    if (cur != null)
                    {
                        cur.PropertyChanged -= Value_PropertyChanged;
                    }
                    if (value != null)
                    {
                        value.PropertyChanged += Value_PropertyChanged;
                    }
                    SetValue(ChatTempProperty, value);

                    onChatTemplateChanged(value);
                }
            }
        }
        public static readonly DependencyProperty ChatTempProperty = DependencyProperty.Register(nameof(ChatTemp), typeof(ChatTemplate), typeof(ChatMasterControl), new PropertyMetadata(null));

        private ChatTable Chat
        {
            get { return ChatTemp?.chat; }
            set { if (ChatTemp is null) { throw new InvalidOperationException("Can't set ChatTemp.chat - ChatTemp is null in ChatMasterControl."); } ChatTemp.chat = value; }
        }

        private XMPPClient Client
        {
            get { return ChatTemp?.client; }
            set { if (ChatTemp is null) { throw new InvalidOperationException("Can't set ChatTemp.client - ChatTemp is null in ChatMasterControl."); } ChatTemp.client = value; }
        }

        private MUCChatInfoTable MUCInfo
        {
            get { return ChatTemp?.mucInfo; }
            set { if (ChatTemp is null) { throw new InvalidOperationException("Can't set ChatTemp.MUCInfo - ChatTemp is null in ChatMasterControl."); } ChatTemp.mucInfo = value; }
        }

        private bool subscriptionRequest;
        private ChatMessageTable lastChatMessage;
        private MessageResponseHelper<IQMessage> updateBookmarkHelper;
        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        /// <summary>
        /// Basic Constructor
        /// </summary>
        /// <history>
        /// 27/08/2017 Created [Fabian Sauter]
        /// </history>
        public ChatMasterControl()
        {
            this.InitializeComponent();
            this.subscriptionRequest = false;
            this.lastChatMessage = null;
            this.updateBookmarkHelper = null;
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--
        private void onChatTemplateChanged(ChatTemplate chatTemp)
        {
            showClient(chatTemp.client);
            showChat(chatTemp.chat);
            showMUCInfo(chatTemp.mucInfo, chatTemp.chat);
        }

        private void showPresenceSubscriptionRequest()
        {
            accountAction_grid.Visibility = Visibility.Visible;
            accountAction_tblck.Text = Chat.chatJabberId + "  has requested to subscribe to your presence!";
            accountActionRefuse_btn.Content = "Refuse";
            accountActionAccept_btn.Content = "Accept";
            subscriptionRequest = true;
        }

        private void showRemovedChat()
        {
            accountAction_grid.Visibility = Visibility.Visible;
            accountAction_tblck.Text = Chat.chatJabberId + " has removed you from his roster and/or has unsubscribed you from his presence. Do you = to unsubscribe him from your presence?";
            accountActionRefuse_btn.Content = "Keep";
            accountActionAccept_btn.Content = "Remove";
            subscriptionRequest = false;
        }

        private void showMUCInfo(MUCChatInfoTable info, ChatTable chat)
        {
            if (info != null && chat != null)
            {
                // Chat jabber id:
                name_tblck.Text = string.IsNullOrWhiteSpace(info.name) ? chat.chatJabberId : info.name;

                // Menu Flyout:
                muteMUC_tmfo.Text = chat.muted ? "Unmute" : "Mute";
                muteMUC_tmfo.IsChecked = chat.muted;
                bookmark_tmfo.Text = chat.inRoster ? "Remove bookmark" : "Bookmark";

                //Slide list item:
                slideListItem_sli.LeftLabel = bookmark_tmfo.Text;
            }
        }

        private void showChat(ChatTable chat)
        {
            if (chat != null)
            {
                if (chat.chatType != ChatType.MUC)
                {
                    // Chat jabber id:
                    name_tblck.Text = chat.chatJabberId;

                    // Subscription state:
                    accountAction_grid.Visibility = Visibility.Collapsed;
                    requestPresenceSubscription_mfo.Visibility = Visibility.Collapsed;
                    cancelPresenceSubscription_mfo.Visibility = Visibility.Collapsed;
                    rejectPresenceSubscription_mfo.Visibility = Visibility.Collapsed;
                    probePresence_mfo.Visibility = Visibility.Collapsed;
                    presenceSubscription_mfo.IsEnabled = true;

                    switch (chat.subscription)
                    {
                        case "to":
                            cancelPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            probePresence_mfo.Visibility = Visibility.Visible;
                            break;
                        case "both":
                            cancelPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            rejectPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            probePresence_mfo.Visibility = Visibility.Visible;
                            break;
                        case "subscribe":
                            presenceSubscription_mfo.IsEnabled = false;
                            showPresenceSubscriptionRequest();
                            break;
                        case "unsubscribe":
                            requestPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            showRemovedChat();
                            break;
                        case "from":
                            requestPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            rejectPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            break;
                        case "none":
                        default:
                            requestPresenceSubscription_mfo.Visibility = Visibility.Visible;
                            break;
                    }

                    // Menu flyout:
                    mute_tmfo.Text = chat.muted ? "Unmute" : "Mute";
                    mute_tmfo.IsChecked = chat.muted;
                    removeFromRoster_mfo.Text = chat.inRoster ? "Remove from roster" : "Add to roster";

                    //Slide list item:
                    slideListItem_sli.LeftLabel = removeFromRoster_mfo.Text;
                }

                // Subscription pending:
                if (chat.subscriptionRequested)
                {
                    presence_tblck.Visibility = Visibility.Visible;
                    cancelPresenceSubscription_mfo.Visibility = Visibility.Visible;
                    requestPresenceSubscription_mfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    presence_tblck.Visibility = Visibility.Collapsed;
                }

                // Last action date:
                if (chat.lastActive != null)
                {
                    DateTime lastActiveLocal = chat.lastActive;
                    if (lastActiveLocal.Date.CompareTo(DateTime.Now.Date) == 0)
                    {
                        lastAction_tblck.Text = lastActiveLocal.ToString("HH:mm");
                    }
                    else
                    {
                        lastAction_tblck.Text = lastActiveLocal.ToString("dd.MM.yyyy");
                    }
                }
                else
                {
                    lastAction_tblck.Text = "";
                }

                // Last chat message:
                Task.Run(async () =>
                {
                    ChatMessageTable lastMsg = ChatDBManager.INSTANCE.getLastChatMessageForChat(chat.id);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => showLastChatMessage(lastMsg));
                });

                // Status icons:
                muted_tbck.Visibility = chat.muted ? Visibility.Visible : Visibility.Collapsed;
                inRooster_tbck.Visibility = chat.inRoster ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void showClient(XMPPClient client)
        {
            // Chat color:
            if (UiUtils.isHexColor(client.getXMPPAccount().color))
            {
                SolidColorBrush brush = UiUtils.convertHexStringToBrush(client.getXMPPAccount().color);
                brush.Opacity = 0.9;
                color_rcta.Fill = brush;
            }
            else
            {
                color_rcta.Fill = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void showLastChatMessage(ChatMessageTable chatMessage)
        {
            // Remove the event subscription for the last message:
            if (lastChatMessage != null)
            {
                lastChatMessage.ChatMessageChanged -= ChatMessage_ChatMessageChanged;
            }

            if (chatMessage != null)
            {
                lastChatMessage = chatMessage;
                chatMessage.ChatMessageChanged -= ChatMessage_ChatMessageChanged;
                chatMessage.ChatMessageChanged += ChatMessage_ChatMessageChanged;
                switch (chatMessage.state)
                {
                    case MessageState.UNREAD:
                        lastChat_tblck.Foreground = new SolidColorBrush((Color)Resources["SystemAccentColor"]);
                        break;
                    case MessageState.SENDING:
                    case MessageState.SEND:
                    case MessageState.READ:
                    default:
                        lastChat_tblck.Foreground = (SolidColorBrush)Resources["SystemControlBackgroundBaseMediumBrush"];
                        break;
                }

                if (lastChatMessage.isImage)
                {
                    lastChatIcon_tblck.Text = "\uE722";
                    lastChatIcon_tblck.Visibility = Visibility.Visible;
                    lastChat_tblck.Text = chatMessage.message ?? "You received an image";
                }
                else
                {
                    switch (lastChatMessage.type)
                    {
                        case DirectMUCInvitationMessage.TYPE_MUC_DIRECT_INVITATION:
                            lastChatIcon_tblck.Text = "\uE8F2";
                            lastChatIcon_tblck.Visibility = Visibility.Visible;
                            lastChat_tblck.Text = "You have been invited to a MUC room";
                            break;

                        case MessageMessage.TYPE_ERROR:
                            lastChatIcon_tblck.Text = "\xE7BA";
                            lastChatIcon_tblck.Visibility = Visibility.Visible;
                            lastChat_tblck.Text = chatMessage.message ?? "You received an error message";
                            break;

                        case MUCHandler.TYPE_CHAT_INFO:
                            lastChatIcon_tblck.Text = "\uE946";
                            lastChatIcon_tblck.Visibility = Visibility.Visible;
                            lastChat_tblck.Text = (chatMessage.message ?? "-");
                            break;

                        default:
                            lastChatIcon_tblck.Visibility = Visibility.Collapsed;
                            lastChat_tblck.Text = chatMessage.message ?? "";
                            break;
                    }
                }
            }
            else
            {
                lastChat_tblck.Text = "";
            }
        }

        private async void INSTANCE_NewChatMessage(ChatDBManager handler, NewChatMessageEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Chat.id.Equals(args.MESSAGE.chatId))
                {
                    showLastChatMessage(args.MESSAGE);
                }
            });
        }

        private async Task presenceSubscriptionRequestClickedAsync(bool accepted)
        {
            await Client.GENERAL_COMMAND_HELPER.answerPresenceSubscriptionRequestAsync(Chat.chatJabberId, accepted);
            Chat.subscription = accepted ? "to" : "none";
            ChatDBManager.INSTANCE.setChat(Chat, false, true);
        }

        private async Task switchChatInRoosterAsync()
        {
            if (Chat is null)
            {
                return;
            }

            if (Chat.inRoster)
            {
                await Client.GENERAL_COMMAND_HELPER.removeFromRosterAsync(Chat.chatJabberId).ConfigureAwait(false);
            }
            else
            {
                await Client.GENERAL_COMMAND_HELPER.addToRosterAsync(Chat.chatJabberId).ConfigureAwait(false);
            }
        }

        private async Task sendPresenceProbeAsync()
        {
            if (Chat is null)
            {
                return;
            }

            PresenceProbeMessage probe = new PresenceProbeMessage(Client.getXMPPAccount().getIdDomainAndResource(), Chat.chatJabberId);
            await Client.sendAsync(probe);
            Logger.Info("Send presence probe from " + probe.getFrom() + " to " + probe.getTo());
        }

        private void switchMUCBookmarkes()
        {
            if (MUCInfo != null && Chat != null)
            {
                Chat.inRoster = !Chat.inRoster;
                ChatDBManager.INSTANCE.setChatTableValue(nameof(Chat.id), Chat.id, nameof(Chat.inRoster), Chat.inRoster);
                setBookarks();
                showChat(Chat);
                showMUCInfo(MUCInfo, Chat);
            }
        }

        private void setBookarks()
        {
            List<ConferenceItem> conferences = MUCDBManager.INSTANCE.getXEP0048ConferenceItemsForAccount(Client.getXMPPAccount().getIdAndDomain());
            if (updateBookmarkHelper != null)
            {
                updateBookmarkHelper.Dispose();
            }
            updateBookmarkHelper = Client.PUB_SUB_COMMAND_HELPER.setBookmars_xep_0048(conferences, onSetBookmarkMessage, onSetBookmarkTimeout);
        }

        private void onSetBookmarkTimeout(MessageResponseHelper<IQMessage> helper)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                TextDialog dialog = new TextDialog("Failed to update bookmark!\nServer did not respond in time.", "Error");
                await UiUtils.showDialogAsyncQueue(dialog);
            }).AsTask();
        }

        private bool onSetBookmarkMessage(MessageResponseHelper<IQMessage> helper, IQMessage msg)
        {
            if (msg is IQErrorMessage errMsg)
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    TextDialog dialog = new TextDialog("Failed to bookmark!\nServer responded: " + errMsg.ERROR_OBJ.ERROR_NAME, "Error");
                    await UiUtils.showDialogAsyncQueue(dialog);
                }).AsTask();
                return true;
            }
            if (string.Equals(msg.TYPE, IQMessage.RESULT))
            {
                return true;
            }
            return false;
        }

        private async Task deleteChatAsync()
        {
            if (Chat is null)
            {
                return;
            }

            DeleteChatDialog dialog = new DeleteChatDialog();
            await UiUtils.showDialogAsyncQueue(dialog);
            if (dialog.deleteChat)
            {
                if (Chat.inRoster)
                {
                    if (Chat.chatType == ChatType.MUC)
                    {
                        if (Client != null && !Client.isConnected())
                        {
                            TextDialog errDialog = new TextDialog
                            {
                                Title = "Warning",
                                Text = "Unable to remove bookmark - account not connected!"
                            };
                            await UiUtils.showDialogAsyncQueue(errDialog);
                        }
                        else
                        {
                            // Remove bookmark:
                            switchMUCBookmarkes();
                            MUCDBManager.INSTANCE.setMUCChatInfo(MUCInfo, true, false);
                        }
                    }
                    else
                    {
                        await Client.GENERAL_COMMAND_HELPER.removeFromRosterAsync(Chat.chatJabberId).ConfigureAwait(false);
                    }
                }
                ChatDBManager.INSTANCE.setChat(Chat, true, true);
                if (!dialog.keepChatLog)
                {
                    ChatDBManager.INSTANCE.deleteAllChatMessagesForChat(Chat.id);
                }
            }
        }

        private async Task leaveRoomAsync()
        {
            if (Client != null && MUCInfo != null && Chat != null)
            {
                await MUCHandler.INSTANCE.leaveRoomAsync(Client, Chat, MUCInfo).ConfigureAwait(false);
            }
        }

        private async Task joinRoomAsync()
        {
            if (Client != null && MUCInfo != null && Chat != null)
            {
                await MUCHandler.INSTANCE.enterMUCAsync(Client, Chat, MUCInfo).ConfigureAwait(false);
            }
        }

        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--
        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (Chat != null && sender is Grid grid)
            {
                switch (Chat.chatType)
                {
                    case ChatType.CHAT:
                        chat_mfo.ShowAt(grid, e.GetPosition(grid));
                        break;
                    case ChatType.MUC:
                        muc_mfo.ShowAt(grid, e.GetPosition(grid));
                        break;
                    default:
                        break;
                }
            }
        }

        private void mute_tmfo_Click(object sender, RoutedEventArgs e)
        {
            if (Chat != null && Chat.muted != mute_tmfo.IsChecked)
            {
                Chat.muted = mute_tmfo.IsChecked;
                ChatDBManager.INSTANCE.setChat(Chat, false, true);
            }
        }

        private void muteMUC_tmfo_Click(object sender, RoutedEventArgs e)
        {
            if (Chat != null && Chat.muted != muteMUC_tmfo.IsChecked)
            {
                Chat.muted = muteMUC_tmfo.IsChecked;
                ChatDBManager.INSTANCE.setChat(Chat, false, true);
            }
        }

        private async void accountActionAccept_btn_Click(object sender, RoutedEventArgs e)
        {
            if (subscriptionRequest)
            {
                await presenceSubscriptionRequestClickedAsync(true).ConfigureAwait(false);
            }
            else
            {
                Chat.subscription = "none";
                ChatDBManager.INSTANCE.setChat(Chat, false, false);
            }
        }

        private async void accountActionRefuse_btn_Click(object sender, RoutedEventArgs e)
        {
            if (subscriptionRequest)
            {
                await presenceSubscriptionRequestClickedAsync(false).ConfigureAwait(false);
            }
            else
            {
                Chat.subscription = "none";
                ChatDBManager.INSTANCE.setChat(Chat, false, false);
            }
        }

        private async void deleteChat_mfo_Click(object sender, RoutedEventArgs e)
        {
            await deleteChatAsync().ConfigureAwait(false);
        }

        private async void removeFromRoster_mfo_Click(object sender, RoutedEventArgs e)
        {
            await switchChatInRoosterAsync().ConfigureAwait(false);
        }

        private async void requestPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {
            await Client.GENERAL_COMMAND_HELPER.requestPresenceSubscriptionAsync(Chat.chatJabberId);
            Chat.subscriptionRequested = true;
            ChatDBManager.INSTANCE.setChat(Chat, false, true);
        }

        private async void cancelPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {
            await Client.GENERAL_COMMAND_HELPER.unsubscribeFromPresenceAsync(Chat.chatJabberId);
            switch (Chat.subscription)
            {
                case "to":
                    Chat.subscription = "none";
                    break;

                case "both":
                    Chat.subscription = "from";
                    break;

                default:
                    break;
            }
            ChatDBManager.INSTANCE.setChat(Chat, false, true);
        }

        private async void rejectPresenceSubscription_mfo_Click(object sender, RoutedEventArgs e)
        {
            await Client.GENERAL_COMMAND_HELPER.answerPresenceSubscriptionRequestAsync(Chat.chatJabberId, false);
            switch (Chat.subscription)
            {
                case "from":
                    Chat.subscription = "none";
                    break;

                case "both":
                    Chat.subscription = "to";
                    break;

                default:
                    break;
            }
            ChatDBManager.INSTANCE.setChat(Chat, false, true);
        }

        private async void ChatMessage_ChatMessageChanged(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => showLastChatMessage(lastChatMessage));
        }

        private async void INSTANCE_ChatMessageChanged(ChatDBManager handler, ChatMessageChangedEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (Chat != null && lastChatMessage != null && Equals(args.MESSAGE.chatId, Chat.id) && Equals(lastChatMessage.id, args.MESSAGE.id))
                {
                    showLastChatMessage(args.MESSAGE);
                }
            });
        }

        private void showInfo_mfo_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(MUCInfoPage), new NavigatedToMUCInfoEventArgs(Chat, Client, MUCInfo));
        }

        private void showProfile_mfo_Click(object sender, RoutedEventArgs e)
        {
            (Window.Current.Content as Frame).Navigate(typeof(UserProfilePage), new NavigatedToUserProfileEventArgs(Chat, Client));
        }

        private void bookmark_tmfo_Click(object sender, RoutedEventArgs e)
        {
            switchMUCBookmarkes();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Subscribe to the chat message events:
            ChatDBManager.INSTANCE.ChatMessageChanged -= INSTANCE_ChatMessageChanged;
            ChatDBManager.INSTANCE.ChatMessageChanged += INSTANCE_ChatMessageChanged;
            ChatDBManager.INSTANCE.NewChatMessage -= INSTANCE_NewChatMessage;
            ChatDBManager.INSTANCE.NewChatMessage += INSTANCE_NewChatMessage;
        }

        private async void SlideListItem_sli_SwipeStatusChanged(SlidableListItem sender, SwipeStatusChangedEventArgs args)
        {
            if (args.NewValue == SwipeStatus.Idle)
            {
                if (args.OldValue == SwipeStatus.SwipingPassedLeftThreshold)
                {
                    await deleteChatAsync().ConfigureAwait(false);
                }
                else if (args.OldValue == SwipeStatus.SwipingPassedRightThreshold)
                {
                    if (Client != null && !Client.isConnected())
                    {
                        TextDialog dialog = new TextDialog
                        {
                            Title = "Error",
                            Text = "Account not connected!"
                        };
                        await UiUtils.showDialogAsyncQueue(dialog);
                    }
                    else
                    {
                        switch (Chat.chatType)
                        {
                            case ChatType.CHAT:
                                await switchChatInRoosterAsync().ConfigureAwait(false);
                                break;
                            case ChatType.MUC:
                                switchMUCBookmarkes();
                                break;
                        }
                    }
                }
            }
        }

        private void muc_mfo_Opening(object sender, object e)
        {
            if (MUCInfo != null)
            {
                switch (MUCInfo.state)
                {
                    case MUCState.ERROR:
                    case MUCState.DISCONNECTED:
                        join_mfo.Visibility = Visibility.Visible;
                        leave_mfo.Visibility = Visibility.Collapsed;
                        break;

                    case MUCState.DISCONNECTING:
                        join_mfo.Visibility = Visibility.Collapsed;
                        leave_mfo.Visibility = Visibility.Collapsed;
                        break;

                    case MUCState.ENTERING:
                    case MUCState.ENTERD:
                        join_mfo.Visibility = Visibility.Collapsed;
                        leave_mfo.Visibility = Visibility.Visible;
                        break;
                }
            }
            else
            {
                join_mfo.Visibility = Visibility.Collapsed;
                leave_mfo.Visibility = Visibility.Collapsed;
            }
        }

        private async void leave_mfo_Click(object sender, RoutedEventArgs e)
        {
            await leaveRoomAsync().ConfigureAwait(false);
        }

        private async void join_mfo_Click(object sender, RoutedEventArgs e)
        {
            await joinRoomAsync().ConfigureAwait(false);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (lastChatMessage != null)
            {
                lastChatMessage.ChatMessageChanged -= ChatMessage_ChatMessageChanged;
            }

            // Unsubscribe from the chat message events:
            ChatDBManager.INSTANCE.NewChatMessage -= INSTANCE_NewChatMessage;
            ChatDBManager.INSTANCE.ChatMessageChanged -= INSTANCE_ChatMessageChanged;
        }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ChatTemp is null)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(ChatTemplate.chat):
                    showChat(ChatTemp.chat);
                    showMUCInfo(ChatTemp.mucInfo, ChatTemp.chat);
                    break;

                case nameof(ChatTemplate.client):
                    showClient(ChatTemp.client);
                    break;

                case nameof(ChatTemplate.mucInfo):
                    showMUCInfo(ChatTemp.mucInfo, ChatTemp.chat);
                    break;

                default:
                    break;
            }
        }

        private async void ProbePresence_mfo_Click(object sender, RoutedEventArgs e)
        {
            await sendPresenceProbeAsync().ConfigureAwait(false);
        }
        #endregion
    }
}
