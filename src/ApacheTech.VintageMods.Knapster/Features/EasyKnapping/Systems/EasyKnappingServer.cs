using System;
using System.Collections.Generic;
using System.Text;
using ApacheTech.VintageMods.Knapster.Abstractions;
using ApacheTech.VintageMods.Knapster.Features.EasyClayForming;
using Vintagestory.API.Common;

namespace ApacheTech.VintageMods.Knapster.Features.EasyKnapping.Systems
{
    public sealed class EasyKnappingServer : FeatureServerSystemBase<EasyKnappingSettings, EasyKnappingPacket>
    {
        protected override string SubCommandName => "Knapping";

        protected override EasyKnappingPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return new EasyKnappingPacket { Enabled = enabledForPlayer };
        }
    }
}
