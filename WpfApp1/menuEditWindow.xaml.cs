using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
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

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для menuEditWindow.xaml
    /// </summary>
    public partial class menuEditWindow : Window
    {
        private SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["constr"].ConnectionString);
        public string Image {  get; set; }
        public string Type { get; set; }
        public string ID { get; set; }
        DateTime now = DateTime.Now;
        public menuEditWindow(string IDValue,string Name,int Price,string ImageValue,string TypeValue)
        {
            InitializeComponent();
            DataContext = this;
            nameTextBlock.Text = Name;
            ID = IDValue;
            Type = TypeValue;
            Image = ImageValue;
            priceTextBlock.Text = Price.ToString();
            fillComboBox();
        }
        private void fillComboBox()
        {
            try
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand("SELECT DISTINCT Menu_Type FROM Menu", connection);
                List<string> menuTypes = new List<string>();
                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    menuTypes.Add(rdr["Menu_Type"].ToString());
                }
                typeComboBox.ItemsSource = menuTypes;
                typeComboBox.SelectedItem = Type;
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
        int Price;
        private bool Success => int.TryParse(priceTextBlock.Text, out Price);
        public async void setRed()
        {
            nameTextBlock.Background = new SolidColorBrush(Color.FromRgb(239, 142, 142));
            priceTextBlock.Background = new SolidColorBrush(Color.FromRgb(239, 142, 142));
            await Task.Delay(1000);
            nameTextBlock.Background = Brushes.White;
            priceTextBlock.Background = Brushes.White;
        }
        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(nameTextBlock.Text) && string.IsNullOrEmpty(priceTextBlock.Text))
            {
                setRed();
            }
            else if (string.IsNullOrEmpty(nameTextBlock.Text))
            {
                nameTextBlock.Background = new SolidColorBrush(Color.FromRgb(239, 142, 142));
                await Task.Delay(1000);
                nameTextBlock.Background = Brushes.White;
            }
            else if (string.IsNullOrEmpty(priceTextBlock.Text))
            {
                priceTextBlock.Background = new SolidColorBrush(Color.FromRgb(239, 142, 142));
                await Task.Delay(1000);
                priceTextBlock.Background = Brushes.White;
            }
            else
            {
                if (Success == true)
                {
                    if (Price > 0)
                    {
                        var Result = MessageBox.Show("Вы точно хотите применить изменения?", "Предупреждение о внесении изменений", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (Result == MessageBoxResult.Yes)
                        {
                            try
                            {
                                connection.Open();
                                SqlCommand cmd = new SqlCommand($"UPDATE Menu SET Menu_Name='{nameTextBlock.Text}', Menu_Price='{priceTextBlock.Text}', Menu_Type='{typeComboBox.SelectedItem}',Menu_Edit_Date='{now.Year}-{now.Month}-{now.Day}' WHERE Menu_ID='{ID}';", connection);
                                cmd.ExecuteScalar();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Error: " + ex.Message);
                            }
                            finally
                            {
                                MessageBox.Show("Успешно!\nОбновите страницу для отображения изменений.", "Изменения внесены", MessageBoxButton.OK, MessageBoxImage.Information);
                                connection.Close();
                                this.Close();
                            }
                        }
                        else if (Result == MessageBoxResult.No)
                        {
                            
                        } 
                    }
                    else
                    {
                        setRed();
                    }
                }
                else
                {
                    setRed();
                }
            }
        }   
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
