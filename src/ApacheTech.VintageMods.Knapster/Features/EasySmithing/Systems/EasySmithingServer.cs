using ApacheTech.VintageMods.Knapster.Abstractions;
using ApacheTech.VintageMods.Knapster.Extensions;

namespace ApacheTech.VintageMods.Knapster.Features.EasySmithing.Systems
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EasySmithingServer : FeatureServerSystemBase<EasySmithingSettings, EasySmithingPacket>
    {
        protected override string SubCommandName => "Smithing";

        protected override void FeatureSpecificCommands(IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand)
        {
            subCommand
                .HasSubCommand("cost", v => v
                    .WithAlias("c")
                    .WithHandler(OnChangeCostPerClick)
                    .Build());
        }

        protected sealed override void DisplayInfo(IPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            sb.AppendLine(LangEx.FeatureString("Knapster", "Mode", SubCommandName, Settings.Mode));
            sb.Append(LangEx.FeatureString("EasySmithing", "CostPerClick", Settings.CostPerClick));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        protected override EasySmithingPacket GeneratePacketPerPlayer(IPlayer player, bool enabledForPlayer)
        {
            return new EasySmithingPacket
            {
                Enabled = enabledForPlayer, 
                CostPerClick = Settings.CostPerClick
            };
        }

        private void OnChangeCostPerClick(IPlayer player, int groupId, CmdArgs args)
        {
            Settings.CostPerClick = GameMath.Clamp(args.PopInt().GetValueOrDefault(1), 1, 10);
            var message = LangEx.FeatureString("EasySmithing", "CostPerClick", Settings.CostPerClick);
            Sapi.SendMessage(player, groupId, message, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }
    }
}