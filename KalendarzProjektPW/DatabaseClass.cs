using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace KalendarzProjektPW
{
    class DatabaseClass
    {
        private SQLiteConnection DatabaseConnection;
        private readonly object _monitor12;
        public DatabaseClass(object monitor)
        {
           _monitor12 = monitor;
           DatabaseConnection = new SQLiteConnection("Data Source=Database.sqlite;Version=3;");
           DatabaseConnection.Open();
        }

        public void SQLExecutor(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, DatabaseConnection);
            command.ExecuteNonQuery();
        }
        public SQLiteDataReader SQLReader(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, DatabaseConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            return reader;
        }

        public void UploadCoursesToDatabase(TextBlock DBLog,TextBlock counter,Image semaphoreRed)
        {
            lock (_monitor12)
            {
                int i = 0;
                SQLExecutor("DELETE FROM Course;");
                SQLExecutor("VACUUM;");
                Application.Current.Dispatcher.Invoke(delegate
                {
                    DBLog.Text = "";
                });
                string startupPath = Environment.CurrentDirectory;
                string[] przedmioty = File.ReadAllLines(startupPath + @"\Rozklad\new.txt");
                string sql;
                List<string> info = new List<string>();
                string Name = String.Empty;

                foreach (string line in przedmioty.Skip(1))
                {
                    i++;
                    sql = "insert into Course values(";
                    info = line.Split(',').ToList<string>();
                    if (info[0].Equals("Biznesowe"))
                    {
                        //name+location
                        Name = info[0] + info[1] + " " + info[2];
                        sql += "'" + Name + "',";
                        //date
                        sql +="'" +info[3] + "',";
                        //start time
                        sql += "'" + info[4] + "',";
                        //end time
                        sql += "'" + info[6] + "')";
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            counter.Text = Convert.ToString(i);
                            DBLog.Text += Name + "\n";
                        });
                        SQLExecutor(sql);
                    }
                    else
                    {
                        Name = info[0] + " " + info[1];
                        sql += "'" + info[0] + " " + info[1] + "',";
                        sql +="'"+ info[2] + "',";
                        sql += "'" + info[3] + "',";
                        sql += "'" + info[5] + "')";
                        Application.Current.Dispatcher.Invoke(delegate
                        {
                            counter.Text = Convert.ToString(i);
                            DBLog.Text += Name + "\n";
                        });
                        SQLExecutor(sql);
                    }
                }
                Application.Current.Dispatcher.Invoke(delegate
                {
                    semaphoreRed.Visibility = Visibility.Hidden;
                });
                Monitor.PulseAll(_monitor12);
            }
        }
    }
}
