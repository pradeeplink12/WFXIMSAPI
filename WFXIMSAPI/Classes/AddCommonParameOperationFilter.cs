using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace WFXIMSAPI.Classes
{
    public class AddCommonParameOperationFilter : IOperationFilter
    {

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

            if (descriptor != null && !descriptor.ControllerName.StartsWith("Weather"))
            {
                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "pageParams",
                    In = ParameterLocation.Header,
                    Description = "The pageParams for WEBAPI",
                    Required = true
                });

                operation.Parameters.Add(new OpenApiParameter()
                {
                    Name = "apiParams",
                    In = ParameterLocation.Header,
                    Description = "The apiParams for WEBAPI",
                    Required = true
                });

            }
        }
    }
}
