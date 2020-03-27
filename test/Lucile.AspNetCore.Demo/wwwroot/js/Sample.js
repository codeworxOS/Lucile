var Sample = /** @class */ (function () {
    function Sample() {
        this._message = AppSettings.configuration.Level2.Prop1;
    }
    Sample.prototype.process = function () {
        alert(this._message);
    };
    return Sample;
}());
//# sourceMappingURL=Sample.js.map