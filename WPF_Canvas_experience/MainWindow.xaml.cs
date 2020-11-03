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
        FromPosition fromPosition;
        ToPosition toPosition;
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

            history = new History();
            fromPosition = new FromPosition(0, 0);
            toPosition = new ToPosition(0, 0);
            counter = 0;
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
            txtboxConsole.MinHeight = 50;
            txtboxConsole.MinWidth = 535;

            btnUndo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            btnRedo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
        }
        #endregion Config

        #region Structures
        class FromPosition
        {
            double x, y;

            public FromPosition(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public double X
            {
                get { return x; }
                set { x = value; }
            }

            public double Y
            {
                get { return y; }
                set { y = value; }
            }
        }

        class ToPosition
        {
            double x, y;

            public ToPosition(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public double X
            {
                get { return x; }
                set { x = value; }
            }

            public double Y
            {
                get { return y; }
                set { y = value; }
            }
        }

        class Status
        {
            string action;
            string figureName;
            ToPosition toPosition;
            FromPosition fromPosition;

            public Status()
            {
                this.action = "Undefined";
                this.FigureName = "Undefined";
                fromPosition = new FromPosition(0, 0);
                toPosition = new ToPosition(0, 0);
            }

            public Status(string action, string figureName, FromPosition fromPosition, ToPosition toPosition)
            {
                this.action = action;
                this.figureName = figureName;
                this.fromPosition = fromPosition;
                this.toPosition = toPosition;
            }

            public Status(string action, string figureName, double fromPosX, double fromPosY, double toPosX, double toPosY)
            {
                this.action = action;
                this.figureName = figureName;
                this.fromPosition = new FromPosition(fromPosX, fromPosY);
                this.toPosition = new ToPosition(toPosX, toPosY);
            }

            public string Action
            {
                get { return action; }
                set { action = value; }
            }

            public string FigureName
            {
                get { return figureName; }
                set { figureName = value; }
            }

            public ToPosition ToPosition
            {
                get { return toPosition; }
                set { toPosition = value; }
            }

            public FromPosition FromPosition
            {
                get { return fromPosition; }
                set { fromPosition = value; }
            }

            public override string ToString()
            {
                var str = string.Format("{0}; {1}; {2};{3}; {4};{5}", action, figureName,
                    fromPosition.X, fromPosition.Y, toPosition.X, toPosition.Y);
                return str;
            }
        }

        private class History
        {
            List<string> actionsHistory = new List<string>();
            Stack<Status> stackUndo = new Stack<Status>();
            Stack<Status> stackRedo = new Stack<Status>();

            public int Count
            {
                get
                {
                    return actionsHistory.Count;
                }
            }

            public void AddActionRecord(Status status)
            {
                actionsHistory.Add(status.ToString());
                stackRedo.Push(status);
                //stackUndo.Push(reverse(status));
            }

            private Status reverse(Status status)
            {
                switch (status.Action)
                {
                    case "ADD":
                        status.Action = "DEL";
                        break;
                    case "DEL":
                        status.Action = "ADD";
                        break;
                    case "MOVE":
                        var temp = status.FromPosition.X;
                        status.FromPosition.X = status.ToPosition.X;
                        status.ToPosition.X = temp;
                        temp = status.FromPosition.Y;
                        status.FromPosition.Y = status.ToPosition.Y;
                        status.ToPosition.Y = temp;
                        break;
                }
                return status;
            }

            public Status Undo()
            {
                // TODO
                if (stackUndo.Count <= 0)
                    return null;

                Status status = stackUndo.Pop();
                actionsHistory.Add("Undo:" + status);
                //stackRedo.Push(reverse(status));

                return status;
            }

            public Status Redo()
            {
                // TODO
                return null;
            }

            public string List()
            {
                string txt = "";
                foreach (var item in actionsHistory)
                    txt += item + "\n";
                return txt;
            }
        }
        #endregion Structures

        #region Controls
        private void Report(Status status)
        {
            history.AddActionRecord(status);
            txtboxConsole.Text = status.ToString();
        }

        private void Undo()
        {
            // TODO
            Status lastStatus = new Status();
            lastStatus = history.Undo();

            if (lastStatus != null)
            {
                MessageBox.Show(lastStatus.ToString());
            }
        }

        private void Redo()
        {
            // TODO
            MessageBox.Show("Redo ...");
        }

        #region Open and Save Files
        private void SaveFile()
        {
            string txtOutput = "History\n";
            txtOutput += history.List();
            Export(txtOutput, "Text Files (*.txt; *.csv) | *.txt; *.csv");
        }

        private void Export(object value, string filter)
        {
            if (value.Equals(null))
                return;

            Type t = value.GetType();
            if (!t.Equals(typeof(string)) && !t.Equals(typeof(BitmapImage)))
                return;

            try
            {
                SaveFileDialog saveDlg = new SaveFileDialog();
                saveDlg.RestoreDirectory = true;
                saveDlg.Filter = filter;

                if (saveDlg.ShowDialog().Equals(true))
                {
                    FileStream fs = new FileStream(saveDlg.FileName, FileMode.Create);

                    if (t.Equals(typeof(string)))
                    {
                        StreamWriter writer = new StreamWriter(fs);
                        writer.Write(value);
                        writer.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Action canceled!");
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
            //Export(canvas, "Image Files (*.jpg; *.bmp; *.png) | *.jpg; *.bmp; *.png");
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
                    // Save Text
                    SaveFile();
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

        #region Add Figure
        private void NewFigure(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            var figureName = "Undefined";
            counter++;

            int x = 0;
            int y = 0;
            int width = 50;
            int height = 50;

            if (feSource.Name.Equals("menuRectangle"))
            {
                // Geomtry
                var path = new System.Windows.Shapes.Path();
                RectangleGeometry rectangle = new RectangleGeometry();

                rectangle.Rect = new Rect(x, y, width, height);
                path.Fill = Brushes.Red;
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 1;
                path.Data = rectangle;
                path.Name = string.Format("Rectangle{0:0000}", counter);
                this.RegisterName(path.Name, path);

                Canvas.SetLeft(path, x);
                Canvas.SetTop(path, y);
                drawingArea.Children.Add(path);

                figureName = path.Name;
            }

            if (feSource.Name.Equals("menuEllipse"))
            {
                // Shape
                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.Fill = Brushes.Red;
                ellipse.Name = string.Format("Ellipse{0:0000}", counter);
                this.RegisterName(ellipse.Name, ellipse);

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                drawingArea.Children.Add(ellipse);

                figureName = ellipse.Name;
            }

            if (feSource.Name.Equals("menuImage"))
            {
                Image image = new Image();
                image.Source = ImportImage();

                if (image.Source != null)
                {
                    // TODO
                    // Resize Image

                    image.Name = string.Format("Image{0:0000}", counter);
                    this.RegisterName(image.Name, image);

                    Canvas.SetLeft(image, x);
                    Canvas.SetTop(image, y);
                    drawingArea.Children.Add(image);

                    figureName = image.Name;
                }
            }
            Report(new Status("ADD", figureName, new FromPosition(x, y), new ToPosition(x, y)));
        }
        #endregion Add Figure

        private void CanvasLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            mousePosition = e.GetPosition(this);
            feSource.CaptureMouse();

            if (!feSource.Name.Equals("drawingArea"))
            {
                fromPosition.X = mousePosition.X;
                fromPosition.Y = mousePosition.Y;
            }

            lblInform.Content = string.Format("Capture {0} ...", feSource.Name);
        }

        private void CanvasLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            Point mouseCurrentPosition = e.GetPosition(this);

            toPosition.X = mouseCurrentPosition.X;
            toPosition.Y = mouseCurrentPosition.Y;
            feSource.ReleaseMouseCapture();

            if (!feSource.Name.Equals("drawingArea"))
            {
                Report(new Status("MOVE", feSource.Name, fromPosition.X, fromPosition.Y, toPosition.X, toPosition.Y));
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