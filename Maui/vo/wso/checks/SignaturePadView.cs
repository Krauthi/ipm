using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace iPMCloud.Mobile.Controls;

public class SignaturePadView : GraphicsView, IDrawable
{
    private readonly List<List<PointF>> _paths = new();
    private List<PointF> _currentPath;
    private bool _isDrawing;

    public SignaturePadView()
    {
        Drawable = this;
        BackgroundColor = Colors.White;

        StartInteraction += OnStartInteraction;
        DragInteraction += OnDragInteraction;
        EndInteraction += OnEndInteraction;
    }

    // Bindable Property für Signatur als Base64
    public static readonly BindableProperty SignatureImageProperty =
        BindableProperty.Create(
            nameof(SignatureImage),
            typeof(string),
            typeof(SignaturePadView),
            default(string),
            BindingMode.TwoWay);

    public string SignatureImage
    {
        get => (string)GetValue(SignatureImageProperty);
        set => SetValue(SignatureImageProperty, value);
    }

    // Bindable Property für Stift-Farbe
    public static readonly BindableProperty StrokeColorProperty =
        BindableProperty.Create(
            nameof(StrokeColor),
            typeof(Color),
            typeof(SignaturePadView),
            Colors.Black);

    public Color StrokeColor
    {
        get => (Color)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    // Bindable Property für Stift-Dicke
    public static readonly BindableProperty StrokeWidthProperty =
        BindableProperty.Create(
            nameof(StrokeWidth),
            typeof(float),
            typeof(SignaturePadView),
            2f);

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    private void OnStartInteraction(object sender, TouchEventArgs e)
    {
        _isDrawing = true;
        _currentPath = new List<PointF> { e.Touches[0] };
        _paths.Add(_currentPath);
        Invalidate();
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        if (_isDrawing && _currentPath != null)
        {
            var point = e.Touches[0];

            if (_currentPath.Count == 0 || Distance(_currentPath[^1], point) >= 0.5f)
            {
                _currentPath.Add(point);
                Invalidate();
            }
        }
    }

    private void OnEndInteraction(object sender, TouchEventArgs e)
    {
        _isDrawing = false;
        _currentPath = null;
        Invalidate();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // Hintergrund zeichnen
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        // Signatur zeichnen
        canvas.StrokeColor = StrokeColor;
        canvas.StrokeSize = StrokeWidth;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;

        foreach (var points in _paths)
        {
            DrawSmoothPath(canvas, points);
        }
    }

    private void DrawSmoothPath(ICanvas canvas, List<PointF> points)
    {
        if (points == null || points.Count == 0)
            return;

        if (points.Count == 1)
        {
            canvas.FillColor = StrokeColor;
            canvas.FillCircle(points[0].X, points[0].Y, StrokeWidth / 2f);
            return;
        }

        if (points.Count == 2)
        {
            var linePath = new PathF();
            linePath.MoveTo(points[0]);
            linePath.LineTo(points[1]);
            canvas.DrawPath(linePath);
            return;
        }

        var smoothPath = new PathF();
        smoothPath.MoveTo(points[0]);

        for (int i = 1; i < points.Count - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            var midpoint = GetMidpoint(current, next);
            smoothPath.QuadTo(current.X, current.Y, midpoint.X, midpoint.Y);
        }

        smoothPath.LineTo(points[^1]);
        canvas.DrawPath(smoothPath);
    }

    private static PointF GetMidpoint(PointF first, PointF second)
    {
        return new PointF((first.X + second.X) / 2f, (first.Y + second.Y) / 2f);
    }

    private static float Distance(PointF first, PointF second)
    {
        var dx = second.X - first.X;
        var dy = second.Y - first.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    // Signatur löschen
    public void Clear()
    {
        _paths.Clear();
        _currentPath = null;
        SignatureImage = null;
        Invalidate();
    }

    // Prüfen ob Signatur vorhanden
    public bool IsBlank => _paths.Count == 0;

    // Signatur als Byte-Array exportieren
    public async Task<byte[]> GetImageStreamAsync(SignatureImageFormat format = SignatureImageFormat.Png)
    {
        if (IsBlank)
            return null;

        // Screenshot der GraphicsView erstellen
        var result = await this.CaptureAsync();
        if (result == null)
            return null;

        using var stream = await result.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var bytes = memoryStream.ToArray();

        // Als Base64 speichern
        SignatureImage = Convert.ToBase64String(bytes);

        return bytes;
    }

    // Signatur laden
    public void LoadSignature(string base64Image)
    {
        if (string.IsNullOrEmpty(base64Image))
        {
            Clear();
            return;
        }

        SignatureImage = base64Image;
        // TODO: Base64 zu Pfaden konvertieren (optional)
    }
}

public enum SignatureImageFormat
{
    Png,
    Jpeg
}
