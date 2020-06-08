interface ISettings {
    [key: string]: any;
}
declare class AppSettings {
    static configuration: ISettings;
    /**
     *  Getting setting value.
     */
    static getValue<T>(key: string): T;
}
