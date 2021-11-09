using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace Darl.GraphQL.Pages
{
    public class ContactsModel : PageModel
    {
        readonly IKGTranslation _conn;
        private readonly IHttpContextAccessor _context;

        public ContactsModel(IKGTranslation trans, IHttpContextAccessor context)
        {
            _conn = trans;
            _context = context;
        }
        public async Task<JsonResult> OnGetData([FromQuery] int? page, [FromQuery] int? limit, [FromQuery] string sortBy, [FromQuery] string direction, [FromQuery] string lastName, [FromQuery] string company)
        {
            try
            {
                if (_context != null && _context.HttpContext != null && _context.HttpContext.Request.Headers.ContainsKey("apikey"))
                {
                    var key = _context.HttpContext.Request.Headers["apikey"].FirstOrDefault();
                    var user = await _conn.GetUserByApiKey(key ?? String.Empty);
                    if (user == null || user.accountState != DarlUser.AccountState.admin)
                    {
                        return new JsonResult(new { records = new List<Contact>(), total = 0 });
                    }
                }
                else if (_context != null && _context.HttpContext != null && (_context.HttpContext.User == null || !_context.HttpContext.User.IsInRole("Admin")))
                {
                    return new JsonResult(new { records = new List<Contact>(), total = 0 });
                }
                int recordsTotal = 0;

                // getting all Customer data  
                var responseData = await _conn.GetContactsQueryable();
                //Sorting  
                if (!(string.IsNullOrEmpty(sortBy) && string.IsNullOrEmpty(direction)))
                {
                    var prop = GetResponseProperty(sortBy);
                    if (prop != null)
                    {
                        if (direction == "asc")
                        {
                            responseData = responseData.OrderBy(prop.GetValue).AsQueryable();
                        }
                        else
                        {
                            responseData = responseData.OrderByDescending(prop.GetValue).AsQueryable();
                        }
                    }
                }
                //Search  
                if (!string.IsNullOrEmpty(lastName))
                {
                    responseData = responseData.Where(m => m.LastName != null && m.LastName.ToLower().Contains(lastName.ToLower()));
                }

                if (!string.IsNullOrEmpty(company))
                {
                    responseData = responseData.Where(m => m.Company != null && m.Company.ToLower().Contains(company.ToLower()));
                }

                //total number of rows counts   
                recordsTotal = responseData.Count();
                //Paging  
                List<Contact> data;
                if (page.HasValue && limit.HasValue)
                {
                    int start = (page.Value - 1) * limit.Value;
                    data = responseData.Skip(start).Take(limit.Value).ToList();
                }
                else
                {
                    data = responseData.ToList();
                }
                //Returning Json Data  
                return new JsonResult(new { records = data, total = recordsTotal });
            }
            catch
            {
                return new JsonResult(new { records = new List<Contact>(), total = 0 });
            }
        }

        public async Task OnPostDeleteAsync(string id)
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("apikey"))
            {
                var user = await _conn.GetUserByApiKey(_context.HttpContext.Request.Headers["apikey"].FirstOrDefault());
                if (user == null || user.accountState != DarlUser.AccountState.admin)
                    return;
            }
            else if (_context.HttpContext.User == null || !_context.HttpContext.User.IsInRole("Admin"))
            {
                return;
            }
            await _conn.DeleteContactAsync(id);
        }

        public async Task OnPostUpdateAsync()
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("apikey"))
            {
                var user = await _conn.GetUserByApiKey(_context.HttpContext.Request.Headers["apikey"].FirstOrDefault());
                if (user == null || user.accountState != DarlUser.AccountState.admin)
                    return;
            }
            else if (_context.HttpContext.User == null || !_context.HttpContext.User.IsInRole("Admin"))
            {
                return;
            }
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                string decoded = HttpUtility.UrlDecode(requestBody);
                decoded = decoded.Substring(8);
                if (requestBody.Length > 0)
                {
                    try
                    {
                        var con = JsonConvert.DeserializeObject<Contact>(decoded);
                        if (con.Created == DateTime.MinValue)
                            con.Created = DateTime.Now;
                        if (string.IsNullOrEmpty(con.Id))
                            con.Id = Guid.NewGuid().ToString();
                        await _conn.UpdateContactAsync(con);
                    }
                    catch
                    {

                    }

                }
            }
        }

        public async Task OnPostCreateAsync()
        {
            if (_context.HttpContext.Request.Headers.ContainsKey("apikey"))
            {
                var user = await _conn.GetUserByApiKey(_context.HttpContext.Request.Headers["apikey"].FirstOrDefault());
                if (user == null || user.accountState != DarlUser.AccountState.admin)
                    return;
            }
            else if (_context.HttpContext.User == null || !_context.HttpContext.User.IsInRole("Admin"))
            {
                return;
            }
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyTo(stream);
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                string decoded = HttpUtility.UrlDecode(requestBody);
                decoded = decoded.Substring(8);
                if (requestBody.Length > 0)
                {
                    try
                    {
                        var con = JsonConvert.DeserializeObject<Contact>(decoded);
                        con.Created = DateTime.Now;
                        await _conn.CreateContactAsync(con);
                    }
                    catch
                    {

                    }

                }
            }
        }


        private PropertyInfo? GetResponseProperty(string name)
        {
            var properties = typeof(Contact).GetProperties();
            PropertyInfo? prop = null;
            foreach (var item in properties)
            {
                if (item.Name.ToLower().Equals(name.ToLower()))
                {
                    prop = item;
                    break;
                }
            }
            return prop;
        }
    }
}