var ServiceLookup = (function () {
    function ServiceLookup() {
        this.onErrorEvent = new LiteEvent();
        this.onResultEvent = new LiteEvent();
        this.configuredLookup = FlickerBoxConfiguration.lookupUrl;
        if (this.isNullOrEmpty(this.configuredLookup)) {
            this.configuredLookup = "ws://localhost:8099/";
        }
        jQuery.support.cors = true;
    }
    Object.defineProperty(ServiceLookup.prototype, "onError", {
        get: function () {
            return this.onErrorEvent;
        },
        enumerable: true,
        configurable: true
    });

    Object.defineProperty(ServiceLookup.prototype, "onResult", {
        get: function () {
            return this.onResultEvent;
        },
        enumerable: true,
        configurable: true
    });

    ServiceLookup.prototype.getService = function (serviceName) {
        var _this = this;
        if (this.configuredLookup.substring(0, 7) === "http://") {
            console.log('requesting...');
            var url = this.configuredLookup + "?get=" + serviceName;
            $.get(url, function (s) {
                console.log('request success' + s);
                _this.onResultEvent.raise(s.trim());
            }).fail(function (xhr, textStatus, errorThrown) {
                console.log('request failed' + textStatus + errorThrown);
                _this.onErrorEvent.raise(textStatus);
            });
        } else {
            if (this.configuredLookup.substring(0, 5) === "ws://") {
                this.onResultEvent.raise(this.configuredLookup.substring(5, this.configuredLookup.length));
            } else {
                this.onErrorEvent.raise("Invalid configuration : " + this.configuredLookup);
            }
        }
    };
    ServiceLookup.prototype.isNullOrEmpty = function (str) {
        return !(str != null && str.length);
    };
    return ServiceLookup;
})();
//# sourceMappingURL=ServiceLookup.js.map
