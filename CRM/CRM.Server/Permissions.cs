namespace CRM.Server.Permissions
{
    /// <summary>
    /// Organization permissions (1) / 机构权限
    /// </summary>
    public enum Org : short
    {
        All = 1000,

        List = 1001,
        Query = 1002,
        View = 1003,

        AddContact = 1083,

        QueryProfile = 1091,
        ViewProfile = 1092,
        AddProfile = 1093,
        AddComment = 1097,

        UpdateSettings = 1201
    }

    /// <summary>
    /// Department permissions (2) / 部门权限
    /// </summary>
    public enum Dept : short
    {
        All = 2000,

        List = 2001,
        Query = 2002,
        View = 2003,
        Add = 2011,
        Edit = 2012,
        Delete = 2013,

        AddContact = 2083,

        QueryProfile = 2091,
        ViewProfile = 2092,
        AddProfile = 2093,
        AddComment = 2097
    }

    /// <summary>
    /// User permissions (3) / 用户权限
    /// </summary>
    public enum User : short
    {
        All = 3000,

        List = 3001,
        Query = 3002,
        View = 3003,
        Add = 3011,
        Edit = 3012,
        Delete = 3013,

        AddContact = 3083,

        QueryProfile = 3091,
        ViewProfile = 3092,
        AddProfile = 3093,
        AddComment = 3097
    }

    /// <summary>
    /// Customer permissions (6) / 客户权限
    /// </summary>
    public enum Customer : short
    {
        All = 6000,

        List = 6001,
        Query = 6002,
        View = 6003,
        Add = 6011,
        Edit = 6012,
        Delete = 6013,

        AddContact = 6083,

        QueryProfile = 6091,
        ViewProfile = 6092,
        AddProfile = 6093,
        AddComment = 6097
    }

    /// <summary>
    /// Supplier permissions (7) / 供应商权限
    /// </summary>
    public enum Supplier : short
    {
        All = 7000,

        List = 7001,
        Query = 7002,
        View = 7003,
        Add = 7011,
        Edit = 7012,
        Delete = 7013,

        AddContact = 7083,

        QueryProfile = 7091,
        ViewProfile = 7092,
        AddProfile = 7093,
        AddComment = 7097
    }

    /// <summary>
    /// Product permissions (8) / 产品权限
    /// </summary>
    public enum Product : short
    {
        All = 8000,

        List = 8001,
        Query = 8002,
        View = 8003,
        Add = 8011,
        Edit = 8012,
        Delete = 8013
    }

    /// <summary>
    /// Order permissions (9) / 订单权限
    /// </summary>
    public enum Order : short
    {
        All = 9000,

        List = 9001,
        Query = 9002,
        View = 9003,
        Add = 9011,
        Edit = 9012,
        Delete = 9013
    }

    /// <summary>
    /// Purchase order permissions (10) / 采购权限
    /// </summary>
    public enum PO : short
    {
        All = 10000,

        List = 10001,
        Query = 10002,
        View = 10003,
        Add = 10011,
        Edit = 10012,
        Delete = 10013
    }

    /// <summary>
    /// Inventory permissions (11) / 库存权限
    /// </summary>
    public enum Inventory : short
    {
        All = 11000,

        Query = 11002
    }

    /// <summary>
    /// Finance permissions (18) / 财务权限
    /// </summary>
    public enum Finance : short
    {
        All = 18000,

        Query = 18002
    }
}