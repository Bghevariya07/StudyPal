using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{detail}")]
    public async Task<ActionResult<List<UserProfile>>> GetUserProfilesByFirstName(string detail)
    {
        var profiles = await _userService.GetUserProfilesByDetail(detail);
        if (profiles == null || profiles.Count == 0)
        {
            return NotFound();
        }
        return profiles;
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<List<UserProfile>>> GetAllUserProfiles()
    {
        var profiles = await _userService.GetAllUserProfiles();
        if (profiles == null || profiles.Count == 0)
        {
            return NotFound();
        }
        return profiles;
    }

    [HttpGet("profiles/{email}")]
    public async Task<ActionResult<UserProfile>> GetUsernameFromEmail(string email)
    {
        var profile = await _userService.GetUsernameFromEmail( email );
        if (profile == null)
        {
            return NotFound();
        }
        return profile;
    }
}
