class Message {
    Type: string;
    constructor(public ToFriendName: string,
        public FromFriendName: string,
        public FromPublicId: string, 
        public Id: string,
        public UtcCreationTime: Date,
        public Content: string,
        public State: string) {
        this.Type = this.getName();
    }

    private getName() {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((<any> this).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    }

    public toJson() {
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
    }
} 