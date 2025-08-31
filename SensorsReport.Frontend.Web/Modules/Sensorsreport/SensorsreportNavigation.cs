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

//[assembly: NavigationMenu(1000, "SensorsReport", icon: "fa-network-wired")]
[assembly: NavigationLink(1000, "Alarms", typeof(Alarm.AlarmPage), icon: "fa-bell")]
[assembly: NavigationLink(1100, "Alarm Rules", typeof(AlarmRule.AlarmRulePage), icon: "fa-clipboard-list")]
[assembly: NavigationLink(1200, "Alarm Types", typeof(AlarmType.AlarmTypePage), icon: "fa-tags")]
[assembly: NavigationLink(1300, "Notification Rules", typeof(NotificationRules.Pages.NotificationRulePage), icon: "fa-clipboard-check")]
[assembly: NavigationLink(1400, "Notification Users", typeof(NotificationUser.NotificationUsersPage), icon: "fa-user-check")]
[assembly: NavigationLink(1500, "Notifications", typeof(Notification.NotificationPage), icon: "fa-bullhorn")]
[assembly: NavigationLink(1600, "Log Rules", typeof(Logrule.LogRulePage), icon: "fa-tasks")]

[assembly: NavigationLink(2100, "Email Templates", typeof(EmailTemplates.Pages.EmailTemplatePage), icon: "fa-mail-bulk")]
[assembly: NavigationLink(2200, "SMS Templates", typeof(SmsTemplates.Pages.SmsTemplatePage), icon: "fa-sms")]
[assembly: NavigationLink(2300, "Variable Templates", typeof(VariableTemplate.VariableTemplatePage), icon: "fa-code")]

[assembly: NavigationLink(9100, "Api Key", typeof(ApiKey.Pages.ApiKeyPage), icon: "fa-key")]
[assembly: NavigationLink(9200, "Users", typeof(Users.Pages.UserPage), icon: "fa-users")]
[assembly: NavigationLink(9300, "Groups", typeof(Groups.Pages.GroupPage), icon: "fa-users-cog")]