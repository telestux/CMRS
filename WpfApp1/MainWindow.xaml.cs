using LiveCharts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Google.Protobuf.WellKnownTypes;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.X500;
using System.Timers;
using System.Windows.Threading;
using System.Diagnostics;
using System.Management;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using LiveCharts.Wpf;
using System.Windows.Controls.Primitives;

namespace WpfApp1
{
    public class LastDishes
    {
        public string Orders_ID { get; set; }
        public int Orders_Bill { get; set; }
        public string Orders_Dish_List { get; set; }
        public string Orders_Time { get; set; }
        public string Orders_Status { get; set; }
        public string Orders_Serving_time { get; set; }
    }
    public class Garcon
    {
        public string GarconName { get; set; }
        public int CountOfOrders { get; set; }
    }
    public class Menu
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public string Image { get; set; }
        public string Type { get; set; }
    }

    public partial class MainWindow : Window
    {
        private SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);

        public ChartValues<double> ChartValues { get; set; }
        public ChartValues<double> MonthOrdersChartValues { get; set; }

        public ChartValues<double> FirstMonthValues { get; set; }
        public ChartValues<double> SecondMonthValues { get; set; }
        public ChartValues<double> ThirdMonthValues { get; set; }

        public ChartValues<double> Occupancy { get; set; }

        public object FirstMonth { get; set; }
        public object SecondMonth { get; set; }
        public object ThirdMonth { get; set; }

        public string FirstMonthText { get; set; }
        public string SecondMonthText { get; set; }
        public string ThirdMonthText { get; set; }

        public List<string> Labels { get; set; }
        public List<string> Labels2 { get; set; }
        public List<string> Labels3 { get; set; }
        public List<string> Occupancy_Labels { get; set; }

        public List<double> Max2 { get; set; }
        public double Max2Value { get; set; }
        public int Max { get; set; }
        public int MonthOrdersMax { get; set; }
        public string MonthSells { get; set; }

        public string PicturePath { get; set; }
        public string Dish { get; set; }
        public string OrdersCount { get; set; }
        public string MonthOrdersCount { get; set; }

        public int Occupancy_Max { get; set; }
        DateTime now = DateTime.Now;

        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            //showMainPage();
            showAnalyticsPage();
        }
        //Main
        private void getOrders()
        {
            mainButton.Tag = "Selected";
            DataContext = this;
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE MONTH(CONVERT(DATE, Orders_Date, 104)) = MONTH(GETDATE()) AND YEAR(CONVERT(DATE, Orders_Date, 104)) = YEAR(GETDATE())", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    MonthSells = "0 руб.";
                }
                else
                {
                    MonthSells = String.Format("{0:n0}", Convert.ToInt32(cmd.ExecuteScalar())) + " руб.";
                }
                ChartValues = new ChartValues<double> { };
                Labels = new List<string> { };
                int daysCout = DateTime.DaysInMonth(now.Year, now.Month);
                for (int i = 1; i < now.Day + 1; i++)
                {
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{now.Year}-{now.Month}-{i}'", connection);
                    Labels.Add(i.ToString());
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        ChartValues.Add(1);
                    }
                    else
                    {
                        ChartValues.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }
                cmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = CONVERT(DATE, GETDATE(), 104) AND Orders_Status = 'Активен'", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    ActiveOrders.Text = "0";
                }
                else
                {
                    ActiveOrders.Text = cmd.ExecuteScalar().ToString();
                }

                cmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = CONVERT(DATE, GETDATE(), 104)", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    TodayOrders.Text = "0";
                }
                else
                {
                    TodayOrders.Text = cmd.ExecuteScalar().ToString();
                }
                cmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = CONVERT(DATE, GETDATE(), 104) AND Orders_Status = 'Отменён'", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    CancelledOrders.Text = "0";
                    Max = 0;
                }
                else
                {
                    CancelledOrders.Text = cmd.ExecuteScalar().ToString();
                    Max = Convert.ToInt32(ChartValues.Max() + (ChartValues.Max() * 0.3));
                }
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

        private string getMonths(int Month)
        {
            switch (Month)
            {
                case 1:
                    return "Январь";
                case 2:
                    return "Февраль";
                case 3:
                    return "Март";
                case 4:
                    return "Апрель";
                case 5:
                    return "Май";
                case 6:
                    return "Июнь";
                case 7:
                    return "Июль";
                case 8:
                    return "Август";
                case 9:
                    return "Сентябрь";
                case 10:
                    return "Октябрь";
                case 11:
                    return "Ноябрь";
                case 12:
                    return "Декабрь";
                default:
                    return "";
            }
        }

        private void getLast3MonthOrders()
        {
            DataContext = this;
            DateTime threeMonthsAgo = DateTime.Today.AddMonths(-2);
            FirstMonthText = getMonths(threeMonthsAgo.Month);
            int daysCout = DateTime.DaysInMonth(threeMonthsAgo.Year, threeMonthsAgo.Month);
            Labels2 = new List<string> { };
            Max2 = new List<double> { };
            for (int c = 1; c < 32; c++)
            {
                Labels2.Add(c.ToString());
            }
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("", connection);
                FirstMonthValues = new ChartValues<double> { };
                for (int i = 1; i <= daysCout; i++)
                {
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{threeMonthsAgo.Year}-{threeMonthsAgo.Month}-{i}'", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        Max2.Add(1);
                        FirstMonthValues.Add(1);
                    }
                    else
                    {
                        Max2.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                        FirstMonthValues.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }
                threeMonthsAgo = DateTime.Today.AddMonths(-1);
                daysCout = DateTime.DaysInMonth(threeMonthsAgo.Year, threeMonthsAgo.Month);
                SecondMonthText = getMonths(threeMonthsAgo.Month);
                SecondMonthValues = new ChartValues<double> { };
                for (int i = 1; i <= daysCout; i++)
                {
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{threeMonthsAgo.Year}-{threeMonthsAgo.Month}-{i}'", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        Max2.Add(1);
                        SecondMonthValues.Add(1);
                    }
                    else
                    {
                        Max2.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                        SecondMonthValues.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }
                daysCout = DateTime.DaysInMonth(now.Year, now.Month);
                ThirdMonthText = getMonths(now.Month);
                ThirdMonthValues = new ChartValues<double> { };
                Labels = new List<string> { };
                for (int i = 1; i <= daysCout; i++)
                {
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{now.Year}-{now.Month}-{i}'", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        Max2.Add(1);
                        Labels.Add(i.ToString());
                        ThirdMonthValues.Add(1);
                    }
                    else
                    {
                        Max2.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                        Labels.Add(i.ToString());
                        ThirdMonthValues.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }
                Max2Value = Max2.Max() + Math.Round((Max2.Max() * 0.2), 0);
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

        private void getTopDish()
        {
            DataContext = this;
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT TOP 1 Word, COUNT(*) AS WordCount FROM ( SELECT TRIM(value) AS Word FROM Orders" +
                    $" CROSS APPLY STRING_SPLIT(Orders_Dish_List, ',') AS SplitWords WHERE MONTH(CONVERT(DATE, Orders_Date, 104)) = MONTH(GETDATE()) AND YEAR(CONVERT(DATE, Orders_Date, 104)) = YEAR(GETDATE()) AND TRIM(value) " +
                    $"NOT IN ('Pouilly-Montrachet', 'Chablis', 'Saint-Émilion', 'Côte dOr', 'Chateau Angludet', 'Чёрный чай', 'Зелёный чай', 'Domaine Clarence')) " +
                    $"AS FilteredWords GROUP BY Word ORDER BY WordCount DESC;", connection);

                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                {
                    Dish = "Нет информации";
                    OrdersCount = "0";
                }
                else
                {
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Dish = rdr["Word"].ToString();
                        OrdersCount = rdr["WordCount"].ToString();
                    }
                    rdr.Close();

                    if (int.TryParse(OrdersCount, out int orders))
                    {
                        if (orders % 10 == 1 && orders % 100 != 11)
                        {
                            OrdersCount += " заказ";
                        }
                        else if ((orders % 10 >= 2 && orders % 10 <= 4) && (orders % 100 < 10 || orders % 100 >= 20))
                        {
                            OrdersCount += " заказа";
                        }
                        else
                        {
                            OrdersCount += " заказов";
                        }
                    }
                    else
                    {
                        OrdersCount = "Неверный формат";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
        }

        private void getOrdersCount()
        {
            DataContext = this;
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE MONTH(CONVERT(DATE, Orders_Date, 104)) = MONTH(GETDATE()) AND YEAR(CONVERT(DATE, Orders_Date, 104)) = YEAR(GETDATE())", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()))
                {
                    MonthOrdersCount = "0 заказов";
                }
                else
                {
                    MonthOrdersCount = cmd.ExecuteScalar().ToString() + " заказов";
                }

                MonthOrdersChartValues = new ChartValues<double> { };
                MonthOrdersChartValues.Add(1);
                Labels3 = new List<string> { };
                int daysCout = DateTime.DaysInMonth(now.Year, now.Month);
                for (int i = 1; i < now.Day + 1; i++)
                {
                    cmd = new SqlCommand($"SELECT COUNT(*) FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{now.Year}-{now.Month}-{i}'", connection);
                    Labels3.Add(i.ToString());
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        MonthOrdersChartValues.Add(1);
                    }
                    else
                    {
                        MonthOrdersChartValues.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }
                MonthOrdersMax = Convert.ToInt32(MonthOrdersChartValues.Max() + (MonthOrdersChartValues.Max() * 0.3));
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

        private void getLastOrders()
        {
            DataContext = this;
            List<LastDishes> Dishes = new List<LastDishes>();
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT TOP 50 * FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{now.Date:yyyy-MM-dd}' AND Orders_Time <= CONVERT(VARCHAR, GETDATE(), 108) ORDER BY Orders_Time DESC;", connection);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    LastDishes tableFiller = new LastDishes
                    {
                        Orders_ID = rdr["Orders_ID"].ToString(),
                        Orders_Bill = Convert.ToInt32(rdr["Orders_Bill"]),
                        Orders_Time = rdr["Orders_Time"].ToString().Substring(0, 5),
                        Orders_Serving_time = rdr["Orders_serving_time"].ToString().Substring(0, 5),
                        Orders_Dish_List = rdr["Orders_Dish_List"].ToString(),
                        Orders_Status = rdr["Orders_Status"].ToString(),
                    };
                    Dishes.Add(tableFiller);
                }
                DishesGrid.ItemsSource = Dishes;
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
        //Menu
        public void getMenu(string parameter)
        {
            DataContext = this;
            List<Menu> Dishes = new List<Menu>();
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT * FROM Menu {parameter};", connection);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Menu tableFiller = new Menu
                    {
                        ID = rdr["Menu_ID"].ToString(),
                        Name = rdr["Menu_Name"].ToString(),
                        Price = Convert.ToInt32(rdr["Menu_Price"]),
                        Image = ($"{getProjectFolderPath()}\\Images\\Dishes\\{rdr["Menu_Image"]}"),
                        Type = rdr["Menu_Type"].ToString(),
                    };
                    if (tableFiller.Image == $"{getProjectFolderPath()}\\Images\\Dishes\\")
                    {
                        tableFiller.Image = ($"{getProjectFolderPath()}\\Images\\Dishes\\no photo.png");
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


        //Analytics
        private void getAnalyticsCount(int days)
        {
            DataContext = this;
            List<Garcon> garcons = new List<Garcon>();
            if (days == 1)
            {
                try
                {
                    connection.Open();
                    CustomersDate.Text = $"На\n{DateTime.Now.ToString("dd.MM.yyyy")}";
                    SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) AS TotalVisitors FROM Orders WHERE CONVERT(DATE, Orders_Date, 104) = '{now.Date:yyyy-MM-dd}'", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        CustomersCount.Text = "0";
                    }
                    else
                    {
                        CustomersCount.Text = cmd.ExecuteScalar().ToString();
                    }
                    //REVENUE
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date = DATEADD(DAY, 0, CAST(GETDATE() AS DATE));", connection);
                    SqlCommand cmd2 = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date = DATEADD(DAY, -1, CAST(GETDATE() AS DATE));", connection);
                    revenueMoney.Text = $"{0}%";
                    revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int firstNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        int secondNumber = Convert.ToInt32(cmd2.ExecuteScalar());
                        double weekPercentDifference;
                        if (firstNumber > secondNumber)
                        {
                            weekPercentDifference = ((double)(secondNumber - firstNumber) / secondNumber) * 100;

                            revenueBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (firstNumber < secondNumber)
                        {
                            weekPercentDifference = ((double)(firstNumber - secondNumber) / Math.Max(firstNumber, secondNumber)) * 100;
                            revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                revenueMoney.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    revenueText.Text = $"На сегодня\n{DateTime.Now.ToString("dd.MM.yyyy")}";
                    //CUSTOMERS
                    cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date = DATEADD(DAY, 0, CAST(GETDATE() AS DATE));", connection);
                    cmd2 = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date = DATEADD(DAY, -1, CAST(GETDATE() AS DATE));", connection);
                    customersPercent.Text = $"{0}%";
                    customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int first = Convert.ToInt32(cmd.ExecuteScalar());
                        int second = Convert.ToInt32(cmd2.ExecuteScalar());
                        int maxNumber = Math.Max(first, second);
                        int minNumber = Math.Min(first, second);
                        double weekPercentDifference;
                        if (first > second)
                        {
                            weekPercentDifference = ((double)(second - first) / second) * 100;

                            customersBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (first < second)
                        {
                            weekPercentDifference = ((double)(first - second) / Math.Max(first, second)) * 100;
                            customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                customersPercent.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    customersText.Text = $"На сегодня\n{DateTime.Now.ToString("dd.MM.yyyy")}";
                    cmd = new SqlCommand($"SELECT Orders_Garcon, COUNT(*) AS Count FROM Orders WHERE Orders_Date = CAST(GETDATE() AS DATE) GROUP BY Orders_Garcon ORDER BY Count DESC;", connection);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Garcon tableFiller = new Garcon
                        {
                            GarconName = rdr["Orders_Garcon"].ToString(),
                            CountOfOrders = Convert.ToInt32(rdr["Count"]),
                        };
                        garcons.Add(tableFiller);
                    }
                    GarconsGrid.ItemsSource = garcons;
                    connection.Close();
                    connection.Open();
                    cmd = new SqlCommand($"SELECT AVG(DATEDIFF(MINUTE, Orders_Time, Orders_serving_time)) AS Average_Waiting_Time FROM Orders WHERE Orders_Date = CAST(GETDATE() AS DATE);", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        timeWaiting.Text = "0 минут";
                    }
                    else
                    {
                        timeWaiting.Text = $"{cmd.ExecuteScalar()} минут";
                    }
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
            else if (days == 7)
            {
                try
                {
                    connection.Open();
                    CustomersDate.Text = $"С {DateTime.Today.AddDays(-7).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) AS TotalVisitors FROM Orders WHERE Orders_Date >= DATEADD(DAY, -7, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        CustomersCount.Text = "0";
                    }
                    else
                    {
                        CustomersCount.Text = cmd.ExecuteScalar().ToString();
                    }
                    //REVENUE
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(DAY, -7, GETDATE()) AND Orders_Date < GETDATE();", connection);
                    SqlCommand cmd2 = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(DAY, -14, GETDATE()) AND Orders_Date < DATEADD(DAY, -7, GETDATE());", connection);
                    revenueMoney.Text = $"{0}%";
                    revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int firstNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        int secondNumber = Convert.ToInt32(cmd2.ExecuteScalar());
                        double weekPercentDifference;
                        if (firstNumber > secondNumber)
                        {
                            weekPercentDifference = ((double)(secondNumber - firstNumber) / secondNumber) * 100;

                            revenueBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (firstNumber < secondNumber)
                        {
                            weekPercentDifference = ((double)(firstNumber - secondNumber) / Math.Max(firstNumber, secondNumber)) * 100;
                            revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                revenueMoney.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    revenueText.Text = $"С {DateTime.Today.AddDays(-7).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    //CUSTOMERS
                    cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date = DATEADD(DAY, 0, CAST(GETDATE() AS DATE));", connection);
                    cmd2 = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date = DATEADD(DAY, -1, CAST(GETDATE() AS DATE));", connection);
                    customersPercent.Text = $"{0}%";
                    customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int first = Convert.ToInt32(cmd.ExecuteScalar());
                        int second = Convert.ToInt32(cmd2.ExecuteScalar());
                        int maxNumber = Math.Max(first, second);
                        int minNumber = Math.Min(first, second);
                        double weekPercentDifference;
                        if (first > second)
                        {
                            weekPercentDifference = ((double)(second - first) / second) * 100;

                            customersBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (first < second)
                        {
                            weekPercentDifference = ((double)(first - second) / Math.Max(first, second)) * 100;
                            customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                customersPercent.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    customersText.Text = $"С {DateTime.Today.AddDays(-7).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    cmd = new SqlCommand($"SELECT Orders_Garcon, COUNT(*) AS Count FROM Orders WHERE Orders_Date >= DATEADD(DAY, -7, GETDATE()) GROUP BY Orders_Garcon ORDER BY Count DESC;", connection);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Garcon tableFiller = new Garcon
                        {
                            GarconName = rdr["Orders_Garcon"].ToString(),
                            CountOfOrders = Convert.ToInt32(rdr["Count"]),
                        };
                        garcons.Add(tableFiller);
                    }
                    GarconsGrid.ItemsSource = garcons;
                    connection.Close();
                    connection.Open();
                    cmd = new SqlCommand($"SELECT AVG(DATEDIFF(MINUTE, Orders_Time, Orders_serving_time)) AS Average_Waiting_Time FROM Orders WHERE Orders_Date >= DATEADD(DAY, -7, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        timeWaiting.Text = "0 минут";
                    }
                    else
                    {
                        timeWaiting.Text = $"{cmd.ExecuteScalar()} минут";
                    }
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
            else if (days == 30)
            {
                try
                {
                    connection.Open();
                    CustomersDate.Text = $"С {DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date >= DATEADD(DAY, -30, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        CustomersCount.Text = "0";
                    }
                    else
                    {
                        CustomersCount.Text = cmd.ExecuteScalar().ToString();
                    }
                    //REVENUE
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -1, GETDATE());", connection);
                    SqlCommand cmd2 = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -2, GETDATE()) AND Orders_Date < DATEADD(MONTH, -1, GETDATE());", connection);
                    revenueMoney.Text = $"{0}%";
                    revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int firstNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        int secondNumber = Convert.ToInt32(cmd2.ExecuteScalar());
                        double weekPercentDifference;
                        if (firstNumber > secondNumber)
                        {
                            weekPercentDifference = ((double)(secondNumber - firstNumber) / secondNumber) * 100;

                            revenueBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (firstNumber < secondNumber)
                        {
                            weekPercentDifference = ((double)(firstNumber - secondNumber) / Math.Max(firstNumber, secondNumber)) * 100;
                            revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                revenueMoney.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    revenueText.Text = $"С {DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    //CUSTOMERS
                    cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -1, GETDATE()) AND Orders_Date < GETDATE();", connection);
                    cmd2 = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -2, GETDATE()) AND Orders_Date < DATEADD(MONTH, -1, GETDATE());", connection);
                    customersPercent.Text = $"{0}%";
                    customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int first = Convert.ToInt32(cmd.ExecuteScalar());
                        int second = Convert.ToInt32(cmd2.ExecuteScalar());
                        int maxNumber = Math.Max(first, second);
                        int minNumber = Math.Min(first, second);
                        double weekPercentDifference;
                        if (first > second)
                        {
                            weekPercentDifference = ((double)(second - first) / second) * 100;

                            customersBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (first < second)
                        {
                            weekPercentDifference = ((double)(first - second) / Math.Max(first, second)) * 100;
                            customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                customersPercent.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    customersText.Text = $"С {DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    cmd = new SqlCommand($"SELECT Orders_Garcon, COUNT(*) AS Count FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -2, GETDATE()) AND Orders_Date < DATEADD(MONTH, -1, GETDATE()) GROUP BY Orders_Garcon ORDER BY Count DESC;", connection);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Garcon tableFiller = new Garcon
                        {
                            GarconName = rdr["Orders_Garcon"].ToString(),
                            CountOfOrders = Convert.ToInt32(rdr["Count"]),
                        };
                        garcons.Add(tableFiller);
                    }
                    GarconsGrid.ItemsSource = garcons;
                    connection.Close();
                    connection.Open();
                    cmd = new SqlCommand($"SELECT AVG(DATEDIFF(MINUTE, Orders_Time, Orders_serving_time)) AS Average_Waiting_Time FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -2, GETDATE()) AND Orders_Date < DATEADD(MONTH, -1, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        timeWaiting.Text = "0 минут";
                    }
                    else
                    {
                        timeWaiting.Text = $"{cmd.ExecuteScalar()} минут";
                    }
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
            else if (days == 90)
            {
                try
                {
                    connection.Open();
                    CustomersDate.Text = $"С {DateTime.Today.AddMonths(-3).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) AS TotalVisitors FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -3, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        CustomersCount.Text = "0";
                    }
                    else
                    {
                        CustomersCount.Text = cmd.ExecuteScalar().ToString();
                    }
                    //REVENUE
                    cmd = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -3, GETDATE());", connection);
                    SqlCommand cmd2 = new SqlCommand($"SELECT SUM(Orders_Bill) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -6, GETDATE()) AND Orders_Date < DATEADD(MONTH, -3, GETDATE());", connection);
                    revenueMoney.Text = $"{0}%";
                    revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int firstNumber = Convert.ToInt32(cmd.ExecuteScalar());
                        int secondNumber = Convert.ToInt32(cmd2.ExecuteScalar());
                        double weekPercentDifference;
                        if (firstNumber > secondNumber)
                        {
                            weekPercentDifference = ((double)(secondNumber - firstNumber) / secondNumber) * 100;

                            revenueBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (firstNumber < secondNumber)
                        {
                            weekPercentDifference = ((double)(firstNumber - secondNumber) / Math.Max(firstNumber, secondNumber)) * 100;
                            revenueBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                revenueMoney.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                revenueMoney.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    revenueText.Text = $"С {DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    //CUSTOMERS
                    cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -3, GETDATE());", connection);
                    cmd2 = new SqlCommand($"SELECT SUM(Orders_Customers_Count) FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -6, GETDATE()) AND Orders_Date < DATEADD(MONTH, -3, GETDATE());", connection);
                    customersPercent.Text = $"{0}%";
                    customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xD4, 0xD4, 0xD4));
                    if (!Convert.IsDBNull(cmd.ExecuteScalar()) && !Convert.IsDBNull(cmd2.ExecuteScalar()))
                    {
                        int first = Convert.ToInt32(cmd.ExecuteScalar());
                        int second = Convert.ToInt32(cmd2.ExecuteScalar());
                        int maxNumber = Math.Max(first, second);
                        int minNumber = Math.Min(first, second);
                        double weekPercentDifference;
                        if (first > second)
                        {
                            weekPercentDifference = ((double)(second - first) / second) * 100;

                            customersBorder.Background = new SolidColorBrush(Color.FromRgb(0xA0, 0xE1, 0x82));
                            customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                        }
                        else if (first < second)
                        {
                            weekPercentDifference = ((double)(first - second) / Math.Max(first, second)) * 100;
                            customersBorder.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEA, 0x72, 0x72));
                            if (Math.Abs((int)weekPercentDifference) < 1)
                            {
                                customersPercent.Text = $"{Math.Abs((int)weekPercentDifference)}%";
                            }
                            else
                            {
                                customersPercent.Text = $"-{Math.Abs((int)weekPercentDifference)}%";
                            }
                        }
                    }
                    else
                    {
                    }
                    customersText.Text = $"С {DateTime.Today.AddMonths(-3).ToString("dd.MM.yyyy")}\nпо {DateTime.Now.ToString("dd.MM.yyyy")}";
                    cmd = new SqlCommand($"SELECT Orders_Garcon, COUNT(*) AS Count FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -3, GETDATE()) GROUP BY Orders_Garcon ORDER BY Count DESC;", connection);
                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Garcon tableFiller = new Garcon
                        {
                            GarconName = rdr["Orders_Garcon"].ToString(),
                            CountOfOrders = Convert.ToInt32(rdr["Count"]),
                        };
                        garcons.Add(tableFiller);
                    }
                    GarconsGrid.ItemsSource = garcons;
                    connection.Close();
                    connection.Open();
                    cmd = new SqlCommand($"SELECT AVG(DATEDIFF(MINUTE, Orders_Time, Orders_serving_time)) AS Average_Waiting_Time FROM Orders WHERE Orders_Date >= DATEADD(MONTH, -3, GETDATE());", connection);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        timeWaiting.Text = "0 минут";
                    }
                    else
                    {
                        timeWaiting.Text = $"{cmd.ExecuteScalar()} минут";
                    }
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
        }
        private void getOccupancy(string formattedDate)
        {
            DataContext = this;
            analyticsOccupancyHourButton.Tag = "Selected";
            occupancyColumnSeries.Title = "За этот час:";
            try
            {
                connection.Open();
                Occupancy = new ChartValues<double> { };
                Occupancy_Labels = new List<string> { };
                int hours = 0;
                if (formattedDate == $"{now.Date:yyyy-MM-dd}") { hours = now.Hour; }
                else { hours = 22; }
                for (int i = 10; i < hours; i++)
                {
                    SqlCommand cmd = new SqlCommand($"SELECT SUM(Orders_Customers_Count) AS Orders_Count FROM Orders WHERE CONVERT(TIME, Orders_Time) >= '{i}:00' AND CONVERT(TIME, Orders_Time) < '{i + 1}:00' AND CONVERT(DATE, Orders_Date, 104) = '{formattedDate}';", connection);
                    if (i < 22)
                    {
                        Occupancy_Labels.Add(i.ToString());
                        if (Convert.IsDBNull(cmd.ExecuteScalar()))
                        {
                            Occupancy.Add(1);
                        }
                        else
                        {
                            Occupancy.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                        }
                    }
                    else { }


                }
                Occupancy_Max = Convert.ToInt32(Occupancy.Max() + (Occupancy.Max() * 0.3));
                occupancyColumnSeries.Values = Occupancy;
                occupancyLabels.Labels = Occupancy_Labels;
                occupancyMax.MaxValue = Occupancy_Max;
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
        private void getOccupancyByWeek()
        {
            DataContext = this;
            occupancyColumnSeries.Title = "За этот день:";
            try
            {
                connection.Open();
                Occupancy = new ChartValues<double> { };
                Occupancy_Labels = new List<string> { };

                // Russian names for days of the week
                string[] daysOfWeek = { "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" };

                // Determine today's day of the week
                DateTime today = DateTime.Today;
                int todayIndex = (int)today.DayOfWeek;

                // Create a list of the last 7 days starting from the previous Sunday
                List<DateTime> lastWeekDates = new List<DateTime>();
                for (int i = 0; i < 7; i++)
                {
                    lastWeekDates.Add(today.AddDays(-todayIndex + i));
                }

                foreach (DateTime date in lastWeekDates)
                {
                    int dayIndex = (int)date.DayOfWeek;
                    string dayName = daysOfWeek[dayIndex];

                    SqlCommand cmd = new SqlCommand($@"
                SELECT SUM(Orders_Customers_Count) AS Orders_Count
                FROM Orders
                WHERE Orders_Date = CONVERT(DATE, '{date:yyyy-MM-dd}');", connection);

                    Occupancy_Labels.Add(dayName);
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        Occupancy.Add(0); // Assuming no orders, set to 0
                    }
                    else
                    {
                        Occupancy.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }

                Occupancy_Max = Convert.ToInt32(Occupancy.Max() + (Occupancy.Max() * 0.3));
                occupancyColumnSeries.Values = Occupancy;
                occupancyLabels.Labels = Occupancy_Labels;
                occupancyMax.MaxValue = Occupancy_Max;
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
        private void getOccupancyForCurrentMonth()
        {
            DataContext = this;
            occupancyColumnSeries.Title = "За этот день:";
            try
            {
                connection.Open();
                Occupancy = new ChartValues<double> { };
                Occupancy_Labels = new List<string> { };

                // Get the first and last date of the current month
                DateTime firstDayOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                // Iterate over each day of the current month
                for (DateTime date = firstDayOfMonth; date <= lastDayOfMonth; date = date.AddDays(1))
                {
                    SqlCommand cmd = new SqlCommand($@"
                SELECT SUM(Orders_Customers_Count) AS Orders_Count
                FROM Orders
                WHERE Orders_Date = CONVERT(DATE, '{date:yyyy-MM-dd}');", connection);

                    Occupancy_Labels.Add(date.ToString("dd MMM")); // Add day and month as label
                    if (Convert.IsDBNull(cmd.ExecuteScalar()))
                    {
                        Occupancy.Add(0); // Assuming no orders, set to 0
                    }
                    else
                    {
                        Occupancy.Add(Convert.ToDouble(cmd.ExecuteScalar()));
                    }
                }

                Occupancy_Max = Convert.ToInt32(Occupancy.Max() + (Occupancy.Max() * 0.3));
                occupancyColumnSeries.Values = Occupancy;
                occupancyLabels.Labels = Occupancy_Labels;
                occupancyMax.MaxValue = Occupancy_Max;
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
        //Buttons
        private void mainButton_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            DeselectAllButtons();
            showMainPage();
            mainButton.Tag = "Selected";
        }

        private void menuButton_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            DeselectAllButtons();
            showMenuPage();
            menuButton.Tag = "Selected";
        }

        private void tablesButton_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            DeselectAllButtons();
            tablesButton.Tag = "Selected";
            showReservationsPage();
        }

        private void analyticsButton_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            DeselectAllButtons();
            analyticsButton.Tag = "Selected";
            analyticsOccupancyHourButton.Tag = "Selected";
            showAnalyticsPage();
            analytcsGrid.Visibility = Visibility.Visible;
        }

        private void infoButton_Click(object sender, RoutedEventArgs e)
        {
            hideAll();
            DeselectAllButtons();
            infoBorder.Visibility = Visibility.Visible;
            infoGrid.Visibility = Visibility.Visible;
            DataContext = this;
            generateHardwareId();
            infoHardwareID.Text = readHardwareId();
            if (CheckDatabaseConnection() == true)
            {
                infoDBStatus.Foreground = Brushes.Green;
                infoDBStatus.Text = "Стабильное";
            }
            else
            {
                infoDBStatus.Foreground = Brushes.Red;
                infoDBStatus.Text = "Отсутствует";
            }
            infoButton.Tag = "Selected";
        }
        //Reservations
        private void getTablesCount()
        {
            DataContext = this;
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT COUNT(*) FROM Reservation WHERE CONVERT(DATE, Reservation_Date, 104) = '{now.Date:yyyy-MM-dd}' AND Reservation_Status = 'Активна' AND Reservation_Start < CONVERT(TIME, GETDATE()) AND Reservation_End > CONVERT(TIME, GETDATE());", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()) || ((int)cmd.ExecuteScalar() == 0))
                {
                    tablesActiveReservations.Text = "0";
                }
                else
                {
                    tablesActiveReservations.Text = cmd.ExecuteScalar().ToString();
                }

                cmd = new SqlCommand($"SELECT COUNT(*) FROM Tables WHERE Tables_Status = 'Занят'", connection);
                if (Convert.IsDBNull(cmd.ExecuteScalar()) || ((int)cmd.ExecuteScalar() == 0))
                {
                    tablesBusyTables.Text = "0";
                }
                else
                {
                    tablesBusyTables.Text = cmd.ExecuteScalar().ToString();
                }
                tablesFreeTables.Text = $"{16 - (Convert.ToInt32(tablesActiveReservations.Text)) - (Convert.ToInt32(tablesBusyTables.Text))}";
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
            getTablesCount();
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
        //Mac Gen 
        public static string generateHardwareId()
        {

            string macAddress = getMacAddress();
            if (!string.IsNullOrEmpty(macAddress))
            {

                using (MD5 md5 = MD5.Create())
                {

                    byte[] hashBytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(macAddress));
                    string hardwareId = BitConverter.ToString(hashBytes).Replace("-", "").ToLower().Substring(0, 10);

                    var dataToSave = new { HardwareId = hardwareId };

                    string filePath = $"{getProjectFolderPath()}\\Services\\hardware_id.json";
                    if (!File.Exists(filePath))
                    {
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(dataToSave));
                    }
                    return hardwareId;
                }
            }
            else
            {
                throw new Exception("Не удалось получить MAC-адрес");
            }
        }
        public static string readHardwareId()
        {
            string filePath = $"{getProjectFolderPath()}\\Services\\hardware_id.json";
            if (File.Exists(filePath))
            {

                string json = File.ReadAllText(filePath);

                JObject jsonObject = JObject.Parse(json);

                return (string)jsonObject["HardwareId"];
            }
            else
            {
                throw new FileNotFoundException("Файл с уникальным идентификатором не найден");
            }
        }
        private static string getMacAddress()
        {
            try
            {

                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();

                string macAddress = string.Empty;
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"])
                    {
                        macAddress = mo["MacAddress"].ToString();
                        break;
                    }
                }
                return macAddress;
            }
            catch (Exception ex)
            {

                Console.WriteLine("Ошибка при получении MAC-адреса: " + ex.Message);
                return null;
            }
        }
        private void menuResetButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("");
            menuResetButton.Tag = "Selected";
        }
        //DBCON
        public static bool CheckDatabaseConnection()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine("Ошибка подключения к базе данных: " + ex.Message);
                return false;
            }
        }
        //Buttons
        private void menuMeatButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Мясо'");
            menuMeatButton.Tag = "Selected";
        }
        private void menuFishButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Рыба'");
            menuFishButton.Tag = "Selected";
        }
        private void menuHotButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Горячее'");
            menuHotButton.Tag = "Selected";
        }
        private void menuSoupsButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Супы'");
            menuSoupsButton.Tag = "Selected";
        }
        private void menuSnacksButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Закуски'");
            menuSnacksButton.Tag = "Selected";
        }
        private void menuDrinksButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Вина' OR Menu_Type='Чай'");
            menuDrinksButton.Tag = "Selected";
        }
        private void menuSweatButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllMenuButtons();
            getMenu("WHERE Menu_Type='Десерты'");
            menuSweatButton.Tag = "Selected";
        }

        private void analyticsDayButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllAnalyticsButton();
            getAnalyticsCount(1);
            analyticsDayButton.Tag = "Selected";
        }
        private void analyticsWeekButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllAnalyticsButton();
            getAnalyticsCount(7);
            analyticsWeekButton.Tag = "Selected";
        }
        private void analyticsMonthButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllAnalyticsButton();
            getAnalyticsCount(30);
            analyticsMonthButton.Tag = "Selected";
        }
        private void analyticsQuarterButton_Click(object sender, RoutedEventArgs e)
        {
            DeselectAllAnalyticsButton();
            getAnalyticsCount(90);
            analyticsQuarterButton.Tag = "Selected";
        }

        private void DeselectAllButtons()
        {
            mainButton.Tag = null;
            menuButton.Tag = null;
            tablesButton.Tag = null;
            analyticsButton.Tag = null;
            infoButton.Tag = null;
        }

        private void DeselectAllMenuButtons()
        {
            menuResetButton.Tag = null;
            menuMeatButton.Tag = null;
            menuHotButton.Tag = null;
            menuSoupsButton.Tag = null;
            menuFishButton.Tag = null;
            menuSweatButton.Tag = null;
            menuSnacksButton.Tag = null;
            menuDrinksButton.Tag = null;
        }
        private void DeselectAllAnalyticsButton()
        {
            analyticsDayButton.Tag = null;
            analyticsWeekButton.Tag = null;
            analyticsMonthButton.Tag = null;
            analyticsQuarterButton.Tag = null;
        }
        private void hideAll()
        {
            mainDishBorder.Visibility = Visibility.Collapsed;
            mainHiBorder.Visibility = Visibility.Collapsed;
            main3Border.Visibility = Visibility.Collapsed;
            mainLastGrid.Visibility = Visibility.Collapsed;
            mainTop.Visibility = Visibility.Collapsed;
            mainTopDish.Visibility = Visibility.Collapsed;

            menuTopBorder.Visibility = Visibility.Collapsed;
            menuGrid.Visibility = Visibility.Collapsed;

            analyticsTopBorder.Visibility = Visibility.Collapsed;
            analytcsGrid.Visibility = Visibility.Collapsed;

            tablesBorder.Visibility = Visibility.Collapsed;
            reservationsGrid.Visibility = Visibility.Collapsed;

            infoBorder.Visibility = Visibility.Collapsed;
            infoGrid.Visibility = Visibility.Collapsed;

        }
        private void showMainPage()
        {
            getOrders();
            getLast3MonthOrders();
            getTopDish();
            getOrdersCount();
            getLastOrders();
            getMenu("");
            mainHiBorder.Visibility = Visibility.Visible;
            mainDishBorder.Visibility = Visibility.Visible;
            main3Border.Visibility = Visibility.Visible;
            mainLastGrid.Visibility = Visibility.Visible;
            mainTop.Visibility = Visibility.Visible;
            mainTopDish.Visibility = Visibility.Visible;
        }
        private void showMenuPage()
        {
            menuTopBorder.Visibility = Visibility.Visible;
            menuGrid.Visibility = Visibility.Visible;
            getMenu("");
        }
        public void showAnalyticsPage()
        {
            getOccupancy($"{now.Date:yyyy-MM-dd}");
            getAnalyticsCount(1);
            analyticsDayButton.Tag = "Selected";
            analyticsTopBorder.Visibility = Visibility.Visible;
            analytcsGrid.Visibility = Visibility.Visible;
        }
        public void showReservationsPage()
        {
            getTablesCount();
            ReservationUpdater();
            tablesBorder.Visibility = Visibility.Visible;
            reservationsGrid.Visibility = Visibility.Visible;
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
        private void menuListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (menuListView.SelectedIndex != -1)
            {
                Menu items = (Menu)menuListView.Items.GetItemAt(menuListView.SelectedIndex);
                if (items != null)
                {
                    menuEditWindow secondWindow = new menuEditWindow(items.ID, items.Name, items.Price, items.Image, items.Type);
                    secondWindow.ShowDialog();
                }
            }
        }

        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime selectedDate = datePicker.SelectedDate.GetValueOrDefault();
            string formattedDate = selectedDate.ToString("yyyy-MM-dd");
            getOccupancy(formattedDate);
            analyticsOccupancyDayButton.Tag = null;
        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            menuPrintWindow secondWindow = new menuPrintWindow();
            secondWindow.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            tablesReservations tablesWindow = new tablesReservations();
            tablesWindow.ShowDialog();
        }

        private void analyticsOccupancyHourButton_Click(object sender, RoutedEventArgs e)
        {
            analyticsOccupancyHourButton.Tag = "Selected";
            analyticsOccupancyDayButton.Tag = null;
            getOccupancy($"{now.Date:yyyy-MM-dd}");
        }

        private void analyticsOccupancyDayButton_Click(object sender, RoutedEventArgs e)
        {
            analyticsOccupancyDayButton.Tag = "Selected";
            analyticsOccupancyHourButton.Tag = null;
            getOccupancyForCurrentMonth();
        }
    }

}
