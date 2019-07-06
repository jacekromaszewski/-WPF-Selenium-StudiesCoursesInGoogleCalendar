using System.Collections.Generic;
using System.Windows;
using Google.Apis.Calendar.v3.Data;
using System.Threading;

namespace KalendarzProjektPW
{

    public partial class MainWindow : Window
    {
        private readonly object monitor12 = new object();
        private readonly object monitor23 = new object();
        private Queue<Event> events = new Queue<Event>();
        public MainWindow()
        {
            
            DatabaseClass databaseClass = new DatabaseClass(monitor12);
            EventCreatorClass eventCreatorClass = new EventCreatorClass(monitor12, monitor23, databaseClass, events);
            EventLoaderClass eventLoaderClass = new EventLoaderClass(monitor23, events);
            List<Thread> threads=new List<Thread>();
            InitializeComponent();
            SeleniumClass selenium = new SeleniumClass(ComboGroup, ComboSemester, INFO, USER, PASS,HIDE);
            StartButton.Click += (sender, EventArgs) => StartButton_Click(sender, EventArgs, databaseClass,eventCreatorClass,eventLoaderClass,threads);
            CancelButton.Click += (sender, EventArgs) => Cancel_Click(sender, EventArgs,threads);
            LoginButton.Click += (sender, EventArgs) => LoginClick(sender, EventArgs, selenium);
            SelectButton.Click += (sender, EventArgs) => SelectClick(sender, EventArgs, selenium);
            DownloadButton.Click += (sender, EventArgs) => DownloadClick(sender, EventArgs, selenium);

        }

        private void StartButton_Click(object sender, RoutedEventArgs e, DatabaseClass databaseClass,EventCreatorClass eventCreatorClass,EventLoaderClass eventLoaderClass,List<Thread> threads)
        {
            Thread databaseThread = new Thread(() => databaseClass.UploadCoursesToDatabase(DBLog, DBCounter, S1Red));
            threads.Add(databaseThread);
            Thread creatorThread = new Thread(() => eventCreatorClass.CreateEvents(EventCreateLog, S1Red, EventCreatedCounter, CreatorLight, LoaderLight));
            threads.Add(creatorThread);
            Thread loaderThread = new Thread(() => eventLoaderClass.EventUpload(EventLoaderLog, EventCreateLog, EventCreatedCounter, LoaderCounter, LoaderLight, CreatorLight,S1Red,StartButton,CancelButton,DeleteButton));
            threads.Add(loaderThread);
                
            foreach(var th in threads)
            {
                th.Start();
            }
            StartButton.Visibility = Visibility.Hidden;
            CancelButton.Visibility = Visibility.Visible;
            DeleteButton.IsEnabled = false;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            EventLoaderClass.DeleteCalendar();
            DeleteInfo.Text = "Deleted";
        }

        private  void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(StartButton.Visibility==Visibility.Hidden)
                e.Cancel = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Cancel_Click(object sender, RoutedEventArgs e, List<Thread> threads)
        {
            foreach(Thread th in threads)
            {
                if (th.IsAlive)
                    th.Abort();
            }
            threads.Clear();
            StartButton.Visibility = Visibility.Visible;
            CancelButton.Visibility = Visibility.Hidden;
            DeleteButton.IsEnabled = true;
        }

        private void LoginClick(object sender, RoutedEventArgs e)
        {
      
        }
        private void LoginClick(object sender, RoutedEventArgs e,SeleniumClass selenium)
        {

            if (selenium.Login())
            {
                SelectButton.IsEnabled = true;
            }
        }

        private void SelectClick(object sender, RoutedEventArgs e)
        {

        }
        private void SelectClick(object sender, RoutedEventArgs e,SeleniumClass selenium)
        {
            if(selenium.SelectGroup())
            {
                DownloadButton.IsEnabled = true;
            }
        }

        private void DownloadClick(object sender, RoutedEventArgs e)
        {

        }
        private void DownloadClick(object sender, RoutedEventArgs e,SeleniumClass selenium)
        {
            selenium.Download();
        }
    }
}