using System;
using System.Windows;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    public partial class EditExerciseWindow : Window
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();
        private readonly int _exerciseId;

        public EditExerciseWindow(int id, string name, string category, string muscleGroup)
        {
            InitializeComponent();
            _exerciseId = id;

            // Предзаполнение полей
            tbName.Text = name;
            cmbCategory.Text = category;
            tbMuscleGroup.Text = muscleGroup;

            // Заполнение списка категорий
            var categories = _dbHelper.ExecuteQuery(
                "SELECT DISTINCT Category FROM Exercises ORDER BY Category");
            foreach (System.Data.DataRow row in categories.Rows)
                cmbCategory.Items.Add(row["Category"].ToString());
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
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

            // Проверка дубликата (кроме текущей записи)
            var duplicate = _dbHelper.ExecuteScalarWithParams(
                "SELECT COUNT(*) FROM Exercises WHERE Name = @name AND Id <> @id",
                new Microsoft.Data.SqlClient.SqlParameter("@name", tbName.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@id", _exerciseId));

            if (duplicate != null && Convert.ToInt32(duplicate) > 0)
            {
                MessageBox.Show("Упражнение с таким названием уже существует!",
                    "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int result = _dbHelper.ExecuteNonQueryWithParams(
                "UPDATE Exercises SET Name = @name, Category = @category, " +
                "MuscleGroup = @muscleGroup WHERE Id = @id",
                new Microsoft.Data.SqlClient.SqlParameter("@name", tbName.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@category", cmbCategory.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@muscleGroup", tbMuscleGroup.Text.Trim()),
                new Microsoft.Data.SqlClient.SqlParameter("@id", _exerciseId));

            if (result > 0)
                DialogResult = true;
        }
    }
}
