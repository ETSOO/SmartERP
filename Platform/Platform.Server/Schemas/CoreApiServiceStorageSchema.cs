using Json.Schema;

namespace Platform.Server.Schemas
{
    /// <summary>
    /// Storage API schema
    /// 存储接口模式
    /// </summary>
    /// <seealso cref="PlatformShared.Extentions.ApiOptions.StorageApiOptions"/>
    public static class CoreApiServiceStorageSchema
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
                    ("provider", new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .Enum("local", "s3", "minio")
                    ),
                    ("root", new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                    ),
                    ("urlRoot", new JsonSchemaBuilder()
                        .Type(SchemaValueType.String)
                        .Format("url")
                    )
                )
                .Required("root", "urlRoot")
            ;

            return builder.Build();
        }
    }
}