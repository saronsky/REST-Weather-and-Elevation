/*Simon Aronsky
 * 02/02/2021
 * Program 2
 */


using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;



namespace Program_2_Rest
{
    class Program
    {
        /*This is used to store API keys and base URLS for the 2 APIs being used*/
        const string weatherAPIKey = "2ad9265743a2c5043112387ee3bebbd4";
        const string elevationAPIKey = "AIzaSyBqKCh3xUtreyZ-5t9AQJabqrD_f8vzXGw";
        const string weatherURI = "http://api.openweathermap.org/data/2.5/";
        const string elevationURI = "https://maps.googleapis.com/maps/api/elevation/";

        /*This method is used to manage entire class. Manages user input and terminal output
          Precondition: args- string used as input for user from command line. Expecting a city name
          Postcondition: void
        */
        static void Main(string[] args)
        {
            string city;
            if (args.Length == 0)
            {
                Console.WriteLine("No Input City Detected. Using New York City as example\n");
                city = "New York City";
            }
            else
            {
                city = args[0];
            }
            
            string correctedCity = city.ToLower();
            WeatherAPI weather = getWeather(correctedCity);
            if (weather != null)
            {
                Console.WriteLine("The weather in " + city + ", " + weather.sys.country + " is --     Wind: " + weather.wind.speed + "mph   Temperature: " + String.Format("{0:0.00}", kToF(weather.main.temp)) + "F");
                if (weather.main.temp < 40)
                {
                    Console.WriteLine("I would adivse a jacket in any outdoor activites");
                }
                else
                {
                    Console.WriteLine("Enjoy the weather, should be a moderate day today");
                }
                string country = weather.sys.country.ToLower();
                double lat = weather.coord.lat;
                double lon = weather.coord.lon;
                ElevationAPI elevation = getElevation(Math.Round(lat,4), Math.Round(lon,4));
                if (elevation != null)
                {
                    Console.WriteLine("\nAccording to Google Elevation, " + city + " is located at " +elevation.results[0].elevation+ "m above sea level");
                    if (elevation.results[0].elevation < 1000)
                    {
                        Console.WriteLine("Take a deep breath, you have lots of oxygen down there");
                    }
                    else
                    {
                        Console.WriteLine("Dont forget your oxygen tank");
                    }
                }
            }
            else
            {
                Console.WriteLine("Unfortunaetly, we were unable to find your city in our database");
            }
        }

        /* This method is used to convert Kelvin to Farenheit
           Precondtion: kTemp- a double for the temperature in Kelvin
           Postcondition: float- the conversion into farenheit*/
        static float kToF(double kTemp)
        {
            return (float)((kTemp - 273.15) * (9.0 / 5) + 32);
        }

        /* This method is used to manage the weather API requests
           Precondition: city- a string of the city name
           Postcondtion: WeatherAPI- the object version of the JSON returned by the API*/
        static WeatherAPI getWeather(string city)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(weatherURI);
            string uri = "weather?q=" + city + "&appid=" + weatherAPIKey;
            string result = handleResponse(client, uri, -1);
            if (result.Length != 0)
            {
                WeatherAPI weatherDetails = JsonConvert.DeserializeObject<WeatherAPI>(result);
                return weatherDetails;
            }
            return null;
        }
        /* This method is used to manage the elevation API requests
           Precondition: lat, lon- the latitude and longitude of the city
           Postcondtion: ElevationAPI- the object version of the JSON returned by the API*/
        static ElevationAPI getElevation(double lat, double lon)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(elevationURI);
            string uri = "json?locations=" + lat+","+lon+"&key=" + elevationAPIKey;
            string result = handleResponse(client, uri, -1);
            if (result.Length != 0)
            {
                ElevationAPI elevationDetails = JsonConvert.DeserializeObject<ElevationAPI>(result);
                return elevationDetails;
            }
            return null;
        }

        /* This method is used to handle the responses by the API. Handles exponential backoff and failed requests
           Precondtion: client- HTTP client. uri- the header portion of the URL for the API request, retry- the number of seconds to wait
           Postcondition: string- a string of the JSON output*/
        static string handleResponse(HttpClient client, string uri, int retry)
        {
            if (retry > 0)
                System.Threading.Thread.Sleep(retry * 1000);
            HttpResponseMessage response = client.GetAsync(uri).Result;
            if (!response.IsSuccessStatusCode)
            {
                int statusCode = (int)response.StatusCode;
                if (statusCode / 100 == 5 && retry < 8)
                {
                    if (retry < 1)
                        retry++;
                    else
                        retry *= 2;
                    return handleResponse(client, uri, retry);
                }
                Console.WriteLine("API request failed. Status code: " + response.StatusCode);
                return "";
            }
            return response.Content.ReadAsStringAsync().Result;
        }
    }


    /* JSON Objects for Deserialization*/

    public class ElevationAPI
    {
        public Result[] results { get; set; }
        public string status { get; set; }
    }

    public class Result
    {
        public float elevation { get; set; }
        public Location location { get; set; }
        public float resolution { get; set; }
    }

    public class Location
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }
















    public class WeatherAPI
    {
        public Coord coord { get; set; }
        public Weather[] weather { get; set; }
        public string _base { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public Rain rain { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Main
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
    }

    public class Rain
    {
        public float _1h { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

}
