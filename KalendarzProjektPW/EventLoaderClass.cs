using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace KalendarzProjektPW
{
    class EventLoaderClass
    {
      private Queue<Event> _events;
      private object _monitor;
      private CalendarService _service;
        public static void DeleteCalendar()
        {

            StreamReader r = new StreamReader("credentials.json");
            string json = r.ReadToEnd();
            string[] value = json.Split('"');

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
              new ClientSecrets
              {
                  ClientId = value[5],
                  ClientSecret = value[25],
              },
              new[] { CalendarService.Scope.Calendar },
              "user",
              CancellationToken.None).Result;
            CalendarService service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });
            try
            {
                service.Calendars.Clear("primary").Execute();
            }
            catch { }

        }
        public EventLoaderClass(object monitor, Queue<Event> events)
        {
            _monitor = monitor;
            _events = events;

            StreamReader r = new StreamReader("credentials.json");
            string json = r.ReadToEnd();
            string[] value = json.Split('"');
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = value[5],
                    ClientSecret = value[25],
                },
                new[] { CalendarService.Scope.Calendar },
                "user", CancellationToken.None).Result;

            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Calendar API Sample",
            });
        }

        public void EventUpload(TextBlock log,TextBlock logCreate,TextBlock counter,TextBlock myCounter,Ellipse myLight,Ellipse Light,Image S1Red,Button StartButton, Button CancelButton,Button DeleteButton)
        {
            Event tmp;
            bool flag = true;
            int i = 0;
            while (flag)
            {
                lock (_monitor)
                {
                    while (_events.Count == 0)
                    {
                        Monitor.Wait(_monitor);
                    }
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        myLight.Visibility = Visibility.Visible;
                        Light.Visibility = Visibility.Hidden;
                    });
                    tmp = _events.Dequeue();
                    Application.Current.Dispatcher.Invoke(delegate
                    {
                        logCreate.Text = "";
                        counter.Text = Convert.ToString(_events.Count);
                        foreach (var e in _events)
                        {
                            logCreate.Text += e.Summary + "\n";
                        }
                    });
                    Monitor.PulseAll(_monitor);
                }
                _service.Events.Insert(tmp, "primary").Execute();
                i++;
                Application.Current.Dispatcher.Invoke(delegate
                {
                    myCounter.Text = Convert.ToString(i);
                    log.Text += tmp.Summary + "\n";
                    if (S1Red.Visibility == Visibility.Visible && counter.Text=="0")
                        flag = false;
                });
            }
            Application.Current.Dispatcher.Invoke(delegate
            {
                StartButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Hidden;
                DeleteButton.IsEnabled = true;
            });
        }
    }
}
