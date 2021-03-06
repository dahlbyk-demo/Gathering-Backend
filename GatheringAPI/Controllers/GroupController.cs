using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GatheringAPI.Models.Api;
using Microsoft.AspNetCore.Mvc;
using GatheringAPI.Models;
using GatheringAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GatheringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        //private readonly GatheringDbContext _context;
        private readonly IGroup repository;

        public GroupController(IGroup groupRepository)
        {
            repository = groupRepository;
        }

        // GET: api/Group
        [HttpGet]
        public IEnumerable<GroupDto> GetGroups()
        {
            long userId = UserId;
            return repository.GetAll(userId);
        }

        private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // GET: api/Group/5
        [HttpGet("{id}")]
        public GroupDto GetGroup(long id)
        {
            long userId = UserId;
            return repository.Find(id, userId);
        }

        // PUT: api/Group/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGroup(long id, Group @group)
        {
            if (id != @group.GroupId)
            {
                return BadRequest();
            }

            bool didUpdate = await repository.UpdateAsync(@group, UserId);

            if (!didUpdate)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Group
        [HttpPost]
        public async Task<ActionResult<Group>> PostGroup(Group @group)
        {
            await repository.CreateAsync(@group, UserId);
            return CreatedAtAction("GetGroup", new { id = @group.GroupId }, @group);
        }

        // DELETE: api/Group/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Group>> DeleteGroup(long id)
        {
            long userId = UserId;
            var @group = await repository.DeleteAsync(id, userId);

            if (@group == null)
            {
                return NotFound();
            }
            return @group;
        }

        // POST: api/Group/5/Event/3
        [HttpPost("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> AddEvent(long groupId, long eventId)
        {
            await repository.AddEventAsync(groupId, eventId);
            repository.SendInvites(eventId);


            return CreatedAtAction(nameof(AddEvent), new { groupId, eventId }, null);
        }

        // DELETE: api/Group/5/Event/3
        [HttpDelete("{groupId}/Event/{eventId}")]
        public async Task<ActionResult> DeleteEvent(long groupId, long eventId)
        {
            await repository.DeleteEventAsync(groupId, eventId);
            return Ok();
        }

        // PUT: api/Group/5/Event/3
        [HttpPut("{groupId}/Event/{eventId}")]
        public async Task<bool> UpdateEvent(long groupId, Event @event)
        {
            bool didUpdate = await repository.UpdateEventAsync(groupId, @event);
            return didUpdate;
        }

        // POST: api/Group/5/User/2
        [HttpPost("{groupId}/User/{userId}")]
        public async Task<ActionResult> AddUser(long groupId, long userId)
        {
            await repository.AddUserAsync(groupId, userId);
            return CreatedAtAction(nameof(AddUser), new { groupId, userId }, null);
        }

        //POST: api/Group/5/Event
        [HttpPost("{groupId}/Event")]
        public async Task<ActionResult<Event>> AddEventToGroup(Event @event, long groupId)
        {
            await repository.CreateEventAsync(@event, UserId, groupId);
            return Ok();
        }
        
    }
}
