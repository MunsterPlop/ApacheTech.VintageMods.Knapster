using Gantry.Services.FileSystem.Features;
using Newtonsoft.Json;

namespace ApacheTech.VintageMods.Knapster.Features.EasyClayForming
{
    /// <summary>
    ///     Represents user-controllable settings used for the mod.
    /// </summary>
    /// <seealso cref="FeatureSettings" />
    [JsonObject]
    public class EasyClayFormingSettings : FeatureSettings
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Determines the number of voxels that are handled at one time, when using the Easy Clay Forming feature.
        /// </summary>
        public int VoxelsPerClick { get; set; } = 1;

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
    }
}