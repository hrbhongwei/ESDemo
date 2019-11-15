using ESDemo.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESDemo.Extensions
{
    /// <summary>
    /// api接口服务
    /// </summary>
    public interface IApiTokenService
    {
        /// <summary>
        /// 转换成token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        string ConvertLoginToken(int userId, string userName);
        /// <summary>
        /// 根据token解密信息
        /// </summary>
        /// <returns></returns>
        UserApiTokenPayload GetUserPayloadByToken();
    }
    /// <summary>
    /// 
    /// </summary>
    public class UserApiTokenPayload
    {
        /// <summary>
        /// 
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
    /// <summary>
    /// apiToken_唯一key
    /// </summary>
    public class ApiTokenConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public ApiTokenConfig(string key)
        {
            Api_Token_Key = key;
        }
        /// <summary>
        /// 
        /// </summary>
        public string Api_Token_Key { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class ApiTokenService : IApiTokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _api_key_token;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="httpContextAccessor"></param>
        public ApiTokenService(ApiTokenConfig token
            , IHttpContextAccessor httpContextAccessor
            )
        {
            _httpContextAccessor = httpContextAccessor;
            _api_key_token = token.Api_Token_Key;
        }
        /// <summary>
        /// 换取登录token 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string ConvertLoginToken(int userId, string userName)
        {
            return JwtHelper.Encode(new UserApiTokenPayload() { UserId = userId, UserName = userName }, _api_key_token);
        }
        private UserApiTokenPayload _cachePaload;
        /// <summary>
        /// 获取登录信息 
        /// </summary>
        /// <remarks>
        /// 获取header或者参数携带的x-token参数
        /// </remarks>
        /// <returns></returns>
        public UserApiTokenPayload GetUserPayloadByToken()
        {
            if (_cachePaload != null)
                return _cachePaload;
            var token = _httpContextAccessor.HttpContext.Request.Headers["X-Token"];
            //header或者query带有x-token参数
            token = string.IsNullOrEmpty(token) ? _httpContextAccessor.HttpContext.Request.Query["x-token"] : token;
            if (string.IsNullOrEmpty(token))
                return null;
            _cachePaload = JwtHelper.Decode<UserApiTokenPayload>(token, _api_key_token);
            return _cachePaload;
        }
    }
}
