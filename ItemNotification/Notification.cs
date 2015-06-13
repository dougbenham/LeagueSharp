using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace ItemNotification
{
    class Notification
    {
        public static bool DisplayItemName
        {
            get { return Program.NotificationsMenu.Item("DisplayItemName").GetValue<bool>(); }
        }

        public static int TextWidth
        {
            get { return Program.NotificationsMenu.Item("TextWidth").GetValue<Slider>().Value; }
        }

        public static int TextureWidth
        {
            get { return Program.NotificationsMenu.Item("TextureWidth").GetValue<Slider>().Value; }
        }

        public static int TextureHeight
        {
            get { return Program.NotificationsMenu.Item("TextureHeight").GetValue<Slider>().Value; }
        }

        public static int BorderWidth
        {
            get { return Program.NotificationsMenu.Item("BorderWidth").GetValue<Slider>().Value; }
        }

        public static int Width
        {
            get { return TextureWidth * 2 + BorderWidth * 2 + (DisplayItemName ? TextWidth : 0); }
        }

        /// <param name="heroTexture"></param>
        /// <param name="itemTexture"></param>
        /// <param name="text"></param>
        /// <param name="duration">Duration (-1 for Infinite)</param>
        public Notification(Texture heroTexture, Texture itemTexture, string text, int duration)
        {
            HeroTexture = heroTexture;
            ItemTexture = itemTexture;
            Text = text;
            this.duration = duration;
            TextColor.A = 0xFF;
            BoxColor.A = 0xFF;
            BorderColor.A = 0xFF;
            Font.PreloadText(text);
        }

        public void Show()
        {
            initial = DateTime.Now;
            position = new Vector2((Drawing.Width / 2.0f) - (Width / 2.0f), CalculateVerticalOffset());
        }

        #region Functions

        private byte GetAlpha()
        {
            var now = DateTime.Now;
            var end = initial.AddMilliseconds(duration);
            if (now > end) return 0;

            return (byte)(((end - now).TotalMilliseconds * 255 / duration));
        }

        private float CalculateVerticalOffset()
        {
            return NotificationManager.GetNotificationIndex(this) * Height;
        }

        private static Vector2[] GetLine(float x1, float y1, float x2, float y2)
        {
            return new[] { new Vector2(x1, y1), new Vector2(x2, y2) };
        }

        #endregion

        #region Public Fields

        public Texture HeroTexture;
        public Texture ItemTexture;

        public string Text;

        public bool Delete { get; set; }
        
        public static float Height
        {
            get { return TextureHeight + BorderWidth * 2; }
        }

        public float TextXOffset
        {
            get
            {
                var x = 0;
                if (HeroTexture != null) x += TextureWidth - 1;
                if (ItemTexture != null) x += TextureWidth - 1;
                return x;
            }
        }

        #region Colors

        /// <summary>
        ///     Notification's Text Color
        /// </summary>
        public ColorBGRA TextColor = new ColorBGRA(255f, 255f, 255f, 255f);

        /// <summary>
        ///     Notification's Box Color
        /// </summary>
        public ColorBGRA BoxColor = new ColorBGRA(0f, 0f, 0f, 255f);

        /// <summary>
        ///     Notification's Border Color
        /// </summary>
        public ColorBGRA BorderColor = new ColorBGRA(255f, 255f, 255f, 255f);

        /// <summary>
        ///     Notification's Picture Color
        /// </summary>
        public ColorBGRA PictureColor = new ColorBGRA(255f, 255f, 255f, 255f);

        /// <summary>
        ///     Notification's Font
        /// </summary>
        public Font Font = new Font(
            Drawing.Direct3DDevice, 0xE, 0x0, FontWeight.DoNotCare, 0x0, false, FontCharacterSet.Default,
            FontPrecision.Default, FontQuality.Antialiased, FontPitchAndFamily.DontCare | FontPitchAndFamily.Decorative,
            "Tahoma");

        #endregion

        #endregion

        #region Private Fields

        /// <summary>
        ///     Locally saved Notification's Duration
        /// </summary>
        private int duration;

        /// <summary>
        ///     Locally saved position
        /// </summary>
        private Vector2 position;

        /// <summary>
        ///     Locally saved update position
        /// </summary>
        private Vector2 updatePosition;

        private DateTime initial;

        /// <summary>
        ///     Locally saved Line
        /// </summary>
        private readonly Line line = new Line(Drawing.Direct3DDevice)
        {
            Antialias = false,
            GLLines = true
        };

        /// <summary>
        ///     Locally saved Sprite
        /// </summary>
        private readonly Sprite sprite = new Sprite(Drawing.Direct3DDevice);

        #endregion

        #region Required Functions

        /// <summary>
        ///     Called for Drawing onto screen
        /// </summary>
        public void OnDraw()
        {
            if (Delete) return;

            #region Outline

            line.Width = Width;

            line.Begin();
            line.Draw(GetLine(position.X + Width / 2.0f, position.Y, position.X + Width / 2.0f, position.Y + Height), BorderColor);
            line.End();

            #endregion

            #region Box

            line.Width = Width - BorderWidth * 2;

            line.Begin();
            line.Draw(GetLine(position.X + Width / 2.0f, position.Y + BorderWidth, position.X + Width / 2.0f, position.Y + Height - BorderWidth), BoxColor);
            line.End();

            #endregion

            sprite.Begin();

            #region Picture

            if (HeroTexture != null)
            {
                var tmp = sprite.Transform;
                sprite.Transform = Matrix.Translation(position.X + BorderWidth, position.Y + BorderWidth, 0);
                sprite.Draw(HeroTexture, PictureColor);
                sprite.Transform = tmp;
            }

            if (ItemTexture != null)
            {
                var tmp = sprite.Transform;
                sprite.Transform = Matrix.Translation(position.X + BorderWidth + TextureWidth, position.Y + BorderWidth, 0);
                sprite.Draw(ItemTexture, PictureColor);
                sprite.Transform = tmp;
            }

            #endregion

            #region Text

            if (DisplayItemName)
            {
                var textDimension = Font.MeasureText(sprite, Text);
                var finalText = Text;

                if (textDimension.Width + 5 > TextWidth)
                {
                    for (var i = Text.Length - 1; i > 3; --i)
                    {
                        var text = Text.Substring(0, i);
                        var textWidth = Font.MeasureText(sprite, text).Width;

                        if (textWidth + 5 <= TextWidth || i == 4)
                        {
                            finalText = text.Substring(0, text.Length - 3) + "...";
                            break;
                        }
                    }
                }

                textDimension = Font.MeasureText(sprite, finalText);

                var rectangle = new Rectangle((int) (position.X + TextXOffset), (int) position.Y, (int) TextWidth, (int) Height);

                Font.DrawText(
                    sprite, finalText, rectangle.TopLeft.X + (rectangle.Width - textDimension.Width) / 2,
                    rectangle.TopLeft.Y + (rectangle.Height - textDimension.Height) / 2, TextColor);
            }

            #endregion

            sprite.End();
        }

        public void OnUpdate()
        {
            if (Delete) return;

            #region Duration handler

            if (duration > 0)
                TextColor.A = BoxColor.A = BorderColor.A = PictureColor.A = GetAlpha();

            if (duration > 0 && TextColor.A == 0x0 && BoxColor.A == 0x0 && BorderColor.A == 0x0 && PictureColor.A == 0x0)
            {
                Delete = true;
                return;
            }

            #endregion

            #region Mouse

            var mouseLocation = Drawing.WorldToScreen(Game.CursorPos);
            if (Utils.IsUnderRectangle(mouseLocation, position.X, position.Y, Width, Height))
            {
                initial = DateTime.Now;
                TextColor.A = 0xFF;
                BoxColor.A = 0xFF;
                BorderColor.A = 0xFF;
            }

            #endregion

            #region Movement

            if (updatePosition != Vector2.Zero)
            {
                bool moved = false;
                if (position.X < updatePosition.X)
                {
                    position.X += 1f;
                    moved = true;
                }
                if (position.X > updatePosition.X)
                {
                    position.X -= 1f;
                    moved = true;
                }
                if (position.Y < updatePosition.Y)
                {
                    position.Y += 1f;
                    moved = true;
                }
                if (position.Y > updatePosition.Y)
                {
                    position.Y -= 1f;
                    moved = true;
                }
                if (!moved)
                    updatePosition = Vector2.Zero;
            }

            if (updatePosition == Vector2.Zero)
            {
                var location = CalculateVerticalOffset(); // calculates where we should be
                if (position.Y > location)
                {
                    updatePosition = new Vector2(position.X, location);
                }
            }

            #endregion
        }

        public void OnPreReset()
        {
            sprite.OnLostDevice();
        }

        public void OnPostReset()
        {
            sprite.OnResetDevice();
        }
        
        #endregion

        #region Disposal

        /// <summary>
        ///     IDisposable callback
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///     Safe disposal callback
        /// </summary>
        /// <param name="safe">Is Pre-Finailized / Safe (values not cleared by GC)</param>
        private void Dispose(bool safe)
        {
            Delete = true;

            if (safe)
            {
                Text = null;

                TextColor = new ColorBGRA();
                BoxColor = new ColorBGRA();
                BorderColor = new ColorBGRA();

                Font = null;

                duration = 0;

                position = Vector2.Zero;
                updatePosition = Vector2.Zero;
            }
        }
        
        ~Notification()
        {
            Dispose(false);
        }

        #endregion
    }
}
