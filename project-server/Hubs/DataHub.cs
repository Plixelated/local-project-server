using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Hubs;

[Authorize(Policy = "UserOnlyAccess")]
public class DataHub() : Hub
{
}
