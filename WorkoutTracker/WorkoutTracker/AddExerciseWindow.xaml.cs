using System.Linq;
using System.Windows;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    public partial class AddExerciseWindow : Window
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

        public string ExerciseName { get; private set; } = "";

        public AddExerciseWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            var categories = _dbHelper.ExecuteQuery(
                "SELECT DISTINCT Category FROM Exercises ORDER BY Category");

            foreach (System.Data.DataRow row in categories.Rows)
                cmbCategory.Items.Add(row["Category"].ToString());
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Валидация (ПР5)
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Введите название упражнения!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(cmbCategory.Text))
            {
                MessageBox.Show("Введите категорию упражнения!",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка дубликата (ПР5)
            var duplicate = _dbHelper.ExecuteScalarWithParams(
                "SELECT COUNT(*) FROM Exercises WHERE Name = @name",
                new Microsoft.Data.SqlClient.SqlParameter("@name", tbName.Text.Trim()));

            if (duplicate != null && Convert.ToInt32(duplicate) > 0)
            {
                MessageBox.Show("Упражнение с таким названием уже существует!",
                    "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int result = _dbHelper.ExecuteNonQueryWithParams(
                "INSERT INTO Exercises (Name, Category, MuscleGroup) VALUES (@name, @category, @muscleGroup)",
                new Microsoft.Data.SqlClient.SqlParameter("@name", tbName.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@category", cmbCategory.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@muscleGroup", tbMuscleGroup.Text.Trim()));

            if (result > 0)
            {
                ExerciseName = tbName.Text.Trim();
                DialogResult = true;
            }
        }
    }
}
