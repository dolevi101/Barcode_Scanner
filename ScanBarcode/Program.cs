using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
//using CoreCompat.System.Drawing;
using System.Drawing.Common;
using System.Threading;
using CnetSDK.Barcode.Scanner.Trial;

namespace ScanBarcode
{
    public class Program
    {
        private static VideoCapture _capture = null;
        private static int captureCounter = 0;
        private static int cartID = 123;

        static void Main(string[] args)
        {
            Console.WriteLine("Started capturing.");
            /*
                 Console.WriteLine("Type yes to start capturing!");

                 string result = Console.ReadLine();
                 if (result.Equals("yes"))
                 {
                     InitializeCamera();
                     CapturePicture();
                 }*/
            InitializeCamera();
            CapturePicture();
            Console.ReadKey();
        }

        public static void SendRequest(int loc_row, int loc_col, int cartID)
        {
            //string rslt;

            //int retcode;

            MSXML2.XMLHTTP myxml;

            myxml = new MSXML2.XMLHTTP();
            Console.WriteLine("cart Postion=: " + loc_row + "," + loc_col);
            myxml.open("POST", "https://changecartposition.azurewebsites.net/api/HttpTriggerCSharp1?code=cM5bXc0uMoDzQ6ZyDeLpJaNDJhzldRAutUMXX7RqxprrpGaTgUY4iQ==&name={%27loc_row%27:%20%27" + loc_row + "%27,%20%27loc_col%27:%27" + loc_col + "%27,%20%27id%27:%27" + cartID + "%27}", "", "", "");

            myxml.send();

            //rslt = myxml.responseText;
        }

        static int CapturePicture()
        {
            while (true)
            {
                bool stop = GetBarcode();
                if (stop)
                    break;
                Thread.Sleep(500);
            }
            return 0;
        }
        private static bool GetBarcode()
        {
            bool toContinue = CheckIfActive();
            if (!toContinue)
                return false;
            Console.WriteLine();
            Console.WriteLine("image number " + captureCounter);
            captureCounter++;
            Bitmap image = _capture.QueryFrame().Bitmap; //take a picture
                                                         //image.Save("C:\\Users\\Dolev\\Desktop\\david2.bmp");


            ScanResult[] results = CSBarcodeScanner.ScanBarcode(image, CSBarcodeType.QRCode);
            /*if (results == null) return false;
            foreach (ScanResult result in results)
            {
                Console.WriteLine(results.Length);
                Console.WriteLine("|||||||||" + result.BarcodeData);
            }*/

            //string[] results;
            //results = BarcodeReader.read(image, BarcodeReader.QRCODE);

            //results = BarcodeReader.read(image, BarcodeReader.CODABAR);
            if (results == null)
            {
                Console.WriteLine("RESULT IS:               NULL");
                return false;
            }
            //int cartPosition = -1;
            string cartPositionWithGarbage = results[0].BarcodeData;
            string cartPosition = (cartPositionWithGarbage.Split('*'))[1];
            Console.WriteLine("RESULT IS:               " + cartPosition);
            //if (string.TryParse(results[0], out cartPosition))
            //{
            //  SendRequest(cartPosition, cartID);
            //}

            //check if result is in template
            string[] location = cartPosition.Split(',');
            if (location.Length != 2)
                return false;
            int loc_col, loc_row;
            if (int.TryParse(location[0], out loc_row) && int.TryParse(location[1], out loc_col))
            {
                if (loc_row >= 0 && loc_col >= 0)
                {
                    Console.WriteLine("Success: Barcode was found");
                    SendRequest(loc_row, loc_col, cartID);
                }
            }

            //Console.WriteLine(results[0]);
            return false;
        }

        private static bool CheckIfActive()
        {
            //string rslt;
            MSXML2.XMLHTTP myxml;

            myxml = new MSXML2.XMLHTTP();
            //myxml.open("GET", "https://checkiscartactive.azurewebsites.net/api/HttpTriggerCSharp1?code=UsoPBXQxe6ByaHn6olWX4Qh81VRamu9l5NAwWbtUzNxUkphc9iNOLQ==&name={%20%27id%27:%27"+ cartID +"%27}", "", "", "");
            myxml.open("POST", "https://checkiscartactive.azurewebsites.net/api/HttpTriggerCSharp1?code=/nQFv2sG/spMvU1dCUY72DrI5Dw0WlWDJzfadTlqTX0ubRv0cJcjUQ==&name={%20%27id%27:%27" + cartID + "%27}", "", "", "");


            myxml.send();
            Thread.Sleep(1000);
            //Thread.Sleep(500);
            //rslt = myxml.responseText;
            //Console.WriteLine("is Active?: "+rslt);
            //return rslt.Equals("\"True\"");
            return true;
        }

        private static void InitializeCamera()
        {
            try
            {
                CvInvoke.UseOpenCL = false;


            }
            catch
            {
                Console.WriteLine("exception after Cv.Invoke");
                //Console.WriteLine("hhh");
            }
            try
            {
                _capture = new VideoCapture();
            }
            catch
            {
                Console.WriteLine("exception new VideoCapture");
            }

        }

    }
}