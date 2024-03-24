using ColorThiefDotNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Screen_Capture
{
    internal class Program
    {

        static ColorThief colors = new ColorThief();
        static int primaryScreen = 0;
        static string targetFile = "Z:\\docker\\home\\nodered-data\\stefan\\capture.json";


        static void Main(string[] args)
        {

            SetDpiAwareness();


            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (Screen.AllScreens[i].Primary)
                    primaryScreen = i;
            }

            Task worker = Task.Run(() => Loop());

            Console.ReadKey(true);

        }

        private enum ProcessDPIAwareness
        {
            ProcessDPIUnaware = 0,
            ProcessSystemDPIAware = 1,
            ProcessPerMonitorDPIAware = 2
        }

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(ProcessDPIAwareness value);

        private static void SetDpiAwareness()
        {
            try
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
                }
            }
            catch (EntryPointNotFoundException)//this exception occures if OS does not implement this API, just ignore it.
            {
            }
        }

        static void Loop()
        {
            while (true)
            {
                Rectangle captureRectangle = Screen.AllScreens[primaryScreen].Bounds;
                Bitmap captureBmp = new Bitmap((int)((float)captureRectangle.Width * .8), (int)((float)captureRectangle.Height * .8), PixelFormat.Format32bppRgb);
                Graphics captureGraphics = Graphics.FromImage(captureBmp);
                captureGraphics.CopyFromScreen(captureRectangle.Left + (int)((float)captureRectangle.Width * .1), captureRectangle.Top + (int)((float)captureRectangle.Height * .1), 0, 0, captureRectangle.Size);
                // captureBmp.Save(@"D:\temp\capture.jpg", ImageFormat.Jpeg);
                QuantizedColor quantizedColor = colors.GetColor(captureBmp);
                ColorDefinition colorDefinition = new ColorDefinition(quantizedColor);
                File.WriteAllText(targetFile, colorDefinition.ToString(), Encoding.UTF8);
                Thread.Sleep(1000);
            }
        }

    }

    public struct ColorDefinition
    {
        public int[] singleColor;
        public int brightness;
        public bool isDark;
        public bool onetime;

        public ColorDefinition(QuantizedColor color)
        {
            singleColor = new int[] { color.Color.R, color.Color.G, color.Color.B };
            brightness = (int)(color.Color.ToHsl().L * 255.0);
            isDark = color.IsDark;
            onetime = true;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

    }

}
