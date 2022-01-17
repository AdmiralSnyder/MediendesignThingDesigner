using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;

namespace MediendesignThingDesigner;

public class SVGRenderer
{
    public bool Absolute { get { return true; } }
    public double Width { get; private set; }
    public double Height { get; private set; }

    public MemoryStream SVGDocument { get; private set; }
    private XmlTextWriter Writer { get { return m_writer; } }
    private Canvas SourceCanvas;

    private XmlTextWriter m_writer;

    /// <summary>
    /// Creates a new SVGRenderer, which will produce an output SVG with the specified width and height.
    /// </summary>
    /// <param name="width">Width of the output SVG.</param>
    /// <param name="height">Height of the output SVG.</param>
    public SVGRenderer(double width, double height)
    {
        SVGDocument = new();
        m_writer = new(SVGDocument, Encoding.UTF8) { Formatting = Formatting.Indented };
        Width = width;
        Height = height;
    }

    public void Begin()
    {
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        Writer.WriteStartDocument();
        Writer.WriteComment(" Generator: " + ("LALALAL") + ", " + version + " ");
        Writer.WriteDocType("svg", "-//W3C//DTD SVG 1.1//EN", "http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd", null);
        Writer.WriteStartElement("svg", "http://www.w3.org/2000/svg");
        Writer.WriteAttributeString("version", "1.1");
        Writer.WriteAttributeString("width", this.Width.ToString());
        Writer.WriteAttributeString("height", this.Height.ToString());
    }

    public void End()
    {
        Writer.WriteEndDocument();
        Writer.Flush();
    }

    public void StartSection(object tag)
    {
        // Do nothing.
    }

    public void DrawLine(Point start, Point end, double thickness, Color? color = null)
    {
        m_writer.WriteStartElement("line");
        m_writer.WriteAttributeString("x1", start.X.ToString());
        m_writer.WriteAttributeString("y1", start.Y.ToString());
        m_writer.WriteAttributeString("x2", end.X.ToString());
        m_writer.WriteAttributeString("y2", end.Y.ToString());
        m_writer.WriteAttributeString("style", $"stroke:{GetRGBString(color)};stroke-linecap:square;stroke-width:{thickness}");
        m_writer.WriteEndElement();
    }

    private static string GetRGBString(Color? color)
    {
        return color.HasValue ? $"rgb({color.Value.R},{color.Value.G},{color.Value.B})" : "rgb(0,0,0)";
    }

    public void DrawRectangle(Point start, Size size, double thickness, bool fill = false, Color? color = null)
    {
        m_writer.WriteStartElement("rect");
        m_writer.WriteAttributeString("x", start.X.ToString());
        m_writer.WriteAttributeString("y", start.Y.ToString());
        m_writer.WriteAttributeString("width", size.Width.ToString());
        m_writer.WriteAttributeString("height", size.Height.ToString());
        m_writer.WriteAttributeString("style", $"fill-opacity:0;stroke:{GetRGBString(color)};stroke-width:{thickness.ToString()}");
        m_writer.WriteEndElement();
    }

    public void DrawEllipse(Point centre, double radiusX, double radiusY, double thickness, bool fill = false, Color? color = null)
    {
        string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

        m_writer.WriteStartElement("ellipse");
        m_writer.WriteAttributeString("cx", centre.X.ToString());
        m_writer.WriteAttributeString("cy", centre.Y.ToString());
        m_writer.WriteAttributeString("rx", radiusX.ToString());
        m_writer.WriteAttributeString("ry", radiusY.ToString());
        m_writer.WriteAttributeString("style", $"fill-opacity:{fillOpacity};fill:{GetRGBString(color)};stroke:rgb(0,0,0);stroke-width:{thickness}");
        m_writer.WriteEndElement();
    }

    internal void DrawShape(Shape shape)
    {
        if (shape is Line line) DrawLine(line);
        else if (shape is Polygon poly) DrawPolygon(poly);
        else throw new NotImplementedException();
    }

    internal void DrawLine(Line line) => DrawLine(new(line.X1, line.Y1), new(line.X2, line.Y2), line.StrokeThickness, StrokeColor(line));

    private Color? StrokeColor(Shape shape) => shape.Stroke is SolidColorBrush b ? b.Color : null;

    private Color? FillColor(Shape shape) => shape.Fill is SolidColorBrush b ? b.Color : null;

    internal void DrawPolygon(Polygon poly)
    {
        var x = Canvas.GetLeft(poly);
        var y = Canvas.GetTop(poly);
        Point firstPoint = poly.Points[0];
        firstPoint.Offset(x, y);
        var commands = poly.Points.Skip(1).Append(poly.Points[0]).Select(p =>
        {
            p.Offset(x, y);
            return new LineTo(p, false);
        });
        DrawPath(firstPoint, commands, poly.StrokeThickness, poly.Fill is not null, StrokeColor(poly), FillColor(poly));

    }


    public void DrawPath(Point start, IEnumerable<PathCommand> commands, double thickness, bool fill = false, Color? strokeColor = null, Color? fillColor = null)
    {
        string data = $"{new MoveTo(start)}";
        foreach (PathCommand pathCommand in commands)
        {
            data += $" {pathCommand}";
        }

        string fillOpacity = ((fill ? 255f : 0f) / 255f).ToString();

        m_writer.WriteStartElement("path");
        m_writer.WriteAttributeString("d", data);
        m_writer.WriteAttributeString("style", $"fill-opacity:{fillOpacity};fill:{GetRGBString(fillColor)};stroke:{GetRGBString(strokeColor)};stroke-width:{thickness}");
        m_writer.WriteEndElement();
    }

    //public void DrawText(Point anchor, TextAlignment alignment, IEnumerable<TextRun> textRuns, Brush bru = null)
    //{
    //    m_writer.WriteStartElement("text");
    //    m_writer.WriteAttributeString("x", anchor.X.ToString());
    //    m_writer.WriteAttributeString("y", anchor.Y.ToString());

    //    string textAnchor = "start";
    //    if (alignment == TextAlignment.BottomCentre || alignment == TextAlignment.CentreCentre || alignment == Render.TextAlignment.TopCentre)
    //        textAnchor = "middle";
    //    else if (alignment == TextAlignment.BottomRight || alignment == TextAlignment.CentreRight || alignment == Render.TextAlignment.TopRight)
    //        textAnchor = "end";

    //    string dy = "-0.3em";
    //    if (alignment == TextAlignment.CentreCentre || alignment == TextAlignment.CentreLeft || alignment == TextAlignment.CentreRight)
    //        dy = ".3em";
    //    else if (alignment == TextAlignment.TopCentre || alignment == TextAlignment.TopLeft || alignment == TextAlignment.TopRight)
    //        dy = "1em";

    //    m_writer.WriteAttributeString("style", "font-family:Arial;font-size:" + textRuns.FirstOrDefault().Formatting.Size.ToString() + ";text-anchor:" + textAnchor);
    //    m_writer.WriteAttributeString("dy", dy);

    //    foreach (TextRun run in textRuns)
    //    {
    //        if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
    //            m_writer.WriteStartElement("tspan");
    //        if (run.Formatting.FormattingType == TextRunFormattingType.Subscript)
    //        {
    //            m_writer.WriteAttributeString("baseline-shift", "sub");
    //            m_writer.WriteAttributeString("style", "font-size:0.8em");
    //        }
    //        else if (run.Formatting.FormattingType == TextRunFormattingType.Superscript)
    //        {
    //            m_writer.WriteAttributeString("baseline-shift", "super");
    //            m_writer.WriteAttributeString("style", "font-size:0.8em");
    //        }
    //        m_writer.WriteString(run.Text);
    //        if (run.Formatting.FormattingType != TextRunFormattingType.Normal)
    //            m_writer.WriteEndElement();
    //    }

    //    m_writer.WriteEndElement();
    //}
}

public abstract class PathCommand
{
    public PathCommand(char character, bool relative = false) => Character = relative ? Char.ToLower(character) : Char.ToUpper(character);
    public bool Absolute { get; set; }
    protected abstract string SvgText();
    public char Character;
    public override string ToString() => $"{Character}{SvgText()}";
}

public class MoveTo : PathCommand
{
    public MoveTo(Point point, bool relative = false) : base('m', relative) => Point = point;

    public Point Point { get; set; }
    protected override string SvgText() => $"{Point.X} {Point.Y}";
}

public class LineTo : PathCommand
{
    public LineTo(Point point, bool relative = false) : base('l', relative) => Point = point;
    public Point Point { get; set; }
    protected override string SvgText() => $"{Point.X} {Point.Y}";
}

