using Gantry.Services.FileSystem.Features;
using Newtonsoft.Json;
using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping
{
    /// <summary>
    ///     Represents user-controllable settings used for the mod.
    /// </summary>
    /// <seealso cref="FeatureSettings" />
    [JsonObject]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasyKnappingSettings : FeatureSettings
    {
        [JsonConstructor]
        public EasyKnappingSettings()
        {
            Enabled = true;
        }

        /// <summary>
        ///     Determines whether the EasyKnapping Feature should be used.
        /// </summary>
        public bool Enabled { get; set; }
    }
}