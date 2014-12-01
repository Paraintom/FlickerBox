class GetAllFriendRequest {
    Type: string;
    constructor() {
        this.Type = this.getName();
    }

    private getName() {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((<any> this).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    }

    public toJson() {
        var result = JSON.stringify(this);
        return result;
    }
}  