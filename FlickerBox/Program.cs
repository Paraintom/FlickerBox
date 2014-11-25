using System;
using System.Collections.Generic;
using System.Threading;
using FlickerBox.ClientInteraction;
using FlickerBox.Communication;
using FlickerBox.Directory;
using FlickerBox.Identity;
using Message = FlickerBox.Messages.Message;

namespace FlickerBox
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            IChannelFactory channelFactory = new ChannelFactory();
            var identityManager = new IdentityManager();
            var commandListener = new CommandListener(identityManager, channelFactory);
            IClientCommandHandler handler = new ClientCommandHandler(identityManager, channelFactory);
            //Hook up events from client to all friends
            commandListener.OnFlagMessageRead += ((sender, ack) => handler.AcknowledgeRead(ack));
            commandListener.OnFriendRequestReceived += (sender, request) => handler.Discover(request);
            commandListener.OnGetAllFriendsReceived += (sender, request) => handler.ResendAllFriends();
            commandListener.OnGetAllMessagesFromReceived += (sender, command) => handler.ResendMessages(command.FromTime.FromJavascriptTicks());
            commandListener.OnMessageToSendReceived += (sender, message) => handler.Send(message);
            //Hook up events from friends to client
            handler.OnFriendToSend += (sender, friend) => commandListener.SendFriends(new List<Friend>() { friend });
            handler.OnReceived += (sender, message) => commandListener.SendMessages(new List<Message>() { message });
            handler.OnAcknowledged += (sender, ack) => commandListener.SendStateChanged(ack);
            while (true)
            {
                Thread.Sleep(300000);
                NLog.LogManager.GetCurrentClassLogger().Info("I am still alive!(Providing some infos here...)");
            }
        }
    }
}
