//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
//using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.PointOfService;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Diagnostics;


namespace SDKTemplate
{
    public partial class MainPage : Page
    {
        public const string FEATURE_NAME = "Barcode Scanner";

        List<Scenario> scenarios = new List<Scenario>
        {
            //new Scenario() { Title = "DataReceived event", ClassType = typeof(Scenario1_BasicFunctionality) },
            //new Scenario() { Title = "Release/Retain functionality", ClassType = typeof(Scenario2_MultipleScanners) },
            //new Scenario() { Title = "Active Symbologies", ClassType = typeof(Scenario3_ActiveSymbologies) },
            //new Scenario() { Title = "Symbology Attributes", ClassType = typeof(Scenario4_SymbologyAttributes) },
            new Scenario() { Title = "Displaying a Barcode Preview", ClassType = typeof(Scenario5_DisplayingBarcodePreview) },
        };
    }

    public class Scenario
    {
        public string Title { get; set; }
        public Type ClassType { get; set; }
    }

    public partial class DeviceHelpers
    {
        public static async Task<BarcodeScanner> GetFirstBarcodeScannerAsync(PosConnectionTypes connectionTypes = PosConnectionTypes.All)
        {
            return await DeviceHelpers.GetFirstDeviceAsync(BarcodeScanner.GetDeviceSelector(connectionTypes), async (id) => await BarcodeScanner.FromIdAsync(id));
        }
    }

    public partial class DataHelpers
    {
        public static string GetDataString(IBuffer data)
        {

            if (data == null)
            {
                return "No data";
            }

            // Just to show that we have the raw data, we'll print the value of the bytes.
            // Arbitrarily limit the number of bytes printed to 20 so the UI isn't overloaded.
            string result = CryptographicBuffer.EncodeToHexString(data);
            /*if (result.Length > 40)
            {
                result = result.Substring(0, 40) + "...";
            }*/
            string cartPosition = HextoString(result);
            string[] location = cartPosition.Split(',');
            if (location.Length != 2)
            {
                //System.Console.WriteLine(location.ToString);
                return "ERROR: more than 2 coordinates";
            }
            int loc_col, loc_row;
            if (int.TryParse(location[0], out loc_row) && int.TryParse(location[1], out loc_col))
            {
                if (loc_row >= 0 && loc_col >= 0)
                {
                    //Console.WriteLine("Success: Barcode was found");
                    int cartID = 123;
                    SendRequest(loc_row, loc_col, cartID);
                }
            }
            return "\nResult: " +HextoString(result);
            //return result;
        }

        private static async Task SendRequest(int loc_row, int loc_col, object cartID)
        {
            ///////
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("utf-8"));

            //< read webApi >
            string sUrl_Api_Create = "https://changecartposition.azurewebsites.net/api/HttpTriggerCSharp1?code=cM5bXc0uMoDzQ6ZyDeLpJaNDJhzldRAutUMXX7RqxprrpGaTgUY4iQ==";

            HttpResponseMessage httpResponseMessage = null;

            try
            {
                //< create Upload_Content >
                JsonObject jsonObject = new JsonObject();

                jsonObject["name"] = JsonValue.CreateStringValue("{'loc_row': '"+ loc_row + "','loc_col': '"+ loc_col + "', 'id':'"+ cartID + "'}");
                StringContent string_to_Upload_Content = new StringContent(jsonObject.Stringify());
                string_to_Upload_Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string_to_Upload_Content.Headers.ContentType.Parameters.Add(new System.Net.Http.Headers.NameValueHeaderValue("charset", "utf-8"));
                //</ create Upload_Content >

                //< upload >
                //httpPost=Create
                httpResponseMessage = await client.PostAsync(sUrl_Api_Create, string_to_Upload_Content);
                //</ upload >
            }
            catch (Exception)
            {
                //handling exception
            }
        }

        public static string HextoString(string InputText)
        {

            byte[] bb = Enumerable.Range(0, InputText.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(InputText.Substring(x, 2), 16))
                             .ToArray();
            return System.Text.Encoding.ASCII.GetString(bb);
            // or System.Text.Encoding.UTF7.GetString
            // or System.Text.Encoding.UTF8.GetString
            // or System.Text.Encoding.Unicode.GetString
            // or etc.
        }

        public static string GetDataLabelString(IBuffer data, uint scanDataType)
        {
            // Only certain data types contain encoded text.
            // To keep this simple, we'll just decode a few of them.
            if (data == null)
            {
                return "No data";
            }

            // This is not an exhaustive list of symbologies that can be converted to a string.
            if (scanDataType == BarcodeSymbologies.Upca ||
                scanDataType == BarcodeSymbologies.UpcaAdd2 ||
                scanDataType == BarcodeSymbologies.UpcaAdd5 ||
                scanDataType == BarcodeSymbologies.Upce ||
                scanDataType == BarcodeSymbologies.UpceAdd2 ||
                scanDataType == BarcodeSymbologies.UpceAdd5 ||
                scanDataType == BarcodeSymbologies.Ean8 ||
                scanDataType == BarcodeSymbologies.TfStd)
            {
                // The UPC, EAN8, and 2 of 5 families encode the digits 0..9
                // which are then sent to the app in a UTF8 string (like "01234").
                return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, data);
            }

            // Some other symbologies (typically 2-D symbologies) contain binary data that
            // should not be converted to text.
            return string.Format("Decoded data unavailable. Raw label data: {0}", DataHelpers.GetDataString(data));
        }
    }

    public partial class BindingHelpers
    {
        // Inverters for binding.
        public static bool Not(bool value) => !value;
        public static Visibility CollapsedIf(bool value) => value ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// This class is used for data-binding.
    /// </summary>
    public class BarcodeScannerInfo
    {
        public BarcodeScannerInfo(String deviceName, String deviceId)
        {
            DeviceName = deviceName;
            DeviceId = deviceId;
        }

        public String Name => $"{DeviceName} ({DeviceId})";
        public String DeviceId { get; private set; }
        private string DeviceName;
    }

    /// <summary>
    /// This class is used for data-binding.
    /// </summary>
    public class SymbologyListEntry
    {
        public SymbologyListEntry(uint symbologyId, bool symbologyEnabled = true)
        {
            Id = symbologyId;
            IsEnabled = symbologyEnabled;
        }

        public uint Id { get; private set; }
        public bool IsEnabled { get; set; }
        public String Name => BarcodeSymbologies.GetName(Id);
    }
}
