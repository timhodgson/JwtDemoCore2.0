using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Models;
using Models.Mappings;
using Newtonsoft.Json;
using Resources;
using Resources.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
  public class EmployeeController : Controller
  {
    // delegate for easy switching between insert or update
    private delegate Task<HttpResponseMessage> Upsert(String requestUri, HttpContent content);

    private readonly IMapper mapper;
    private readonly String apiUrl;
    private readonly IRestClient restClient;

    public EmployeeController(IRestClient client)
    {
      restClient = client;

      apiUrl = "/api/employee/";

      // Setup AutoMapper for mapping between Resource and Model
      var config = new AutoMapper.MapperConfiguration(cfg =>
      {
        cfg.AddProfiles(typeof(EmployeeMapping).GetTypeInfo().Assembly);
      });

      mapper = config.CreateMapper();
    }


    public IActionResult Index()
    {
      return View();
    }


    [HttpGet]
    public async Task<IActionResult> Load(String sort, String order, Int32 offset, Int32 limit, String search, String searchFields)
    {
      //  setup url with query parameters
      var queryString = new Dictionary<String, String>();
      queryString["sortBy"] = sort ?? "";
      queryString["sortDirection"] = order ?? "";
      queryString["skip"] = offset.ToString();
      queryString["take"] = limit.ToString();
      queryString[nameof(search)] = search ?? "";
      queryString[nameof(searchFields)] = searchFields ?? "";

      // convert dictionary to query params
      var uriBuilder = new UriBuilder(restClient.BaseAddress + apiUrl)
      {
        Query = QueryHelpers.AddQueryString("", queryString)
      };

      using (var client = restClient.CreateClient(User))
      {
        using (var response = await client.GetAsync(uriBuilder.Uri))
        {
          var document = await response.Content.ReadAsStringAsync();

          var loadResult = JsonConvert.DeserializeObject<LoadResult<EmployeeResource>>(document);

          // Convert loadResult into Bootstrap-Table compatible format
          var result = new
          {
            total = loadResult.CountUnfiltered,
            rows = loadResult.Items
          };

          return Json(result);
        }
      }
    }


    [HttpGet]
    [Authorize(Policy = "HR Only")]
    public async Task<IActionResult> Edit(Guid id)
    {
      // Check if new or edit
      String url = apiUrl + ((id.IsNullOrEmpty()) ? "create" : $"{id}");

      using (var client = restClient.CreateClient(User))
      {
        using (var response = await client.GetAsync(url))
        {
          var document = await response.Content.ReadAsStringAsync();

          if (response.StatusCode == HttpStatusCode.OK)
          {
            var demoDoc = JsonConvert.SerializeObject(new EmployeeResource());
            var resource = JsonConvert.DeserializeObject<EmployeeResource>(document);

            var result = mapper.Map<EmployeeModel>(resource);

            return PartialView(nameof(Edit), result);
          }

          else
          {
            var result = new ResourceResult<EmployeeResource>();

            if (response.StatusCode == HttpStatusCode.NotFound)
              result.Errors.Add(new ValidationError($"Record with id {id} is not found"));

            return StatusCode(response.StatusCode.ToInt32(), result);
          }
        }
      }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "HR Only")]
    public async Task<IActionResult> Edit([FromForm]EmployeeModel model)
    {
      if (!ModelState.IsValid)
        PartialView();

      // Map model to resource
      var resource = mapper.Map<EmployeeResource>(model);

      // save resource to Json
      var resourceDocument = JsonConvert.SerializeObject(resource);

      using (var content = new StringContent(resourceDocument, Encoding.UTF8, "application/json"))
      {
        using (var client = restClient.CreateClient(User))
        {
          // determen call update or insert
          Upsert upsert = client.PutAsync;

          // no RowVersion indicates insert
          if (model.RowVersion.IsNullOrEmpty())
            upsert = client.PostAsync;

          using (var response = await upsert(apiUrl, content))
          {
            // init result
            var result = new ResourceResult<EmployeeResource>(resource);

            // read result from RESTful service
            var responseDocument = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created)
            {
              // Fetch created or updated resource from response
              result.Resource = JsonConvert.DeserializeObject<EmployeeResource>(responseDocument); ;
            }
            else
            {
              // fetch errors and or exceptions
              result = JsonConvert.DeserializeObject<ResourceResult<EmployeeResource>>(responseDocument);
            }

            // Set error message for concurrency error
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
              result.Errors.Clear();
              result.Errors.Add(new ValidationError("This record is modified by another user"));
              result.Errors.Add(new ValidationError("Your work is not saved and replaced with new content"));
              result.Errors.Add(new ValidationError("Please review the new content and if required edit and save again"));
            }

            if (response.StatusCode.IsInSet(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.Conflict))
              return StatusCode(response.StatusCode.ToInt32(), result);

            // copy errors so they will be rendered in edit form
            foreach (var error in result.Errors)
              ModelState.AddModelError(error.MemberName ?? "", error.Message);

            // Update model with Beautify effect(s) and make it visible in the partial view
            IEnumerable<PropertyInfo> properties = model.GetType().GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
              var rawValue = property.GetValue(model);
              var attemptedValue = rawValue == null ? "" : Convert.ToString(rawValue, CultureInfo.InvariantCulture);

              ModelState.SetModelValue(property.Name, rawValue, attemptedValue);
            }

            // No need to specify model here, it has no effect on the render process :-(
            return PartialView();
          }
        }
      }
    }


    [HttpPost]
    [Authorize(Policy = "HR-Manager Only")]
    public async Task<IActionResult> Delete(Int32 id)
    {
      String url = apiUrl + $"{id}";

      using (var client = restClient.CreateClient(User))
      {
        using (var response = await client.DeleteAsync(url))
        {
          var responseDocument = await response.Content.ReadAsStringAsync();

          // create only response if somethomg off has happenend
          if (response.StatusCode != HttpStatusCode.OK)
          {
            var result = JsonConvert.DeserializeObject<ResourceResult<EmployeeResource>>(responseDocument);

            return StatusCode(response.StatusCode.ToInt32(), result);
          }

          return Content(null);
        }
      }
    }
  }
}
