var flickerBoxApp = angular.module('flickerBoxApp', []);

flickerBoxApp.controller('FriendsAndMessagesCtrl', function ($scope) {
    var localStoragePrivateIdKey = "privateId";
    var localStorageLastMessageReceived = "lastMessageReceived";
    var localStorageAllFriends = "allFriends";

    //lookup part ...
    $scope.url;
    startUrlLookup();
    var ffClient = null;
    var now = new Date();
    var lastReconnectAttempt = new Date();
    lastReconnectAttempt.setDate(now.getDate() - 1);
    ;

    // Discover new friends
    $scope.newFriendName;
    $scope.newFriendPassPhrase;

    $scope.addFriend = function () {
        console.log("Trying to discover " + $scope.newFriendName + "...");
        ffClient.addFriend($scope.newFriendName, $scope.newFriendPassPhrase);
    };

    // Discover new friends
    // Send new Message
    $scope.sendMessage = function (contentNewMessage) {
        var id = generateUUID();
        console.log("Sending message to " + $scope.friendSelected.Name + ", id=" + id + "content=" + contentNewMessage);
        var sentMessage = ffClient.send(contentNewMessage, $scope.friendSelected.Name, id);
        onMessage(sentMessage);
    };

    // Send new Message End
    $scope.friendSelected;

    $scope.privateId = localStorage.getItem(localStoragePrivateIdKey);

    $scope.friends = [];

    var savedFriends = localStorage.getItem(localStorageAllFriends);
    if (!isNullOrEmpty(savedFriends)) {
        $scope.friends = JSON.parse(savedFriends);
    }

    $scope.onPrivateIdChanged = function () {
        console.log("onPrivateIdChanged for " + $scope.privateId + ", starting flickerBoxClient.");
        localStorage.setItem(localStoragePrivateIdKey, $scope.privateId);
        startFlickerBox();
    };

    $scope.setFriendSelected = function (s) {
        $scope.friendSelected = s;
    };

    $scope.queryAllFriends = function () {
        ffClient.getAllFriends();
    };
    $scope.clearData = function () {
        localStorage.clear();
    };

    //If we have a subject, we can start connecting to the flickerBox...
    if (!isNullOrEmpty($scope.privateId)) {
        console.log("Subject set, starting the connection.");
        $scope.onPrivateIdChanged();
    }

    function isNullOrEmpty(str) {
        return !(str != null && str.length);
    }

    $scope.messageListOrder = 'UtcCreationTime';
    $scope.friendListOrder = '-nbUnreadMessages';

    function startUrlLookup() {
        var lookup = new ServiceLookup();
        lookup.onError.subscribe(function (a) {
            return console.log("Error from ServiceLookup:" + a);
        });
        lookup.onResult.subscribe(function (result) {
            console.log("Result from ServiceLookup:" + result);
            $scope.url = result;
            startFlickerBox();
        });
        lookup.getService("FastFlicker");
    }

    function startFlickerBox() {
        if ($scope.privateId != null && $scope.privateId.length && $scope.url != null && $scope.url.length) {
            console.log("Starting the box with privateId=" + $scope.privateId + ", and url=" + $scope.url);
            ffClient = new FlickerBoxClient($scope.privateId);
            ffClient.onConnected.subscribe(function () {
                return $scope.$apply(onConnected());
            });
            ffClient.onDisconnected.subscribe(function () {
                return $scope.$apply(onDisconnected());
            });
            ffClient.onError.subscribe(function (a) {
                return console.log("Error:" + a);
            });
            ffClient.onMessage.subscribe(function (m) {
                return $scope.$apply(onMessage(m));
            });
            ffClient.onStateChange.subscribe(function (a) {
                return console.log("Ack:" + a.Id + ":" + AckState[a.State]);
            });
            ffClient.onFriend.subscribe(function (f) {
                return $scope.$apply(onFriend(f));
            });
            ffClient.connect("ws://" + $scope.url + "/");
        } else {
            console.log("Not ready yet to start the box...");
        }
    }

    function generateUUID() {
        var d = new Date().getTime();
        var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = (d + Math.random() * 16) % 16 | 0;
            d = Math.floor(d / 16);
            return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
        });
        return uuid;
    }
    ;

    //Events handlers
    function onFriend(friend) {
        var newFriendName = friend.Name;
        if (Enumerable.from($scope.friends).select(function (o) {
            return o.Name;
        }).contains(newFriendName)) {
            console.warn("Skipping Friend already received :" + newFriendName);
            return;
        }
        console.log("New Friend received :" + newFriendName);
        $scope.friends.push(new Friend(newFriendName));
        saveState();
    }

    function saveState() {
        var stringify = JSON.stringify($scope.friends);
        console.log("Saving to local storage :" + stringify);
        localStorage.setItem(localStorageAllFriends, stringify);
    }
    function onConnected() {
        $scope.isConnected = true;
        console.log("Connected");

        //We check when was the last time we ask for message and ask for the delta
        var lastString = localStorage.getItem(localStorageLastMessageReceived);

        var date = new Date(-31556952000);
        if (lastString != null) {
            date = new Date(parseInt(lastString));
            console.log("asking all message since " + date);
        } else {
            console.log("No date found in local storage, asking for all messages...");
        }
        ffClient.getAllMessages(date);
    }

    function onDisconnected() {
        $scope.isConnected = false;
        console.log("We lost the connection with the box!");
        checkWellConnected();
    }

    $scope.isConnected = false;

    function checkWellConnected() {
        console.log("Checking that we are connected...");
        if (ffClient.isConnected()) {
            console.log("We are well connected!");
            $scope.isConnected = true;
        } else {
            var disconnectTime = new Date();
            var totalSecondsElapsedSinceLastCall = (disconnectTime.getTime() - lastReconnectAttempt.getTime()) / 1000;
            if (totalSecondsElapsedSinceLastCall > 10) {
                lastReconnectAttempt = disconnectTime;
                console.log("It is new, let's try again in 5s");
                setTimeout(function () {
                    return reconnect();
                }, 5000);
                setTimeout(function () {
                    return checkWellConnected();
                }, 15000);
            } else {
                console.log("Hum, we tried few seconds ago. Let's wait a little before reconnection...");
                setTimeout(function () {
                    return checkWellConnected();
                }, 15000);
            }
        }
    }

    function reconnect() {
        console.log("reconnecting...");
        ffClient.connect("ws://" + $scope.url + "/");
    }

    function onMessage(message) {
        var friendName;
        var fromFriendName = message.FromFriendName;
        var toFriendName = message.ToFriendName;
        if (fromFriendName != null && fromFriendName.length != 0) {
            //incoming message!
            friendName = fromFriendName;
        } else {
            //message sent
            if (toFriendName != null && toFriendName.length != 0) {
                friendName = toFriendName;
            } else {
                console.log("Message without to or from, skipping it..." + message);
                return;
            }
        }
        console.log("M:" + friendName + "->" + message.Content);

        // First ... if it is a new friend, we add him
        onFriend(new Friend(friendName));

        //Now we are sure to have him, let's add the message!
        var friend = Enumerable.from($scope.friends).first(function (o) {
            return o.Name === friendName;
        });
        if (!Enumerable.from(friend.Messages).select(function (o) {
            return o.Id;
        }).contains(message.Id)) {
            friend.Messages.push(message);

            //We save the state ...
            saveState();

            //And update the last message received!
            localStorage.setItem(localStorageLastMessageReceived, (+new Date).toString());
            ffClient.ackRead(message.Id);
        } else {
            console.log("Ignoring message already received Id:" + message.Id);
        }
    }
});
//# sourceMappingURL=controllers.js.map
