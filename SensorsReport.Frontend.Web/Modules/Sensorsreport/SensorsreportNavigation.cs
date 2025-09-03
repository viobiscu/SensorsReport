using ApiKey = SensorsReport.Frontend.SensorsReport.ApiKey;
using Users = SensorsReport.Frontend.SensorsReport.User;
using Groups = SensorsReport.Frontend.SensorsReport.Group;
using EmailTemplates = SensorsReport.Frontend.SensorsReport.EmailTemplate;
using SmsTemplates = SensorsReport.Frontend.SensorsReport.SmsTemplate;
using NotificationRules = SensorsReport.Frontend.SensorsReport.NotificationRule;
using AlarmRule = SensorsReport.Frontend.SensorsReport.AlarmRule.Pages;
using Logrule = SensorsReport.Frontend.SensorsReport.LogRule.Pages;
using VariableTemplate = SensorsReport.Frontend.SensorsReport.VariableTemplate.Pages;
using AlarmType = SensorsReport.Frontend.SensorsReport.AlarmType.Pages;
using NotificationUser = SensorsReport.Frontend.SensorsReport.NotificationUsers.Pages;
using Notification = SensorsReport.Frontend.SensorsReport.Notification.Pages;
using Alarm = SensorsReport.Frontend.SensorsReport.Alarm.Pages;
using Sensors = SensorsReport.Frontend.SensorsReport.Sensor.Pages;
using SensorsHistory = SensorsReport.Frontend.SensorsReport.SensorHistory.Pages;

[assembly: NavigationMenu(1000, "Sensors", icon: "fa-network-wired")]
[assembly: NavigationLink(1100, "Sensors/Sensors", typeof(Sensors.SensorPage), icon: "fa-microchip")]
[assembly: NavigationLink(1200, "Sensors/Alarms", typeof(Alarm.AlarmPage), icon: "fa-bell")]

[assembly: NavigationMenu(2000, "Maintenance", icon: "fa-tools")]
[assembly: NavigationMenu(2100, "Maintenance/Rules", icon: "fa-cogs")]
[assembly: NavigationLink(2200, "Maintenance/Rules/Log Rules", typeof(Logrule.LogRulePage), icon: "fa-tasks")]
[assembly: NavigationLink(2300, "Maintenance/Rules/Alarm Rules", typeof(AlarmRule.AlarmRulePage), icon: "fa-clipboard-list")]
[assembly: NavigationLink(2400, "Maintenance/Rules/Notification Rules", typeof(NotificationRules.Pages.NotificationRulePage), icon: "fa-clipboard-check")]

[assembly: NavigationMenu(3000, "Template", icon: "fa-file")]
[assembly: NavigationLink(3100, "Template/Email Templates", typeof(EmailTemplates.Pages.EmailTemplatePage), icon: "fa-mail-bulk")]
[assembly: NavigationLink(3200, "Template/SMS Templates", typeof(SmsTemplates.Pages.SmsTemplatePage), icon: "fa-sms")]
[assembly: NavigationLink(3300, "Template/Variable Templates", typeof(VariableTemplate.VariableTemplatePage), icon: "fa-code")]

[assembly: NavigationMenu(4000, "Users", icon: "fa-user-shield")]
[assembly: NavigationLink(4100, "Users/Users", typeof(Users.Pages.UserPage), icon: "fa-users")]
[assembly: NavigationLink(4200, "Users/Groups", typeof(Groups.Pages.GroupPage), icon: "fa-users-cog")]

[assembly: NavigationMenu(5000, "Management", icon: "fa-gear")]
[assembly: NavigationLink(5100, "Management/Alarm Types", typeof(AlarmType.AlarmTypePage), icon: "fa-tags")]
[assembly: NavigationLink(5200, "Management/Notification Users", typeof(NotificationUser.NotificationUsersPage), icon: "fa-user-check")]
[assembly: NavigationLink(5300, "Management/Notifications", typeof(Notification.NotificationPage), icon: "fa-bullhorn")]
[assembly: NavigationLink(5400, "Management/Api Key", typeof(ApiKey.Pages.ApiKeyPage), icon: "fa-key")]
[assembly: NavigationLink(5500, "Management/Sensor History", typeof(SensorsHistory.SensorHistoryPage), icon: "fa-history")]