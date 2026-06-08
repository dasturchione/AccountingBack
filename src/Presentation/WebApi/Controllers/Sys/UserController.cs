using Application.Features.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Constants;
using WebApi.Authorization;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ModuleAuthorize(PermissionCodeConst.UserView)]
        public async Task<IResult> GetAllAsync([FromQuery] UserListFilter filter, CancellationToken ct = default)
        {
            var response = await _userService.GetAllAsync(filter, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpGet("{id:long}")]
        [ModuleAuthorize(PermissionCodeConst.UserViewDetail)]
        public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var response = await _userService.GetByIdAsync(id, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpPost]
        [ModuleAuthorize(PermissionCodeConst.UserCreate)]
        public async Task<IResult> CreateAsync([FromBody] UserCreateDto dto, CancellationToken ct = default)
        {
            var response = await _userService.CreateAsync(dto, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpPut("{id:int}")]
        [ModuleAuthorize(PermissionCodeConst.UserUpdate)]
        public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] UserUpdateDto dto, CancellationToken ct = default)
        {
            var response = await _userService.UpdateAsync(id, dto, ct);
            return response.Match(Results.NoContent, CustomResults.Problem);
        }

        [HttpDelete("{id:int}")]
        [ModuleAuthorize(PermissionCodeConst.UserDelete)]
        public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var response = await _userService.DeleteAsync(id, ct);
            return response.Match(Results.NoContent, CustomResults.Problem);
        }
    }
}
