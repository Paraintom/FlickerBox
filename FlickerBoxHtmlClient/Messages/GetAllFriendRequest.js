var GetAllFriendRequest = (function () {
    function GetAllFriendRequest() {
        this.Type = this.getName();
    }
    GetAllFriendRequest.prototype.getName = function () {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec(this.constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };

    GetAllFriendRequest.prototype.toJson = function () {
        var result = JSON.stringify(this);
        return result;
    };
    return GetAllFriendRequest;
})();
//# sourceMappingURL=GetAllFriendRequest.js.map
