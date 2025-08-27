using ApiKey = SensorsReport.Frontend.SensorsReport.ApiKey;
using Users = SensorsReport.Frontend.SensorsReport.User;

[assembly: NavigationMenu(1000, "SensorsReport", icon: "fa-network-wired")]
[assembly: NavigationLink(1100, "SensorsReport/Api Key", typeof(ApiKey.Pages.ApiKeyPage), icon: "fa-key")]
[assembly: NavigationLink(1200, "SensorsReport/Users", typeof(Users.Pages.UserPage), icon: "fa-users")]
