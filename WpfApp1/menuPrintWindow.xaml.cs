using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    public class DataGridMenuFiller
    {
        public int Price { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
    }
    public partial class menuPrintWindow : Window
    {
        private SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
        public menuPrintWindow()
        {
            DateTime now = DateTime.Now;
            InitializeComponent();
            actualText.Text = $"Актуально на {now.Day}.{now.Month}.{now.Year}";
            
            try
            {
                connection.Open();
                List<DataGridMenuFiller> menu = new List<DataGridMenuFiller>();
                SqlCommand cmd = new SqlCommand($"SELECT * FROM Menu;", connection);
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    DataGridMenuFiller tableFiller = new DataGridMenuFiller
                    {
                        Name = rdr["Menu_Name"].ToString(),
                        Price = Convert.ToInt32(rdr["Menu_Price"]),
                        Type = rdr["Menu_Type"].ToString(),
                        Date = rdr["Menu_Edit_Date"].ToString().Substring(0, 10),
                    };
                    menu.Add(tableFiller);
                }
                menuGrid.ItemsSource = menu;
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

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            printButton.Visibility = Visibility.Collapsed;
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(printGrid, "Печать");
                this.Close();
            }
        }
        private void DataGridRow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
