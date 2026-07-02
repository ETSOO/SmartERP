using com.etsoo.CoreFramework.User;
using com.etsoo.Utils;
using com.etsoo.WebUtils;
using Microsoft.EntityFrameworkCore;
using Platform.Server.Application;
using PlatformShared.Database;
using System.Net;
using System.Text;

namespace Platform.Server.Services
{
    public class StorageService : CommonService, IStorageService
    {
        readonly MyDbContext _db;
        readonly IHttpContextAccessor _accessor;
        readonly IStorageFactory _storageFactory;

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        /// <param name="db">Database EF</param>
        /// <param name="app">Application</param>
        /// <param name="userAccessor">User accessor</param>
        /// <param name="logger">Logger</param>
        /// <param name="accessor">HttpContext accessor</param>
        /// <param name="storageFactory">Storage factory</param>
        public StorageService(MyDbContext db,
            IMyApp app,
            CurrentUserAccessor userAccessor,
            ILogger<StorageService> logger,
            IHttpContextAccessor accessor,
            IStorageFactory storageFactory)
            : base(app, userAccessor.User, "storage", logger)
        {
            _db = db;
            _accessor = accessor;
            _storageFactory = storageFactory;
        }

        /// <summary>
        /// Download file
        /// 下载文件
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="orgId">Organization id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task DownloadFileAsync(string path, int? orgId, CancellationToken cancellationToken = default)
        {
            // Validate path to avoid directory traversal attack
            if (path.Contains("..") || path.Contains("%2e%2e") || path.Contains("%252e%252e"))
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "Invalid path");
                return;
            }

            // Storage
            var storage = await _storageFactory.CreateAsync(orgId, cancellationToken);

            await using var stream = await storage.ReadAsync(path, cancellationToken);
            if (stream != null && _accessor.HttpContext != null)
                await stream.CopyToAsync(_accessor.HttpContext.Response.Body, cancellationToken);
        }

        /// <summary>
        /// Download organization file
        /// 下载机构文件
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task DownloadOrgFileAsync(string path, CancellationToken cancellationToken = default)
        {
            var orgId = _storageFactory.GetOrgIdFromPath(path);
            if (orgId < 1)
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "Invalid org path");
                return;
            }

            await DownloadFileAsync("Orgs/" + path, orgId, cancellationToken);
        }

        /// <summary>
        /// Download user file
        /// 下载用户文件
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="userId">User id</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task DownloadUserFileAsync(string path, string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId) || userId.Equals("U" + User?.Id))
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "Invalid user path");
                return;
            }

            await DownloadFileAsync(path, null, cancellationToken);
        }

        /// <summary>
        /// Editor styles
        /// 编辑器样式
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async ValueTask EditorStylesAsync(CancellationToken cancellationToken = default)
        {
            var response = _accessor.HttpContext?.Response;
            if (response == null)
                return;

            response.ContentType = "text/css";
            response.Headers.Append("Cache-Control", "public, max-age=604800");

            await response.BodyWriter.WriteAsync(Encoding.UTF8.GetBytes("""
                body {
                  box-sizing: border-box;
                }
                img { 
                  max-width: 100%;
                }
                pre {
                  background-color: #f3f3f3;
                  padding: 12px;
                }
                """), cancellationToken);
        }

        /// <summary>
        /// Download profile attachment
        /// 下载档案附件
        /// </summary>
        /// <param name="id">Profile id</param>
        /// <param name="timestamp">Timestamp</param>
        /// <param name="key">Access key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ProfileAttachmentAsync(long id, long timestamp, string key, CancellationToken cancellationToken = default)
        {
            // Validate timestamp
            var expiry = SharedUtils.JsMilisecondsToUTC(timestamp);
            if (expiry < DateTime.UtcNow)
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "Expired");
                return;
            }

            // Validate key
            var keyExpected = await App.HashPasswordAsync(timestamp.ToString() + id.ToString());

            if (key != keyExpected)
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "Invalid key");
                return;
            }

            // Check id
            var profile = await _db.PersonProfileAttachments
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new { a.Profile.Person.OrgId, a.FileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (profile == null)
            {
                await _accessor.SetStatusCodeAsync(HttpStatusCode.BadRequest, "No ID");
                return;
            }

            await DownloadFileAsync(profile.FileName, profile.OrgId, cancellationToken);
        }
    }
}
