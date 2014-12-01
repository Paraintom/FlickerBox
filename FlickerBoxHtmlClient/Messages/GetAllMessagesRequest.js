var GetAllMessagesRequest = (function () {
    function GetAllMessagesRequest(FromTime) {
        this.FromTime = FromTime;
        this.Type = this.getName();
    }
    GetAllMessagesRequest.prototype.getName = function () {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec(this.constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };

    GetAllMessagesRequest.prototype.toJson = function () {
        var mapped = { Type: this.Type, FromTime: this.FromTime.getTime().toString() };
        var result = JSON.stringify(mapped);
        return result;
    };
    return GetAllMessagesRequest;
})();
//# sourceMappingURL=GetAllMessagesRequest.js.map
