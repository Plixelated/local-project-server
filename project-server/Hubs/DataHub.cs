using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Hubs;

//SignalR Hub
//Empty because no additional services need to be performed
//Currently access is restricted to only those who have a user account
[Authorize(Policy = "UserOnlyAccess")]
public class DataHub() : Hub
{
}
