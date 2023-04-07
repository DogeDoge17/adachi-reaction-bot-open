using System;
using System.Drawing;
using System.Threading;
using System.Timers;
using Tweetinvi;
using Tweetinvi.Parameters;
//using static System.Net.Mime.MediaTypeNames;

namespace adachi_reaction_bot
{
    public partial class Form1 : Form
    {
        TwitterClient client;

        string[] words;

        public Form1()
        {
            //ui stuff
            InitializeComponent();

            //reads the file with all the words in it
            var read = File.OpenText(Directory.GetCurrentDirectory() + "/words.txt");

            //puts each word into an arrays
            words = read.ReadToEnd().Split("\n");

            //signs you in to twitter
            client = new TwitterClient("consumer key", "consumer secret", "access token", "access secret");

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

                //adds the image or something
                var tweetinviLogoBinary = File.ReadAllBytes($"{Directory.GetCurrentDirectory()}/output.png");

                //uploads the image methinks
                var uploadedImage = await client.Upload.UploadTweetImageAsync(tweetinviLogoBinary);

                //tweets the tweeet
                var tweetWithImage = await client.Tweets.PublishTweetAsync(new PublishTweetParameters(word) { Medias = { uploadedImage } });
            }
            //fail-safe
            catch (Exception ex)
            {
                //writes to the console you'll never see
                Console.WriteLine(ex.Message);
            }
        }
    }
}