using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Interactive_Communication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public class Provider
        {
            public string type { get; set; }
            public string voice_id { get; set; }
        }

        public class Root
        {
            public Script script { get; set; }
            public string source_url { get; set; }
        }

        public class Script
        {
            public string type { get; set; }
            public string input { get; set; }
            public Provider provider { get; set; }
        }

        public class RootVideo
        {
            public object user { get; set; }
            public object metadata { get; set; }
            public string audio_url { get; set; }
            public DateTime created_at { get; set; }
            public object face { get; set; }
            public object config { get; set; }
            public string source_url { get; set; }
            public string created_by { get; set; }
            public string status { get; set; }
            public string driver_url { get; set; }
            public DateTime modified_at { get; set; }
            public string user_id { get; set; }
            public string result_url { get; set; }
            public string id { get; set; }
            public int duration { get; set; }
            public DateTime started_at { get; set; }
        }


        public class RootVideoIdReq
        {
            public string id { get; set; }
            public DateTime created_at { get; set; }
            public string created_by { get; set; }
            public string status { get; set; }
        }

        private const string ApiUri = "https://api.d-id.com/talks";

        private string currentVideoLink = "ms-appx:///Assets/sampleMockVid.mp4";

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                if (Dispatcher == null) // For console App
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    if (Dispatcher.HasThreadAccess)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                    else
                    {
                        _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PropertyChanged(this, new PropertyChangedEventArgs(propertyName)));
                    }
                }
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

          /*  string videSrc = Task.Run(async () => await ConvertTextToVideoLinkId("Hello, this is a demo for our hackathon 2023 project on Open AI.This is an AI generated video.")).Result;*/
            string videSrc = "https://d-id-talks-prod.s3.us-west-2.amazonaws.com/google-oauth2%7C115582271522269271554/tlk_QgfrvTxfb6nGhGNSt-l6H/1683622090499.mp4?AWSAccessKeyId=AKIA5CUMPJBIK65W6FGA&Expires=1683708495&Signature=PayJhhr%2FrRA0FiJMFgatiVZn%2B54%3D&X-Amzn-Trace-Id=Root%3D1-645a08cf-5de44550482bcf775dcfe6a3%3BParent%3D69bf43941cbf7c34%3BSampled%3D1%3BLineage%3D6b931dd4%3A0";

            Debug.WriteLine("Fetchingthe video url started src " + videSrc);
            this.currentVideoLink = videSrc;
            VideoState.Source = MediaSource.CreateFromUri(new Uri(videSrc));
            VideoState.Visibility = Visibility.Visible;

        }

        public string CurrentVideoLink { get { return this.currentVideoLink; } set { this.currentVideoLink = value; OnPropertyChanged(); } }


        private static async Task<string> FetchTheVideoLink(string id)
        {
            string respVideoSrc = "ms-appx:///Assets/sampleMockVid.mp4";
            try
            {
                Debug.WriteLine("[IdToVideoLink] Fetchingthe video url started: ");
                string requestUri = ApiUri + "/" + id;
                Uri VideoReqUrl = new Uri(requestUri);

                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, VideoReqUrl);
                request.Headers.Add("Authorization", "Basic Y0c5dmFtRjJZWEpoYldKaGJHeDVNamxBWjIxaGFXd3VZMjl0OnMzdm9jOUVEOUNYN3hWdWkyT2hZZQ==");
                var content = new StringContent("", null, "text/plain");
                request.Content = content;
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[IdToVideoLink] Fetchingthe video url response: " + resp);
                RootVideo videoResp = JsonConvert.DeserializeObject<RootVideo>(resp);
                Debug.WriteLine($"[IdToVideoLink] Fetchingthe video url from object:  {videoResp.result_url}");


                respVideoSrc = videoResp.result_url;
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"[IdToVideoLink] Fetchingthe video url sexceptiond: {ex.Message}");
            }

            return respVideoSrc;

        }

        private static async Task<string> ConvertTextToVideoLinkId(string text)
        {
            string respVideoSrc = "ms-appx:///Assets/sampleMockVid.mp4";
            try
            {
                Debug.WriteLine("[ConvertTextToVideoLinkId] Fetching the video from text started: ");
                Uri VideoReqUrl = new Uri(ApiUri);
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.d-id.com/talks");
                request.Headers.Add("Authorization", "Basic Y0c5dmFtRjJZWEpoYldKaGJHeDVNamxBWjIxaGFXd3VZMjl0OnMzdm9jOUVEOUNYN3hWdWkyT2hZZQ==");

                Script scriptC = new Script();
                scriptC.type = "text";
                scriptC.input = text;
                Provider pr = new Provider();
                pr.type = "microsoft";
                pr.voice_id = "en-US-JennyNeural";
                scriptC.provider = pr;
                Root root = new Root();
                root.script = scriptC;
                root.source_url = "https://s-media-cache-ak0.pinimg.com/736x/b2/b5/b3/b2b5b30e4b4b75365ce57ca30040e76a--female-faces-female-face-claims.jpg";

                string contS = JsonConvert.SerializeObject(root);
                var content = new StringContent(contS, null, "application/json");
                request.Content = content;
                Debug.WriteLine("[ConvertTextToVideoLinkId] Fetching the video result2 respest: " + contS);
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var resp = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[ConvertTextToVideoLinkId] Fetchingthe video result2 response: " + resp);
                RootVideoIdReq respVideoId = JsonConvert.DeserializeObject<RootVideoIdReq>(resp);
                Debug.WriteLine($"[ConvertTextToVideoLinkId] Fetching the video url from object:  {respVideoId.id}");

                Thread.Sleep(5000);
                respVideoSrc = await FetchTheVideoLink(respVideoId.id).ConfigureAwait(false);
                return respVideoSrc;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConvertTextToVideoLinkId] Fetching the video text sexceptiond: {ex.Message}");
            }

            return respVideoSrc;
        }
    }
}
