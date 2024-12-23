using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace adachi_reaction_bot
{
    public partial class Form1 : Form
    {

        Language[] langs;

        string? username;
        string? password;

        public Form1()
        {
            InitializeComponent();

            langs = new Language[]
            {
                new(Directory.GetCurrentDirectory() + "/lang/es.txt", "es", 5.0),
                new(Directory.GetCurrentDirectory() + "/lang/en.txt", "en", 100.0),
            };


            WaitABit();
        }


        async void WaitABit()
        {
            await Task.Delay(500);

            //aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            ///aTimer.Start();
            ///

            //  aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            //aTimer.Start();

            var timer = new PeriodicTimer(TimeSpan.FromMinutes(30));

            //this.Icon = Properties.Resources.icon;
            TopLevel = false;
            Hide();
            if (!File.Exists("login.txt"))
            {
                File.Create("login.txt");
                MessageBox.Show("Created a login file. Please fill it out");
                Application.Exit();
            }

            using (StreamReader reader = new StreamReader("login.txt"))
            {
                username = reader.ReadLine();
                password = reader.ReadLine();
            }

            await Login(username, password);


            Run();

            while (await timer.WaitForNextTickAsync())
            {
                Run();
            }
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Run();
        }

        float GetFontSize(string input, Graphics g, Language lang)
        {
            float fontSize = 34;

            foreach (char c in input)
                if (char.ConvertToUtf32(input, input.IndexOf(c)) > 127)
                {
                    fontSize = 21; //make max size smaller if accent
                    break;
                }

            // Create a new font object with the starting size
            Font font = new Font("Comic Sans MS", fontSize);

            // Set the text string
            string text = input;

            // Set the maximum width of the text in pixels
            float maxWidth = 700 - 10;

            // Measure the width of the text in pixels
            SizeF textSize = g.MeasureString(text, font);

            // Adjust the font size until the text fits within the maximum width
            while (textSize.Width > maxWidth)
            {
                fontSize--;
                font = new Font("Comic Sans MS", fontSize);
                textSize = g.MeasureString(text, font);
            }

            // Draw the text on the image
            //    g.DrawString(text, font, Brushes.Black, new PointF(5, 5));

            return fontSize;
        }

        /// <summary>
        /// The method the bot runs every 30 minutes. Generates a tweet then calls a method to tweet it.
        /// </summary>
        async void Run()
        {
            try
            {
                Language lang = Language.RollChances(langs);

                ///--------               
                /// Handles the variables the bot uses to put onto the image               
                
                var word = lang.GetWord();
                //var word = DrawingHelper.CustomWord($"Custom Word Here");

                Image image = DrawingHelper.GetRandomImage();
                //Image image = DrawingHelper.CustomImage(AdachiExpressions.BlushHappy);

                Color randomColor = DrawingHelper.RandomColour(151);
                //Color randomColor = Color.HotPink;
                //Color randomColor = DrawingHelper.CustomColour(255,255,255);

                ///--------

                SolidBrush drawBrush = new SolidBrush(randomColor);
                PointF drawPoint = new PointF(0, 570);

                Image bg = Image.FromFile($"{Directory.GetCurrentDirectory()}/bg.png");
                using (Graphics g = Graphics.FromImage(bg))
                {
                    float fontSize = GetFontSize(word.formatted, g, lang);

                    Font drawFont = new Font("Comic Sans MS", fontSize);

                    g.DrawImage(image, new Rectangle(0, 0, 700, 555), new Rectangle(107, 65, 285, 270), GraphicsUnit.Pixel);

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.DrawString(word.formatted, drawFont, drawBrush, new RectangleF(0, 570, 700, 130), stringFormat);

                    bg.Save(Path.Combine(Directory.GetCurrentDirectory(), "output.png"));
                }
               
                Tweet(word.raw, $"{Directory.GetCurrentDirectory()}/output.png");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        async void Tweet(string text, string filePath)
        {
            try
            {
                int timeoutMs = 10000; string url = null;

                url ??= CultureInfo.InstalledUICulture switch
                {
                    { Name: var n } when n.StartsWith("fa") => //iran check
                        "http://www.aparat.com",
                    { Name: var n } when n.StartsWith("zh") => //china check
                        "http://www.baidu.com",
                    _ =>
                        "http://www.gstatic.com/generate_204",
                };

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
            }
            catch
            {
                return;
            }


            IBrowser browser;
            IBrowserType chrome;
            IPage page;

            IPlaywright playwright;

            playwright = await Playwright.CreateAsync();
            chrome = playwright.Chromium;
            browser = await chrome.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            page = await browser.NewPageAsync();

            while (true)
            {
                var json = "";
                if (!File.Exists("cookies.json"))
                    await Login(username, password);
                json = File.ReadAllText("cookies.json");
                var cookies = JsonConvert.DeserializeObject<Microsoft.Playwright.Cookie[]>(json);

                // Add cookies to page
                await page.Context.AddCookiesAsync(cookies);

                await page.GotoAsync("https://twitter.com/compose/tweet", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
                await page.ReloadAsync();
                if (page.Url.Contains("https://x.com/i/flow/login"))
                {
                    if (File.Exists("cookies.json"))
                        File.Delete("cookies.json");
                    await Login(username, password);
                    continue;
                }

                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await Task.Delay(7000);
                break;
            }

            var fileChooser = await page.RunAndWaitForFileChooserAsync(async () =>
            {
                await page.GetByRole(AriaRole.Button, new() { Name = "Add photos or video" }).ClickAsync();
            });
            await fileChooser.SetFilesAsync(filePath);

            await page.GetByRole(AriaRole.Textbox, new() { Name = "Post text" }).TypeAsync(text);

            await Task.Delay(4000);

            await page.GetByTestId("tweetButton").ClickAsync();

            await Task.Delay(3000);
            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }

        async Task Login(string username, string password)
        {
            //checks if there is already a cookies file
            if (File.Exists($"{Directory.GetCurrentDirectory()}/cookies.json"))
                return;

            //checks for internet so the website can load properly 
            //  if (!InternetCheck())
            //    throw new Exception("Failed to login. Please make sure you have a working internet connection.");

            IBrowser browser;
            IBrowserType chrome;
            IPage page;
            IPlaywright playwright;

            playwright = await Playwright.CreateAsync();
            chrome = playwright.Chromium;
            browser = await chrome.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            page = await browser.NewPageAsync();

            await page.GotoAsync("https://x.com/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });


            await page.GetByTestId("loginButton").ClickAsync();
            await page.WaitForURLAsync("https://x.com/i/flow/login");

            await page.GetByLabel("Phone, email, or username").TypeAsync(username);
            await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            await Task.Delay(100);

            await page.GetByLabel("Password", new() { Exact = true }).TypeAsync(password);

            await page.GetByTestId("LoginForm_Login_Button").ClickAsync();
            await Task.Delay(5000);

            var cookies = await page.Context.CookiesAsync();

            var json = JsonConvert.SerializeObject(cookies);
            File.WriteAllText("cookies.json", json);

            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }
    }
}