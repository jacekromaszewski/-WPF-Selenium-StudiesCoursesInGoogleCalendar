using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KalendarzProjektPW
{
    class SeleniumClass
    {
       
        
            private static IWebDriver Driver { get; set; }
            private static async Task AllowHeadlessDownload(ChromeDriverService driverService)
            {

                string startupPath = Environment.CurrentDirectory;
                var jsonContent = new JObject(
                    new JProperty("cmd", "Page.setDownloadBehavior"),
                    new JProperty("params",
                    new JObject(new JObject(
                        new JProperty("behavior", "allow"),
                        new JProperty("downloadPath", startupPath + @"\Rozklad\Down")))));
                var content = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");
                var sessionIdProperty = typeof(ChromeDriver).GetProperty("SessionId");
                var sessionId = sessionIdProperty.GetValue(Driver, null);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = driverService.ServiceUrl;
                    var result = await client.PostAsync("session/" + sessionId.ToString() + "/chromium/send_command", content);
                    var resultContent = await result.Content.ReadAsStringAsync();
                }
            }

        ComboBox ComboGroup, ComboSemester;
        TextBlock INFO;
        TextBox USER;
        PasswordBox PASS;
        CheckBox HIDE;
        public SeleniumClass(ComboBox ComboGroup,ComboBox ComboSemester,TextBlock INFO,TextBox USER, PasswordBox PASS,CheckBox HIDE)
        {
            this.ComboGroup = ComboGroup;
            this.ComboSemester = ComboSemester;
            this.INFO = INFO;
            this.USER = USER;
            this.PASS = PASS;
            this.HIDE = HIDE;
        }

        public bool SelectGroup()
            {
                try
                {
                    var Group = ComboGroup.Text;
                    var semester = String.Empty;
                    if (ComboSemester.Text.Equals("Semestr zimowy")) semester = "&mid=328&iid=20181&pos=0&rdo=1&t=6800743";
                    else semester = "&mid=328&iid=20182&pos=0&rdo=1&t=6800666";
                    String URL = Driver.Url;
                    String[] newURL = URL.Split('&');
                    URL = newURL[0] + semester;
                    Driver.Navigate().GoToUrl(URL);
                    Driver.FindElement(By.LinkText(Group)).Click();
                    INFO.Text = "Group " + Group + " selected";
                return true;
                }
                catch { INFO.Text = "NO MATCH GROUP";  return false; }
            }
           

            public  bool Login()
            {
                try
                {
                    var chromeOptions = new ChromeOptions();
                    chromeOptions.AddArguments("--window-size=800,600");
                    chromeOptions.AddArguments("--disable-gpu");
                    chromeOptions.AddArguments("--disable-extensions");
                    chromeOptions.AddArguments("--proxy-server='direct://'");
                    chromeOptions.AddArguments("--proxy-bypass-list=*");
                    chromeOptions.AddArguments("--start-maximized");
                bool check = HIDE.IsChecked ?? false;
                if (check)
                        chromeOptions.AddArguments("--headless");
                    var driverService = ChromeDriverService.CreateDefaultService();
                    driverService.HideCommandPromptWindow = true;
                    Driver = new ChromeDriver(driverService, chromeOptions);
                    Task.Run(() => SeleniumClass.AllowHeadlessDownload(driverService));
                    Driver.Navigate().GoToUrl("https://s1.wcy.wat.edu.pl/ed1/");
                    Driver.FindElement(By.Name("userid")).SendKeys(USER.Text);
                    Driver.FindElement(By.Name("password")).SendKeys(PASS.Password.ToString());
                    Driver.FindElement(By.ClassName("inputLogL")).Click();
                    try
                    {
                        var err = Driver.FindElement(By.ClassName("bError"));
                        INFO.Text = Driver.FindElement(By.ClassName("bError")).Text;
                    return false;
                    }
                    catch
                    {
                        INFO.Text = "ACCESS GRANTED";
                        return true;
                        
                    }
                }
                catch
                {
                Application.Current.Dispatcher.Invoke(delegate
                {
                    INFO.Text = "BROWSER ERROR";
                });
                return false;
                }
            }
        
            public void Download()
            {
                Driver.FindElement(By.XPath("//*[@title='Zapisz formularz w pliku  tekstowym w formacie OutLook']")).Click();
                
                string startupPath = Environment.CurrentDirectory;
                while (File.Exists(startupPath + @"\Rozklad\Down\*.txt")) { }
                string batFilePath = startupPath + @"\copy.bat";
                if (!File.Exists(batFilePath))
                {
                    using (FileStream fs = File.Create(batFilePath))
                    {
                        fs.Close();
                    }
                }
                using (StreamWriter sw = new StreamWriter(batFilePath))
                {
                    sw.WriteLine(@"cd {0}\Rozklad", startupPath);
                    sw.WriteLine("timeout /t 1");
                    sw.WriteLine(@"del .\old.txt");
                    sw.WriteLine("timeout /t 1");
                    sw.WriteLine(@"move .\new.txt .\old.txt");
                    sw.WriteLine("timeout /t 1");
                    sw.WriteLine(@"move .\Down\*.txt .\new.txt");
                    sw.WriteLine("timeout /t 1");
                }
                Process process = Process.Start(batFilePath);
          
                process.WaitForExit();
                try
                {
                    StreamReader file = new StreamReader(@"Rozklad\new.txt");
                    StreamReader file2 = new StreamReader(@"Rozklad\old.txt");
                    bool flag = false;
                    string line, line2;
                    while (((line = file.ReadLine()) != null) && ((line2 = file2.ReadLine()) != null) && !flag)
                    {
                        if (line != line2) flag = true;
                    }
                    if (flag) INFO.Text = "CALENDARS DIFFERENT";
                    else INFO.Text = "CALENDARS SAME";
                }
                catch { INFO.Text = "CAN'T FIND FILE"; }
            }
            
           
           
          
        
    }
}
