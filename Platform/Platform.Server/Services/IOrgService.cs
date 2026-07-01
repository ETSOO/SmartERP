using com.etsoo.ApiModel.Dto.SmartERP.MessageQueue;
using com.etsoo.CoreFramework.Authentication;
using com.etsoo.Utils.Actions;
using Json.Schema;
using Platform.Server.Dto.Org;
using Platform.Server.Endpoints.Org.RQ;
using PlatformShared.Database.Models;
using PlatformShared.Dto;
using System.Buffers;

namespace Platform.Server.Services
{
    public interface IOrgService
    {
        ValueTask<IActionResult> FormatRQAsync(IOrgRQ rq, UserRole role, CancellationToken cancellationToken);
        Task<IActionResult> CreateAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> CreateApiAsync(OrgCreateApiRQ rq, CancellationToken cancellationToken = default);
        ValueTask<IActionResult> CreateResourceAsync(OrgCreateResourceRQ rq, CancellationToken cancellationToken = default);
        Task<(IActionResult result, int? id)> CreateWithIdAsync(OrgCreateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<IResult> DownloadFileAsync(OrgDownloadKind kind, long id, CancellationToken cancellationToken = default);
        Task<string?> FormatHtmlContentAsync(string content, CancellationToken cancellationToken = default);
        Task<CustomResourceData[]> GetCustomResourcesAsync(string culture, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrgGetMyData>> GetMyAsync(OrgGetMyRQ rq, CancellationToken cancellationToken = default);
        Task GetMyAsync(OrgGetMyRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> LeaveAsync(int id, CancellationToken cancellationToken = default);
        Task ListAsync(OrgListRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<bool> OwnsAsync(OrgOwnsRQ rq, CancellationToken cancellationToken = default);
        Task<bool> OwnsAsync(int id, UserRole userRole, CancellationToken cancellationToken = default);
        Task<IEnumerable<OrgQueryData>> QueryAsync(OrgQueryRQ rq, CancellationToken cancellationToken = default);
        Task QueryAsync(OrgQueryRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryApiAsync(OrgQueryApiRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task QueryResourceAsync(OrgQueryResourceRQ rq, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task ReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        JsonSchema? ReadApiSchema(CoreApiService service);
        Task<IActionResult> SendEmailAsync(SendEmailMessage message, CancellationToken cancellationToken = default);
        Task<IActionResult> SendSMSAsync(SendSMSMessage message, CancellationToken cancellationToken = default);
        Task<IActionResult> SendProfileEmailAsync(SendProfileEmailRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAsync(OrgUpdateRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateApiAsync(OrgUpdateApiRQ rq, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateAvatarAsync(int id, Stream avatarStream, string contentType, CancellationToken cancellationToken = default);
        Task<IActionResult> UpdateCompanySealAsync(int id, Stream stream, string contentType, CancellationToken cancellationToken = default);
        Task UpdateReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task UpdateApiReadAsync(int id, IBufferWriter<byte> writer, CancellationToken cancellationToken = default);
        Task<IActionResult> UploadFilesAsync(IEnumerable<IFormFile> files, long id, string folder, string sign, CancellationToken cancellationToken = default);
        Task<IActionResult> UploadProfileFilesAsync(long id, IEnumerable<IFormFile> files, string action, CancellationToken cancellationToken = default);
        Task<OrgUpdateResourceReadData?> UpdateResourceReadAsync(int id, CancellationToken cancellationToken = default);
    }
}