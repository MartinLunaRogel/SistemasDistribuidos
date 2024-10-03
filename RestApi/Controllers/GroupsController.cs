using System.Net;
using Microsoft.AspNetCore.Mvc;
using RestApi.Dtos;
using RestApi.Exceptions;
using RestApi.Mappers;
using RestApi.Services;

namespace RestApi.Controller;

[ApiController]
[Route("[controller]")]
public class GroupsController : ControllerBase {
    private readonly IGroupService _groupService;
    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    //localhosts:port/groups/192282892929
    [HttpGet("{id}")]
    public async Task <ActionResult<GroupResponse>> GetGroupById(string id, CancellationToken cancellationToken)
    {
        var group = await _groupService.GetGroupByIdAsync(id, cancellationToken);
        
        if (group is null)
        {
            return NotFound();
        }

        return Ok(group.ToDto());
    }

    [HttpGet]
    public async Task<ActionResult<IList<GroupResponse>>> GetAllByName([FromQuery] string name, [FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string orderBy, CancellationToken cancellationToken)
    {
        var groups = await _groupService.GetAllByNameAsync(name, pageNumber,pageSize,orderBy ,cancellationToken);
        
        return Ok(groups.Select(group => group.ToDto()).ToList());
    } 

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGroup(string id, CancellationToken cancellationToken){
        try
        {
            await _groupService.DeleteGroupByIdAsync(id, cancellationToken);
            return NoContent();
        }
        catch (GroupNotFoundException)
        {
           return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponse>> CreateGroup([FromBody] CreateGroupRequest groupRequest, CancellationToken cancellationToken){
        try
        {
            var group = await _groupService.CreateGroupAsync(groupRequest.Name, groupRequest.User, cancellationToken);
            return CreatedAtAction(nameof(GetGroupById), new {id = group.Id}, group.ToDto());
        }
        catch (InvalidGroupRequestFormatException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors occured.", HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", ["Users array is empty"]}
            }));
        }
        catch(GroupAlreadyExistsException){
            return Conflict(NewValidationProblemDetails("One or more validation errors occured.", HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", ["Group with some name already exist"]}
            }));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGroup(string id, [FromBody] UpdateGroupRequest groupRequest, CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.UpdateGroupAsync(id, groupRequest.Name, groupRequest.Users, cancellationToken);
            return NoContent();
        }
        catch(GroupNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidGroupRequestFormatException)
        {
            return BadRequest(NewValidationProblemDetails("One or more validation errors occured.", HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", ["Users array is empty"]}
            }));
        }
        catch(GroupAlreadyExistsException){
            return Conflict(NewValidationProblemDetails("One or more validation errors occured.", HttpStatusCode.BadRequest, new Dictionary<string, string[]>{
                {"Groups", ["Group with some name already exist"]}
            }));
        }
    }

    private static ValidationProblemDetails NewValidationProblemDetails(string title, HttpStatusCode statusCode, Dictionary<string, string[]> errors){
        return new ValidationProblemDetails{
            Title = title,
            Status = (int) statusCode,
            Errors = errors
        };
    }
}