interface ISettings
{
    StringProp: string;

    NumberProp: number;

    DateTimeProp: Date | string;

    Level2: TestSubSetting;
}

interface TestSubSetting
{
    Prop1: string;

    Prop2: string;
}