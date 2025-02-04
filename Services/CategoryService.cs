using trackit.server.Dtos;
using trackit.server.Models;
using trackit.server.Repositories.Interfaces;
using trackit.server.Services.Interfaces;

namespace trackit.server.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _repository.GetAllAsync();
            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                RequirementTypeId = c.RequirementTypeId
            });
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                RequirementTypeId = category.RequirementTypeId
            };
        }

        public async Task<CategoryDto> AddAsync(CategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                RequirementTypeId = dto.RequirementTypeId
            };

            var createdCategory = await _repository.AddAsync(category);

            return new CategoryDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                RequirementTypeId = createdCategory.RequirementTypeId
            };
        }

        public async Task<CategoryDto> UpdateAsync(int id, string name, int requirementTypeId)
        {
            var existingCategory = await _repository.GetByIdAsync(id);
            if (existingCategory == null) throw new KeyNotFoundException("Category not found");

            existingCategory.Name = name;
            existingCategory.RequirementTypeId = requirementTypeId;

            var updatedCategory = await _repository.UpdateAsync(existingCategory);
            return new CategoryDto
            {
                Id = updatedCategory.Id,
                Name = updatedCategory.Name,
                RequirementTypeId = updatedCategory.RequirementTypeId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        // MÉTODO NUEVO: Retorna las categorías que pertenecen al RequirementType indicado
        public async Task<IEnumerable<CategoryDto>> GetByRequirementTypeAsync(int requirementTypeId)
        {
            var categories = await _repository.GetByRequirementTypeAsync(requirementTypeId);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                RequirementTypeId = c.RequirementTypeId
            });
        }
    }
}