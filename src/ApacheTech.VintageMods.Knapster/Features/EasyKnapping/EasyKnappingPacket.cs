using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasyKnappingPacket
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; init; }
    }
}