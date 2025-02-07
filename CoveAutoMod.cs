using Cove.Server.Plugins;
using Cove.Server.Actor;
using Cove.Server;
using Newtonsoft.Json;
using System.IO;
using System.Net; //For webclient
using System.Collections.Specialized;

// Change the namespace and class name!
namespace CoveAutoMod
{
    public class ModConfig
    {
        [JsonProperty("warnwords")]
        public required string[] WarnWords { get; set; }
        [JsonProperty("kickwords")]
        public required string[] KickWords { get; set; }
        [JsonProperty("banwords")]
        public required string[] BanWords { get; set; }
        [JsonProperty("webhookurl")]
        public string LogWebhook { get; set; }
    }
    public class CoveAutoMod : CovePlugin
    {
        public ModConfig modConfig;
        public bool WebhookToggle = false;
        string eventLog = "automodlog.json";
        public CoveAutoMod(CoveServer server) : base(server) { }

        public override void onInit()
        {
            base.onInit();

            Log("Enabling the AutoMod!");

            string json = File.ReadAllText("automodconfig.json");
            modConfig = JsonConvert.DeserializeObject<ModConfig>(json);

            if(!modConfig.LogWebhook.Equals("")) WebhookToggle = true;

            RegisterCommand("report", (player, args) =>
            {
                if(args.Length < 2) return;

                string reportedUser = args[0];
                string reportMessage = string.Join(" ", args[1..args.Length]);
                userReport(player, reportedUser, reportMessage);
            });
        }

        public override void onChatMessage(WFPlayer sender, string message)
        {
            base.onChatMessage(sender, message);

            message = message.ToLower();

            foreach (string banWord in modConfig.BanWords)
            {
                if (message.Contains(banWord))
                {
                    BanPlayer(sender);
                    logModerationAction(sender, message, "ban");
                    break;
                }
            }
            foreach (string kickWord in modConfig.KickWords)
            {
                if (message.Contains(kickWord))
                {
                    KickPlayer(sender);
                    logModerationAction(sender, message, "kick");
                    break;
                }
            }
            foreach (string warnWord in modConfig.WarnWords)
            {
                if(message.Contains(warnWord))
                {
                    SendPlayerChatMessage(sender, "You said a word we don't like! Probably don't do it again! The word was: " + warnWord);
                    logModerationAction(sender, message, "warn");
                    break;
                }
            }
        }

        public void userReport(WFPlayer sender, string reportedUser, string message)
        {
            string formatMessage = "----------------------------------------" + "\n"
                + "User: " + sender.Username + " has sent in a report. Please review this and do something pls" + "\n"
                + "Reported User: " + reportedUser + "\n"
                + "Report Message: " + message;

            using (StreamWriter writer = File.AppendText(eventLog))
            {
                writer.WriteLine(formatMessage);
            }

            if(WebhookToggle) sendWebhookMessage(formatMessage);

            SendPlayerChatMessage(sender, "Your report has been recieved! We will look at it and make a decision, thanks!");
        }

        public void logModerationAction(WFPlayer sender, string message, string action)
        {            
            string logMessage = "----------------------------------------" + "\n"
                + "Player Name: " + sender.Username + "\n"
                + "Player Message: " + message + "\n"
                + "Time:" + System.DateTime.Now + "\n"
                + "Action Taken: " + action;

            using (StreamWriter writer = File.AppendText(eventLog))
            {
                writer.WriteLine(logMessage);
            }

            if(WebhookToggle)
            {
                sendWebhookMessage(logMessage);
            }
        }

        public void sendWebhookMessage(string message)
        {
            NameValueCollection discordValues = new NameValueCollection();
            discordValues.Add("username", "CoveAutoMod");
            discordValues.Add("avatar_url", "https://i.imgur.com/5pQ9KKr.png");
            discordValues.Add("content", message);
            new WebClient().UploadValues(modConfig.LogWebhook, discordValues);            
        }

        public override void onEnd()
        {
            base.onEnd();
            UnregisterCommand("report");
        }
    }
}