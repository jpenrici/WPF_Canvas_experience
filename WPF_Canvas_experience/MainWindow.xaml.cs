using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_Canvas_experience
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        const string TXTFILTERS = "Text Files (*.txt; *.csv) | *.txt; *.csv";
        const string IMGFILTERS = "Image Files (*.jpg; *.jpeg; *.gif; *.bmp; *.png) | *.jpg; *.jpeg; *.gif; *.bmp; *.png";
        Dictionary<string, Status> figures;
        string currentFigure;
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
            figures = new Dictionary<string, Status>();
        }
        #endregion

        #region Config
        private void ConfigWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Normal;
            ResizeMode = ResizeMode.NoResize;
            //WindowStyle = WindowStyle.None;
            Height = 600;
            Width = 800;
            Title = "WPF Canvas Experience";
            Background = new SolidColorBrush(Color.FromRgb(60, 120, 120));
        }

        private void ConfigMenu()
        {
            menuBackground.Header = "Black";    // Canvas background

            menuSave.ToolTip = new ToolTip { Content = "Save history to text file." };
            menuExport.ToolTip = new ToolTip { Content = "Save Canvas Image." };
            menuClear.ToolTip = new ToolTip { Content = "Remove figures." };

            menuSave.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuSaveAs.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuExport.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuQuit.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuClear.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuUndo.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuRedo.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuBackground.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuBackgroundImage.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            menuAbout.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);

            string[] figures = {
                "Rectangle",
                "Ellipse",
                "Image"
            };

            foreach (var item in figures)
            {
                MenuItem newItem = new MenuItem();
                newItem.Header = item;
                newItem.Name = "menu" + item.Replace(" ", "");
                newItem.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
                menuAdd.Items.Add(newItem);
            }
        }

        private void ConfigCanvas()
        {
            drawingArea.HorizontalAlignment = HorizontalAlignment.Center;
            drawingArea.Background = Brushes.White;
            drawingArea.Cursor = Cursors.Arrow;
            drawingArea.Margin = new Thickness(2, 2, 2, 2);
            drawingArea.Height = Height - 150;
            drawingArea.Width = Width - 20;

            ContextMenu contextMenu = new ContextMenu();
            drawingArea.ContextMenu = contextMenu;

            string[] popup = {
                "Background White",
                "Background Black"
            };

            foreach (var item in popup)
            {
                MenuItem newItem = new MenuItem();
                newItem.Header = item;
                newItem.Name = "popup" + item.Replace(" ", "");
                newItem.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
                contextMenu.Items.Add(newItem);
            }

            drawingArea.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(CanvasLeftButtonDown);
            drawingArea.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(CanvasLeftButtonUp);
            drawingArea.PreviewMouseRightButtonDown += new MouseButtonEventHandler(CanvasRightButtonDown);
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
            txtboxConsole.Width = Width - 65;
            txtboxConsole.Margin = new Thickness(5, 0, 5, 0);

            btnUndo.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
            btnRedo.Click += new RoutedEventHandler(CommonClickHandlerFromMenu);
        }
        #endregion Config

        #region Structures
        enum RecordType
        {
            None,
            Undo,
            Redo,
            Normal,
            Undefined
        }

        class Status
        {
            string action;
            string figure;
            Point origin;
            Point destiny;
            double scale;
            double rotate;
            RecordType recordType;

            public Status(Status status) : this(status.Action, status.Figure, status.Origin, status.Destiny,
                status.Scale, status.Rotate, status.RecordType)
            { }

            public Status(string action, string figure, Point origin, Point destiny,
                double scale, double rotate, RecordType recordType)
            {
                this.action = action;
                this.figure = figure;
                this.origin = origin;
                this.destiny = destiny;

                if (scale < 1)
                    scale = 1;
                this.scale = scale;

                if (rotate < 0 && rotate > 360)
                    rotate = 0;
                this.rotate = rotate;

                if (recordType < RecordType.None && recordType > RecordType.Undefined)
                    recordType = RecordType.None;
                this.recordType = recordType;
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

            public double Scale
            {
                get { return scale; }
                set { scale = value; }
            }

            public double Rotate
            {
                get { return rotate; }
                set { rotate = value; }
            }

            public RecordType RecordType
            {
                get { return recordType; }
                set { recordType = value; }
            }

            public override string ToString()
            {
                return string.Format("{0} {1} {2} {3} {4} {5} [{6}]", action, figure, origin, destiny,
                    scale, rotate, Enum.GetName(typeof(RecordType), recordType));
            }
        }

        private class History
        {
            List<Status> actions;
            Stack<Status> stackUndo;
            Stack<Status> stackRedo;
            string filename;

            public History()
            {
                actions = new List<Status>();
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

            public int CountUndo
            {
                get { return stackUndo.Count; }
            }

            public int CountRedo
            {
                get { return stackRedo.Count; }
            }

            public void ActionRecord(Status status)
            {
                actions.Add(status);

                if (!status.RecordType.Equals(RecordType.Undo))
                    stackUndo.Push(Reverse(status));
            }

            public Status Undo
            {
                get
                {
                    if (stackUndo.Count < 1)
                        return null;

                    Status last = stackUndo.Pop();
                    stackRedo.Push(Reverse(last));
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

            private Status Reverse(Status status)
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
                    case "SCALEUP":
                        copy.Action = "SCALEDOWN";
                        break;
                    case "SCALEDOWN":
                        copy.Action = "SCALEUP";
                        break;
                    case "LROTATE":
                        copy.Action = "RROTATE";
                        break;
                    case "RROTATE":
                        copy.Action = "LROTATE";
                        break;
                }

                return copy;
            }

            public Status Last
            {
                get
                {
                    if (actions.Count < 1)
                        return null;
                    return actions[actions.Count - 1];
                }
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
        private void Report(Status status)
        {
            if (status != null)
            {
                try
                {
                    figures.Add(status.Figure, status);
                }
                catch (ArgumentException)
                {
                    figures[status.Figure] = status;
                    Debug.WriteLine(status);
                }

                history.ActionRecord(status);
                string output = string.Format("{0}: {1} {2} {3} {4} ", history.Count, status.Action,
                    status.Figure, status.Origin, status.Destiny);

                switch (status.RecordType)
                {
                    case RecordType.Undo:
                        output += "[Undo]";
                        break;
                    case RecordType.Redo:
                        output += "[Redo]";
                        break;
                    case RecordType.Normal:
                        break;
                }
                txtboxConsole.AppendText(output + "\n");
                txtboxConsole.ScrollToEnd();
            }
        }

        private Status CurrentStatus(string figureName)
        {
            Status currentStatus = null;
            if (!figures.TryGetValue(figureName, out currentStatus))
                return null;
            return currentStatus;
        }

        private string CurrentHistory()
        {
            return string.Format("Figures: {0}, History: {1}, Undo: {2}, Redo: {3}\n",
                figures.Count, history.Count, history.CountUndo, history.CountRedo);
        }

        #region Undo/Redo
        private void Edit(Status last, RecordType recordType)
        {
            if (last != null)
            {
                last.RecordType = recordType;
                Change(last);
            }
            else
            {
                txtboxConsole.AppendText("Nothing to do.\n");
                txtboxConsole.ScrollToEnd();
            }

            CurrentHistory();
        }

        private void Undo()
        {
            Edit(history.Undo, RecordType.Undo);
        }

        private void Redo()
        {
            Edit(history.Redo, RecordType.Redo);
        }
        #endregion Undo/Redo

        #region Edit
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
                        figure.SetValue(Canvas.LeftProperty, status.Destiny.X);
                        figure.SetValue(Canvas.TopProperty, status.Destiny.Y);
                        break;
                    case "SCALEUP":
                        status.Scale += 0.5;
                        break;
                    case "SCALEDOWN":
                        status.Scale -= 0.5;
                        break;
                    case "RROTATE":
                        status.Rotate += 5;
                        break;
                    case "LROTATE":
                        status.Rotate -= 5;
                        break;
                }
                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(new ScaleTransform(status.Scale, status.Scale));
                transformGroup.Children.Add(new RotateTransform(status.Rotate, status.Origin.X, status.Origin.Y));
                figure.RenderTransform = transformGroup;

                Report(status);
            }
        }

        private void Transform(string action)
        {
            Status currentStatus = CurrentStatus(currentFigure);
            if (currentStatus == null)
                return;
            currentStatus.Action = action;
            Change(currentStatus);
        }
        #endregion

        #region Add Figure
        private void Add(string figure)
        {
            // default
            int x = 0;
            int y = 0;
            int width = 50;
            int height = 50;
            int id = counter++;
            string name = "";

            Random rnd = new Random();
            byte R = (byte)rnd.Next(1, 255);
            byte G = (byte)rnd.Next(1, 255);
            byte B = (byte)rnd.Next(1, 255);
            Brush brush = new SolidColorBrush(Color.FromRgb(R, G, B));

            ContextMenu contextMenu = new ContextMenu();

            string[] popup = {
                "Scale UP", "Scale DOWN",
                "Left Rotate", "Right Rotate"
            };

            foreach (var item in popup)
            {
                MenuItem newItem = new MenuItem();
                newItem.Header = item;
                newItem.Name = "popup" + item.Replace(" ", "");
                newItem.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
                contextMenu.Items.Add(newItem);
            }

            #region Add Rectangle
            if (figure.Equals("Rectangle"))
            {
                // Geomtry
                var path = new System.Windows.Shapes.Path();
                RectangleGeometry rectangle = new RectangleGeometry();

                rectangle.Rect = new Rect(x, y, width, height);
                path.Fill = brush;
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 1;
                path.Data = rectangle;
                path.Name = string.Format("Rectangle_ID{0:0000}", id);
                path.ContextMenu = contextMenu;
                this.RegisterName(path.Name, path);

                Canvas.SetLeft(path, x);
                Canvas.SetTop(path, y);
                drawingArea.Children.Add(path);

                name = path.Name;
            }
            #endregion

            #region Add Ellipse
            if (figure.Equals("Ellipse"))
            {
                // Shape
                Ellipse ellipse = new Ellipse();
                ellipse.Width = width;
                ellipse.Height = height;
                ellipse.Stroke = Brushes.Black;
                ellipse.StrokeThickness = 0.5;
                ellipse.Fill = brush;
                ellipse.Name = string.Format("Ellipse_ID{0:0000}", id);
                ellipse.ContextMenu = contextMenu;
                this.RegisterName(ellipse.Name, ellipse);

                Canvas.SetLeft(ellipse, x);
                Canvas.SetTop(ellipse, y);
                drawingArea.Children.Add(ellipse);

                name = ellipse.Name;
            }
            #endregion

            #region Add Image
            if (figure.Equals("Image"))
            {
                Image image = new Image();
                BitmapImage bmp = ImportImage();

                if (bmp != null)
                {
                    double pxW = drawingArea.ActualWidth / 4;
                    double pxH = drawingArea.ActualHeight / 4;
                    double bmpW = bmp.PixelWidth;
                    double bmpH = bmp.PixelHeight;
                    if (bmpW < pxW && bmpH < pxH)
                        image.Source = bmp;
                    else
                        image.Source = new TransformedBitmap(bmp, new ScaleTransform(pxW / bmpW, pxH / bmpH));

                    image.Name = string.Format("Image_ID{0:0000}", id);
                    image.ContextMenu = contextMenu;
                    this.RegisterName(image.Name, image);

                    Canvas.SetLeft(image, x);
                    Canvas.SetTop(image, y);
                    drawingArea.Children.Add(image);

                    name = image.Name;
                }
            }
            #endregion

            if (!name.Equals(""))
                Report(new Status("ADD", name, new Point(x, y), new Point(x, y), 1, 0, RecordType.Normal));
        }
        #endregion Add Figure

        #region Menu Control
        private void MenuControl(string menuName)
        {
            switch (menuName)
            {
                // File
                case "menuSave":
                    Export(history.Filename, history.List(), TXTFILTERS);
                    break;
                case "menuSaveAs":
                    Export("", history.List(), TXTFILTERS);
                    break;
                case "menuExport":
                    ExportImageCanvas();
                    break;

                // Edit
                case "menuClear":
                    txtboxConsole.Text = "Figures removed!\n";
                    drawingArea.Children.Clear();
                    history = new History();
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
                    if (drawingArea.Background == Brushes.White)
                    {
                        drawingArea.Background = Brushes.Black;
                        menuBackground.Header = "White";
                    }
                    else
                    {
                        drawingArea.Background = Brushes.White;
                        menuBackground.Header = "Black";
                    }
                    break;
                case "popupBackgroundWhite":
                    drawingArea.Background = Brushes.White;
                    menuBackground.Header = "Black";
                    break;
                case "popupBackgroundBlack":
                    drawingArea.Background = Brushes.Black;
                    menuBackground.Header = "White";
                    break;
                case "menuBackgroundImage":
                    ImageBrush bkg = new ImageBrush();
                    bkg.ImageSource = ImportImage();
                    if (bkg != null)
                        drawingArea.Background = bkg;
                    break;

                // Add
                case "menuRectangle":
                    Add("Rectangle");
                    break;
                case "menuEllipse":
                    Add("Ellipse");
                    break;
                case "menuImage":
                    Add("Image");
                    break;

                // Context
                case "popupScaleUP":
                    Transform("SCALEUP");
                    break;
                case "popupScaleDOWN":
                    Transform("SCALEDOWN");
                    break;
                case "popupLeftRotate":
                    Transform("LROTATE");
                    break;
                case "popupRightRotate":
                    Transform("RROTATE");
                    break;

                // About
                case "menuAbout":
                    var msg = "Canvas in WPF C#.\nSimple experience!";
                    MessageBox.Show(msg);
                    break;

                // Exit
                case "menuQuit":
                    MessageBox.Show("Goodbye!");
                    Close();
                    break;
            }
        }
        #endregion

        #region Open/Save
        private void Export(string filename, object obj, string filter)
        {
            if (obj.Equals(null))
                return;

            Type t = obj.GetType();
            if (!t.Equals(typeof(string)) && !t.Equals(typeof(BitmapImage)) && !t.Equals(typeof(RenderTargetBitmap)))
                return;

            try
            {
                if (filename.Equals(""))
                {
                    SaveFileDialog saveDlg = new SaveFileDialog();
                    saveDlg.RestoreDirectory = true;
                    saveDlg.Filter = filter;

                    if (saveDlg.ShowDialog().Equals(false))
                        return;

                    filename = saveDlg.FileName;
                }

                FileStream fs = new FileStream(filename, FileMode.Create);
                string extension = System.IO.Path.GetExtension(filename).ToLower();

                if (extension.Equals(".txt") || extension.Equals(".csv"))
                {
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write(obj);
                    writer.Close();
                    history.Filename = filename;
                    lblInform.Content = string.Format("Saved console history: {0}", filename);
                    return;
                }

                BitmapEncoder encoder;
                if (extension.Equals(".gif"))
                    encoder = new GifBitmapEncoder();
                else if (extension.Equals(".png"))
                    encoder = new PngBitmapEncoder();
                else if (extension.Equals(".jpg") || extension.Equals(".jpeg"))
                    encoder = new JpegBitmapEncoder();
                else
                    return;

                if (t.Equals(typeof(BitmapImage)))
                    encoder.Frames.Add(BitmapFrame.Create((BitmapImage)obj));
                else
                    encoder.Frames.Add(BitmapFrame.Create((RenderTargetBitmap)obj));

                encoder.Save(fs);
                fs.Close();

                lblInform.Content = string.Format("Exported image: {0}", filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private BitmapImage ImportImage()
        {
            BitmapImage bitmap = null;
            try
            {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = IMGFILTERS;

                if (openDlg.ShowDialog().Equals(true))
                    bitmap = new BitmapImage(new Uri(openDlg.FileName));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return bitmap;
        }

        private void ExportImageCanvas()
        {
            int pixelHeight = (int)drawingArea.ActualHeight;
            int pixelWidth = (int)drawingArea.ActualWidth;
            int dpi = 75;

            RenderTargetBitmap target;
            target = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Default);
            target.Render(drawingArea);

            Export("", target, IMGFILTERS);
        }
        #endregion Open/Save
        #endregion Controls

        #region Events  
        private void CommonClickHandlerFromMenu(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            MenuControl(feSource.Name);
            e.Handled = true;
        }

        private void CanvasLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            mousePosition = e.GetPosition(null);
            feSource.CaptureMouse();

            if (!feSource.Name.Equals("drawingArea"))
            {
                fromPosition.X = Canvas.GetLeft(feSource);
                fromPosition.Y = Canvas.GetTop(feSource);
            }

            lblInform.Content = feSource.Name;
        }

        private void CanvasLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            toPosition.X = Canvas.GetLeft(feSource);
            toPosition.Y = Canvas.GetTop(feSource);
            feSource.ReleaseMouseCapture();

            if (!feSource.Name.Equals("drawingArea"))
                if (!fromPosition.Equals(toPosition))
                {
                    Status currentStatus = CurrentStatus(feSource.Name);
                    if (currentStatus != null)
                        Report(new Status("MOVE", currentStatus.Figure, fromPosition, toPosition, currentStatus.Scale,
                            currentStatus.Rotate, RecordType.Normal));
                }

            lblInform.Content = "Ready ...";
        }

        private void CanvasRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            currentFigure = feSource.Name;

            lblInform.Content = feSource.Name;
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            if (feSource.IsMouseCaptured)
            {
                if (feSource.Name.Equals("drawingArea"))
                    return;

                Point mouseCurrentPosition = e.GetPosition(null);
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

                lblInform.Content = string.Format("Position ({0},{1}), Move {2} ...", X, Y, feSource.Name);
            }
            else
            {
                drawingArea.Cursor = Cursors.Arrow;
            }
        }
        #endregion Events 
    }
}