using ApacheTech.Common.DependencyInjection.Abstractions;
using ApacheTech.Common.DependencyInjection.Abstractions.Extensions;
using ApacheTech.VintageMods.FluentChatCommands;
using Gantry.Core;
using Gantry.Core.DependencyInjection;
using Gantry.Services.FileSystem.Abstractions.Contracts;
using Gantry.Services.FileSystem.Configuration.Extensions;
using Gantry.Services.FileSystem.DependencyInjection;
using Gantry.Services.FileSystem.Enums;
using Gantry.Services.HarmonyPatches.DependencyInjection;
using Gantry.Services.Network.DependencyInjection;
using JetBrains.Annotations;
using Vintagestory.API.Server;

namespace ApacheTech.VintageMods.Knapster
{
    /// <summary>
    ///     Entry-point for the mod. This class will configure and build the IOC Container, and Service list for the rest of the mod.
    ///     
    ///     Registrations performed within this class should be global scope; by convention, features should aim to be as stand-alone as they can be.
    /// </summary>
    /// <remarks>
    ///     Only one derived instance of this class should be added to any single mod within
    ///     the VintageMods domain. This class will enable Dependency Injection, and add all
    ///     of the domain services. Derived instances should only have minimal functionality, 
    ///     instantiating, and adding Application specific services to the IOC Container.
    /// </remarks>
    /// <seealso cref="ModHost" />
    [UsedImplicitly]
    internal sealed class Program : ModHost
    {
        protected override void ConfigureUniversalModServices(IServiceCollection services)
        {
            services.AddFileSystemService(o => o.RegisterSettingsFiles = false);
            services.AddHarmonyPatchingService(o => o.AutoPatchModAssembly = true);
            services.AddNetworkService();
#if DEBUG
            HarmonyLib.Harmony.DEBUG = true;
#endif
        }

        /// <summary>
        ///     Full start to the mod on the server side
        /// <br /><br />In 1.17+ do NOT use this to add or update behaviors or attributes or other fixed properties of any block, item or entity, in code (additional to what is read from JSON).
        /// It is already too late to do that here, it will not be seen client-side. Instead, code which needs to do that should be registered for event sapi.Event.AssetsFinalizers.  See VSSurvivalMod system BlockReinforcement.cs for an example.
        /// </summary>
        /// <param name="api"></param>
        public override void StartServerSide(ICoreServerAPI api)
        {
            IOC.Services.Resolve<IFileSystemService>()
                .RegisterSettingsFile("settings-world-server.json", FileScope.World);

            FluentChat.ServerCommand("knapster")
                .RequiresPrivilege(Privilege.controlserver)
                .RegisterWith(api)
                .HasDescription(LangEx.FeatureString("Knapster", "ServerCommandDescription"));
        }

        /// <summary>
        ///     If this mod allows runtime reloading, you must implement this method to unregister any listeners / handlers
        /// </summary>
        public override void Dispose()
        {
            ApiEx.Run(
                FluentChat.DisposeClientCommands, 
                FluentChat.DisposeServerCommands);
        }
    }
}