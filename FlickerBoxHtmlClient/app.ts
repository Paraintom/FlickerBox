/// <reference path="FlickerBoxClient.ts" />
//Should not be used anymore, to clean after analysis.
class Greeter {
    element: HTMLElement;
    span: HTMLElement;
    timerToken: number;
    ffClient: FlickerBoxClient;

    constructor(element: HTMLElement) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
    }

    start(subject: string) {
        this.ffClient = new FlickerBoxClient(subject);
        this.ffClient.onConnected.subscribe(() => this.element.innerHTML += "Connected <br>");
        this.ffClient.onDisconnected.subscribe(() => this.element.innerHTML += "Disconnected <br>");
        this.ffClient.onError.subscribe((a) => this.element.innerHTML += "Error:" + a + " <br>");
        this.ffClient.onMessage.subscribe((m) => this.element.innerHTML += "M:" + m.FromFriendName+"->"+ m.Content + " <br>");
        this.ffClient.onStateChange.subscribe((a) => this.element.innerHTML += "Ack:" + a.Id + ":" + AckState[a.State] + " <br>");
        this.ffClient.onFriend.subscribe((f) => this.element.innerHTML += "New Friend received :" + f.Name + " <br>");
        this.ffClient.connect("ws://localhost:8099/");
        //this.ffClient.connect("ws://87.113.40.158:8099/");
    }

    stop() {
        clearTimeout(this.timerToken);
    }

    static newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}
window.onload = () => {

    document.getElementById("start").onclick = () => {
        var el = document.getElementById('content');
        var greeter = new Greeter(el);
        greeter.start((<HTMLInputElement>document.getElementById("privateId")).value);

        document.getElementById("ackRead").onclick = () => { greeter.ffClient.ackRead("testId"); };
        document.getElementById("GetAllMess").onclick = () => {
            var since = new Date();
            since.setDate(since.getDate() - 5);
            greeter.ffClient.getAllMessages(since);
        };
        document.getElementById("GetAllFriends").onclick = () => { greeter.ffClient.getAllFriends(); };
        document.getElementById("newFriendGo").onclick = () => {
            var name = (<HTMLInputElement>document.getElementById("newFriendName")).value;
            var passphrase = (<HTMLInputElement>document.getElementById("newFriendPassPhrase")).value;
            greeter.ffClient.addFriend(name, passphrase);
        };

        document.getElementById("sendMessage").onclick = () => {
            var to = (<HTMLInputElement>document.getElementById("toMessage")).value;
            var content = (<HTMLInputElement>document.getElementById("contentMessage")).value;
            greeter.ffClient.send(content, to, Greeter.newGuid());
        };
    };
};