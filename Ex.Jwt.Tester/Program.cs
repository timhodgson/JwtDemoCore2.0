using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ex.Jwt.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Ex.Jwt.Tester");
            Console.WriteLine("Sliding Demo");
            string jwt = SlidingDemo();

            Console.WriteLine(GetValue(jwt, "jadds4z@1688.com").Result);

            //             if (env.IsDevelopment())
            {
                Console.WriteLine($"{Environment.NewLine}Ready, press key to close");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Demonstrates how to renew the JWT
        /// </summary>
        /// <returns></returns>
        private static string SlidingDemo()
        {
            try
            {
                var jwt = Login("hrworker@xyz.com", "password").Result;
                if ( jwt != null )
                {
                    Console.WriteLine("Logged in");
                    Task.Delay(1000).Wait();
                    var renewed = RenewJwt(jwt).Result;
                    if (renewed != null)
                    {
                        Console.WriteLine("JWT renewed");
                    }
                    else
                    {
                        Console.WriteLine("Renewal failed");
                    }
                }
                else
                {
                    Console.WriteLine("Login failed");
                }
                return jwt;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return null;
        }

        private static async Task<String> GetValue(String jwt, String email)
        {
            var url = "http://localhost:41932";
            var apiUrl = $"/api/values/{WebUtility.UrlEncode(email)}";

            using (var client = new HttpClient() { BaseAddress = new Uri(url) })
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

                using (var response = await client.GetAsync(apiUrl))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                        return await response.Content.ReadAsStringAsync();

                    else return null;
                }
            }
        }

        #region Login Functions
        #region RenewJWT
        /// <summary>
        /// Renews a JWT, simply by specifying it.
        /// </summary>
        /// <param name="jwt">The JWT</param>
        /// <returns></returns>
        private static async Task<String> RenewJwt(String jwt)
        {
            var url = "";
            var apiUrl = "";
#if DEBUG            
            url = "http://localhost:49842/"; // Ex.Jwt.Issuer
            apiUrl = "/api/security/renewtoken/";
#else
            url = "http://localhost/";
            apiUrl = "/Jwt.Issuer/api/security/renewtoken/";
#endif

            using (var client = new HttpClient() { BaseAddress = new Uri(url) })
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using (var content = new FormUrlEncodedContent(new Dictionary<String, String>() { { "", jwt } }))
                {
                    using (var response = await client.PostAsync(apiUrl, content))
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        return response.StatusCode == HttpStatusCode.OK ? result : null;
                    }
                }
            }
        }
        #endregion

        #region Login
        /// <summary>
        /// Login to the Ex.Jwt.Issuer WebAPI
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private static async Task<String> Login(String email, String password)
        {
            var url = "";
            var apiUrl = "";
#if DEBUG            
            url = "http://localhost:49842/"; // Jwt.Issuer project!
            apiUrl = "/api/security/login/";
#else
            url = "http://localhost/";
            apiUrl = "/Jwt.Issuer/api/security/login/";
#endif

            using (var client = new HttpClient() { BaseAddress = new Uri(url) })
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var loginResource = new
                {
                    Email = email,
                    Password = password
                };

                var resourceDocument = JsonConvert.SerializeObject(loginResource);

                using (var content = new StringContent(resourceDocument, Encoding.UTF8, "application/json"))
                {
                    using (var response = await client.PostAsync(apiUrl, content))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var result = await response.Content.ReadAsStringAsync();

                            return result;
                        }
                        else
                        {
                            throw new Exception(string.Format("Uri={2}, Status Code={0}, Error={1}", response.StatusCode, response.ReasonPhrase,
                                response.RequestMessage.RequestUri.AbsoluteUri));
                        }

                    }
                }
            }
        }
        #endregion
        #endregion
    }

}
