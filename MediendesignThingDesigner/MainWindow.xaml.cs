using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediendesignThingDesigner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private class MyCommand<T> : ICommand
        {
            public MyCommand(Action<T> action, T parameter) => (Action, Parameter) = (action, parameter);

            public Action<T> Action { get; }
            public T Parameter { get; }

            public event EventHandler? CanExecuteChanged;

            public bool CanExecute(object? parameter) => true;

            public void Execute(object? parameter)
            {
                Action(Parameter);
            }
        }

        private SolidColorBrush YellowBrush = new(Colors.Yellow);
        private SolidColorBrush BlackBrush = new(Colors.Black);

        private void ToggleColors(Shape shape) => shape.Fill = shape.Fill == YellowBrush ? BlackBrush : YellowBrush;

        private void FillWithTriangles(object sender, RoutedEventArgs e)
        {
            var rect = new Rectangle() { Width = 300, Height = 300, Tag = new Point(0, 0) };
            var rects = SplitRectHorz(rect, 4);
            rects = rects.SelectMany(r => SplitRectVert(r, 4));
            var tris = rects.SelectMany(SplitRectIntoTriangles);
            Draw(tris);
        }

        private void ApplyBehaviour(Shape shape) => shape.InputBindings.Add(new MouseBinding(new MyCommand<Shape>(ToggleColors, shape), new(MouseAction.LeftClick)));

        private void Draw(IEnumerable<Shape> shapes)
        {
            foreach (var shape in shapes)
            {
                Draw(shape);
            }
        }

        private void Draw(Shape shape)
        {
            shape.Fill = new SolidColorBrush(Colors.Black);
            shape.Stroke = new SolidColorBrush(Colors.White);
            var location = (Point)shape.Tag;
            Canvas.SetLeft(shape, location.X);
            Canvas.SetTop(shape, location.Y);
            ApplyBehaviour(shape);
            DrawingCanvas.Children.Add(shape);
            //rect.Visibility = Visibility.Visible;
        }

        private IEnumerable<Rectangle> SplitRectHorz(Rectangle rect, int parts)
        {
            Point location = (Point)rect.Tag;
            double width = rect.Width / parts;
            for (int i = 0; i < parts; i++)
            {
                yield return CreateRect(location.X + i * width, location.Y, width, rect.Height);
            }
        }

        private IEnumerable<Rectangle> SplitRectVert(Rectangle rect, int parts)
        {
            Point location = (Point)rect.Tag;
            double height = rect.Height / parts;
            for (int i = 0; i < parts; i++)
            {
                yield return CreateRect(location.X, location.Y + i * height, rect.Width, height);
            }
        }

        private IEnumerable<Polygon> SplitRectIntoTriangles(Rectangle rect)
        {
            Point center = new(rect.Width / 2, rect.Height / 2);
            Polygon north = new();
            north.Points.Add(new(0d, 0d));
            north.Points.Add(new(rect.Width, 0d));
            north.Points.Add(center);
            north.Tag = rect.Tag;
            yield return north;

            Polygon west = new();
            west.Points.Add(new(0d, 0d));
            west.Points.Add(center);
            west.Points.Add(new(0d, rect.Height));
            west.Tag = rect.Tag;
            yield return west;

            Polygon south = new();
            south.Points.Add(new(0d, rect.Height));
            south.Points.Add(center);
            south.Points.Add(new(rect.Width, rect.Height));
            south.Tag = rect.Tag;
            yield return south;

            Polygon east = new();
            east.Points.Add(new(rect.Width, 0d));
            east.Points.Add(center);
            east.Points.Add(new(rect.Width, rect.Height));
            east.Tag = rect.Tag;
            yield return east;
        }

        private Rectangle CreateRect(double x, double y, double width, double height) => new Rectangle() { Width = width, Height = height, Tag = new Point(x, y) };
    }
}
