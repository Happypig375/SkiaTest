using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using Typography.OpenFont;

namespace SkiaTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            const float ptSize = 80;
            const string text = "𝑑";
            const int windowWidth = 350;
            const int windowHeight = 300;
            var t = new OpenFontReader().Read(GetType().Assembly.GetManifestResourceStream(Array.Find(GetType().Assembly.GetManifestResourceNames(), n => n.EndsWith(".otf"))));
            var g = t.Lookup(char.ConvertToUtf32(text, 0));

            Size = new Size(windowWidth, windowHeight);
            Text = "Typography -> SkiaSharp";

            var view = new SKControl();
            view.PaintSurface += (_, E) =>
            {
                var tx = new SkiaTx();
                var c = E.Surface.Canvas;
                c.Scale(1, -1);
                c.Translate(0, -Height / 4);
                
                tx.Read(g._ownerCffFont, g._cff1GlyphData, t.CalculateScaleToPixelFromPointSize(ptSize));
                c.DrawPath(tx.Path, new SKPaint { Color = SKColors.Black, Style = SKPaintStyle.Stroke });
            };
            view.Dock = DockStyle.Fill;
            Controls.Add(view);

            var gdi = new Form { Text = "Typography -> Gdi+", Size = new Size(windowWidth, windowHeight) };
            gdi.Paint += (_, E) =>
            {
                var tx = new GdiTx();
                var G = E.Graphics;
                G.ScaleTransform(1, -1);
                G.TranslateTransform(0, -Height / 4);

                tx.Read(g._ownerCffFont, g._cff1GlyphData, t.CalculateScaleToPixelFromPointSize(ptSize));
                G.DrawPath(Pens.Black, tx.Path);
            };
            gdi.Show();
        }

        class SkiaTx : IGlyphTranslator
        {
            public SKPath Path { get; private set; }

            public void BeginRead(int contourCount) => Path = new SKPath();

            public void CloseContour() => Path.Close();

            public void Curve3(float x1, float y1, float x2, float y2) => Path.QuadTo(x1, y1, x2, y2);

            public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => Path.CubicTo(x1, y1, x2, y2, x3, y3);

            public void EndRead() { }

            public void LineTo(float x1, float y1) => Path.LineTo(x1, y1);

            public void MoveTo(float x0, float y0) => Path.MoveTo(x0, y0);
        }

        class GdiTx : IGlyphTranslator
        {
            PointF lastPoint;
            public System.Drawing.Drawing2D.GraphicsPath Path { get; private set; }

            public void BeginRead(int contourCount) => Path = new System.Drawing.Drawing2D.GraphicsPath();

            public void CloseContour() => Path.CloseFigure();

            public void Curve3(float x1, float y1, float x2, float y2) => throw null;

            public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => Path.AddBezier(lastPoint, new PointF(x1, y1), new PointF(x2, y2), lastPoint = new PointF(x3, y3));

            public void EndRead() { }

            public void LineTo(float x1, float y1) => Path.AddLine(lastPoint, lastPoint = new PointF(x1, y1));

            public void MoveTo(float x0, float y0) => lastPoint = new PointF(x0, y0);
        }
    }
}
