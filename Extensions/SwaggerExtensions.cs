﻿using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESDemo.Controllers
{
    /// <summary>
    /// 添加控制器模块说明
    /// </summary>
    public class ApplyTagDescriptions : IDocumentFilter
    {
        /// <summary>
        /// apply
        /// </summary>
        /// <param name="swaggerDoc"></param>
        /// <param name="context"></param>
        public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Tags = new List<Tag>
            {
                new Tag { Name = "Account", Description = "登录相关接口" },
                new Tag { Name = "UserCenter", Description = "用户中心接口" },
            };
        }
    }
    /// <summary>
    /// 操作过过滤器 添加通用参数等
    /// </summary>
    public class AssignOperationVendorExtensions : IOperationFilter
    {
        /// <summary>
        /// apply
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation == null || context == null)
                return;
            operation.Parameters = operation.Parameters ?? new List<IParameter>();

            //token
            var isAuthor = context.ApiDescription.ControllerAttributes().Any(e => e.GetType() == typeof(UserAuthorizeAttribute)) || context.ApiDescription.ActionAttributes().Any(e => e.GetType() == typeof(UserAuthorizeAttribute));
            if (isAuthor)
            {
                //in query header 
                operation.Parameters.Insert(0, new NonBodyParameter() { Name = "X-Token", In = "header", Description = "身份验证票据", Required = true, Type = "string" });
            }
            //接口版本
            var versionInfo = operation.Parameters.FirstOrDefault(e => e.Name == "X-Version") as NonBodyParameter;
            if (versionInfo != null)
            {
                operation.Parameters.Remove(versionInfo);
                versionInfo.Default = context.ApiDescription.GroupName ?? typeof(ApiVersions).GetEnumNames().LastOrDefault();
                versionInfo.Description = "接口版本";
                operation.Parameters.Insert(0, versionInfo);
            }

            var fileAttr = context.ApiDescription.ActionAttributes().OfType<SwaggerFileUploadAttribute>().FirstOrDefault();
            if (fileAttr != null)
            {
                //operation.Parameters.Add("multipart/form-data");
                operation.Parameters.Add(new NonBodyParameter
                {
                    Name = "file",
                    In = "formData",
                    Description = "上传图片",
                    Required = (fileAttr as SwaggerFileUploadAttribute).Required,
                    Type = "file"
                });

            }
        }
    }
    /// <summary>
    /// 文件上传
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerFileUploadAttribute : Attribute
    {
        public bool Required { get; private set; }

        public SwaggerFileUploadAttribute(bool Required = true)
        {
            this.Required = Required;
        }
    }
}
