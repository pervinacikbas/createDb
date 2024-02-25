using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.IO;
using static System.IO.File;
using System.Threading;


namespace createDB
{
    internal class Program
    {
        public string arama { get; set; }
     
        //public static string director { get; set; }
        static async Task Main (string[] args)
        {
            //Json verilerini DBye atma
            using (SqlConnection baglanti = new SqlConnection("Data Source=DESKTOP-EQLPKL0\\SQLEXPRESS;Initial Catalog=Movies;Integrated Security=True"))
            {
                string[] lines = System.IO.File.ReadAllLines(@"C:\Users\lenovo\Downloads\wordsEng.txt");
                foreach (string line in lines)
                {
                    string arama = line;
                    //string arama = "L'Année dernière à Marienbad";

                    using (var httpClient = new HttpClient())
                    {
                        string url = "https://www.beyazperde.com/_/autocomplete/" + arama;

                        HttpResponseMessage response = await httpClient.GetAsync(url);


                        if (response.IsSuccessStatusCode)
                        {
                            string responseContent = await response.Content.ReadAsStringAsync();
                            //List<MovieData> movieDataList = JsonSerializer.Deserialize<List<MovieData>>(responseContent);
                            //MovieData movieData = JsonSerializer.Deserialize<MovieData>(responseContent);

                            //dynamic myobj = JsonConvert.DeserializeObject<dynamic>(responseContent);

                            var resuts = JObject.Parse(responseContent)["results"];
                            char[] charsToTrim1 = { '[', ']', '\"' };
                            char[] spearator = { '.' };

                            for (int i = 0; i < resuts.Count(); i++)
                            {
                                var et = JObject.Parse(responseContent)["results"][i]["entity_type"];
                                string entityType = et.ToString();
                                if (entityType == "movie")
                                {
                                    var orLabel = JObject.Parse(responseContent)["results"][i]["original_label"];//MovieName
                                    var label = JObject.Parse(responseContent)["results"][i]["label"];//MovieTurkishName
                                    var dd = JObject.Parse(responseContent)["results"][i]["data"]["director_name"];//directors need forloop
                                    var Id = JObject.Parse(responseContent)["results"][i]["entity_id"];//entityID
                                    var year = JObject.Parse(responseContent)["results"][i]["data"]["year"];//realeseYear
                                    var foto = JObject.Parse(responseContent)["results"][i]["data"]["thumbnail"];//fotograf
                                    var genres = JObject.Parse(responseContent)["results"][i]["genres"];//type need forloop
                                    var lang = JObject.Parse(responseContent)["results"][i]["tags"];//lang hangi indexte olduğunu bulan loop 
                                    string hedefBaslangic = "Localization.Language.";
                                    List<string> listDirector = new List<string>();
                                    List<string> listType = new List<string>();
                                    bool yy;
                                    int releaseYear;
                                    string language = string.Empty;
                                    string ll;
                                    string photoURL = string.Empty;
                                    for (int j = 0; j < lang.Count(); j++)
                                    {

                                        var lan = lang[j];
                                        string lan1 = lan.ToString();
                                        bool control = lan1.StartsWith(hedefBaslangic);
                                        //string laguage = lan1;

                                        if (control)
                                        {
                                            String[] strlist = lan1.Split(spearator);
                                            ll = strlist[2];
                                            language = ll.ToString();
                                            //Console.WriteLine(language);
                                            break;
                                        }
                                    }
                                    //Console.WriteLine("\r Directors:\r");
                                    for (int j = 0; j < dd.Count(); j++)//directorLoop
                                    {
                                        var ddd = JObject.Parse(responseContent)["results"][i]["data"]["director_name"][j];
                                        string direc = ddd.ToString();
                                        string director = direc.Trim(charsToTrim1);
                                        director = director.Trim();
                                        director = director.Trim('"');
                                        listDirector.Add(director);
                                        //Console.WriteLine(director + "\r");
                                    }

                                    //Console.WriteLine("\r Types: \r");
                                    for (int j = 0; j < genres.Count(); j++)//typeLoop
                                    {
                                        var genre = JObject.Parse(responseContent)["results"][i]["genres"][j];
                                        string types = genre.ToString();
                                        string type = types.Trim(charsToTrim1);
                                        type = type.Trim();
                                        type = type.Trim('"');
                                        listType.Add(type);
                                        //Console.WriteLine(type + "\r");
                                    }
                                    string movieName = orLabel.ToString();
                                    //Console.WriteLine("\r mn: " + movieName);
                                    string movieTurkishNames = label.ToString();
                                    //Console.WriteLine("\r mtn: " + movieTurkishNames);
                                    int ID = Convert.ToInt32(Id);
                                    //Console.WriteLine("\r ID: " + ID);
                                    //int releaseYear = Convert.ToInt32(year);
                                   
                                    

                                    if (foto != null)
                                    {
                                        photoURL = foto.ToString();
                                        //Console.WriteLine("\r url: " + photoURL);
                                    }
                                    else
                                    {
                                        photoURL = string.Empty;
                                    }
                                    
                                    string yyy= year.ToString();
                                    yy= int.TryParse(yyy, out releaseYear);

                                
                                    
                                    try
                                    {
                                        baglanti.Open();
                                        try
                                        {
                                            //using (var cmd = new SqlDataAdapter())
                                            using (SqlCommand moviesAdd = new SqlCommand("INSERT INTO Movies(movieName,movieTurkishNames,language,releaseYear,entityID,photoURL)" +
                                                "VALUES (@mName,@mTName,@lan,@year,@eId,@pURL);", baglanti))
                                            {
                                                moviesAdd.Parameters.AddWithValue("@mName", movieName);
                                                moviesAdd.Parameters.AddWithValue("@mTName", movieTurkishNames);
                                                moviesAdd.Parameters.AddWithValue("@lan", language);
                                                moviesAdd.Parameters.AddWithValue("@year", releaseYear);
                                                moviesAdd.Parameters.AddWithValue("@eId", ID);
                                                moviesAdd.Parameters.AddWithValue("@pURL", photoURL);

                                                //Console.WriteLine(photoURL + language);
                                                moviesAdd.ExecuteNonQuery();


                                            }
                                            Console.WriteLine("başarılı");
                                        }
                                         catch (Exception e)
                                        {
                                        Console.WriteLine(e);
                                        }
                                        try
                                        {
                                            
                                            foreach (var item in listDirector)
                                            {
                                                using (SqlCommand movieDirector = new SqlCommand("INSERT INTO movieDirector(movieID,director) VALUES (@eID,@director);", baglanti))
                                                {
                                                    movieDirector.Parameters.AddWithValue("@eID", ID);
                                                    movieDirector.Parameters.AddWithValue("@director", item);

                                                    movieDirector.ExecuteNonQuery();
                                                }
                                            }
                                            foreach (var item1 in listType)
                                            {
                                                using (SqlCommand movieType = new SqlCommand("INSERT INTO movieTypes (movieID,type) VALUES (@eID,@type);", baglanti))
                                                {
                                                    movieType.Parameters.AddWithValue("@eID", ID);
                                                    movieType.Parameters.AddWithValue("@type", item1);

                                                    movieType.ExecuteNonQuery();
                                                }
                                            }

                                        }
                                        catch (Exception x)
                                        {
                                            Console.WriteLine(x);
                                            
                                        }
                                   
                                       
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex);
                                        baglanti.Close () ;
                                    }
                                    baglanti.Close ();
                                }

                            }
                        }


                        else
                        {
                            Console.WriteLine("İstek başarısız. Hata kodu: " + response.StatusCode);
                        }
                        Thread.Sleep(1000);

                    }
                    Console.WriteLine(arama);
                }
                
                
            }

                //Console.WriteLine("....");
                //string arama = Console.ReadLine();

               
        }

    }

}


