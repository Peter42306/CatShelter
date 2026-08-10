namespace CatShelter.Services.PhotoStorage
{
    public interface IPhotoStorage
    {
        Task<string> UploadAsync(
            IFormFile file,
            int animalId,
            CancellationToken ct = default);

        Task DeleteAsync(
            string storageKey,
            CancellationToken ct = default);

        string GetPublicUrl(string storageKey);
    }
}
