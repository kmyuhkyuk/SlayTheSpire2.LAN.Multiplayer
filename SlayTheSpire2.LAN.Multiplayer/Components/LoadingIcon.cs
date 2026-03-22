using Godot;

namespace SlayTheSpire2.LAN.Multiplayer.Components
{
    internal partial class LoadingIcon : TextureRect
    {
        private Tween? _tween;

        public override void _Ready()
        {
            var svgImage = new Image();

            svgImage.LoadSvgFromString(
                "<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"64px\" viewBox=\"0 -960 960 960\" width=\"64px\" fill=\"#FFFFFF\"><path d=\"M323-111q-73-31-127-85t-85-127q-31-73-31-157t31-157q31-73 85-127t127-85q73-31 157-31 12 0 21 9t9 21q0 12-9 21t-21 9q-141 0-240.5 99.5T140-480q0 141 99.5 240.5T480-140q141 0 240.5-99.5T820-480q0-12 9-21t21-9q12 0 21 9t9 21q0 84-31 157t-85 127q-54 54-127 85T480-80q-84 0-157-31Z\"/></svg>");

            Texture = ImageTexture.CreateFromImage(svgImage);

            PivotOffset = Size / 2;

            _tween = CreateTween().SetLoops();

            _tween.TweenProperty(this, "rotation", Mathf.DegToRad(360), 2f).AsRelative();
        }
    }
}