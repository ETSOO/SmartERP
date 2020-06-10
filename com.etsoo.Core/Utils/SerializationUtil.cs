using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace com.etsoo.Core.Utils
{
    /// <summary>
    /// Serialization tools
    /// 序列化工具类
    /// </summary>
    public static class SerializationUtil
    {
        /// <summary>
        /// Create Json serializer encoder to support Unicode like Chinese output without encoded
        /// 创建Json序列化编码器支持输出Unicode字符，比如中文，避免被编码
        /// </summary>
        /// <returns>Encoder</returns>
        public static JavaScriptEncoder CreateJsonSerializerEncoder()
        {
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRanges(UnicodeRanges.All);

            return JavaScriptEncoder.Create(encoderSettings);
        }
    }
}