using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace iPMCloud.Mobile.vo
{

    public static class SignatureRender
    {
        public static byte[] RenderToPng(
        IReadOnlyList<IDrawingLine> lines,
        int targetWidth,
        int targetHeight,
        SKColor background,
        SKColor strokeColor,
        float strokeWidth,
        float padding = 12f)
        {
            using var surface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;
            canvas.Clear(background);

            if (lines is null || lines.Count == 0)
                return Encode(surface);

            // Bounding Box
            double minX = double.MaxValue, minY = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue;
            int pointCount = 0;

            foreach (var line in lines)
            {
                if (line?.Points is null) continue;

                foreach (var p in line.Points)
                {
                    pointCount++;
                    minX = Math.Min(minX, p.X);
                    minY = Math.Min(minY, p.Y);
                    maxX = Math.Max(maxX, p.X);
                    maxY = Math.Max(maxY, p.Y);
                }
            }

            if (pointCount < 2)
                return Encode(surface);

            var spanX = Math.Max(1e-6, maxX - minX);
            var spanY = Math.Max(1e-6, maxY - minY);

            var availableW = Math.Max(1f, targetWidth - 2 * padding);
            var availableH = Math.Max(1f, targetHeight - 2 * padding);

            var scale = (float)Math.Min(availableW / spanX, availableH / spanY);

            // Zentrieren
            var contentW = (float)(spanX * scale);
            var contentH = (float)(spanY * scale);

            var offsetX = (float)(padding + (availableW - contentW) / 2.0 - minX * scale);
            var offsetY = (float)(padding + (availableH - contentH) / 2.0 - minY * scale);

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = strokeColor,
                // StrokeWidth NICHT 1:1 übernehmen, sondern mit skalieren (sonst evtl. kaum sichtbar)
                StrokeWidth = Math.Max(1f, strokeWidth) * scale,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            foreach (var line in lines)
            {
                if (line?.Points is null || line.Points.Count < 2) continue;

                using var path = new SKPath();

                var p0 = line.Points[0];
                path.MoveTo((float)p0.X * scale + offsetX, (float)p0.Y * scale + offsetY);

                for (int i = 1; i < line.Points.Count; i++)
                {
                    var p = line.Points[i];
                    path.LineTo((float)p.X * scale + offsetX, (float)p.Y * scale + offsetY);
                }

                canvas.DrawPath(path, paint);
            }

            return Encode(surface);
        }

        private static byte[] Encode(SKSurface surface)
        {
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}