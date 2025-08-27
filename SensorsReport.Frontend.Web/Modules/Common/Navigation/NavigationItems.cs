[assembly: NavigationGroup("SensorsReport.Frontend", "fa-home", Default = true)]

[assembly: NavigationSection("SensorsReport.Frontend/Demo Modules",
    Include = ["Northwind", "Basic Samples", "Advanced Samples", "Application Samples", "UI Elements", "Theme Samples"])]

[assembly: NavigationSection("SensorsReport.Frontend/Pro Features",
    Include = ["Meeting", "Work Log"])]

[assembly: NavigationGroup(9000, "Administration", icon: "fa-tools")]

[assembly: NavigationSection("Administration/General", Default = true)]

[assembly: NavigationSection("Administration/Localization",
    Include = ["Administration/Languages", "Administration/Translations"])]

[assembly: NavigationSection("Administration/Security",
    Include = ["Administration/Roles", "Administration/User Management"])]

[assembly: NavigationLink(1000, "Dashboard", url: "~/", permission: "", icon: "fa-tachometer")]