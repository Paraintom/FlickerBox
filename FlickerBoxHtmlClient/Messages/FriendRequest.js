var FriendRequest = (function () {
    function FriendRequest(Name, Passphrase) {
        this.Name = Name;
        this.Passphrase = Passphrase;
        this.Type = this.getName();
    }
    FriendRequest.prototype.getName = function () {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec(this.constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };

    FriendRequest.prototype.toJson = function () {
        var result = JSON.stringify(this);
        return result;
    };
    return FriendRequest;
})();
//# sourceMappingURL=FriendRequest.js.map
