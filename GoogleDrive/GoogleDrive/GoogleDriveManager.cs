using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.IO;
using System.Linq;
using System.Threading;

namespace GoogleDrive
{
    /// <summary>
    /// Класс для работы с google-диском.
    /// </summary>
    public class GoogleDriveManager : IGoogleDriveManager
    {
        private static readonly string[] _scopes = new string[] { DriveService.Scope.Drive, DriveService.Scope.DriveFile };

        private readonly DriveService _service;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="userName">Наименование учётной записи для авторизации приложения в google.</param>
        /// <param name="clientSecretFile">Путь к файлу с ClientId и ClientSecret для учётной записи приложения в google.</param>
        public GoogleDriveManager(string userName, string clientSecretFile)
        {
            GoogleClientSecrets googleClientSecrets;
            using (var stream = File.OpenRead(clientSecretFile))
                googleClientSecrets = GoogleClientSecrets.Load(stream);

            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                googleClientSecrets.Secrets, _scopes, userName, CancellationToken.None).Result;

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "GoogleDrive",
            });
        }

        /// <summary>
        /// Выгрузка файла на google-диск.
        /// </summary>
        /// <param name="fileId">Идентификатор файла на google-диске (если известен).</param>
        /// <param name="file">Путь к выгружаемому файлу.</param>
        /// <returns>Идентификатор файла на google-диске.</returns>
        public string UploadFile(string fileId, string file)
        {
            var body = new Google.Apis.Drive.v3.Data.File();
            body.Name = Path.GetFileName(file);
            body.MimeType = GetMimeType(file);

            using (var stream = File.OpenRead(file))
                if (fileId == null)
                {
                    var request = _service.Files.Create(body, stream, body.MimeType);
                    var result = request.Upload();
                    if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
                        return request.ResponseBody.Id;
                    else
                        throw result.Exception;
                }
                else
                {
                    var request = _service.Files.Update(body, fileId, stream, body.MimeType);
                    var result = request.Upload();
                    if (result.Status == Google.Apis.Upload.UploadStatus.Completed)
                        return fileId;
                    else
                        throw result.Exception;
                }
        }

        /// <summary>
        /// Получение идентификатора файла на google-диске по его наименованию.
        /// </summary>
        /// <param name="fileName">Наименованвие файла.</param>
        /// <returns>Идентификатор файла на google-диске.</returns>
        public string GetFileId(string fileName)
        {
            var list = _service.Files.List().Execute();

            var file = list.Files.SingleOrDefault(f => f.Name == fileName);
            if (file == null)
                return null;

            return file.Id;
        }

        /// <summary>
        /// Чтение файла с google-диска по его идентификатору.
        /// </summary>
        /// <param name="fileId">Идентификатор файла на google-диске.</param>
        /// <returns>Байты считанного файла.</returns>
        public byte[] ReadFile(string fileId)
        {
            var request = _service.Files.Get(fileId);
            using (var memoryStream = new MemoryStream())
            {
                request.Download(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = Path.GetExtension(fileName).ToLower();
            var regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }
    }
}