using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// 自动以控制器名称作为Swagger内部Tag分组（同一份文档内折叠）
/// </summary>
public class SwaggerTagGroupFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is ControllerActionDescriptor controllerDesc)
        {
            // 1. 获取控制器Tag名称（优先自定义中文特性，无则取控制器名）
            var tagAttr = controllerDesc.ControllerTypeInfo
                .GetCustomAttributes(typeof(SwaggerTagAttribute), false)
                .FirstOrDefault() as SwaggerTagAttribute;

            string tagName = tagAttr?.TagName ?? controllerDesc.ControllerName;

            // 2. 构建 TagReference，适配 ISet 类型
            /*
             * 无法将类型 “System.Collections.Generic.List<Microsoft.OpenApi.OpenApiTag>” 隐式转换为 “System.Collections.Generic.ISet<Microsoft.OpenApi.OpenApiTagReference>”。存在一个显式转换 (是否缺少强制转换？)
             */
            operation.Tags = new HashSet<OpenApiTagReference>
                {
                    new OpenApiTagReference(tagName)
                };
        }
    }
}

/// <summary>
/// 自定义控制器中文Tag特性
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SwaggerTagAttribute : Attribute
{
    public string TagName { get; }
    public SwaggerTagAttribute(string tagName)
    {
        TagName = tagName;
    }
}