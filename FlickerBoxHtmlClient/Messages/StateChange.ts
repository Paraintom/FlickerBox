class StateChange {
    Type: string;
    constructor(public Id: string, public State: AckState) {
        this.Type = "Ack";
    }

    private getName() {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((<any> this).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    }

    public toJson() {
        var mapped = { Type:this.Type, Id: this.Id, State: AckState[this.State] };
        var result = JSON.stringify(mapped);
        return result;
    }
}

enum AckState {
    Delivered,
    Read,
    Error,
    Received,
    Sent
}