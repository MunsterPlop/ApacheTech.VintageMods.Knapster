using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasyClayFormingPacket
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Determines the number of voxels that are handled at one time, when using the Easy Clay Forming feature.
        /// </summary>
        public int VoxelsPerClick { get; set; }

        /// <summary>
        ///     Initialises a new instance of the <see cref="EasyClayFormingPacket"/> class.
        /// </summary>
        public static EasyClayFormingPacket FromSettings(EasyClayFormingSettings settings)
        {
            return new EasyClayFormingPacket
            {
                Enabled = settings.Enabled, 
                VoxelsPerClick = settings.VoxelsPerClick
            };
        }

        /// <summary>
        ///     Converts these settings to a <see cref="EasyClayFormingSettings"/> instance.
        /// </summary>
        public EasyClayFormingSettings ToSettings()
        {
            return new EasyClayFormingSettings
            {
                Enabled = Enabled,
                VoxelsPerClick = VoxelsPerClick
            };
        }
    }
}