using ProtoBuf;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class EasyKnappingPacket
    {
        /// <summary>
        ///     Determines whether the EasyClayForming Feature should be used.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        ///     Initialises a new instance of the <see cref="EasyKnappingPacket"/> class.
        /// </summary>
        public static EasyKnappingPacket FromSettings(EasyKnappingSettings settings)
        {
            return new EasyKnappingPacket
            {
                Enabled = settings.Enabled
            };
        }

        /// <summary>
        ///     Converts these settings to a <see cref="EasyKnappingSettings"/> instance.
        /// </summary>
        public EasyKnappingSettings ToSettings()
        {
            return new EasyKnappingSettings
            {
                Enabled = Enabled
            };
        }
    }
}