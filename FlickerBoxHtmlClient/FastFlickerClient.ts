/// <reference path="LiteEvent.ts" />
// Interface : 
// 1 - Create the chat object ---> 
//  url = "ws://"+ip+":"+port+"/";
//	chat = new FastFLickerChat(url);
// 2- Listen to events ---> 
// onNewMessage mainly :
//this.chat.onMessage.subscribe(function (a) {
//    return _this.span.innerHTML += "Message received:" + a + " \n";
//});
// But also other events ():
//this.chat.onConnected.subscribe(function () {
//    return _this.span.innerHTML += "Connected \n";
//});
//this.chat.onDisconnected.subscribe(function () {
//    return _this.span.innerHTML += "Disconnected \n";
//});
//this.chat.onError.subscribe(function (a) {
//    return _this.span.innerHTML += "Error:" + a.message + " \n";
//});
// 3- Connect to subject and send messages ---> 
//	chat.listenTo(subjectString);
//  chat.doSend(msgToSend);
//                              Enjoy!
class FastFlickerClient {
    url: string;
    currentSubject: string;
    websocket: WebSocket;

    //Events
    private onConnectedEvent = new LiteEvent<void>();
    public get onConnected(): ILiteEvent<void> { return this.onConnectedEvent; }
    private onDisconnectedEvent = new LiteEvent<void>();
    public get onDisconnected(): ILiteEvent<void> { return this.onDisconnectedEvent; }
    private onErrorEvent = new LiteEvent<ErrorEvent>();
    public get onError(): ILiteEvent<ErrorEvent> { return this.onErrorEvent; }
    private onMessageEvent = new LiteEvent<string>();
    public get onMessage(): ILiteEvent<string> { return this.onMessageEvent; }

    constructor(url: string) {
        this.url = url;
    }

    public listenTo(subject: string) {
        this.currentSubject = subject;
        if (this.websocket != null) {
            //closing the existing connection		
            this.websocket.onopen = null;
            this.websocket.onclose = null;
            this.websocket.onmessage = null;
            this.websocket.onerror = null;
            this.websocket.close();
        }
        this.websocket = new WebSocket(this.url);
        this.websocket.onopen = evt => { this.onOpen(evt); };
        this.websocket.onclose = evt => { this.onClose(evt); };
        this.websocket.onmessage = evt => { this.onMessageReceived(evt); };
        this.websocket.onerror = evt => { this.onErrorReceived(evt); };
    }

    public isConnected() {
        return this.websocket != null
            && this.websocket.readyState == WebSocket.OPEN;
    }

    public doSend(message: string) {
        if (this.websocket != null) {
            this.websocket.send(message);
        }
    }

    private onOpen(evt: Object) {
        if ((typeof this.currentSubject != 'undefined')) {
            this.doSend(this.currentSubject);
        }
        this.onConnectedEvent.raise();
    }

    private onClose(evt: Object) { this.onDisconnectedEvent.raise(); }

    private onMessageReceived(evt: MessageEvent) {
        if (evt.data != this.currentSubject)
            this.onMessageEvent.raise(evt.data);
    }

    private onErrorReceived(evt: ErrorEvent) { this.onErrorEvent.raise(evt); }
}