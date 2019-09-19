using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Android_Enterprise_QR_Code
{
    public partial class Screenshot : Form
    {
        public Screenshot()
        {
            InitializeComponent();
        }

        //These variables control the mouse position
        int selectX;
        int selectY;
        int selectWidth;
        int selectHeight;
        public Pen selectPen;


        //This variable control when you start the right click
        bool start = false;

        public Image retImage;

        // Desired Width of QR Code. Aspect Ratio is maintained.
        private const int BOXWIDTH = 500;

        private void Screenshot_Load(object sender, EventArgs e)
        {
            this.Top = 0;
            this.Left = 0;
            //Hide the Form
            this.Hide();
            //Create the Bitmap
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                                     Screen.PrimaryScreen.Bounds.Height);
            //Create the Graphic Variable with screen Dimensions
            Graphics graphics = Graphics.FromImage(printscreen as Image);
            //Copy Image from the screen
            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            //Create a temporal memory stream for the image
            using (MemoryStream s = new MemoryStream())
            {
                //save graphic variable into memory
                printscreen.Save(s, ImageFormat.Bmp);
                pictureBox1.Size = new System.Drawing.Size(this.Width, this.Height);
                //set the picture box with temporary stream
                pictureBox1.Image = Image.FromStream(s);
            }
            //Show Form
            this.Show();
            //Cross Cursor
            Cursor = Cursors.Cross;

        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //validate if there is an image
            if (pictureBox1.Image == null)
                return;
            //validate if right-click was trigger
            if (start)
            {
                //refresh picture box
                pictureBox1.Refresh();
                //set corner square to mouse coordinates
                selectWidth = e.X - selectX;
                selectHeight = e.Y - selectY;
                //draw dotted rectangle
                pictureBox1.CreateGraphics().DrawRectangle(selectPen,
                          selectX, selectY, selectWidth, selectHeight);
            }
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            //validate when user right-click
            if (!start)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    //starts coordinates for rectangle
                    selectX = e.X;
                    selectY = e.Y;
                    selectPen = new Pen(Color.Red, 1);
                    selectPen.DashStyle = DashStyle.DashDotDot;
                }
                //refresh picture box
                pictureBox1.Refresh();
                //start control variable for draw rectangle
                start = true;
            }
            else
            {
                //validate if there is image
                if (pictureBox1.Image == null)
                    return;
                //same functionality when mouse is over
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    pictureBox1.Refresh();
                    selectWidth = e.X - selectX;
                    selectHeight = e.Y - selectY;
                    pictureBox1.CreateGraphics().DrawRectangle(selectPen, selectX,
                             selectY, selectWidth, selectHeight);

                }
                start = false;
                //function save image to clipboard
                ReturnCapture();
            }
        }

        private void ReturnCapture()
        {
            //validate if something selected
            if (selectWidth > 0)
            {
                try
                {
                    Rectangle rect = new Rectangle(selectX, selectY, selectWidth, selectHeight);
                    //create bitmap with original dimensions
                    Bitmap OriginalImage = new Bitmap(pictureBox1.Image, pictureBox1.Width, pictureBox1.Height);
                    //create bitmap with selected dimensions
                    Bitmap _img = new Bitmap(selectWidth, selectHeight);
                    //create graphic variable
                    Graphics g = Graphics.FromImage(_img);
                    //set graphic attributes
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.DrawImage(OriginalImage, 0, 0, rect, GraphicsUnit.Pixel);
                    //insert image stream into clipboard
                    ImageDimensions newDim = CalculateNewDimensionsForImage(_img);
                    if (_img.Width > 500) { retImage = _img; } else { retImage = ResizeImage(_img, newDim.Width, newDim.Height); }
                }
                catch
                {
                    MessageBox.Show("Failed to capture image, try again.");
                }
            }
            //End application
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private static ImageDimensions CalculateNewDimensionsForImage(Bitmap original)
        {
            //calculate aspect ratio
            float aspect = original.Width / (float)original.Height;
            int newWidth, newHeight;

            //calculate new dimensions based on aspect ratio
            newWidth = (int)(BOXWIDTH * aspect);
            newHeight = (int)(newWidth / aspect);

            //if one of the two dimensions exceed the box dimensions
            if (newWidth > BOXWIDTH || newHeight > BOXWIDTH)
            {
                //depending on which of the two exceeds the box dimensions set it as the box dimension and calculate the other one based on the aspect ratio
                if (newWidth > newHeight)
                {
                    newWidth = BOXWIDTH;
                    newHeight = (int)(newWidth / aspect);
                }
                else
                {
                    newHeight = BOXWIDTH;
                    newWidth = (int)(newHeight * aspect);
                }
            }

            return new ImageDimensions()
            {
                Height = newHeight,
                Width = newWidth
            };
        }
    }
}

    public class ImageDimensions
    {
        public int Height { get; set; }
        public int Width { get; set; }
    }
