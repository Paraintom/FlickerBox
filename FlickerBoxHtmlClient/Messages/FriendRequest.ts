class FriendRequest {
    Type: string;
    constructor(public Name: string, public Passphrase: string) {
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