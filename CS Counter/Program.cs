using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace CS_Counter
{
    class CSUpdate
    {
        public int Count { get; set; }
        public int Difference { get; set; }
        public DateTime Timestamp { get; set; }
    }
    class CsCounter
    {
        private static Menu _menu2;
        private static MenuItem _menuenable2;
        private static MenuItem _menuenable3;
        private static MenuItem _menuenable4;
        private static MenuItem _xPos;
        private static MenuItem _yPos;
        private static MenuItem _highlight;
        private static MenuItem _highlightTime;

        private static readonly Render.Text Text = new Render.Text(0, 0, "", 12, new ColorBGRA(255, 0, 0, 255), "Verdana");
        private static readonly Render.Text TextHighlight = new Render.Text(0, 0, "", 16, new ColorBGRA(255, 0, 0, 255));
        private static Line _line;
        private static Texture CdFrameTexture;
        private static Sprite Sprite;
        private static int X;
        private static int Y;

        private static Dictionary<int, CSUpdate> cs = new Dictionary<int, CSUpdate>(); 

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            TextHighlight.TextFontDescription = new FontDescription
            {
                FaceName = "Verdana",
                Height = 18,
                OutputPrecision = FontPrecision.Default,
                Quality = FontQuality.Default,
                Weight = FontWeight.UltraBold
            };

            _menu2 = new Menu("CS Counter", "menu2", true);

            _line = new Line(Drawing.Direct3DDevice);

            _menuenable2 = new MenuItem("menu.drawings.enable2", "CS Count").SetValue(true);
            _menu2.AddItem(_menuenable2);
            _menuenable3 = new MenuItem("menu.drawings.enable3", "My CS Count").SetValue(true);
            _menu2.AddItem(_menuenable3);
            _menuenable4 = new MenuItem("menu.drawings.enable4", "Allies CS Count").SetValue(true);
            _menu2.AddItem(_menuenable4);
            _xPos = new MenuItem("menu.Calc.calc5", "X - Position").SetValue(new Slider(0, -100));
            _menu2.AddItem(_xPos);
            _yPos = new MenuItem("menu.Calc.calc6", "Y - Position").SetValue(new Slider(0, -100));
            _menu2.AddItem(_yPos);
            _highlight = new MenuItem("menu.highlight", "Highlight CS Change").SetValue(new Circle(true, Color.Yellow));
            _menu2.AddItem(_highlight);
            _highlightTime = new MenuItem("menu.highlightTime", "Highlight CS Change - Length (ms)").SetValue(new Slider(1500, 200, 5000));
            _menu2.AddItem(_highlightTime);

            _menu2.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (hero.IsDead ||
                    !hero.IsVisible ||
                    !_menuenable2.GetValue<bool>() ||
                    (hero.IsAlly && !_menuenable4.GetValue<bool>() && !hero.IsMe) ||
                    (hero.IsMe && !_menuenable3.GetValue<bool>()))
                    continue;

                var newCS = hero.MinionsKilled + hero.NeutralMinionsKilled + hero.SuperMonsterKilled;
                var pos = Drawing.WorldToScreen(hero.Position);
                pos.X -= 50 + _xPos.GetValue<Slider>().Value;
                pos.Y += 20 + _yPos.GetValue<Slider>().Value;

                _line.Begin();
                _line.Draw(new[] { new Vector2(pos.X, pos.Y - 2), new Vector2(pos.X + 100, pos.Y - 2) }, new ColorBGRA(255, 255, 255, 255));
                _line.End();

                _line.Begin();
                _line.Draw(new[] { new Vector2(pos.X, pos.Y + 14), new Vector2(pos.X + 100, pos.Y + 14) }, new ColorBGRA(255, 255, 255, 255));
                _line.End();

                _line.Begin();
                _line.Draw(new[] { new Vector2(pos.X, pos.Y - 2), new Vector2(pos.X, pos.Y + 14) }, new ColorBGRA(255, 255, 255, 255));
                _line.End();

                _line.Begin();
                _line.Draw(new[] { new Vector2(pos.X + 100, pos.Y - 2), new Vector2(pos.X + 100, pos.Y + 14) }, new ColorBGRA(255, 255, 255, 255));
                _line.End();

                Text.X = (int)pos.X;
                Text.X += 110 / 6;

                Text.Y = (int)pos.Y;
                Text.text = "CS Count: " + newCS;
                Text.Color = new ColorBGRA(255, 255, 255, 255);
                Text.OnEndScene();

                CSUpdate csUpdate;
                if (!cs.TryGetValue(hero.NetworkId, out csUpdate))
                {
                    csUpdate = new CSUpdate() { Count = -1 };
                    cs[hero.NetworkId] = csUpdate;
                }

                if (csUpdate.Count != newCS)
                {
                    if (csUpdate.Count >= 0) // only show an update after the initial setup
                    {
                        csUpdate.Difference += newCS - csUpdate.Count;
                        csUpdate.Timestamp = DateTime.Now;
                    }
                    csUpdate.Count = newCS;
                }

                TimeSpan timeSinceLastCS = DateTime.Now - csUpdate.Timestamp;
                if (_highlight.IsActive() && timeSinceLastCS.TotalMilliseconds < _highlightTime.GetValue<Slider>().Value)
                {
                    var color = _highlight.GetValue<Circle>().Color;
                    TextHighlight.Color = new ColorBGRA(color.R, color.G, color.B, color.A);
                    TextHighlight.X = (int)pos.X + 100 + 5;
                    TextHighlight.Y = (int)pos.Y - 3;
                    TextHighlight.text = "+" + csUpdate.Difference;
                    TextHighlight.OnEndScene();
                }
                else
                    csUpdate.Difference = 0;
            }
        }
    }
}
