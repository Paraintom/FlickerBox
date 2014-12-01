/// <reference path="FastFlickerClient.ts" />
//                              Enjoy!
var FlickerBoxClient = (function () {
    function FlickerBoxClient(privateId) {
        //Events
        this.onErrorEvent = new LiteEvent();
        //################ Connection - Disconnection ########################
        this.onBoxConnectionEvent = new LiteEvent();
        this.onBoxDisconnectionEvent = new LiteEvent();
        //#####################################################################
        //################ Friends Management #################################
        this.onFriendEvent = new LiteEvent();
        //#####################################################################
        //################ Message Handling ###################################
        this.onMessageEvent = new LiteEvent();
        this.onStateChangeEvent = new LiteEvent();
        this.privateId = privateId;
    }
    Object.defineProperty(FlickerBoxClient.prototype, "onError", {
        get: function () {
            return this.onErrorEvent;
        },
        enumerable: true,
        configurable: true
    });

    FlickerBoxClient.prototype.onFastFlickerError = function (event) {
        this.onErrorEvent.raise(event.message);
    };

    FlickerBoxClient.prototype.isArray = function (what) {
        return Object.prototype.toString.call(what) === '[object Array]';
    };
    FlickerBoxClient.prototype.onFastFlickerMessage = function (message) {
        try  {
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
                    var state1 = fromFriendName === "" ? AckState[4 /* Sent */] : AckState[3 /* Received */];
                    var newMessage = new Message(toFriendName, fromFriendName, fromPublicId, id, utcCreationTime, content, state1);
                    this.onMessageEvent.raise(newMessage);
                    break;
                case "Ack":
                    var idMessage = jsonParsed["Id"];
                    var stateString = jsonParsed["State"];
                    var state = AckState[stateString];
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
    };

    Object.defineProperty(FlickerBoxClient.prototype, "onConnected", {
        get: function () {
            return this.onBoxConnectionEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(FlickerBoxClient.prototype, "onDisconnected", {
        get: function () {
            return this.onBoxDisconnectionEvent;
        },
        enumerable: true,
        configurable: true
    });

    FlickerBoxClient.prototype.connect = function (fastFlickerUrl) {
        var _this = this;
        if (this.fastFlickerClient != null) {
            //closing the existing connection
            this.fastFlickerClient.onConnected.unsubscribe(function () {
                _this.onFastFlickerConnected();
            });
            this.fastFlickerClient.onDisconnected.unsubscribe(function () {
                _this.onFastFlickerDisconnected();
            });
            this.fastFlickerClient.onError.unsubscribe(function (m) {
                _this.onFastFlickerError(m);
            });
            this.fastFlickerClient.onMessage.unsubscribe(function (m) {
                _this.onFastFlickerMessage(m);
            });
        }
        this.fastFlickerClient = new FastFlickerClient(fastFlickerUrl);
        this.fastFlickerClient.onConnected.subscribe(function () {
            _this.onFastFlickerConnected();
        });
        this.fastFlickerClient.onDisconnected.subscribe(function () {
            _this.onFastFlickerDisconnected();
        });
        this.fastFlickerClient.onError.subscribe(function (e) {
            _this.onFastFlickerError(e);
        });
        this.fastFlickerClient.onMessage.subscribe(function (m) {
            _this.onFastFlickerMessage(m);
        });

        this.fastFlickerClient.listenTo(this.privateId);
        //onFastFlickerMessage
    };

    FlickerBoxClient.prototype.onFastFlickerConnected = function () {
        this.onBoxConnectionEvent.raise();
        //Then ... that's it?
    };

    FlickerBoxClient.prototype.onFastFlickerDisconnected = function () {
        this.onBoxDisconnectionEvent.raise();
        //Then ... that's it?
    };

    Object.defineProperty(FlickerBoxClient.prototype, "onFriend", {
        get: function () {
            return this.onFriendEvent;
        },
        enumerable: true,
        configurable: true
    });

    FlickerBoxClient.prototype.getAllFriends = function () {
        var request = new GetAllFriendRequest();
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    };

    FlickerBoxClient.prototype.addFriend = function (name, passphrase) {
        var request = new FriendRequest(name, passphrase);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    };

    Object.defineProperty(FlickerBoxClient.prototype, "onMessage", {
        get: function () {
            return this.onMessageEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(FlickerBoxClient.prototype, "onStateChange", {
        get: function () {
            return this.onStateChangeEvent;
        },
        enumerable: true,
        configurable: true
    });

    FlickerBoxClient.prototype.ackRead = function (id) {
        var stateChange = new StateChange(id, 1 /* Read */);
        var toSend = stateChange.toJson();
        this.fastFlickerClient.doSend(toSend);
    };

    FlickerBoxClient.prototype.getAllMessages = function (since) {
        var request = new GetAllMessagesRequest(since);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
    };

    FlickerBoxClient.prototype.send = function (content, to, id) {
        var request = new Message(to, "", "", id, new Date(), content, AckState[4 /* Sent */]);
        var toSend = request.toJson();
        this.fastFlickerClient.doSend(toSend);
        return request;
    };
    return FlickerBoxClient;
})();
//# sourceMappingURL=FlickerBoxClient.js.map
