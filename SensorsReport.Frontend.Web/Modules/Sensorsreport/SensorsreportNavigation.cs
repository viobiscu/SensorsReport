using ApiKey = SensorsReport.Frontend.SensorsReport.ApiKey;

[assembly: NavigationMenu(1000, "SensorsReport", icon: "fa-network-wired")]
[assembly: NavigationLink(1100, "SensorsReport/Api Key", typeof(ApiKey.Pages.ApiKeyPage), icon: "fa-key")]
