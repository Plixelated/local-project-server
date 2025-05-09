using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using project_model;
using project_server.Dtos;

namespace project_server.Hubs;

[Authorize(Policy = "ViewUserData")]
public class DataHub() : Hub
{
}
