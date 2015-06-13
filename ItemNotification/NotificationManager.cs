using System;
using System.Collections.Generic;
using LeagueSharp;
using SharpDX.Direct3D9;

namespace ItemNotification
{
    class NotificationManager
    {
        private static readonly int max = 4;
        private static readonly bool drawInEndScene = true; // false to use OnDraw
        private static readonly List<Notification> notifications = new List<Notification>();
        private static readonly List<Notification> queue = new List<Notification>();
        
        static NotificationManager()
        {
            Game.OnUpdate += Game_OnGameUpdate;
            if (drawInEndScene)
                Drawing.OnEndScene += Drawing_OnDraw;
            else
                Drawing.OnDraw += Drawing_OnDraw;
            Drawing.OnPreReset += Drawing_OnPreReset;
            Drawing.OnPostReset += Drawing_OnPostReset;
        }

        private static void Drawing_OnPreReset(EventArgs args)
        {
            foreach (var notification in notifications)
                notification.OnPreReset();
        }

        private static void Drawing_OnPostReset(EventArgs args)
        {
            foreach (var notification in notifications)
                notification.OnPostReset();
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var notification in notifications)
                notification.OnDraw();
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            List<Notification> list = null;
            foreach (var notification in notifications)
            {
                notification.OnUpdate();
                if (notification.Delete)
                {
                    if (list == null) list = new List<Notification>();
                    list.Add(notification);
                }
            }

            if (list != null)
            {
                foreach (var notification in list)
                    RemoveNotification(notification);
            }
        }

        public static Notification AddNotification(Texture texture, string text, int duration = -1)
        {
            var notification = new Notification(texture, text, duration);
            if (notifications.Count >= max)
                queue.Add(notification);
            else
            {
                notifications.Add(notification);
                notification.Show();
            }
            return notification;
        }

        public static int GetNotificationIndex(Notification notification)
        {
            return notifications.IndexOf(notification);
        }

        public static bool RemoveNotification(Notification notification)
        {
            if (notifications.Count == max && queue.Count > 0)
            {
                notifications.Add(queue[0]);
                queue[0].Show();
                queue.RemoveAt(0);
            }
            notification.Dispose();
            return notifications.Remove(notification);
        }
    }
}
