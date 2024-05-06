using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MySql.Data.MySqlClient;
using System;

namespace AvaloniaApplication.Models
{
    public class DatabaseManager : Window
    {
        private readonly MySqlConnection _connection;

        public DatabaseManager()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            _connection = new MySqlConnection("your_connection_string_here");
            _connection.Open();

            // Example query
            using (var cmd = new MySqlCommand("SELECT * FROM your_table", _connection))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetString(0)); // Print first column value
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _connection.Close();
        }
    }
}