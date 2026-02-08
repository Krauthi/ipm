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
    }

    private void OnDragInteraction(object sender, TouchEventArgs e)
    {
        if (_isDrawing && _currentPath != null)
        {
            _currentPath.Add(e.Touches[0]);
            Invalidate();
        }
    }

    private void OnEndInteraction(object sender, TouchEventArgs e)
    {
        _isDrawing = false;
        _currentPath = null;
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

        foreach (var path in _paths)
        {
            if (path.Count > 1)
            {
                var pathF = new PathF();
                pathF.MoveTo(path[0]);

                for (int i = 1; i < path.Count; i++)
                {
                    pathF.LineTo(path[i]);
                }

                canvas.DrawPath(pathF);
            }
        }
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