using System;
using System.Collections.Generic;
using FlickerBox.Communication;
using FlickerBox.Directory;
using FlickerBox.Messages;

namespace FlickerBox.ClientInteraction
{
    public interface IClientCommandHandler : IFriendDirectory, IMessagesManager
    {
        List<Message> GetAllMessage(DateTime since);
    }
}
