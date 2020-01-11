namespace GoogleDrive
{
    /// <summary>
    /// Интерфейс для работы с google-диском.
    /// </summary>
    public interface IGoogleDriveManager
    {
        /// <summary>
        /// Получение идентификатора файла на google-диске по его наименованию.
        /// </summary>
        /// <param name="fileName">Наименованвие файла.</param>
        /// <returns>Идентификатор файла на google-диске.</returns>
        string GetFileId(string fileName);

        /// <summary>
        /// Чтение файла с google-диска по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла на google-диске.</param>
        /// <returns>Байты считанного файла.</returns>
        byte[] ReadFile(string fileId);

        /// <summary>
        /// Выгрузка файла на google-диск.
        /// </summary>
        /// <param name="fileId">Идентификатор файла на google-диске (если известен).</param>
        /// <param name="file">Путь к выгружаемому файлу.</param>
        /// <returns>Идентификатор файла на google-диске.</returns>
        string UploadFile(string fileId, string file);
    }
}