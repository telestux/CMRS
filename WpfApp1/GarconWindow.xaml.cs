using Microsoft.ReportingServices.Diagnostics.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing;
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
using System.Windows.Shapes;
using Brush = System.Drawing.Brush;
using System.Windows.Threading;

namespace WpfApp1
{
    public class GarconMenu
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public ImageSource Image { get; set; }
        public string Type { get; set; }
    }
    public class GarconDish
    {
        public string DishName { get; set; }
        public int DishPrice { get; set; }
    }
    
    public partial class GarconWindow : Window
    {
        private DispatcherTimer timer; DateTime now = DateTime.Now;
        private List<GarconDish> selectedDishes = new List<GarconDish>();
        public int totalPrice;
        private SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
        public GarconWindow()
        {
            InitializeComponent();
            ReservationUpdater();
            getMenu("");
        }
        public BitmapImage processExistImage(string price, BitmapImage bitImage)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                // Сохранить BitmapImage в MemoryStream
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitImage));
                encoder.Save(stream);
                stream.Position = 0; // Сбросить позицию потока для чтения

                using (Bitmap bitmap = new Bitmap(stream))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // Загружаем шрифт
                        PrivateFontCollection pfc = new PrivateFontCollection();
                        string fontPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "Inter-Medium.ttf");
                        pfc.AddFontFile(fontPath);
                        Font font = new Font(pfc.Families[0], 50); // Используем первый шрифт из коллекции
                        Brush textBrush = System.Drawing.Brushes.White;
                        PointF position = new PointF(130, 410);

                        if (price.Length >=4)
                        {
                            position.X -= 17;
                        }
                        price += " ₽";
                        // Рисуем строку на изображении
                        g.DrawString(price, font, textBrush, position);
                    }

                    // Теперь создаем BitmapImage из измененного Bitmap
                    using (MemoryStream bitmapStream = new MemoryStream())
                    {
                        bitmap.Save(bitmapStream, System.Drawing.Imaging.ImageFormat.Png);
                        bitmapStream.Position = 0; // Сбросить позицию потока перед созданием BitmapImage

                        BitmapImage resultImage = new BitmapImage();
                        resultImage.BeginInit();
                        resultImage.StreamSource = bitmapStream;
                        resultImage.CacheOption = BitmapCacheOption.OnLoad; // Кэшируем изображение
                        resultImage.EndInit();
                        resultImage.Freeze(); // Замораживаем для потокобезопасности

                        return resultImage;
                    }
                }
            }
        }
        public void getMenu(string parameter)
        {
            List<GarconMenu> Dishes = new List<GarconMenu>();
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT * FROM Menu {parameter};", connection);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    GarconMenu tableFiller = new GarconMenu
                    {
                        ID = rdr["Menu_ID"].ToString(),
                        Name = rdr["Menu_Name"].ToString(),
                        Price = Convert.ToInt32(rdr["Menu_Price"]),
                        Type = rdr["Menu_Type"].ToString(),
                    };
                    var imageData = rdr["Menu_Image"] as byte[];
                    if (imageData != null && imageData.Length > 0)
                    {
                        using (var stream = new MemoryStream(imageData))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();                                                             
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            tableFiller.Image = processExistImage(tableFiller.Price.ToString(), bitmap); 
                        }
                    }
                    else
                    {
                        tableFiller.Image = new BitmapImage(new Uri($"{getProjectFolderPath()}\\Images\\Dishes\\no photo.png"));
                    }

                    Dishes.Add(tableFiller);
                }
                menuListView.ItemsSource = Dishes;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        private void orderListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menuListView.SelectedIndex != -1)
            {
                GarconMenu items = (GarconMenu)menuListView.Items.GetItemAt(menuListView.SelectedIndex);
                if (items != null)
                {
                    GarconDish listFiller = new GarconDish
                    {
                        DishPrice = items.Price,
                        DishName = items.Name,
                    };
                    selectedDishes.Add(listFiller);
                    orderListView.ItemsSource = null;
                    orderListView.ItemsSource = selectedDishes;
                    totalPriceButton.Text = $"{(selectedDishes.Sum(dish => dish.DishPrice))} ₽";
                }
            }
        }

        
        public void ReservationUpdater()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            CheckAndUpdateTableStatus();
        }
        public void CheckAndUpdateTableStatus()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand cmd = new SqlCommand($"UPDATE Reservation SET Reservation_Status='Закрыта' WHERE Reservation_Date < '{now.Date:yyyy-MM-dd}' OR (Reservation_Date = '{now.Date:yyyy-MM-dd}' AND Reservation_End <= CONVERT(TIME, GETDATE()));", connection);
                    cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString))
            {
                try
                {
                    connection.Open();
                    for (int tableNumber = 1; tableNumber <= 16; tableNumber++)
                    {

                        string tablesQuery = $"SELECT COUNT(*) FROM Tables WHERE Tables_ID = {tableNumber} AND Tables_Status = 'Занят'";
                        SqlCommand tablesCommand = new SqlCommand(tablesQuery, connection);
                        int reservationCount = (int)tablesCommand.ExecuteScalar();
                        if (reservationCount > 0)
                        {
                            UpdateTableStatusBusy(tableNumber, reservationCount > 0);
                        }
                        else
                        {
                            DateTime currentDateTime = DateTime.Now;
                            string reservationQuery = $"SELECT COUNT(*) FROM Reservation " +
                                                       $"WHERE Reservation_Table = {tableNumber} " +
                                                       $"AND Reservation_Date = '{currentDateTime.Date:yyyy-MM-dd}' " +
                                                       $"AND '{currentDateTime:HH:mm:ss}' BETWEEN Reservation_Start AND Reservation_End AND Reservation_Status='Активна'";
                            SqlCommand reservationCommand = new SqlCommand(reservationQuery, connection);
                            reservationCount = (int)reservationCommand.ExecuteScalar();
                            UpdateTableStatus(tableNumber, reservationCount > 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }

        }
        private void UpdateTableStatus(int tableNumber, bool isOccupied)
        {
            string status = isOccupied ? "Забронирован" : "Свободен";
            SetTableStatus(tableNumber, status);
        }
        private void UpdateTableStatusBusy(int tableNumber, bool isOccupied)
        {
            string status = isOccupied ? "Занят" : "Свободен";
            SetTableStatus(tableNumber, status);
        }
        private void SetTableStatus(int tableNumber, string status)
        {
            switch (tableNumber)
            {
                case 1:
                    tableNumber1.Status = status;
                    break;
                case 2:
                    tableNumber2.Status = status;
                    break;
                case 3:
                    tableNumber3.Status = status;
                    break;
                case 4:
                    tableNumber4.Status = status;
                    break;
                case 5:
                    tableNumber5.Status = status;
                    break;
                case 6:
                    tableNumber6.Status = status;
                    break;
                case 7:
                    tableNumber7.Status = status;
                    break;
                case 8:
                    tableNumber8.Status = status;
                    break;
                case 9:
                    tableNumber9.Status = status;
                    break;
                case 10:
                    tableNumber10.Status = status;
                    break;
                case 11:
                    tableNumber11.Status = status;
                    break;
                case 12:
                    tableNumber12.Status = status;
                    break;
                case 13:
                    tableNumber13.Status = status;
                    break;
                case 14:
                    tableNumber14.Status = status;
                    break;
                case 15:
                    tableNumber15.Status = status;
                    break;
                case 16:
                    tableNumber16.Status = status;
                    break;
                default:
                    break;
            }
        }
        public void ChangeTableState(string Number, string Status)
        {
            ReservationsGrid.Visibility = Visibility.Collapsed;
            OrderGrid.Visibility = Visibility.Visible;
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
            try
            {
                connection.Open();
                string query = $"UPDATE Tables SET Tables_Status='Занят' WHERE Tables_ID = '{Number}';";
                SqlCommand command = new SqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        static string getProjectFolderPath()
        {

            string currentDirectory = Directory.GetCurrentDirectory();
            while (!string.IsNullOrEmpty(currentDirectory))
            {
                if (Directory.GetFiles(currentDirectory, "*.csproj").Length > 0)
                {
                    return currentDirectory;
                }

                currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            }

            return null;
        }
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private bool IsMaximize = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1280;
                    this.Height = 780;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }
    }
}
