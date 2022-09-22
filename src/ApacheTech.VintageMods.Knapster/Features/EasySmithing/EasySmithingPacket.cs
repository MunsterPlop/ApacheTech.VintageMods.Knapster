using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasySmithingPacket
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        ///     Determines the amount of durability that is lost at one time, when using the Easy Smithing feature.
        /// </summary>
        public int CostPerClick { get; init; }
    }
}