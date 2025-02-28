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
        private Dictionary<int, string> studentDict = new Dictionary<int, string>();
        public MainWindow()
        {
            InitializeComponent();
            LoadStudentData();
            loadCourses();
            LoadStudentCountsByCourse();
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

            if (!dtpBirthDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату рождения.");
                return;
            }

            DateTime birthDate = dtpBirthDate.SelectedDate.Value;

            // Проверяем, что дата рождения не в будущем
            if (birthDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем.");
                return;
            }

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
        private void loadCourses()
        {
            allCourses.Items.Clear(); // Очищаем список перед загрузкой данных

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT CourseName FROM Courses"; 
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string courseName = reader["CourseName"].ToString(); 
                        allCourses.Items.Add(courseName); 
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }

        private void LoadStudentCountsByCourse()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT c.CourseName, COUNT(e.StudentId) AS StudentCount
                FROM Courses c
                LEFT JOIN Enrollments e ON c.CourseId = e.CourseId
                GROUP BY c.CourseName
                ORDER BY c.CourseName;";

                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Предполагаем, что у вас есть DataGrid для отображения данных
                    dataGridCourses.ItemsSource = dataTable.DefaultView;
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }
        private void ButtonAddCourse_Click(object sender, RoutedEventArgs e)
        {
            string courseName = textBoxCourseName.Text;

            if (string.IsNullOrEmpty(courseName))
            {
                MessageBox.Show("Пожалуйста, введите название курса.");
                return;
            }

            AddNewCourse(courseName);
        }

        private void AddNewCourse(string courseName)
        {
            // Проверка на пустое значение
            if (string.IsNullOrEmpty(courseName))
            {
                MessageBox.Show("Название курса не может быть пустым.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO dbo.Courses (CourseName) VALUES (@CourseName)";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@CourseName", courseName);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            loadCourses();
                            MessageBox.Show("Курс успешно добавлен.");
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении курса.");
                        }
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }



        private void btnEnroll_Click(object sender, EventArgs e)
        {
            if (listBoxStudents.SelectedItem == null || allCourses.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите студента и курс.");
                return;
            }

            string selectedStudent = listBoxStudents.SelectedItem.ToString();
            // Получаем StudentId из словаря по имени
            int studentId = studentDict.FirstOrDefault(x => x.Value == selectedStudent).Key;

            // Получаем CourseId. Предполагаем, что CourseId начинается с 1.
            int courseId = allCourses.SelectedIndex + 1; // Если CourseId начинается с 1

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Проверяем, есть ли уже запись о студенте на курсе
                    string checkQuery = "SELECT COUNT(*) FROM Enrollments WHERE StudentId = @StudentId AND CourseId = @CourseId";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@StudentId", studentId);
                    checkCommand.Parameters.AddWithValue("@CourseId", courseId);

                    int enrollmentCount = (int)checkCommand.ExecuteScalar();

                    if (enrollmentCount > 0)
                    {
                        MessageBox.Show("Студент уже записан на этот курс.");
                        return; // Выход из метода, если запись уже существует
                    }

                    // Если записи нет, добавляем нового студента на курс
                    string insertQuery = "INSERT INTO Enrollments (StudentId, CourseId) VALUES (@StudentId, @CourseId)";
                    SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@StudentId", studentId);
                    insertCommand.Parameters.AddWithValue("@CourseId", courseId);

                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Студент успешно записан на курс.");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при записи студента на курс.");
                    }
                }
                catch (SqlException ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}");
                }
            }
        }


        private void LoadStudentCourses(object sender, EventArgs e)
        {
            listBoxCourses.Items.Clear(); // Очищаем список перед загрузкой данных
            try
            {
                if (listBoxStudents.SelectedItem == null)
                {
                    MessageBox.Show("Выберите студента");
                    return;
                }

                string selectedStudent = listBoxStudents.SelectedItem.ToString();
                // Получаем StudentId из словаря по имени
                int studentId = studentDict.FirstOrDefault(x => x.Value == selectedStudent).Key;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT c.CourseName FROM Students s " +
                                   "JOIN Enrollments e ON s.StudentId = e.StudentId " +
                                   "JOIN Courses c ON e.CourseId = c.CourseId " +
                                   "WHERE s.StudentId = @StudentId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string courseName = reader["CourseName"].ToString();
                        listBoxCourses.Items.Add(courseName);
                    }

                    if (listBoxCourses.Items.Count == 0)
                    {
                        MessageBox.Show("Курсы не найдены для данного студента.");
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        private async void AddStudentAndEnroll(object sender, EventArgs e)
        {
            string fullName = txtFullName.Text;
            string[] nameParts = fullName.Split(' ');
            string firstName = nameParts[0];
            string lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            if (!dtpBirthDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату рождения.");
                return;
            }

            DateTime birthDate = dtpBirthDate.SelectedDate.Value;

            if (birthDate > DateTime.Now)
            {
                MessageBox.Show("Дата рождения не может быть в будущем.");
                return;
            }

            string courseName = allCourses.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(courseName))
            {
                MessageBox.Show("Имя, фамилия студента и название курса не могут быть пустыми.");
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Добавление студента
                        string addStudentQuery = "INSERT INTO dbo.Students (FirstName, LastName, DateOfBirth) OUTPUT INSERTED.StudentId VALUES (@FirstName, @LastName, @DateOfBirth)";
                        int studentId;

                        using (SqlCommand addStudentCommand = new SqlCommand(addStudentQuery, connection, transaction))
                        {
                            addStudentCommand.Parameters.AddWithValue("@FirstName", firstName);
                            addStudentCommand.Parameters.AddWithValue("@LastName", lastName);
                            addStudentCommand.Parameters.AddWithValue("@DateOfBirth", birthDate);
                            studentId = (int)await addStudentCommand.ExecuteScalarAsync();
                        }

                        // Запись студента на курс
                        string enrollStudentQuery = "INSERT INTO dbo.Enrollments (StudentId, CourseId) VALUES (@StudentId, (SELECT CourseId FROM dbo.Courses WHERE CourseName = @CourseName))";
                        using (SqlCommand enrollStudentCommand = new SqlCommand(enrollStudentQuery, connection, transaction))
                        {
                            enrollStudentCommand.Parameters.AddWithValue("@StudentId", studentId);
                            enrollStudentCommand.Parameters.AddWithValue("@CourseName", courseName);
                            await enrollStudentCommand.ExecuteNonQueryAsync();
                        }
                        // Подтверждаем транзакцию
                        transaction.Commit();
                        MessageBox.Show("Студент успешно добавлен и записан на курс.");

                        // Обновляем интерфейс после завершения операций
                        await UpdateUIAsync();
                        LoadStudentData();
                        LoadStudentCountsByCourse();
                    }
                    catch (SqlException ex)
                    {
                        // В случае ошибки откатываем транзакцию
                        transaction.Rollback();
                        MessageBox.Show($"Ошибка: {ex.Message}");
                    }
                }
            }
        }

        private async Task UpdateUIAsync()
        {
            // Выполняем обновление данных асинхронно
            await Task.WhenAll(
                LoadStudentCountsByCourseAsync(),
                LoadStudentDataAsync(),
                LoadCoursesAsync()
            );
        }

        private async Task LoadStudentCountsByCourseAsync()
        {
            // Ваш код для загрузки количества студентов по курсам
            // Пример:
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT CourseName, COUNT(StudentId) AS StudentCount FROM dbo.Enrollments JOIN dbo.Courses ON Enrollments.CourseId = Courses.CourseId GROUP BY CourseName";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Обновите интерфейс с полученными данными
                        while (await reader.ReadAsync())
                        {
                            string courseName = reader.GetString(0);
                            int studentCount = reader.GetInt32(1);
                            // Обновите ваш интерфейс, например, добавьте данные в ListView или Label
                        }
                    }
                }
            }
        }

        private async Task LoadStudentDataAsync()
        {
            // Ваш код для загрузки данных студентов
            // Пример:
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT FirstName, LastName FROM dbo.Students";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Обновите интерфейс с полученными данными
                        while (await reader.ReadAsync())
                        {
                            string firstName = reader.GetString(0);
                            string lastName = reader.GetString(1);
                            // Обновите ваш интерфейс, например, добавьте данные в ListView или DataGrid
                        }
                    }
                }
            }
        }

        private async Task LoadCoursesAsync()
        {
            // Ваш код для загрузки курсов
            // Пример:
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT CourseName FROM dbo.Courses";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Обновите интерфейс с полученными данными
                        while (await reader.ReadAsync())
                        {
                            string courseName = reader.GetString(0);
                            // Добавьте курс в ComboBox или ListBox
                            allCourses.Items.Add(courseName);
                        }
                    }
                }
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
            studentDict.Clear(); // Очищаем словарь

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT StudentId, FirstName, LastName, DateOfBirth FROM Students";
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int studentId = (int)reader["StudentId"];
                        string fullName = $"{reader["FirstName"]} {reader["LastName"]} - {((DateTime)reader["DateOfBirth"]).ToShortDateString()}";
                        listBoxStudents.Items.Add(fullName);
                        studentDict[studentId] = fullName; // Сохраняем соответствие StudentId и полному имени
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