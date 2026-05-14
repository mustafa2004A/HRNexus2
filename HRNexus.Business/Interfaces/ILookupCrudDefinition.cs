namespace HRNexus.Business.Interfaces;

public interface ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest>
{
    string EntityName { get; }
    bool UsesSoftDelete { get; }
    int GetId(TEntity entity);
    string GetSortText(TEntity entity);
    TDto ToDto(TEntity entity);
    TEntity CreateEntity(TCreateRequest request);
    void UpdateEntity(TEntity entity, TUpdateRequest request);
    void ValidateCreate(TCreateRequest request);
    void ValidateUpdate(int id, TUpdateRequest request);
    void Deactivate(TEntity entity);
}
