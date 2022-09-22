using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasyClayFormingPacket
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; init; }

        /// <summary>
        ///     Determines the number of voxels that are handled at one time, when using the Easy Clay Forming feature.
        /// </summary>
        public int VoxelsPerClick { get; init; }

        /// <summary>
        ///     Initialises a new instance of the <see cref="EasyClayFormingPacket"/> class.
        /// </summary>
        public static EasyClayFormingPacket Create(bool enabled, int voxelsPerClick)
        {
            return new EasyClayFormingPacket
            {
                Enabled = enabled, 
                VoxelsPerClick = voxelsPerClick
            };
        }
    }
}