﻿using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xamarin.Forms;
using Newtonsoft.Json.Linq;
using System.Linq;
using Plugin.Geolocator;
using berry.Model;
using Newtonsoft.Json;

namespace berry
{
    public partial class CustomVision : ContentPage
    {
        public CustomVision()
        {
            InitializeComponent();
        }

        private async void loadCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Name = $"{DateTime.UtcNow}.jpg",
            
                Directory = "Sample",
                PhotoSize = PhotoSize.Medium
            });

            if (file == null)
                return;

            image.Source = ImageSource.FromStream(() =>
            {
                return file.GetStream();
            });

            await postLocationAsync();

            await MakePredictionRequest(file);
        }

        async Task postLocationAsync()
        {

            var gps = CrossGeolocator.Current;
            gps.DesiredAccuracy = 50;

            var position = await gps.GetPositionAsync(TimeSpan.FromMilliseconds(10000));


            

            NotStrawberryModel location_gps = new NotStrawberryModel()
            {
                Longitude = (float)position.Longitude,
                Latitude = (float)position.Latitude
                
                
            };

            await AzureManager.AzureManagerInstance.PostStrawberryInformation(location_gps);
        }



        static byte[] GetImageAsByteArray(MediaFile name)
        {
            var len = name.GetStream();
            BinaryReader binaryReader = new BinaryReader(len);
            return binaryReader.ReadBytes((int)len.Length);
        }

        async Task MakePredictionRequest(MediaFile name)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "ce5fd674a76843fbadde6a1694ebef61");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/475c9df6-b10f-4260-af91-ce586d59e054/image?iterationId=67001aa1-7497-44fb-987a-fc9850c4acc0";

            HttpResponseMessage r;

            byte[] byteData = GetImageAsByteArray(name);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                r = await client.PostAsync(url, content);


                if (r.IsSuccessStatusCode)
                {
                    var responseString = await r.Content.ReadAsStringAsync();

                    

                    EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);

                    double max = responseModel.Predictions.Max(m => m.Probability);

                    TagLabel.Text = (max >= 0.5) ? "Yes, it's a Strawberry\n" : "Nope, this is not a Strawberry\n";



                    JObject rss = JObject.Parse(responseString);


                    var Probability = from p in rss["Predictions"] select (string)p["Probability"];
                    var Tag = from p in rss["Predictions"] select (string)p["Tag"];
                   

                    foreach (var x in Tag)
                    {
                        TagLabel.Text += x + ": \n";
                    }

                    foreach (var x in Probability)
                    {
                        PredictionLabel.Text += "\n" + x;
                    }

                }

                name.Dispose();
            }
        }
    }
}