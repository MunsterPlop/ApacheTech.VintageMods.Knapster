﻿using Vintagestory.API.Common;

// ReSharper disable StringLiteralTypo

[assembly: ModDependency("game", "1.17.2")]
[assembly: ModDependency("survival", "1.17.2")]

[assembly:ModInfo(
    "Knapster",
    "knapster",
    Description = "Easier knapping, clayforming, and smithing, for those with low manual dexterity.",
    Side = "Universal",
    Version = "1.0.0-rc.1",
    RequiredOnClient = true,
    RequiredOnServer = true,
    NetworkVersion = "1.0.0",
    Website = "https://apachegaming.net",
    Contributors = new[] { "Apache" },
    Authors = new []{ "ApacheTech Solutions" })]