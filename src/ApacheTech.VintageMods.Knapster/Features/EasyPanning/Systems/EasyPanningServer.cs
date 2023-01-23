using ApacheTech.VintageMods.Knapster.Abstractions;
using ApacheTech.VintageMods.Knapster.Extensions;

namespace ApacheTech.VintageMods.Knapster.Features.EasyPanning.Systems
{
    [UsedImplicitly]
    public sealed class EasyPanningServer : FeatureServerSystemBase<EasyPanningSettings, EasyPanningPacket>
    {
        protected override string SubCommandName => "Panning";

        protected override void FeatureSpecificCommands(IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand)
        {
            subCommand
                .HasSubCommand("speed", s => s
                    .WithAlias("s")
                    .WithHandler(OnChangeSpeedMultiplier)
                    .Build())
                .HasSubCommand("hungerrate", s => s
                    .WithAlias("h")
                    .WithHandler(OnChangeSaturationMultiplier)
                    .Build());
        }

        protected override EasyPanningPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return EasyPanningPacket.Create(enabledForPlayer, Settings.SpeedMultiplier, Settings.SaturationMultiplier);
        }

        protected override void DisplayInfo(IPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LangEx.FeatureString("Knapster", "Mode", SubCommandName, Settings.Mode));
            sb.AppendLine(LangEx.FeatureString("Knapster", "SpeedMultiplier", SubCommandName, Settings.SpeedMultiplier));
            sb.Append(LangEx.FeatureString("Knapster", "SaturationMultiplier", SubCommandName, Settings.SaturationMultiplier));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        private void OnChangeSpeedMultiplier(IPlayer player, int groupId, CmdArgs args)
        {
            Settings.SpeedMultiplier = GameMath.Clamp(args.PopFloat().GetValueOrDefault(1f), 0f, 2f);
            var message = LangEx.FeatureString("Knapster", "SpeedMultiplier", SubCommandName, Settings.SpeedMultiplier);
            Sapi.SendMessage(player, groupId, message, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }

        private void OnChangeSaturationMultiplier(IPlayer player, int groupId, CmdArgs args)
        {
            Settings.SaturationMultiplier = GameMath.Clamp(args.PopFloat().GetValueOrDefault(1f), 0f, 2f);
            var message = LangEx.FeatureString("Knapster", "SaturationMultiplier", SubCommandName, Settings.SaturationMultiplier);
            Sapi.SendMessage(player, groupId, message, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }
    }
}
