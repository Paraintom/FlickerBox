class GetAllMessagesRequest {
    Type: string;
    constructor(public FromTime : Date) {
        this.Type = this.getName();
    }

    private getName() {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((<any> this).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    }

    public toJson() {
        var mapped = { Type: this.Type, FromTime: this.FromTime.getTime().toString() };
        var result = JSON.stringify(mapped);
        return result;
    }
}   