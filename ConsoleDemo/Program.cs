using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SecTest
{
  class Program
  {
    static void Main(String[] args)
    {
      GetEmployeesDemo();

      ClaimBasedContentDemo();
     
      Console.WriteLine($"{Environment.NewLine}Ready, press key to close");
      Console.ReadKey();
    }


    private static void SlidingDemo()
    {
      var jwt = Login("hrworker@xyz.com", "password").Result;

      Task.Delay(1000).Wait();

      var renewed = RenewJwt(jwt).Result;
    }

    private static async Task<String> RenewJwt(String jwt)
    {
      var url = "http://localhost/Jwt.Issuer/";
      var apiUrl = "/api/security/renewtoken/";

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


    private static void ClaimBasedContentDemo()
    {
      // each token represents a different identity
      var tokens = new String[]
      {
       Login("hrworker@xyz.com", "password").Result,
       Login("employee@xyz.com", "password").Result
      };

      foreach (var token in tokens)
      {
        Console.WriteLine(GetLoginStatus(token).Result);
        Console.WriteLine(GetEmployee(token, "jadds4z@1688.com").Result);
        Console.WriteLine("");
      }
    }

    private static async Task<String> GetLoginStatus(String jwt)
    {
      var url = "http://localhost/Jwt.Resources.WebAPI/";
      var apiUrl = $"/api/employee/loginstatus";

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

    private static async Task<String> GetEmployee(String jwt, String email)
    {
      var url = "http://localhost/Jwt.Resources.WebAPI/";
      var apiUrl = $"/api/employee/{WebUtility.UrlEncode(email)}";

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


    private static void GetEmployeesDemo()
    {
      Console.WriteLine("Request Token");
      var jwt = Login("employee@xyz.com", "password").Result;
      Console.WriteLine($"Token : {jwt}");
      Console.WriteLine("");

      var document = GetEmployees(jwt).Result;
      Console.WriteLine($"Employees: {document}");
      Console.WriteLine("");
    }


    private static async Task<String> GetEmployees(String jwt)
    {
      var url = "http://localhost/Jwt.Resources.WebAPI/";
      var apiUrl = $"/api/employee/";

      using (var client = new HttpClient() { BaseAddress = new Uri(url) })
      {
        client.BaseAddress = new Uri(url);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwt}");

        using (var response = await client.GetAsync(apiUrl))
        {
          if (response.StatusCode == System.Net.HttpStatusCode.OK)
            return await response.Content.ReadAsStringAsync();

          else return null;
        }
      }
    }

    private static async Task<String> Login(String email, String password)
    {
      var url = "http://localhost/Jwt.Issuer/";
      var apiUrl = "/api/security/login/";

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

            else return null;
          }
        }
      }
    }
  }
}