using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace WorkoutTracker.Data
{
    public class DatabaseHelper
    {
        // Имя сервера возьми из Шага 8 ПР1 (результат @@SERVERNAME)
        private readonly string connectionString =
            "Server=localhost;Database=Darchya2151;Trusted_Connection=True;TrustServerCertificate=True;";

        public SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public DataTable ExecuteQuery(string sql)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            return table;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка SQL (№" + ex.Number + "):\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                return new DataTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new DataTable();
            }
        }

        // ПР3: запрос с параметрами (защита от SQL-инъекций)
        public DataTable ExecuteQueryWithParams(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null) command.Parameters.AddRange(parameters);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            return table;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка SQL (№" + ex.Number + "):\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                return new DataTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return new DataTable();
            }
        }

        public int ExecuteNonQuery(string sql)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка SQL (№" + ex.Number + "):\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        // ПР4: ExecuteNonQuery с параметрами + ПР5: детальная обработка ошибок
        public int ExecuteNonQueryWithParams(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null) command.Parameters.AddRange(parameters);
                        return command.ExecuteNonQuery();
                    }
                }
            }
            // ПР5: дубликат (уникальный ключ)
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                MessageBox.Show("Запись с таким названием уже существует!",
                    "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
            // ПР5: нарушение внешнего ключа
            catch (SqlException ex) when (ex.Number == 547)
            {
                MessageBox.Show("Нарушение связи между таблицами:\n" + ex.Message,
                    "Ошибка внешнего ключа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Ошибка SQL (№" + ex.Number + "):\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1;
            }
        }

        public object ExecuteScalarWithParams(string sql, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    using (var command = new SqlCommand(sql, connection))
                    {
                        if (parameters != null) command.Parameters.AddRange(parameters);
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
