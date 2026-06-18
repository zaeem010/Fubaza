using Fubaza.Application.Core.Entities;
using Google.Api.Gax;

namespace Fubaza.Application.Core.Constants
{
    public static class NotificationConstants
    {
        // English Notifications
        public static class En
        {
            public static class Titles
            {
                public const string PlayerAdded = "Player Added";
                public const string PlayerUpdated = "Player Updated";
                public const string PlayerRemoved = "Player Removed";

                public const string OfficialAdded = "Official Added";
                public const string OfficialUpdated = "Official Updated";
                public const string OfficialRemoved = "Official Removed";

                public const string PostPublishedFacebook = "Post Shared on Facebook";
                public const string PostPublishedInstagram = "Post Shared on Instagram";
                public const string ScheduledPostReminder = "Scheduled Post Reminder";
            }

            public static class Bodies
            {
                public const string PlayerAdded = "{0} has been added as a player to your club.";
                public const string PlayerUpdated = "{0}'s information as a player has been updated.";
                public const string PlayerRemoved = "{0} has been removed from players of your club.";

                public const string OfficialAdded = "{0} has been added as {1} to your club.";
                public const string OfficialUpdated = "{0}'s information as {1} has been updated.";
                public const string OfficialRemoved = "{0} ({1}) has been removed from officials of your club.";

                public const string PostPublishedFacebook = "Your scheduled post has been shared on Facebook.";
                public const string PostPublishedInstagram = "Your scheduled post has been shared on Instagram.";
                public const string ScheduledPostReminder = "Your {0} post will be published in 1 hour.";
            }
        }

        // German Notifications
        public static class De
        {
            public static class Titles
            {
                public const string PlayerAdded = "Spieler hinzugefügt";
                public const string PlayerUpdated = "Spieler aktualisiert";
                public const string PlayerRemoved = "Spieler entfernt";

                public const string OfficialAdded = "Offizieller hinzugefügt";
                public const string OfficialUpdated = "Offizieller aktualisiert";
                public const string OfficialRemoved = "Offizieller entfernt";

                public const string PostPublishedFacebook = "Beitrag auf Facebook geteilt";
                public const string PostPublishedInstagram = "Beitrag auf Instagram geteilt";
                public const string ScheduledPostReminder = "Erinnerung an geplanten Beitrag";
            }

            public static class Bodies
            {
                public const string PlayerAdded = "{0} wurde als Spieler zu Ihrem Verein hinzugefügt.";
                public const string PlayerUpdated = "Die Informationen von {0} als Spieler wurden aktualisiert.";
                public const string PlayerRemoved = "{0} wurde aus den Spielern Ihres Vereins entfernt.";

                public const string OfficialAdded = "{0} wurde als {1} zu Ihrem Verein hinzugefügt.";
                public const string OfficialUpdated = "Die Informationen von {0} als {1} wurden aktualisiert.";
                public const string OfficialRemoved = "{0} ({1}) wurde aus den Offiziellen Ihres Vereins entfernt.";

                public const string PostPublishedFacebook = "Ihr geplanter Beitrag wurde auf Facebook geteilt.";
                public const string PostPublishedInstagram = "Ihr geplanter Beitrag wurde auf Instagram geteilt.";
                public const string ScheduledPostReminder = "Ihr Beitrag wird in einer Stunde veröffentlicht.";
            }
        }
    }

}
