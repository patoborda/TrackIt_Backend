using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace trackit.server.Hubs
{

    public class CommentHub : Hub
    {
        public async Task SendComment(int requirementId, string user, string message)
        {
            // Enviar el comentario a todos los clientes en el grupo del requerimiento
            await Clients.Group(requirementId.ToString()).SendAsync("ReceiveComment", user, message);
        }

        public async Task JoinRequirementGroup(int requirementId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, requirementId.ToString());
        }

        public async Task LeaveRequirementGroup(int requirementId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, requirementId.ToString());
        }
    }
}