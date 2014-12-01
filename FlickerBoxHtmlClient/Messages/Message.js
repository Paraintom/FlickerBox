var Message = (function () {
    function Message(ToFriendName, FromFriendName, FromPublicId, Id, UtcCreationTime, Content, State) {
        this.ToFriendName = ToFriendName;
        this.FromFriendName = FromFriendName;
        this.FromPublicId = FromPublicId;
        this.Id = Id;
        this.UtcCreationTime = UtcCreationTime;
        this.Content = Content;
        this.State = State;
        this.Type = this.getName();
    }
    Message.prototype.getName = function () {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec(this.constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };

    Message.prototype.toJson = function () {
        var mapped = {
            Type: this.Type,
            ToFriendName: this.ToFriendName,
            FromFriendName: this.FromFriendName,
            FromPublicId: this.FromPublicId,
            Id: this.Id,
            //We need this mapped for that :
            UtcCreationTime: this.UtcCreationTime.getTime().toString(),
            Content: this.Content
        };
        var result = JSON.stringify(mapped);
        return result;
    };
    return Message;
})();
//# sourceMappingURL=Message.js.map
