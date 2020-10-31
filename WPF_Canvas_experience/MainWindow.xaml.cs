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
        Point mouseXY;
        double canvasLeft;
        double canvasTop;
        List<string> history;
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
            history = new List<string> { };
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
            txtboxConsole.MinHeight = 50;
            txtboxConsole.MinWidth = 530;

            btnUndo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
            btnRedo.Click += new RoutedEventHandler(this.CommonClickHandlerFromMenu);
        }
        #endregion Config

        #region Controls
        private void Report(string action)
        {
            history.Add(action);
            var msg = string.Format("{0}: {1}\n", history.Count, history.Last());
            //txtboxConsole.AppendText(msg);
            txtboxConsole.Text = msg;
        }

        private void Undo()
        {
            // TODO
            MessageBox.Show("Undo ...");
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
        	foreach (var line in history) 
        	{
        		txtOutput += line + "\n";
        	}  	
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
        	} catch (Exception ex)
        	{
        		MessageBox.Show("Erro: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        	}
        }

        private BitmapImage ImportImage()
        {
            BitmapImage bitmap = null;
            try {
                OpenFileDialog openDlg = new OpenFileDialog();
                openDlg.Filter = "Image Files (*.jpg; *.jpeg; *.gif; *.bmp; *.png) | *.jpg; *.jpeg; *.gif; *.bmp; *.png";

                if (openDlg.ShowDialog().Equals(true))
                {
                    bitmap = new BitmapImage(new Uri(openDlg.FileName));
                }
            } catch (Exception ex)
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

        private void NewFigure(object sender, RoutedEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            String msg = "Undefined";

            if (feSource.Name.Equals("menuRectangle"))
            {
                // Geomtry
                var path = new System.Windows.Shapes.Path();
                RectangleGeometry rectangle = new RectangleGeometry();

                rectangle.Rect = new Rect(50, 50, 50, 50);
                path.Fill = Brushes.Red;
                path.Stroke = Brushes.Black;
                path.StrokeThickness = 1;
                path.Data = rectangle;
                path.Name = string.Format("Rectangle{0}", history.Count);
                this.RegisterName(path.Name, path);

                Canvas.SetLeft(path, 0);
                Canvas.SetTop(path, 0);
                drawingArea.Children.Add(path);

                msg = path.Name;                   
            }

            if (feSource.Name.Equals("menuEllipse"))
            {
                // Shape
                Ellipse ellipse = new Ellipse();
                ellipse.Width = 50;
                ellipse.Height = 60;
                ellipse.Fill = Brushes.Red;
                ellipse.Name = string.Format("Ellipse{0}", history.Count);
                this.RegisterName(ellipse.Name, ellipse);                

                Canvas.SetLeft(ellipse, 50);
                Canvas.SetTop(ellipse, 50);
                drawingArea.Children.Add(ellipse);

                msg = ellipse.Name;                   
            }

            if (feSource.Name.Equals("menuImage"))
            {
            	Image image = new Image();
                image.Source = ImportImage();
                
                if (image.Source != null)
                {
                	// TODO
                	// Resize Image

                	image.Name = string.Format("Image{0}", history.Count);
                	this.RegisterName(image.Name, image);

                    Canvas.SetLeft(image, 0);
                    Canvas.SetTop(image, 0);
                    drawingArea.Children.Add(image);

                    msg = image.Name;           	
                }
            }

            Report(string.Format("Add {0}", msg)); 
        }

        private void CanvasLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            mouseXY = e.GetPosition(this);
            canvasLeft = Canvas.GetLeft(feSource);
            canvasTop = Canvas.GetTop(feSource);
            feSource.CaptureMouse();
        }

        private void CanvasLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;
            feSource.ReleaseMouseCapture();
            lblInform.Content = "Ready ...";
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement feSource = e.Source as FrameworkElement;

            if (feSource.IsMouseCaptured)
            {
                Point mouseCurrent = e.GetPosition(this);
                double left = mouseCurrent.X - canvasLeft;
                double top = mouseCurrent.Y - canvasTop;

                feSource.SetValue(Canvas.LeftProperty, canvasLeft + left);
                feSource.SetValue(Canvas.TopProperty, canvasTop + top);
                canvasLeft = Canvas.GetLeft(feSource);
                canvasTop = Canvas.GetTop(feSource);

                drawingArea.Cursor = Cursors.Hand;
                lblInform.Content = string.Format("Mouse({0},{1}), moving {2}", mouseCurrent.X, mouseCurrent.Y, feSource.Name);
            }
            else
            {
                drawingArea.Cursor = Cursors.Arrow;
            }
        }
        #endregion Events 
    }
}
