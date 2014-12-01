var StateChange = (function () {
    function StateChange(Id, State) {
        this.Id = Id;
        this.State = State;
        this.Type = "Ack";
    }
    StateChange.prototype.getName = function () {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec(this.constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };

    StateChange.prototype.toJson = function () {
        var mapped = { Type: this.Type, Id: this.Id, State: AckState[this.State] };
        var result = JSON.stringify(mapped);
        return result;
    };
    return StateChange;
})();

var AckState;
(function (AckState) {
    AckState[AckState["Delivered"] = 0] = "Delivered";
    AckState[AckState["Read"] = 1] = "Read";
    AckState[AckState["Error"] = 2] = "Error";
    AckState[AckState["Received"] = 3] = "Received";
    AckState[AckState["Sent"] = 4] = "Sent";
})(AckState || (AckState = {}));
//# sourceMappingURL=StateChange.js.map
