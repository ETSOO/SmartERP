using com.etsoo.CoreFramework.Application;
using com.etsoo.Utils.Actions;

namespace Platform.Server.Endpoints.Org.RQ
{
    /// <summary>
    /// Organization query request data
    /// 机构查询请求数据
    /// </summary>
    public record OrgQueryRQ : QueryIntRQ
    {
        public string? Pin { get; init; }

        public override IActionResult? Validate()
        {
            var result = base.Validate();
            if (result != null)
            {
                return result;
            }

            if (Pin != null && Pin.Length > 20)
            {
                return ApplicationErrors.NoValidData.AsResult(nameof(Pin));
            }

            return null;
        }
    }
}
