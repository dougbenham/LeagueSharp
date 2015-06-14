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
using Color = System.Drawing.Color;

namespace ItemNotification
{
    class Item
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }

    class Program
    {

        private static ConcurrentDictionary<int, Texture> itemTexture;
        private static ConcurrentDictionary<int, Texture> playerTexture;
        private static ConcurrentDictionary<int, List<Item>> playerItems;
        public static Menu MainMenu;
        public static Menu MonitorMenu;
        public static Menu NotificationsMenu;
        public static Menu ItemsMenu;
        
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_DomainUnload;
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            foreach (var texture in playerTexture.Values.Where(texture => !texture.IsDisposed))
                texture.Dispose();
        }

        static Texture CreateTextureFromItemId(int id, int width, int height)
        {
            var texture = CreateTextureFromResource("_" + id, width, height);
            if (texture != null)
                return texture;

            var bitmap = DataDragon.GetItemBitmap(id);
            if (bitmap != null)
                return CreateTextureFromBitmap(bitmap, width, height);

            return null;
        }

        static Texture CreateTextureFromBitmap(Bitmap bitmap, int width, int height)
        {
            bitmap = bitmap.Resize(width, height);
            return Texture.FromMemory(
                    Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), bitmap.Width, bitmap.Height, 0,
                    Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);
        }

        static Texture CreateTextureFromResource(string name, int width, int height)
        {
            var bitmap = (Resources.ResourceManager.GetObject(name) as Bitmap);
            if (bitmap == null) return null;

            return CreateTextureFromBitmap(bitmap, width, height);
        }

        static void AddPlayer(Obj_AI_Hero hero)
        {
            playerTexture.TryAdd(hero.NetworkId, CreateTextureFromResource(hero.ChampionName, Notification.TextureWidth, Notification.TextureHeight));
            playerItems.TryAdd(hero.NetworkId, hero.InventoryItems.Select(inventoryItem => new Item {Id = (int) inventoryItem.Id, DisplayName = inventoryItem.DisplayName}).ToList());
        }

        static Texture GetItemTexture(int id)
        {
            if (itemTexture.ContainsKey(id))
                return itemTexture[id];
            else
            {
                var texture = CreateTextureFromItemId(id, Notification.TextureWidth, Notification.TextureHeight);
                itemTexture.TryAdd(id, texture);
                return texture;
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            MainMenu = new Menu("ItemNotification", "ItemNotification", true);
            MonitorMenu = new Menu("Monitor", "Monitor");
            MonitorMenu.AddItem(new MenuItem("Self", "Self").SetValue(new Circle(false, Color.Black)));
            MonitorMenu.AddItem(new MenuItem("Allies", "Allies").SetValue(new Circle(true, Color.Green)));
            MonitorMenu.AddItem(new MenuItem("Enemies", "Enemies").SetValue(new Circle(true, Color.Red)));
            MainMenu.AddSubMenu(MonitorMenu);
            NotificationsMenu = new Menu("Notifications (Press F5 to recompute texture sizes)", "Notifications");
            NotificationsMenu.AddItem(new MenuItem("Max", "Maximum Notifications Visible").SetValue(new Slider(3, 1, 6)));
            NotificationsMenu.AddItem(new MenuItem("Delay", "Notification Delay").SetValue(new Slider(5000, 50, 10000)));
            NotificationsMenu.AddItem(new MenuItem("TextureWidth", "Image Width (Hero & Item)").SetValue(new Slider(64, 10, 96)));
            NotificationsMenu.AddItem(new MenuItem("TextureHeight", "Image Height (Hero & Item)").SetValue(new Slider(64, 10, 96)));
            NotificationsMenu.AddItem(new MenuItem("BorderWidth", "Border Width").SetValue(new Slider(3, 0, 10)));
            NotificationsMenu.AddItem(new MenuItem("DisplayItemName", "Display Item Name").SetValue(false));
            NotificationsMenu.AddItem(new MenuItem("TextWidth", "Item Name Area Width").SetValue(new Slider(180, 50, 400)));
            MainMenu.AddSubMenu(NotificationsMenu);
            ItemsMenu = new Menu("Items", "Items");
            ItemsMenu.AddItem(new MenuItem("IncludePotions", "Potions").SetValue(false));
            ItemsMenu.AddItem(new MenuItem("IncludeTrinkets", "Trinkets").SetValue(false));
            ItemsMenu.AddItem(new MenuItem("IncludeStealthWards", "Stealth Wards").SetValue(false));
            ItemsMenu.AddItem(new MenuItem("IncludePinkWards", "Vision Wards").SetValue(true));
            MainMenu.AddSubMenu(ItemsMenu);
            MainMenu.AddItem(new MenuItem("Test", "Test").SetValue(false));
            MainMenu.AddToMainMenu();

            itemTexture = new ConcurrentDictionary<int, Texture>();
            playerTexture = new ConcurrentDictionary<int, Texture>();
            playerItems = new ConcurrentDictionary<int, List<Item>>();

            foreach (var hero in HeroManager.AllHeroes.Where(hero => hero.IsValid && hero.IsVisible))
                AddPlayer(hero);

            Game.OnUpdate += Game_OnUpdate;
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (MainMenu.Item("Test").GetValue<bool>())
            {
                MainMenu.Item("Test").SetValue(false);

                var bitmap = (Resources.ResourceManager.GetObject("Ashe") as Bitmap).Resize(Notification.TextureWidth, Notification.TextureHeight);
                var texture = Texture.FromMemory(
                        Drawing.Direct3DDevice, (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[])), bitmap.Width, bitmap.Height, 0,
                        Usage.None, Format.A1, Pool.Managed, Filter.Default, Filter.Default, 0);

                var color = MonitorMenu.Item("Self").GetValue<Circle>().Color;
                NotificationManager.AddNotification(texture, GetItemTexture((int)ItemId.Infinity_Edge), "Self", NotificationsMenu.Item("Delay").GetValue<Slider>().Value).BorderColor = new ColorBGRA(color.R, color.G, color.B, 0xff);
                color = MonitorMenu.Item("Allies").GetValue<Circle>().Color;
                NotificationManager.AddNotification(texture, GetItemTexture(3751), "Ally", NotificationsMenu.Item("Delay").GetValue<Slider>().Value).BorderColor = new ColorBGRA(color.R, color.G, color.B, 0xff);
                color = MonitorMenu.Item("Enemies").GetValue<Circle>().Color;
                NotificationManager.AddNotification(texture, GetItemTexture(3285), "Enemy", NotificationsMenu.Item("Delay").GetValue<Slider>().Value).BorderColor = new ColorBGRA(color.R, color.G, color.B, 0xff);
            }
            foreach (var hero in HeroManager.AllHeroes.Where(hero => hero.IsValid && hero.IsVisible))
            {
                if (hero.IsMe && !MonitorMenu.Item("Self").GetValue<Circle>().Active) continue;
                if (hero.IsAlly && !MonitorMenu.Item("Allies").GetValue<Circle>().Active) continue;
                if (hero.IsEnemy && !MonitorMenu.Item("Enemies").GetValue<Circle>().Active) continue;

                if (playerItems.ContainsKey(hero.NetworkId))
                {
                    var list = playerItems[hero.NetworkId];
                    foreach (var inventoryItem in hero.InventoryItems)
                    {
                        if (!ItemsMenu.Item("IncludePotions").GetValue<bool>())
                        {
                            if (inventoryItem.Id == ItemId.Health_Potion || 
                                inventoryItem.Id == ItemId.Mana_Potion || 
                                inventoryItem.Id == (ItemId) 2009) continue; // Biscuit
                        }
                        if (!ItemsMenu.Item("IncludeTrinkets").GetValue<bool>())
                        {
                            if (inventoryItem.DisplayName.Contains("Trinket") ||
                                inventoryItem.Id.ToString().Contains("Bonetooth_Necklace")) continue;
                        }
                        if (!ItemsMenu.Item("IncludeStealthWards").GetValue<bool>())
                        {
                            if (inventoryItem.Id == ItemId.Stealth_Ward) continue;
                        }
                        if (!ItemsMenu.Item("IncludePinkWards").GetValue<bool>())
                        {
                            if (inventoryItem.Id == ItemId.Vision_Ward) continue;
                        }

                        if (!list.Exists(item => item.Id == (int)inventoryItem.Id))
                        {
                            list.Add(new Item() { Id = (int)inventoryItem.Id, DisplayName = inventoryItem.DisplayName });
                            var notification = NotificationManager.AddNotification(playerTexture[hero.NetworkId], GetItemTexture((int)inventoryItem.Id), inventoryItem.DisplayName, NotificationsMenu.Item("Delay").GetValue<Slider>().Value);
                            var color = new Color();
                            if (hero.IsMe)
                                color = MonitorMenu.Item("Self").GetValue<Circle>().Color;
                            else if (hero.IsAlly)
                                color = MonitorMenu.Item("Allies").GetValue<Circle>().Color;
                            else if (hero.IsEnemy)
                                color = MonitorMenu.Item("Enemies").GetValue<Circle>().Color;
                            notification.BorderColor = new ColorBGRA(color.R, color.G, color.B, 0xff);
                        }
                    }
                }
                else
                    AddPlayer(hero);
            }
        }
    }
}
