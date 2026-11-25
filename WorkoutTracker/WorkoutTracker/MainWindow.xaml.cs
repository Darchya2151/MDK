using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using WorkoutTracker.Data;

namespace WorkoutTracker
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseHelper _dbHelper = new DatabaseHelper();
        private DataTable _allExercises; // кэш всех упражнений (ПР3)

        public MainWindow()
        {
            InitializeComponent();
            LoadCategories(); // ПР3: заполнение категорий при старте
        }

        // ПР2: загрузка всех упражнений
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "";
            cmbCategory.SelectedIndex = -1;
            RefreshData();
        }

        // ПР4: общий метод обновления таблицы (учитывает фильтры)
        private void RefreshData()
        {
            try
            {
                btnLoad.IsEnabled = false;
                tbStatus.Text = "Загрузка данных...";

                LoadExercises(tbSearch.Text, cmbCategory.SelectedItem as string);

                if (_allExercises != null && _allExercises.Rows.Count > 0)
                {
                    tbRecordCount.Text = "Записей: " + _allExercises.Rows.Count;
                    tbStatus.Text = "Данные загружены";
                }
                else
                {
                    dgExercises.ItemsSource = null;
                    tbRecordCount.Text = "Записей: 0";
                    tbStatus.Text = "Записи не найдены";
                }
            }
            finally
            {
                btnLoad.IsEnabled = true;
            }
        }

        // ПР3: загрузка упражнений с параметрами поиска/фильтра
        private void LoadExercises(string searchText, string category)
        {
            string sql = "SELECT Id, Name, Category, MuscleGroup FROM Exercises WHERE 1=1";
            var conditions = new List<string>();
            var parameters = new List<Microsoft.Data.SqlClient.SqlParameter>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                conditions.Add("Name LIKE @searchPattern");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(
                    "@searchPattern", "%" + searchText + "%"));
            }
            if (!string.IsNullOrWhiteSpace(category))
            {
                conditions.Add("Category = @category");
                parameters.Add(new Microsoft.Data.SqlClient.SqlParameter(
                    "@category", category));
            }

            if (conditions.Any())
                sql += " AND " + string.Join(" AND ", conditions);

            sql += " ORDER BY Name";

            _allExercises = _dbHelper.ExecuteQueryWithParams(sql, parameters.ToArray());
            dgExercises.ItemsSource = _allExercises.DefaultView;
        }

        // ПР3: загрузка уникальных категорий
        private void LoadCategories()
        {
            DataTable categories = _dbHelper.ExecuteQuery(
                "SELECT DISTINCT Category FROM Exercises ORDER BY Category");

            cmbCategory.Items.Clear();
            foreach (DataRow row in categories.Rows)
                cmbCategory.Items.Add(row["Category"].ToString());
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void CmbCategory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshData();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            tbSearch.Text = "";
            cmbCategory.SelectedIndex = -1;
            RefreshData();
        }

        // ПР4: добавление упражнения
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddExerciseWindow addWindow = new AddExerciseWindow();
            addWindow.Owner = this;

            if (addWindow.ShowDialog() == true)
            {
                RefreshData();
                LoadCategories(); // ПР5: новая категория появляется в фильтре
                tbStatus.Text = "Упражнение \"" + addWindow.ExerciseName + "\" добавлено";
            }
        }

        // ПР4: удаление упражнения
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgExercises.SelectedItem is DataRowView row)
            {
                int id = Convert.ToInt32(row["Id"]);
                string name = row["Name"].ToString();

                var result = MessageBox.Show(
                    "Удалить упражнение \"" + name + "\"?\n\n" +
                    "Все связанные записи журнала тренировок будут удалены каскадно!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    int affected = _dbHelper.ExecuteNonQueryWithParams(
                        "DELETE FROM Exercises WHERE Id = @id",
                        new Microsoft.Data.SqlClient.SqlParameter("@id", id));

                    if (affected > 0)
                    {
                        RefreshData();
                        LoadCategories();
                        tbStatus.Text = "Упражнение \"" + name + "\" удалено";
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите упражнение для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ПР5: двойной клик — редактирование
        private void DgExercises_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgExercises.SelectedItem is DataRowView row)
            {
                EditExerciseWindow editWindow = new EditExerciseWindow(
                    Convert.ToInt32(row["Id"]),
                    row["Name"].ToString(),
                    row["Category"].ToString(),
                    row["MuscleGroup"].ToString());
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    RefreshData();
                    LoadCategories();
                    tbStatus.Text = "Упражнение обновлено";
                }
            }
        }
    }
}
