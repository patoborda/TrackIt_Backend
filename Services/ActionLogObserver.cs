using System;
using trackit.server.Models;

namespace trackit.server.Services
{
    public class ActionLogObserver : IRequirementObserver
    {
        public void Update(Requirement requirement, string action, string performedBy, string details)
        {
            // Aquí puedes registrar la acción en la base de datos o simplemente mostrarla en consola
            Console.WriteLine($"Requirement ID: {requirement.Id}, Action: {action}, Performed By: {performedBy}, Details: {details}");
        }
    }
}
