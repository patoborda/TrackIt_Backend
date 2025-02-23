using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using trackit.server.Data;

public class ChatHub : Hub
{
    private readonly IDbContextFactory<UserDbContext> _dbContextFactory;

    public ChatHub(IDbContextFactory<UserDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    // Método cuando un usuario se conecta al chat
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Usuario conectado: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    // Método cuando un usuario se desconecta del chat
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Usuario desconectado: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    // Método para unirse a un grupo (requerimiento)
    public async Task JoinRequirementGroup(string requirementId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, requirementId);

        // Opcional: enviar el historial de chat al usuario que se conecta
        await SendChatHistory(requirementId);
    }

    // Método para enviar un mensaje y persistirlo
    public async Task SendMessageToRequirement(string requirementId, string userName, string message)
    {
        if (string.IsNullOrWhiteSpace(requirementId) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(message))
        {
            // Puedes enviar un error o manejar el caso en que falten datos
            return;
        }
        // Guardar el mensaje en la base de datos
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var chatMessage = new ChatMessage
            {
                RequirementId = int.Parse(requirementId), // convierte si es necesario
                UserName = userName,
                Message = message,
                SentAt = DateTime.UtcNow
            };

            context.ChatMessages.Add(chatMessage);
            await context.SaveChangesAsync();
        }

        // Enviar el mensaje a todos los clientes del grupo
        await Clients.Group(requirementId).SendAsync("ReceiveMessage", userName, message);
    }

    // Método para enviar el historial de chat al cliente que se acaba de conectar
    public async Task SendChatHistory(string requirementId)
    {
        using (var context = _dbContextFactory.CreateDbContext())
        {
            var messages = await context.ChatMessages
                .Where(m => m.RequirementId == int.Parse(requirementId))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Enviamos el historial solo al cliente actual
            await Clients.Caller.SendAsync("ReceiveChatHistory", messages);
        }
    }

    // Método para salir del grupo, si fuera necesario
    public async Task LeaveRequirementGroup(string requirementId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, requirementId);
    }
}
