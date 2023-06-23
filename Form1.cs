using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;

namespace adachi_reaction_bot
{
    public partial class Form1 : Form
    {

        string[] words;

        public Form1()
        {
            //ui stuff
            InitializeComponent();


            //puts each word into an arrays
            words = File.OpenText(Directory.GetCurrentDirectory() + "/words.txt").ReadToEnd().Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries).Select(wr => wr.Trim()).ToArray();

            //executed to include the async
            WaitABit();
        }

        async void WaitABit()
        {
            //waits for the program to finalize
            await Task.Delay(500);

            //sets up a timer
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

            //makes sure the program isn't on top
            TopLevel = false;

            //hides it so it becomes a background proccess
            Hide();

            //makes it collapsable to hide
            #region login
            await Login("username", "password");
            #endregion

            //runs the bot part os the program once
            Run();

            //constantly will check to see if the timer is up  
            while (await timer.WaitForNextTickAsync())
            {
                //runs the bot part that tweets
                Run();
            }
        }

        float GetFontSize(string input, Graphics g)
        {
            //set the starting font size
            float fontSize = 34;

            //create a new font object with the starting size
            Font font = new Font("Comic Sans MS", fontSize);

            //set the text string
            string text = input;

            //set the maximum width of the text in pixels
            float maxWidth = 700 - 10;

            //measure the width of the text in pixels
            SizeF textSize = g.MeasureString(text, font);

            //adjust the font size until the text fits within the maximum width
            while (textSize.Width > maxWidth)
            {
                //makes the font smaller
                fontSize--;

                //makes a new font with the new size
                font = new Font("Comic Sans MS", fontSize);

                //measures again
                textSize = g.MeasureString(text, font);
            }

            //returns the value
            return fontSize;
        }

        //the part that generates the image and tweets
        async void Run()
        {
            try
            {
                //gets all the adachi pictures
                var files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}/adachi", "*.png", SearchOption.AllDirectories);

                //comment chooses a random word
                var word = words[Random.Shared.Next(0, words.Length)].ToUpper() + "!";

                //var word = "CUSTOM WORD HERE!";//comment chooses a random word               

                //chooses a random adachi portrait
                Image image = Image.FromFile(files[Random.Shared.Next(0, files.Length)]);

                //Image image = Image.FromFile($"{Directory.GetCurrentDirectory()}/adachi/b12_2_0.png");

                //grabs the background
                Image bg = Image.FromFile($"{Directory.GetCurrentDirectory()}/bg.png");

                //chooses a random colour
                Color randomColor = Color.FromArgb(Random.Shared.Next(151), Random.Shared.Next(151), Random.Shared.Next(151));

                //makes the uh brush
                SolidBrush drawBrush = new SolidBrush(randomColor);

                using (Graphics g = Graphics.FromImage(bg))
                {
                    //grabs the max font size relative to the word
                    float fontSize = GetFontSize(word, g);

                    //makes the font 
                    Font drawFont = new Font("Comic Sans MS", fontSize);

                    //places adachi onto the background and then stretches him to fit properly
                    g.DrawImage(image, new Rectangle(0, 0, 700, 555), new Rectangle(107, 65, 285, 270), GraphicsUnit.Pixel);

                    //makes the text centred
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    //smooths out the text with anti aliasing
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    //draws the text to the image
                    g.DrawString(word, drawFont, drawBrush, new RectangleF(0, 570, 700, 130), stringFormat);

                    //saves the image
                    bg.Save(Path.Combine(Directory.GetCurrentDirectory(), "output.png"));
                }

                //tweets out the finished image
                Tweet(word, $"{Directory.GetCurrentDirectory()}/output.png");
            }
            //fail-safe
            catch (Exception ex)
            {
                //writes to the console you'll never see
                Console.WriteLine(ex.Message);
            }
        }

        //a method to send a tweet using playwright
        async void Tweet(string text, string filePath)
        {
            if (!InternetCheck())
                return;

            IBrowser browser;
            IBrowserType chrome;
            IPage page;
            IPlaywright playwright;

            //makes a playwright instance ig
            playwright = await Playwright.CreateAsync();

            //makes the browser use chrome
            chrome = playwright.Chromium;

            //launches the browser
            browser = await chrome.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

            //makes a new page
            page = await browser.NewPageAsync();

            //reads the cookie file
            var json = File.ReadAllText("cookies.json");

            //turs the cookie file into uhh a cookie array
            var cookies = JsonConvert.DeserializeObject<Microsoft.Playwright.Cookie[]>(json);

            //adds cookies to page
            await page.Context.AddCookiesAsync(cookies);

            //goes to twitter
            await page.GotoAsync("https://twitter.com/compose/tweet", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

            //waits until the page is loaded
            await Task.Delay(7000);

            //types the tweet
            await page.Keyboard.TypeAsync(text);

            //waits for the file explorer to open
            var fileChooser = await page.RunAndWaitForFileChooserAsync(async () =>
            {
                //presses the image button
                await page.GetByRole(AriaRole.Button, new() { Name = "Add photos or video" }).ClickAsync();
            });
            //sets the file explorer file to the 
            await fileChooser.SetFilesAsync(filePath);

            //presses the tweet button
            await page.GetByTestId("tweetButton").ClickAsync();

            //waits for it to send
            await Task.Delay(3000);

            //closes everything
            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }

        //logs you in
        async Task Login(string username, string password)
        {
            //checks if there is already a cookies file
            if (File.Exists($"{Directory.GetCurrentDirectory()}/cookies.json"))
                return;

            if (!InternetCheck())
                throw new Exception("Failed to login. Please make sure you have a working internet connection.");    

            IBrowser browser;
            IBrowserType chrome;
            IPage page;
            IPlaywright playwright;

            //makes a playwright instance ig
            playwright = await Playwright.CreateAsync();

            //makes the browser use chrome
            chrome = playwright.Chromium;

            //launches the browser
            browser = await chrome.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

            //makes a new page
            page = await browser.NewPageAsync();

            //navigates to the main twitter page
            await page.GotoAsync("https://twitter.com/", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

            //clicks the login buttton
            await page.GetByTestId("login").ClickAsync();

            //waits until the page loads i think i actually dont know if this is even necessary but if it works it works
            await page.WaitForURLAsync("https://twitter.com/i/flow/login");

            //types in the username
            await page.GetByLabel("Phone, email, or username").TypeAsync(username);

            //clicks the next button so the password field shows
            await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();

            //waits for it to load
            await Task.Delay(100);

            //typs in the password
            await page.GetByLabel("Password", new() { Exact = true }).TypeAsync(password);

            //clicks the login button to log in
            await page.GetByTestId("LoginForm_Login_Button").ClickAsync();

            //waits until everything is fully loaded and finalized
            await Task.Delay(5000);

            //grabs the page's cookies
            var cookies = await page.Context.CookiesAsync();

            //turns the cookies into a json
            var json = JsonConvert.SerializeObject(cookies);

            //saves the cookies to the computer
            File.WriteAllText("cookies.json", json);

            //closes everything
            await page.CloseAsync();
            await browser.CloseAsync();
            playwright.Dispose();
        }

        public bool InternetCheck()
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
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }

        }
    }
}