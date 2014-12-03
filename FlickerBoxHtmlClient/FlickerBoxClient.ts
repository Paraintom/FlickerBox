/// <reference path="FastFlickerClient.ts" />
//                              Enjoy!
class FlickerBoxClient {
    privateId: string;
    fastFlickerClient: FastFlickerClient;

    //Events
    private onErrorEvent = new LiteEvent<string>();
    public get onError(): ILiteEvent<string> { return this.onErrorEvent; }

    constructor(privateId: string) {
        this.privateId = privateId;
    }

    onFastFlickerError(event: ErrorEvent) {
        this.onErrorEvent.raise(event.message);
    }

    private isArray(what: string) {
        return Object.prototype.toString.call(what) === '[object Array]';
    }
    private onFastFlickerMessage(message: string) {
        try {
        var jsonParsed = JSON.parse(message);
            //If it is an array, we iterate!
            if (this.isArray(jsonParsed)) {
                for (var i = 0, len = jsonParsed.length; i < len; ++i) {
                    var element = jsonParsed[i];
                    this.onFastFlickerMessage(JSON.stringify(element));
                }
                return;
            }
            var type = jsonParsed["Type"];
            switch (type) {
                case "Friend":
                    var name = jsonParsed["Name"];
                    this.onFriendEvent.raise(new Friend(name));
                    break;
                case "Message":
                    var toFriendName = jsonParsed["ToFriendName"];
                    var fromPublicId = jsonParsed["FromPublicId"];
                    var fromFriendName = jsonParsed["FromFriendName"];
                    var id = jsonParsed["Id"];
                    var utcCreationTimeString = jsonParsed["UtcCreationTime"];
                    var utcCreationTimeNumber = +utcCreationTimeString;
                    var utcCreationTime = new Date(utcCreationTimeNumber);
                    var content = jsonParsed["Content"];
                    var state1 = fromFriendName === "" ? AckState[AckState.Sent] : AckState[AckState.Received];
                    var newMessage = new Message(toFriendName, fromFriendName, fromPublicId, id, utcCreationTime, content, state1);
                    this.onMessageEvent.raise(newMessage);
                    break;
                case "Ack":
                    var idMessage = jsonParsed["Id"];
                    var stateString: string = jsonParsed["State"];
                    var state: AckState = <AckState>AckState[stateString];
                    var stateChange = new StateChange(idMessage, state);
                    this.onStateChangeEvent.raise(stateChange);
                    break;
                case "FriendList":
                    var list = jsonParsed["Result"];
                    for (var friend in list) {
                        var friendName = list[friend]["Name"];
                        this.onFriendEvent.raise(new Friend(friendName));
                    }
                    break;
                default:
                    this.onErrorEvent.raise("Message received not handled by the application : " + message);
                    break;
            }


        } catch (e) {
            this.onErrorEvent.raise("Exception while parsing message: " + message + ", " + e.message);

        }
    }
    //################ Connection - Disconnection ########################
    private onBoxConnectionEvent = new LiteEvent<void>();
    public get onConnected(): ILiteEvent<void> { return this.onBoxConnectionEvent; }
    private onBoxDisconnectionEvent = new LiteEvent<void>();
    public get onDisconnected(): ILiteEvent<void> { return this.onBoxDisconnectionEvent; }

     public connect(fastFlickerUrl: string) {
         if (this.fastFlickerClient != null) {
             //closing the existing connection		
             this.fastFlickerClient.onConnected.unsubscribe(() => { this.onFastFlickerConnected(); });
             this.fastFlickerClient.onDisconnected.unsubscribe(() => { this.onFastFlickerDisconnected(); });
             this.fastFlickerClient.onError.unsubscribe((m) => { this.onFastFlickerError(m); });
             this.fastFlickerClient.onMessage.unsubscribe((m) => { this.onFastFlickerMessage(m); });
         }
         this.fastFlickerClient = new FastFlickerClient(fastFlickerUrl);
         this.fastFlickerClient.onConnected.subscribe(() => { this.onFastFlickerConnected(); });
         this.fastFlickerClient.onDisconnected.subscribe(() => { this.onFastFlickerDisconnected(); });
         this.fastFlickerClient.onError.subscribe((e) => { this.onFastFlickerError(e); });
         this.fastFlickerClient.onMessage.subscribe((m) => { this.onFastFlickerMessage(m); });

         this.fastFlickerClient.listenTo(this.privateId);
         //onFastFlickerMessage
     }

    private onFastFlickerConnected() {
        this.onBoxConnectionEvent.raise();
        //Then ... that's it?
    }

    private onFastFlickerDisconnected() {
        this.onBoxDisconnectionEvent.raise();
        //Then ... that's it?
    }
    //#####################################################################
    //################ Friends Management #################################

    private onFriendEvent = new LiteEvent<Friend>();
    public get onFriend(): ILiteEvent<Friend> { return this.onFriendEvent; }

    public getAllFriends() {
        var request = new GetAllFriendRequest();
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    }

    public isConnected() {
        return this.fastFlickerClient != null
            && this.fastFlickerClient.isConnected();
    }

    public addFriend(name: string, passphrase: string) {
        var request = new FriendRequest(name, passphrase);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    }
    //#####################################################################
    //################ Message Handling ###################################

    private onMessageEvent = new LiteEvent<Message>();
    public get onMessage(): ILiteEvent<Message> { return this.onMessageEvent; } 

    private onStateChangeEvent = new LiteEvent<StateChange>();
    public get onStateChange(): ILiteEvent<StateChange> { return this.onStateChangeEvent; } 

    public ackRead(id: string) {
        var stateChange = new StateChange(id, AckState.Read);
        var toSend = stateChange.toJson();
        this.fastFlickerClient.doSend(toSend);
    }

    public getAllMessages(since: Date) {
        var request = new GetAllMessagesRequest(since);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    }

    public send(content : string, to : string, id: string) {
        var request = new Message(to, "", "", id, new Date(), content, AckState[AckState.Sent]);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
        return request;
    }
    //#####################################################################
}