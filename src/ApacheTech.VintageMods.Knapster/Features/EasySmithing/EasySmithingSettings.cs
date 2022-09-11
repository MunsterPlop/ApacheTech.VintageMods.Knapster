using Gantry.Services.FileSystem.Features;
using Newtonsoft.Json;

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing
{
    /// <summary>
    ///     Represents user-controllable settings used for the mod.
    /// </summary>
    /// <seealso cref="FeatureSettings" />
    [JsonObject]
    public class EasySmithingSettings : FeatureSettings
    {
        /// <summary>
        ///     Determines whether the EasySmithing Feature should be used.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Determines the amount of durability that is lost at one time, when using the Easy Smithing feature.
        /// </summary>
        public int CostPerClick { get; set; } = 5;
    }
}