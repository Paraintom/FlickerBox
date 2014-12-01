/// <reference path="FlickerBoxClient.ts" />
//Should not be used anymore, to clean after analysis.
var Greeter = (function () {
    function Greeter(element) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }
    Greeter.prototype.start = function (subject) {
        var _this = this;
        this.ffClient = new FlickerBoxClient(subject);
        this.ffClient.onConnected.subscribe(function () {
            return _this.element.innerHTML += "Connected <br>";
        });
        this.ffClient.onDisconnected.subscribe(function () {
            return _this.element.innerHTML += "Disconnected <br>";
        });
        this.ffClient.onError.subscribe(function (a) {
            return _this.element.innerHTML += "Error:" + a + " <br>";
        });
        this.ffClient.onMessage.subscribe(function (m) {
            return _this.element.innerHTML += "M:" + m.FromFriendName + "->" + m.Content + " <br>";
        });
        this.ffClient.onStateChange.subscribe(function (a) {
            return _this.element.innerHTML += "Ack:" + a.Id + ":" + AckState[a.State] + " <br>";
        });
        this.ffClient.onFriend.subscribe(function (f) {
            return _this.element.innerHTML += "New Friend received :" + f.Name + " <br>";
        });
        this.ffClient.connect("ws://localhost:8099/");
        //this.ffClient.connect("ws://87.113.40.158:8099/");
    };

    Greeter.prototype.stop = function () {
        clearTimeout(this.timerToken);
    };

    Greeter.newGuid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
    return Greeter;
})();
window.onload = function () {
    document.getElementById("start").onclick = function () {
        var el = document.getElementById('content');
        var greeter = new Greeter(el);
        greeter.start(document.getElementById("privateId").value);

        document.getElementById("ackRead").onclick = function () {
            greeter.ffClient.ackRead("testId");
        };
        document.getElementById("GetAllMess").onclick = function () {
            var since = new Date();
            since.setDate(since.getDate() - 5);
            greeter.ffClient.getAllMessages(since);
        };
        document.getElementById("GetAllFriends").onclick = function () {
            greeter.ffClient.getAllFriends();
        };
        document.getElementById("newFriendGo").onclick = function () {
            var name = document.getElementById("newFriendName").value;
            var passphrase = document.getElementById("newFriendPassPhrase").value;
            greeter.ffClient.addFriend(name, passphrase);
        };

        document.getElementById("sendMessage").onclick = function () {
            var to = document.getElementById("toMessage").value;
            var content = document.getElementById("contentMessage").value;
            greeter.ffClient.send(content, to, Greeter.newGuid());
        };
    };
};
//# sourceMappingURL=app.js.map
