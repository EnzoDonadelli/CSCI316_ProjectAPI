using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;  // or Newtonsoft.Json if you prefer

namespace DogApiExample
{
    class Program
    {
        static async Task Main(string[] args)
        {

            using (HttpClient client = new HttpClient())
            {

                try
                {
                    HttpResponseMessage response = await client.GetAsync("https://dog.ceo/api/breeds/image/random");

                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    var dogResponse = JsonSerializer.Deserialize<DogImageResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (dogResponse != null && dogResponse.Status == "success")
                    {
                        Console.WriteLine("Random Dog Image URL: " + dogResponse.Message);
                    }
                    else
                    {
                        Console.WriteLine("API returned failure or unexpected format");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception when calling Dog API: " + ex.Message);
                }
            }
        }
    }

    public class DogImageResponse
    {
        public string Message { get; set; }
        public string Status { get; set; }
    }
}
