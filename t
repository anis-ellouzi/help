using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;

namespace SwaggerLikeApp.Controllers
{
    public class ApiExplorerController : Controller
    {
        public ActionResult Index()
        {
            var endpoints = DiscoverApiEndpoints();
            return View(endpoints);
        }

        private List<ApiEndpoint> DiscoverApiEndpoints()
        {
            var controllers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(Controller).IsAssignableFrom(t) && t.Name.EndsWith("Controller"))
                .ToList();

            var endpointList = new List<ApiEndpoint>();

            foreach (var controller in controllers)
            {
                string controllerName = controller.Name.Replace("Controller", "");

                var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsPublic && !m.IsDefined(typeof(NonActionAttribute)));

                foreach (var method in methods)
                {
                    string httpMethod = GetHttpMethod(method);

                    endpointList.Add(new ApiEndpoint
                    {
                        Controller = controllerName,
                        Action = method.Name,
                        HttpMethod = httpMethod,
                        Parameters = method.GetParameters()
                            .Select(p => new ApiParameter
                            {
                                Name = p.Name,
                                Type = p.ParameterType.Name
                            }).ToList()
                    });
                }
            }

            return endpointList;
        }

        private string GetHttpMethod(MethodInfo method)
        {
            if (method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any()) return "POST";
            if (method.GetCustomAttributes(typeof(HttpPutAttribute), false).Any()) return "PUT";
            if (method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any()) return "DELETE";
            return "GET"; // default
        }
    }

    public class ApiEndpoint
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string HttpMethod { get; set; }
        public List<ApiParameter> Parameters { get; set; }
    }

    public class ApiParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
@model List<SwaggerLikeApp.Controllers.ApiEndpoint>
@{
    Layout = null;
}

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>API Explorer</title>
    <style>
        body { font-family: Arial; margin: 20px; }
        .endpoint { border: 1px solid #ccc; padding: 15px; margin-bottom: 15px; }
        input[type="text"] { margin: 5px; width: 150px; }
        .response { margin-top: 10px; color: green; white-space: pre; }
    </style>
</head>
<body>
    <h1>Discovered API Endpoints</h1>

    @foreach (var endpoint in Model)
    {
        var formId = $"form_{endpoint.Controller}_{endpoint.Action}";
        var resultId = $"result_{endpoint.Controller}_{endpoint.Action}";
        <div class="endpoint">
            <h3>@endpoint.HttpMethod: /@endpoint.Controller/@endpoint.Action</h3>

            <form id="@formId" onsubmit="event.preventDefault(); callApi('@endpoint.Controller', '@endpoint.Action', '@endpoint.HttpMethod', '@formId', '@resultId')">
                @foreach (var param in endpoint.Parameters)
                {
                    <label>@param.Name (@param.Type): <input name="@param.Name" type="text" /></label><br />
                }
                <button type="submit">Test</button>
            </form>
            <div id="@resultId" class="response"></div>
        </div>
    }

    <script>
        function callApi(controller, action, method, formId, resultId) {
            const form = document.getElementById(formId);
            const data = new FormData(form);
            const params = new URLSearchParams();
            for (const pair of data.entries()) {
                params.append(pair[0], pair[1]);
            }

            let url = `/${controller}/${action}`;
            let options = {
                method: method,
                headers: {}
            };

            if (method === 'GET') {
                url += '?' + params.toString();
            } else {
                options.headers['Content-Type'] = 'application/x-www-form-urlencoded';
                options.body = params;
            }

            fetch(url, options)
                .then(res => res.text())
                .then(data => {
                    document.getElementById(resultId).innerText = data;
                }).catch(err => {
                    document.getElementById(resultId).innerText = 'Error: ' + err;
                });
        }
    </script>
</body>
</html>
