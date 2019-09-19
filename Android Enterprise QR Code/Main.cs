using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QRCodeDecoderLibrary;
using QRCodeEncoderLibrary;

namespace Android_Enterprise_QR_Code
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            SystemApps.Checked = false;
            Screenshot sc = new Screenshot();
            sc.ShowDialog();
            textBox1.Text = DecodeQRCode(sc.retImage);
            pictureBox1.Image = EncodeQRCode(textBox1.Text);
            textBox1.Text = DecodeQRCode((Bitmap)pictureBox1.Image);

        }

        private Image EncodeQRCode(string text)
        {
            try
            {
                QRCodeEncoder encoder = new QRCodeEncoder();
                encoder.ModuleSize = 4;
                encoder.QuietZone = 16;
                encoder.ECIAssignValue = -1;
                // encode data
                encoder.Encode(text);               

                // create bitmap
                return encoder.CreateQRCodeBitmap();
            }

            catch (Exception Ex)
            {
                MessageBox.Show("Encoding exception.\r\n" + Ex.Message);
                return null;
            }

        }

        private string DecodeQRCode(Image image)
        {
            // create QR Code decoder object
            QRDecoder Decoder = new QRDecoder();

            // call image decoder methos with <code>Bitmap</code> image of QRCode barcode
            byte[][] DataByteArray = Decoder.ImageDecoder((Bitmap)image);

            // get the ECI Assignment value
            int ECIValue = Decoder.ECIAssignValue;
            if (ECIValue == -1)
            {
                // Assignment value not defined
            }
            else
            {
                // Assignment value between 0 to 999999
            }
            return QRCodeResult(DataByteArray);
        }

        private static string QRCodeResult
                    (
                    byte[][] DataByteArray
                    )
        {
            // no QR code
            if (DataByteArray == null) return string.Empty;

            // image has one QR code
            if (DataByteArray.Length == 1) return ForDisplay(QRDecoder.ByteArrayToStr(DataByteArray[0]));

            // image has more than one QR code
            StringBuilder Str = new StringBuilder();
            for (int Index = 0; Index < DataByteArray.Length; Index++)
            {
                if (Index != 0) Str.Append("\r\n");
                Str.AppendFormat("QR Code {0}\r\n", Index + 1);
                Str.Append(ForDisplay(QRDecoder.ByteArrayToStr(DataByteArray[Index])));
            }
            return Str.ToString();
        }
        private static string ForDisplay
        (
        string Result
        )
        {
            int Index;
            for (Index = 0; Index < Result.Length && (Result[Index] >= ' ' && Result[Index] <= '~' || Result[Index] >= 160); Index++) ;
            if (Index == Result.Length) return Result;

            StringBuilder Display = new StringBuilder(Result.Substring(0, Index));
            for (; Index < Result.Length; Index++)
            {
                char OneChar = Result[Index];
                if (OneChar >= ' ' && OneChar <= '~' || OneChar >= 160)
                {
                    Display.Append(OneChar);
                    continue;
                }

                if (OneChar == '\r')
                {
                    Display.Append("\r\n");
                    if (Index + 1 < Result.Length && Result[Index + 1] == '\n') Index++;
                    continue;
                }

                if (OneChar == '\n')
                {
                    Display.Append("\r\n");
                    continue;
                }

                Display.Append('¿');
            }
            return Display.ToString();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void SystemApps_CheckedChanged(object sender, EventArgs e)
        {
            if (SystemApps.Checked)
            {
                textBox1.Text = textBox1.Text.RemoveLast("}") + ",\"android.app.extra.PROVISIONING_LEAVE_ALL_SYSTEM_APPS_ENABLED\":true}";
                pictureBox1.Image = EncodeQRCode(textBox1.Text);
            }
            else
            {
                textBox1.Text = textBox1.Text.Replace(",\"android.app.extra.PROVISIONING_LEAVE_ALL_SYSTEM_APPS_ENABLED\":true}", "}");
                pictureBox1.Image = EncodeQRCode(textBox1.Text);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            if (!string.IsNullOrEmpty(saveFileDialog1.FileName))
            {
                pictureBox1.Image.Save(saveFileDialog1.FileName);

            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox1.Image);
        }
    }

    public static class MyExtensions
    {
        public static string RemoveLast(this string text, string character)
        {
            if (text.Length < 1) return text;
            return text.Remove(text.ToString().LastIndexOf(character), character.Length);
        }
    }
}
