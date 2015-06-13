using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ItemNotification.Properties;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace ItemNotification
{
    class Item
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }

    class Program
    {
        private static ConcurrentDictionary<int, Texture> playerPicture;
        private static ConcurrentDictionary<int, List<Item>> playerItems;
        
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_DomainUnload;
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            foreach (var texture in playerPicture.Values.Where(texture => !texture.IsDisposed))
                texture.Dispose();
        }

        static void AddPlayer(Obj_AI_Hero hero)
        {
            var bitmap = (Resources.ResourceManager.GetObject(hero.ChampionName) as Bitmap).Resize(Notification.TextureWidth, Notification.TextureHeight);
            var texture = Texture.FromMemory(
                    Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), bitmap.Width, bitmap.Height, 0,
                    Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
            playerPicture.TryAdd(hero.NetworkId, texture);
            
            playerItems.TryAdd(hero.NetworkId, hero.InventoryItems.Select(inventoryItem => new Item { Id = (int)inventoryItem.Id, DisplayName = inventoryItem.DisplayName }).ToList());
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            playerPicture = new ConcurrentDictionary<int, Texture>();
            playerItems = new ConcurrentDictionary<int, List<Item>>();

            /*var bitmap = (Resources.ResourceManager.GetObject("Ashe") as Bitmap).Resize(Notification.TextureWidth, Notification.TextureHeight);
            var texture = Texture.FromMemory(
                    Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), bitmap.Width, bitmap.Height, 0,
                    Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
            NotificationManager.AddNotification(texture, "Test", 5000).BoxColor = new ColorBGRA(0x70, 0, 0, 0xff);*/

            foreach (var hero in HeroManager.AllHeroes.Where(hero => hero.IsValid && hero.IsVisible && !hero.IsMe))
                AddPlayer(hero);

            Game.OnUpdate += Game_OnUpdate;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            foreach (var hero in HeroManager.AllHeroes.Where(hero => hero.IsValid && hero.IsVisible && !hero.IsMe))
            {
                if (playerItems.ContainsKey(hero.NetworkId))
                {
                    var list = playerItems[hero.NetworkId];
                    foreach (var inventoryItem in hero.InventoryItems)
                    {
                        if (inventoryItem.DisplayName.Contains("Potion")) continue;
                        if (inventoryItem.Id == (ItemId) 2009) continue; // Biscuit
                        if (inventoryItem.DisplayName.Contains("Trinket")) continue;

                        if (!list.Exists(item => item.Id == (int)inventoryItem.Id))
                        {
                            list.Add(new Item() { Id = (int)inventoryItem.Id, DisplayName = inventoryItem.DisplayName });
                            var notification = NotificationManager.AddNotification(playerPicture[hero.NetworkId], inventoryItem.DisplayName, 5000);
                            if (hero.IsEnemy)
                                notification.BoxColor = new ColorBGRA(0x70, 0, 0, 0xff);
                        }
                    }
                }
                else
                    AddPlayer(hero);
            }
        }
    }
}
