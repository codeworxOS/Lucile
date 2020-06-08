class Sample {
    private _message: string;

    constructor() {
        this._message = AppSettings.configuration.Level2.Prop1;
    }

    public process(): void {
        alert(this._message);
    }
}