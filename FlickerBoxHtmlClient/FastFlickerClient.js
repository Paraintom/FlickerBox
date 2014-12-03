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
var FastFlickerClient = (function () {
    function FastFlickerClient(url) {
        //Events
        this.onConnectedEvent = new LiteEvent();
        this.onDisconnectedEvent = new LiteEvent();
        this.onErrorEvent = new LiteEvent();
        this.onMessageEvent = new LiteEvent();
        this.url = url;
    }
    Object.defineProperty(FastFlickerClient.prototype, "onConnected", {
        get: function () {
            return this.onConnectedEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(FastFlickerClient.prototype, "onDisconnected", {
        get: function () {
            return this.onDisconnectedEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(FastFlickerClient.prototype, "onError", {
        get: function () {
            return this.onErrorEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(FastFlickerClient.prototype, "onMessage", {
        get: function () {
            return this.onMessageEvent;
        },
        enumerable: true,
        configurable: true
    });

    FastFlickerClient.prototype.listenTo = function (subject) {
        var _this = this;
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
        this.websocket.onopen = function (evt) {
            _this.onOpen(evt);
        };
        this.websocket.onclose = function (evt) {
            _this.onClose(evt);
        };
        this.websocket.onmessage = function (evt) {
            _this.onMessageReceived(evt);
        };
        this.websocket.onerror = function (evt) {
            _this.onErrorReceived(evt);
        };
    };

    FastFlickerClient.prototype.isConnected = function () {
        return this.websocket != null && this.websocket.readyState == WebSocket.OPEN;
    };

    FastFlickerClient.prototype.doSend = function (message) {
        if (this.websocket != null) {
            this.websocket.send(message);
        }
    };

    FastFlickerClient.prototype.onOpen = function (evt) {
        if ((typeof this.currentSubject != 'undefined')) {
            this.doSend(this.currentSubject);
        }
        this.onConnectedEvent.raise();
    };

    FastFlickerClient.prototype.onClose = function (evt) {
        this.onDisconnectedEvent.raise();
    };

    FastFlickerClient.prototype.onMessageReceived = function (evt) {
        if (evt.data != this.currentSubject)
            this.onMessageEvent.raise(evt.data);
    };

    FastFlickerClient.prototype.onErrorReceived = function (evt) {
        this.onErrorEvent.raise(evt);
    };
    return FastFlickerClient;
})();
//# sourceMappingURL=FastFlickerClient.js.map
