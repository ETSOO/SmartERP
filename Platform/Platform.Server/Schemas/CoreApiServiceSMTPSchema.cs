using Json.Schema;

namespace Platform.Server.Schemas
{
    /// <summary>
    /// SMTP API schema
    /// 邮件接口模式
    /// </summary>
    /// <seealso cref="PlatformShared.Extentions.ApiOptions.SMTPApiOptions"/>
    public static class CoreApiServiceSMTPSchema
    {
        /// <summary>
        /// Create the schema
        /// 创建模式
        /// </summary>
        /// <returns>Schema</returns>
        public static JsonSchema Create()
        {
            var builder = new JsonSchemaBuilder()
                .Type(SchemaValueType.Object)
                .Properties(
                    ("cc", new JsonSchemaBuilder()
                        .Type(SchemaValueType.Array)
                        .Items(
                            new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Format("email")
                        )
                        .UniqueItems(true)
                    ),
                    ("bcc", new JsonSchemaBuilder()
                        .Type(SchemaValueType.Array)
                        .Items(
                            new JsonSchemaBuilder()
                                .Type(SchemaValueType.String)
                                .Format("email")
                        )
                        .UniqueItems(true)
                    )
                )
            ;

            return builder.Build();
        }
    }
}