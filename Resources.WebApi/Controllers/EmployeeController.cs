using Microsoft.AspNetCore.Mvc;
using Resources;
using Resources.Service;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Resources.Shared;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

// http://www.restapitutorial.com/httpstatuscodes.html
// https://en.wikipedia.org/wiki/List_of_HTTP_status_codes

namespace Data.WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    //[AllowAnonymous]
    public class EmployeeController : Controller
    {
        private readonly ICustomerResourceService ResourceService;

        public EmployeeController(ICustomerResourceService resourceService)
        {
            ResourceService = resourceService;
        }


        [HttpGet("loginstatus")]
        public IActionResult LoginStatus()
        {
            var isAuthenticated = this.HttpContext.User.Identities.Any(u => u.IsAuthenticated);
            var email = this.User.FindFirst(c => c.Type.ContainsEx("email"))?.Value;

            var result = new
            {
                IsAuthenticated = isAuthenticated,
                Email = email
            };

            return Ok(result);
        }


        [HttpGet("Create")]
        [Authorize(Policy = "HR Only")]
        public IActionResult Create()
        {
            var resource = ResourceService.Create();

            return Json(resource);
        }

        private String RemoveSensitiveFields(EmployeeResource resource)
        {
            // The dynamic Linq Select(params) works only on an IQueryable list
            // thats why 1 one item is added to a list
            var items = new List<EmployeeResource>(new[] { resource }).AsQueryable();

            // Find all property names
            var propertyNames = resource.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

            // Salary only visible to HR department
            if (!User.HasClaim("Department", "HR"))
                propertyNames.Remove(nameof(EmployeeResource.Salary));

            // Dynamic Linq supports dynamic selector
            var selector = $"new({String.Join(",", propertyNames)})";

            // Create dynamic object with authorized fields 
            var reducedResource = items.Select(selector).First();

            // Create JSON
            var result = JsonConvert.SerializeObject(reducedResource, new JsonSerializerSettings() { Formatting = Formatting.Indented });

            return result;
        }


        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            if (id.IsNullOrEmpty())
                return BadRequest();

            var resource = await ResourceService.FindAsync(id);

            return (resource == null) ? NotFound() as IActionResult : new ContentResult() { Content = RemoveSensitiveFields(resource), ContentType = "application/text" };
        }


        [HttpGet("{email}")]
        public IActionResult Email(String email)
        {
            if (email.IsNullOrEmpty())
                return BadRequest();

            var resource = ResourceService.Items().FirstOrDefault(c => c.Email == email);

            return (resource == null) ? NotFound() as IActionResult : new ContentResult() { Content = RemoveSensitiveFields(resource), ContentType = "application/text" };
        }


        [HttpGet]
        public IActionResult Get(String sortBy, String sortDirection, Int32 skip, Int32 take, String search, String searchFields)
        {
            var loadResult = ResourceService.Load(sortBy, sortDirection, skip, take, search, searchFields);

            var fieldNames = loadResult.Items.AsQueryable().ElementType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToList();

            // Salary only visible to HR department
            if (!User.HasClaim("Department", "HR"))
                fieldNames.Remove(nameof(EmployeeResource.Salary));

            var selector = $"new({String.Join(",", fieldNames)})";

            var result = new
            {
                CountUnfiltered = loadResult.CountUnfiltered,
                Items = loadResult.Items.AsQueryable().Select(selector)
            };

            return Json(result);
        }


        [HttpPost]
        [Consumes("application/json")]
        [Produces("application/json", Type = typeof(EmployeeResource))]
        [Authorize(Policy = "HR Only")]
        public async Task<IActionResult> Post([FromBody]EmployeeResource resource)
        {
            try
            {
                // Create rescource
                var serviceResult = await ResourceService.InsertAsync(resource);

                // if return error message if needed
                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                // On succes return url with id and newly created resource  
                return CreatedAtAction(nameof(Get), new { id = serviceResult.Resource.Id }, serviceResult.Resource);
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<EmployeeResource>(resource);

                while (ex != null)
                    result.Exceptions.Add(ex.Message);

                return BadRequest(result);
            }
        }


        [HttpPut]
        [Authorize(Policy = "HR Only")]
        public async Task<IActionResult> Put([FromBody]EmployeeResource resource)
        {
            try
            {
                var currentResource = await ResourceService.FindAsync(resource.Id);

                if (currentResource == null)
                    return NotFound();

                var serviceResult = await ResourceService.UpdateAsync(resource);

                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                return Ok(serviceResult.Resource);
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<EmployeeResource>(resource);

                while (ex != null)
                {
                    result.Exceptions.Add(ex.Message);

                    if (ex is ConcurrencyException)
                        return StatusCode(HttpStatusCode.Conflict.ToInt32(), result);

                    ex = ex.InnerException;
                }

                return BadRequest(result);
            }
        }

        [HttpGet("RoleBasedDemo")]
        [Authorize(Roles = "HR-Worker")]
        public IActionResult RoleBasedDemo()
        {
            return Ok("I am role based");
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = "HR-Manager Only")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var serviceResult = await ResourceService.DeleteAsync(id);

                if (serviceResult.Resource == null)
                    return NoContent();

                if (serviceResult.Errors.Count > 0)
                    return BadRequest(serviceResult);

                return Ok();
            }
            catch (Exception ex)
            {
                var result = new ResourceResult<EmployeeResource>();

                while (ex != null)
                    result.Exceptions.Add(ex.Message);

                return BadRequest(result);
            }
        }
    }
}