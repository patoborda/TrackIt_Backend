using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using trackit.server.Data;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;

namespace trackit.server.Repositories
{
    public class RequirementActionLogRepository : IRequirementActionLogRepository
    {
        private readonly UserDbContext _context;

        public RequirementActionLogRepository(UserDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Agrega un nuevo log de acción a la base de datos.
        /// </summary>
        /// <param name="actionLog">El log de acción a agregar.</param>
        public async Task AddActionLogAsync(RequirementActionLog actionLog)
        {
            await _context.RequirementActionLogs.AddAsync(actionLog);
            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos.
        }

        /// <summary>
        /// Obtiene los logs de acción relacionados con un requerimiento específico.
        /// </summary>
        /// <param name="requirementId">El ID del requerimiento.</param>
        /// <returns>Una lista de logs de acción relacionados con el requerimiento, ordenados por fecha descendente.</returns>
        public async Task<List<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId)
        {
            return await _context.RequirementActionLogs
                .Where(log => log.RequirementId == requirementId) // Filtra por el ID del requerimiento.
                .OrderByDescending(log => log.Timestamp) // Ordena por fecha de creación (más reciente primero).
                .ToListAsync();
        }
    }
}
