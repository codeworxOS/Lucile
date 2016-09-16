namespace Lucile.Data.Metadata
{
    /// <summary>
    /// Navigation-Property Multiplicity
    /// </summary>
    public enum NavigationPropertyMultiplicity
    {
        /// <summary>
        /// 1 oder 0
        /// </summary>
        ZeroOrOne = 0x00,

        /// <summary>
        /// Genau 1
        /// </summary>
        One = 0x01,

        /// <summary>
        /// Viele
        /// </summary>
        Many = 0x02
    }
}