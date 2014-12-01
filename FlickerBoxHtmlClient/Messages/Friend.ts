class Friend {
    Messages : Message[];
    //Type: string;
    constructor(public Name: string) {
        //Useless as we do not plan to SEND it...
         //this.Type = this.getName();
        this.Messages = [];
    }

    /*private getName() {
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((<any> this).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    }*/
 }