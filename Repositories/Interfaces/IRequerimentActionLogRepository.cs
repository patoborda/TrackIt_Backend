using System.Collections.Generic;
using System.Threading.Tasks;
using trackit.server.Models;

namespace trackit.server.Repositories.Interfaces
{
    public interface IRequirementActionLogRepository
    {
        /// <summary>
        /// Agrega un nuevo log de acción relacionado con un requerimiento.
        /// </summary>
        /// <param name="actionLog">El log de acción a agregar.</param>
        Task AddActionLogAsync(RequirementActionLog actionLog);

        /// <summary>
        /// Obtiene los logs de acción relacionados con un requerimiento específico.
        /// </summary>
        /// <param name="requirementId">El ID del requerimiento.</param>
        /// <returns>Una lista de logs de acción relacionados con el requerimiento.</returns>
        Task<List<RequirementActionLog>> GetLogsByRequirementIdAsync(int requirementId);
    }
}
