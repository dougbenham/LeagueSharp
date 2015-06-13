using System;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using SharpDX.Direct3D9;

namespace ItemNotification
{
    class Notification
    {
        /// <param name="duration">Duration (-1 for Infinite)</param>
        public Notification(Texture texture, string text, int duration)
        {
            Texture = texture;
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
            return NotificationManager.GetNotificationIndex(this) * Height + VerticalOffset;
        }

        private static Vector2[] GetLine(float x1, float y1, float x2, float y2)
        {
            return new[] { new Vector2(x1, y1), new Vector2(x2, y2) };
        }

        #endregion

        #region Public Fields

        /// <summary>
        ///     Notification's Picture
        /// </summary>
        public Texture Texture;

        /// <summary>
        ///     Notification's Text
        /// </summary>
        public string Text;

        /// <summary>
        ///     Indicates if notification is going to be deleted
        /// </summary>
        public bool Delete { get; set; }

        public static float Width = 300f;

        public static float Height
        {
            get { return TextureHeight + 2; }
        }

        public float TextXOffset
        {
            get
            {
                return Texture == null ? 0 : TextureWidth - 1;
            }
        }

        public float TextWidth
        {
            get { return Width - TextXOffset; }
        }

        public float VerticalOffset = 0;

        public static int TextureWidth = 32;
        public static int TextureHeight = 32;

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

            #region Box

            line.Width = Width;

            line.Begin();
            line.Draw(GetLine(position.X + Width / 2, position.Y, position.X + Width / 2, position.Y + Height), BoxColor);
            line.End();

            #endregion

            #region Outline

            line.Width = 1;

            line.Begin();
            line.Draw(GetLine(position.X, position.Y, position.X + Width, position.Y), BorderColor); // TOP
            line.Draw(GetLine(position.X, position.Y, position.X, position.Y + Height), BorderColor); // LEFT
            line.Draw(GetLine(position.X + Width, position.Y, position.X + Width, position.Y + Height), BorderColor); // RIGHT
            line.Draw(GetLine(position.X, position.Y + Height, position.X + Width, position.Y + Height), BorderColor); // BOTTOM
            line.End();

            #endregion

            sprite.Begin();

            #region Picture

            if (Texture != null)
            {
                var tmp = sprite.Transform;
                sprite.Transform = Matrix.Translation(position.X + 1, position.Y + 1, 0);
                sprite.Draw(Texture, PictureColor);
                sprite.Transform = tmp;
            }

            #endregion

            #region Text
            
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

            var rectangle = new Rectangle((int)(position.X + TextXOffset), (int)position.Y, (int)TextWidth, (int)Height);

            Font.DrawText(
                sprite, finalText, rectangle.TopLeft.X + (rectangle.Width - textDimension.Width) / 2,
                rectangle.TopLeft.Y + (rectangle.Height - textDimension.Height) / 2, TextColor);
            
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
