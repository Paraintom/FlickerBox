class ServiceLookup {
    private configuredLookup: string;
    private onErrorEvent = new LiteEvent<string>();
    public get onError(): ILiteEvent<string> { return this.onErrorEvent; }
    private onResultEvent = new LiteEvent<string>();
    public get onResult(): ILiteEvent<string> { return this.onResultEvent; }

    constructor() {
        this.configuredLookup = FlickerBoxConfiguration.lookupUrl;
        if (this.isNullOrEmpty(this.configuredLookup)) {
            this.configuredLookup = "ws://localhost:8099/";
        }
        jQuery.support.cors = true;
    }

    getService(serviceName) {
        if (this.configuredLookup.substring(0,7) === "http://") {
            console.log('requesting...');
            var url = this.configuredLookup + "?get=" + serviceName;
            $.get(url, (s) => {
                console.log('request success' + s);
                this.onResultEvent.raise(s.trim());

            }).fail(
            (xhr, textStatus, errorThrown) => {
                console.log('request failed' + textStatus + errorThrown);
                this.onErrorEvent.raise(textStatus);
            });
        } else {
            if (this.configuredLookup.substring(0, 5) === "ws://") {
                this.onResultEvent.raise(this.configuredLookup.substring(5, this.configuredLookup.length));
            } else {
                this.onErrorEvent.raise("Invalid configuration : "+this.configuredLookup);
                
            }
        }
    }
    isNullOrEmpty(str: string) {
        return !(str != null && str.length);
    }
}