using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;

namespace YP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=NPK-102O-11;Database=University;Integrated Security=true;Encrypt=False;";
        public MainWindow()
        {
            InitializeComponent();
            LoadStudentData();
        }
        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text;
            string[] nameParts = fullName.Split(' ');

            if (nameParts.Length < 2)
            {
                MessageBox.Show("Пожалуйста, введите полное имя (Имя Фамилия).");
                return;
            }

            string firstName = nameParts[0];
            string lastName = nameParts[1];
            DateTime birthDate = (DateTime)dtpBirthDate.SelectedDate;

            AddStudent(firstName, lastName, birthDate);
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();

            listBoxSearchResults.Items.Clear(); // Очищаем список перед загрузкой данных

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT FirstName, LastName, DateOfBirth FROM Students WHERE FirstName LIKE @SearchTerm OR LastName LIKE @SearchTerm";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@SearchTerm", string.IsNullOrEmpty(searchTerm) ? "%" : $"{searchTerm}%");

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string fullName = $"{reader["FirstName"]} {reader["LastName"]} - {((DateTime)reader["DateOfBirth"]).ToShortDateString()}";
                        listBoxSearchResults.Items.Add(fullName);
                    }

                    if (listBoxSearchResults.Items.Count == 0)
                    {
                        MessageBox.Show("Студенты не найдены.");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (listBoxStudents.SelectedItem != null)
            {
                string selectedStudent = listBoxStudents.SelectedItem.ToString();
                string[] parts = selectedStudent.Split(new[] { ' ' }, 2); // Разделяем имя и фамилию
                string firstName = parts[0];
                string lastName = parts[1].Split('-')[0].Trim(); // Извлекаем фамилию

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        string query = "DELETE FROM Students WHERE FirstName = @FirstName AND LastName = @LastName";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Студент успешно удален.");
                            LoadStudentData(); // Обновляем список студентов
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить студента. Проверьте данные.");
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите студента для удаления.");
            }
        }

        private void AddStudent(string firstName, string lastName, DateTime birthDate)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO Students (FirstName, LastName, DateOfBirth) VALUES (@FirstName, @LastName, @DateOfBirth)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@LastName", lastName);
                        command.Parameters.AddWithValue("@DateOfBirth", birthDate);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            LoadStudentData();
                            MessageBox.Show("Студент успешно добавлен.");
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении студента.");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private void LoadStudentData()
        {
            listBoxStudents.Items.Clear(); // Очищаем список перед загрузкой данных

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT FirstName, LastName, DateOfBirth FROM Students";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string fullName = $"{reader["FirstName"]} {reader["LastName"]} - {((DateTime)reader["DateOfBirth"]).ToShortDateString()}";
                        listBoxStudents.Items.Add(fullName);
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
    }
}