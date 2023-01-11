using System.Globalization;
using ApacheTech.Common.DependencyInjection.Abstractions;
using ApacheTech.Common.Extensions.System;
using ApacheTech.VintageMods.Knapster.DataStructures;
using ApacheTech.VintageMods.Knapster.Extensions;
using Gantry.Core.DependencyInjection.Registration;
using Gantry.Services.FileSystem.Configuration;
using Gantry.Services.FileSystem.DependencyInjection;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

// ReSharper disable StringLiteralTypo

namespace ApacheTech.VintageMods.Knapster.Abstractions
{
    /// <summary>
    ///     Acts as a base class for all EasyX features, on the server.
    /// </summary>
    /// <typeparam name="TSettings">The type of the settings.</typeparam>
    /// <typeparam name="TPacket">The type of the packet.</typeparam>
    public abstract class FeatureServerSystemBase<TSettings, TPacket> : ServerModSystem, IServerServiceRegistrar
        where TSettings : class, IEasyFeatureSettings, new()
    {
        protected IServerNetworkChannel ServerChannel;
        internal static TSettings Settings { get; private set; } = new();

        /// <summary>
        ///     Allows a mod to include Singleton, or Transient services to the IOC Container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServerModServices(IServiceCollection services)
        {
            services.AddFeatureWorldSettings<TSettings>();
        }

        protected abstract string SubCommandName { get; }

        /// <summary>
        ///     Adds feature specific sub-commands to the feature command. 
        /// </summary>
        /// <param name="subCommand">The sub-command to add features to.</param>
        protected virtual void FeatureSpecificCommands(IFluentChatSubCommandBuilder<IFluentChatCommand> subCommand)
        {
            // Do nothing, by default.
        }

        /// <summary>
        ///     Full start to the mod on the server side
        /// <br /><br />In 1.17+ do NOT use this to add or update behaviors or attributes or other fixed properties of any block, item or entity, in code (additional to what is read from JSON).
        /// It is already too late to do that here, it will not be seen client-side.  Instead, code which needs to do that should be registered for event sapi.Event.AssetsFinalizers.  See VSSurvivalMod system BlockReinforcement.cs for an example.
        /// </summary>
        /// <param name="api"></param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            Settings = ModSettings.World.Feature<TSettings>();

            FluentChat.ServerCommand("knapster")
                .HasSubCommand(SubCommandName, x => x
                    .WithAlias(SubCommandName.Substring(0, 1))
                    .WithHandler(DisplayInfo)
                    .HasSubCommand("mode", m => m.WithAlias("m").WithHandler(OnChangeMode).Build())
                    .HasSubCommand("whitelist", wl => wl.WithAlias("wl").WithHandler(HandleWhitelist).Build())
                    .HasSubCommand("blacklist", bl => bl.WithAlias("bl").WithHandler(HandleBlacklist).Build())
                    .WithFeatureSpecifics(FeatureSpecificCommands)
                    .Build());

            ServerChannel = IOC.Services.Resolve<IServerNetworkService>()
                .DefaultServerChannel
                .RegisterMessageType<TPacket>();

            api.Event.PlayerJoin += player =>
            {
                ServerChannel.SendPacket(GeneratePacket(player), player);
            };
        }

        /// <summary>
        ///     Generates a packet, to send to the specified player.
        /// </summary>
        protected TPacket GeneratePacket(IPlayer player)
        {
            return GeneratePacketPerPlayer(player, IsEnabledFor(player));
        }

        /// <summary>
        ///     Generates a packet, to send to the specified player.
        /// </summary>
        protected abstract TPacket GeneratePacketPerPlayer(IPlayer player, bool isEnabled);

        /// <summary>
        ///     Determines whether this feature is enabled, for the specified player.
        /// </summary>
        internal static bool IsEnabledFor(IPlayer player)
        {
            return Settings.Mode switch
            {
                AccessMode.Disabled => false,
                AccessMode.Enabled => true,
                AccessMode.Whitelist => Settings.Whitelist.Any(p => p.Id == player.PlayerUID),
                AccessMode.Blacklist => Settings.Blacklist.All(p => p.Id != player.PlayerUID),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        /// <summary>
        ///     Call Handler: /knapster (X)
        /// </summary>
        protected virtual void DisplayInfo(IPlayer player, int groupId, CmdArgs args)
        {
            var sb = new StringBuilder();
            sb.Append(LangEx.FeatureString("Knapster", "Mode", Lang.Get(SubCommandName.SplitPascalCase().UcFirst()), Settings.Mode));
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        /// <summary>
        ///     Call Handler: /knapster (X) mode
        /// </summary>
        private void OnChangeMode(IPlayer player, int groupId, CmdArgs args)
        {
            AccessMode? mode = args.PopWord("") switch
            {
                var d when "disabled".StartsWith(d, true, CultureInfo.InvariantCulture) => AccessMode.Disabled,
                var e when "enabled".StartsWith(e, true, CultureInfo.InvariantCulture) => AccessMode.Enabled,
                var w when "whitelist".StartsWith(w, true, CultureInfo.InvariantCulture) => AccessMode.Whitelist,
                var b when "blacklist".StartsWith(b, true, CultureInfo.InvariantCulture) => AccessMode.Blacklist,
                var wl when wl.Equals("wl", StringComparison.InvariantCultureIgnoreCase) => AccessMode.Whitelist,
                _ => null,
            };

            if (mode is null)
            {
                const string validModes = "[D]isabled | [E]nabled | [W]hitelist | [B]lacklist]";
                var invalidModeMessage = LangEx.FeatureString("Knapster", "InvalidMode", validModes);
                Sapi.SendMessage(player, groupId, invalidModeMessage, EnumChatType.Notification);
                return;
            }

            Settings.Mode = mode.Value;
            var modeMessage = LangEx.FeatureString("Knapster", "SetMode", SubCommandName, Settings.Mode);
            Sapi.SendMessage(player, groupId, modeMessage, EnumChatType.Notification);
            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }

        /// <summary>
        ///     Call Handler: /knapster (X) whitelist
        /// </summary>
        private void HandleWhitelist(IPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length > 0)
            {
                AddRemovePlayerFromList(player, groupId, args, Settings.Whitelist, "Whitelist");
                return;
            }

            var sb = new StringBuilder();
            var message = Settings.Whitelist.Count > 0 ? "Results" : "NoResults";
            sb.AppendLine(LangEx.FeatureString("Knapster", $"Whitelist.{message}", SubCommandName));
            foreach (var p in Settings.Whitelist)
            {
                sb.AppendLine($" - {p.Name} (PID: {p.Id})");
            }
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        /// <summary>
        ///     Call Handler: /knapster (X) blacklist
        /// </summary>
        private void HandleBlacklist(IPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length > 0)
            {
                AddRemovePlayerFromList(player, groupId, args, Settings.Blacklist, "Blacklist");
                return;
            }

            var sb = new StringBuilder();
            var message = Settings.Blacklist.Count > 0 ? "Results" : "NoResults";
            sb.AppendLine(LangEx.FeatureString("Knapster", $"Blacklist.{message}", SubCommandName));
            foreach (var p in Settings.Blacklist)
            {
                sb.AppendLine($" - {p.Name} (PID: {p.Id})");
            }
            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        private void AddRemovePlayerFromList(IPlayer player, int groupId, CmdArgs args, ICollection<Player> list, string listType)
        {
            var searchTerm = args.PopWord();
            var players = FuzzyPlayerSearch(searchTerm).ToList();

            switch (players.Count)
            {
                case 0:
                    FoundNoResults(player, groupId, searchTerm);
                    break;
                case 1:
                    FoundSinglePlayer(player, groupId, list, listType, players);
                    break;
                case > 1:
                    FoundMultiplePlayers(player, groupId, searchTerm, players);
                    break;
            }

            ServerChannel?.BroadcastUniquePacket(GeneratePacket);
        }

        private void FoundSinglePlayer(IPlayer player, int groupId, ICollection<Player> list, string listType, List<IPlayer> players)
        {
            var result = players.First();
            var plr = list.SingleOrDefault(p => p.Id == result.PlayerUID);
            if (plr is not null)
            {
                list.Remove(plr);
                Sapi.SendMessage(player, groupId,
                    LangEx.FeatureString("Knapster", $"{listType}.PlayerRemoved", result.PlayerName, SubCommandName),
                    EnumChatType.Notification);
                return;
            }

            list.Add(new Player(result.PlayerUID, result.PlayerName));
            ModSettings.World.Save(Settings);
            Sapi.SendMessage(player, groupId,
                LangEx.FeatureString("Knapster", $"{listType}.PlayerAdded", result.PlayerName, SubCommandName),
                EnumChatType.Notification);
        }

        private void FoundNoResults(IPlayer player, int groupId, string searchTerm)
        {
            Sapi.SendMessage(player, groupId,
                LangEx.FeatureString("Knapster", "PlayerSearch.NoResults", searchTerm), EnumChatType.Notification);
        }

        private void FoundMultiplePlayers(IPlayer player, int groupId, string searchTerm, List<IPlayer> players)
        {
            var sb = new StringBuilder();
            sb.Append(LangEx.FeatureString("Knapster", "PlayerSearch.MultipleResults", searchTerm));
            foreach (var p in players)
            {
                sb.Append($" - {p.PlayerName} (PID: {p.PlayerUID})");
            }

            Sapi.SendMessage(player, groupId, sb.ToString(), EnumChatType.Notification);
        }

        private static IEnumerable<IPlayer> FuzzyPlayerSearch(string searchTerm)
        {
            var onlineClients = ApiEx.ServerMain.PlayersByUid;
            if (onlineClients.ContainsKey(searchTerm)) return new List<IPlayer> { onlineClients[searchTerm] };
            return onlineClients.Values
                .Where(client => client.PlayerName.StartsWith(searchTerm, true, CultureInfo.InvariantCulture));
        }
    }
}