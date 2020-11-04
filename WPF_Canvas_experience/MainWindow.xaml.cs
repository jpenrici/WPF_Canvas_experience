using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace WPF_Canvas_experience
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Field
        Point mousePosition;
        Point fromPosition;
        Point toPosition;
        History history;
        int counter;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            // WPF experience
            Start();
        }

        #region Start
        private void Start()
        {
            ConfigWindow();
            ConfigMenu();
            ConfigCanvas();
            ConfigConsole();

            // Init
            counter = 0;
            history = new History();
        }
        #endregion

        #region Config
        private void ConfigWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            //WindowStyle = WindowStyle.None;
            Height = 400;
            Width = 600;
            Title = "WPF Canvas Experience";
        }

        private void ConfigMenu()
        {
            menuSave.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuSaveAs.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuExport.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuQuit.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuUndo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuRedo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuBackground.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            menuAbout.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);

            string[] figures = {
                "Rectangle",
                "Ellipse",
                "Image",
            };

            foreach (var item in figures)
            {
                var newItem = new MenuItem();
                newItem.Header = item;
                newItem.Name = "menu" + item;
                newItem.Click += new RoutedEventHandler(this.NewFigure);
                menuAdd.Items.Add(newItem);
            }
        }

        private void ConfigCanvas()
        {
            drawingArea.HorizontalAlignment = HorizontalAlignment.Center;
            drawingArea.Background = Brushes.White;
            drawingArea.Cursor = Cursors.Arrow;
            drawingArea.Height = 250;
            drawingArea.Width = 580;

            drawingArea.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(CanvasLeftButtonDown);
            drawingArea.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(CanvasLeftButtonUp);
            drawingArea.PreviewMouseMove += new MouseEventHandler(CanvasMouseMove);
        }

        private void ConfigConsole()
        {
            lblInform.Content = "Ready ...";
            lblInform.HorizontalAlignment = HorizontalAlignment.Left;
            lblInform.MinHeight = 20;

            txtboxConsole.Background = Brushes.Black;
            txtboxConsole.Foreground = Brushes.Green;
            txtboxConsole.IsReadOnly = true;
            txtboxConsole.Height = 50;
            txtboxConsole.Width = 535;

            btnUndo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            btnRedo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
        }
        #endregion Config

        #region Structures
        class Status
        {
            string action;
            string figure;
            Point origin;
            Point destiny;

            public Status(Status status) : this(status.Action, status.Figure, status.Origin, status.Destiny) { }

            public Status(string action, string figure, Point origin, Point destiny)
            {
                this.action = action;
                this.figure = figure;
                this.origin = origin;
                this.destiny = destiny;
            }

            public string Action
            {
                get { return action; }
                set { action = value; }
            }

            public string Figure
            {
                get { return figure; }
                set { figure = value; }
            }

            public Point Origin
            {
                get { return origin; }
                set { origin = value; }
            }

            public Point Destiny
            {
                get { return destiny; }
                set { destiny = value; }
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3}", action, figure, origin, destiny);
            }
        }

        private class History
        {
            List<string> actions;
            Stack<Status> stackUndo;
            Stack<Status> stackRedo;
            string filename;

            public History()
            {
                actions = new List<string>();
                stackUndo = new Stack<Status>();
                stackRedo = new Stack<Status>();

                // default
                filename = "output.txt";
            }

            public string Filename
            {
                get { return filename; }
                set { filename = value; }
            }

            public int Count
            {
                get { return actions.Count; }
            }

            public void ActionRecord(Status status, bool IsUndo)
            {
                actions.Add(status.ToString());

                if (!IsUndo)
                    stackUndo.Push(reverse(status));
            }

            public Status Undo
            {
                get
                {
                    if (stackUndo.Count < 1)
                        return null;
                    Status last = stackUndo.Pop();
                    stackRedo.Push(reverse(last));
                    return last;
                }
            }

            public Status Redo
            {
                get
                {
                    if (stackRedo.Count < 1)
                        return null;
                    Status last = stackRedo.Pop();
                    return last;
                }
            }

            private Status reverse(Status status)
            {
                Status copy = new Status(status);
                switch (copy.Action)
                {
                    case "ADD":
                        copy.Action = "DEL";
                        break;
                    case "DEL":
                        copy.Action = "ADD";
                        break;
                    case "MOVE":
                        var temp = copy.Destiny;
                        copy.Destiny = copy.Origin;
                        copy.Origin = temp;
                        break;
                }
                return copy;
            }

            public string List()
            {
                string txt = "";
                foreach (var item in actions)
                    txt += item + "\n";
                return txt;
            }
        }
        #endregion Structures

        #region Controls
        private void Report(Status status, bool isUndo)
        {
            if (status != null)
            {
                history.ActionRecord(status, isUndo);
                txtboxConsole.AppendText(status.ToString() + "\n");
                txtboxConsole.ScrollToEnd();
            }
        }

        private void Undo()
        {
            Status last = history.Undo;
            Change(last);
            Report(last, true);
        }

        private void Redo()
        {
            Status last = history.Redo;
            Change(last);
            Report(last, false);
        }

        private void Change(Status status)
        {
            if (status == null)
                return;

            dynamic figure = this.FindName(status.Figure);
            if (figure != null)
            {
                switch (status.Action)
                {
                    case "ADD":
                        figure.Visibility = Visibility.Visible;
                        break;
                    case "DEL":
                        figure.Visibility = Visibility.Hidden;
                        break;
                    case "MOVE":
                        // TODO
                        figure.SetValue(Canvas.LeftProperty, status.Destiny.X);
                        figure.SetValue(Canvas.TopProperty, status.Destiny.Y);
                        break;
                }
            }
        }

        private void AddFigure(string figure)
        {
            // default
            int x = 0;
            int y = 0;
            int width = 50;
            int height = 50;
            int id = counter++;
            string name = "Undefined";

            if (figure.Equals("Rectangle"))
            {
                // Geomtry
                var path = new System.Windows.Shapes.Path();
                RectangleGeometry rectangle = new RectangleGeometry();

                rectangle.Rect = new Rect(x, y, width, height);
                path.Fill = Brushes.Red;
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 1;
                path.Data = rectangle;
                path.Name = string.Format("Rectangle{0:0000}", id);
                this.RegisterName(path.Name, path);

                Canvas.SetLeft(path, x);
                Canvas.SetTop(path, y);
                drawingArea.Children.Add(path);

                name = path.Name;
            }

            if (figure.Equals("Ellipse"))
            {
                // Shape
                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.Fill = Brushes.Red;
                ellipse.Name = string.Format("Ellipse{0:0000}", id);
                this.RegisterName(ellipse.Name, ellipse);

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                drawingArea.Children.Add(ellipse);

                name = ellipse.Name;
            }

            if (figure.Equals("Image"))
            {
                Image image = new Image();
                image.Source = ImportImage();

                if (image.Source != null)
                {
                    // TODO
                    // Resize Image

                    image.Name = string.Format("Image{0:0000}", id);
                    this.RegisterName(image.Name, image);

                    Canvas.SetLeft(image, x);
                    Canvas.SetTop(image, y);
                    drawingArea.Children.Add(image);

                    name = image.Name;
                }
            }
            Report(new Status("ADD", name, new Point(x, y), new Point(x, y)), false);
        }

        #region Open and Save Files
        private void Export(string filename, object value, string filter)
        {
            if (value.Equals(null))
                return;

            Type t = value.GetType();
            if (!t.Equals(typeof(string)) && !t.Equals(typeof(BitmapImage)))
                return;

            try
            {
                if (filename.Equals(""))
                {
                    SaveFileDialog saveDlg = new SaveFileDialog();
                    saveDlg.RestoreDirectory = true;
                    saveDlg.Filter = filter;

                    if (saveDlg.ShowDialog().Equals(true))
                    {
                        filename = saveDlg.FileName;
                    }
                    else
                    {
                        MessageBox.Show("Action canceled!");
                        return;
                    }
                }

                FileStream fs = new FileStream(filename, FileMode.Create);

                if (t.Equals(typeof(string)))
                {
                    // Output Text
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write(value);
                    writer.Close();

                    history.Filename = filename;

                    lblInform.Content = string.Format("Saved: {0}", filename);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapImage ImportImage()
        {
            BitmapImage bitmap = null;
            try
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = "Image Files (*.jpg; *.jpeg; *.gif; *.bmp; *.png) | *.jpg; *.jpeg; *.gif; *.bmp; *.png";

                if (openDlg.ShowDialog().Equals(true))
                {
                    bitmap = new BitmapImage(new Uri(openDlg.FileName));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return bitmap;
        }

        private void ExportImage()
        {
            // TODO
            MessageBox.Show("Export Canvas");
            //Export("", canvas, "Image Files (*.jpg; *.bmp; *.png) | *.jpg; *.bmp; *.png");
        }
        #endregion Open and Save Files
        #endregion Controls

        #region Events  
        private void CommonClickHandlerFromMenu(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            switch (feSource.Name)
            {
                case "menuSave":
                    Export(history.Filename, history.List(), "Text Files (*.txt; *.csv) | *.txt; *.csv");
                    break;
                case "menuSaveAs":
                    Export("", history.List(), "Text Files (*.txt; *.csv) | *.txt; *.csv");
                    break;
                case "menuExport":
                    // Save Bitmap
                    ExportImage();
                    break;
                case "btnUndo":
                case "menuUndo":
                    Undo();
                    break;
                case "btnRedo":
                case "menuRedo":
                    Redo();
                    break;
                case "menuBackground":
                    drawingArea.Background = drawingArea.Background == Brushes.White ? Brushes.Black : Brushes.White;
                    menuBackground.Header = string.Format("Background {0}", drawingArea.Background);
                    break;
                case "menuAbout":
                    var msg = "Canvas in WPF C#.\nSimple experience!";
                    MessageBox.Show(msg);
                    break;
                case "menuQuit":
                    MessageBox.Show("Goodbye!");
                    Close();
                    break;
            }
            e.Handled = true;
        }

        private void NewFigure(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            switch (feSource.Name)
            {
                case "menuRectangle":
                    AddFigure("Rectangle");
                    break;
                case "menuEllipse":
                    AddFigure("Ellipse");
                    break;
                case "menuImage":
                    AddFigure("Image");
                    break;
            }
        }

        private void CanvasLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            mousePosition = e.GetPosition(this);
            feSource.CaptureMouse();

            if (!feSource.Name.Equals("drawingArea"))
                fromPosition = mousePosition;

            lblInform.Content = string.Format("Capture {0} ...", feSource.Name);
        }

        private void CanvasLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            Point mouseCurrentPosition = e.GetPosition(this);

            toPosition = mouseCurrentPosition;
            feSource.ReleaseMouseCapture();

            if (!feSource.Name.Equals("drawingArea"))
            {
                Report(new Status("MOVE", feSource.Name, fromPosition, toPosition), false);
            }

            lblInform.Content = "Ready ...";
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            if (feSource.IsMouseCaptured)
            {
                if (feSource.Name.Equals("drawingArea"))
                    return;

                Point mouseCurrentPosition = e.GetPosition(this);
                double X = Canvas.GetLeft(feSource) + (mouseCurrentPosition.X - mousePosition.X);
                double Y = Canvas.GetTop(feSource) + (mouseCurrentPosition.Y - mousePosition.Y);

                if (X >= 0 && X <= (drawingArea.Width - feSource.ActualWidth) &&
                    Y >= 0 && Y <= (drawingArea.Height - feSource.ActualHeight))
                {
                    feSource.SetValue(Canvas.LeftProperty, X);
                    feSource.SetValue(Canvas.TopProperty, Y);
                    mousePosition = mouseCurrentPosition;
                    drawingArea.Cursor = Cursors.Hand;
                }
                else
                {
                    drawingArea.Cursor = Cursors.No;
                }

                lblInform.Content = string.Format("Mouse Position ({0}), Move {1} ...", mousePosition.ToString(), feSource.Name);
            }
            else
            {
                drawingArea.Cursor = Cursors.Arrow;
            }
        }
        #endregion Events 
    }
}