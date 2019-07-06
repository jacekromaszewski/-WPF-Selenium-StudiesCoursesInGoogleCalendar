using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace KalendarzProjektPW
{
    class EventCreatorClass
    {
        private readonly object _monitor12, _monitor23;
        private DatabaseClass _databaseClass;
        private Queue<Event> _events;
        public EventCreatorClass(object monitor,object monitor2,DatabaseClass databaseClass, Queue<Event> events)
        {
            _monitor12 = monitor;
            _monitor23 = monitor2;
            _databaseClass = databaseClass;
            _events = events;
        }

        public void CreateEvents(TextBlock log, Image semaphoreRed,TextBlock counter,Ellipse myLight,Ellipse Light)
        {
            int i = 0;
            lock(_monitor12)
            {
                while(semaphoreRed.Visibility==Visibility.Visible)
                {
                    Monitor.Wait(_monitor12);
                }
                Application.Current.Dispatcher.Invoke(delegate
                {
                    myLight.Visibility = Visibility.Visible;
                    Light.Visibility = Visibility.Hidden;
                });
                var reader = _databaseClass.SQLReader("select * from Course");

                while (reader.Read())
                {
                    var Date =((string) reader["StartDate"]).Split('-');
                    var StartTime = ((string)reader["StartTime"]).Split(':');
                    var EndTime = ((string)reader["EndTime"]).Split(':');
                    var Name = (string)reader["Title"];
                    string Color="11";
                    if (Name.Contains("(w)"))
                        Color = "9";

                    if (StartTime.Length > 1)
                    {
                        Event myEvent = new Event
                        {
                            Summary = Name,
                            ColorId = Color,
                            Start = new EventDateTime()
                            {
                                DateTime = new DateTime(Int32.Parse(Date[0]), Int32.Parse(Date[1]), Int32.Parse(Date[2]), Int32.Parse(StartTime[0]), Int32.Parse(StartTime[1]), 0),
                            },
                            End = new EventDateTime()
                            {
                                DateTime = new DateTime(Int32.Parse(Date[0]), Int32.Parse(Date[1]), Int32.Parse(Date[2]), Int32.Parse(EndTime[0]), Int32.Parse(EndTime[1]), 0),
                            }
                        };

                        lock (_monitor23)
                        {
                            Application.Current.Dispatcher.Invoke(delegate
                            {
                                myLight.Visibility = Visibility.Visible;
                                Light.Visibility = Visibility.Hidden;
                                _events.Enqueue(myEvent);
                                i++;
                                log.Text = "";
                                counter.Text = Convert.ToString(i);
                                foreach(var e in _events )
                                {
                                    log.Text += e.Summary + "\n";
                                }
                            });
                            Monitor.PulseAll(_monitor23);
                        }
                        Thread.Sleep(400);
                    }
                }
                Application.Current.Dispatcher.Invoke(delegate
                {
                    semaphoreRed.Visibility = Visibility.Visible;
                });
            }
            
        }

    }
}
