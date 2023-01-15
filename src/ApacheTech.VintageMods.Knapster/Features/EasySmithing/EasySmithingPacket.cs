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

        /// <summary>
        ///     Determines the number of voxels that are handled at one time, when using the EasyKnapping feature.
        /// </summary>
        public int VoxelsPerClick { get; init; }

        /// <summary>
        ///     Initialises a new instance of the <see cref="EasySmithingPacket"/> class.
        /// </summary>
        public static EasySmithingPacket Create(bool enabled, int voxelsPerClick)
        {
            return new EasySmithingPacket
            {
                Enabled = enabled,
                VoxelsPerClick = voxelsPerClick
            };
        }
    }
}