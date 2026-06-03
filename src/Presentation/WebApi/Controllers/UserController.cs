using Application.Features.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;
using WebApi.Infrastructure;

namespace WebApi.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IResult> GetAllAsync([FromQuery] UserListFilter filter, CancellationToken ct = default)
        {
            var response = await _userService.GetAllAsync(filter, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpGet("{id:long}")]
        public async Task<IResult> GetByIdAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var response = await _userService.GetByIdAsync(id, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpPost]
        public async Task<IResult> CreateAsync([FromBody] UserCreateDto dto, CancellationToken ct = default)
        {
            var response = await _userService.CreateAsync(dto, ct);
            return response.Match(Results.Ok, CustomResults.Problem);
        }

        [HttpPut("{id:int}")]
        public async Task<IResult> UpdateAsync([FromRoute] int id, [FromBody] UserUpdateDto dto, CancellationToken ct = default)
        {
            var response = await _userService.UpdateAsync(id, dto, ct);
            return response.Match(Results.NoContent, CustomResults.Problem);
        }

        [HttpDelete("{id:int}")]
        public async Task<IResult> DeleteAsync([FromRoute] int id, CancellationToken ct = default)
        {
            var response = await _userService.DeleteAsync(id, ct);
            return response.Match(Results.NoContent, CustomResults.Problem);
        }
    }
}
