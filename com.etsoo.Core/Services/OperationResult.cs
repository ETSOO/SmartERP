using com.etsoo.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace com.etsoo.Core.Services
{
    /// <summary>
    /// Database operation result
    /// 数据库操作结果
    /// </summary>
    public class OperationResult
    {
        private int errorCode = 0;

        /// <summary>
        /// Error code
        /// 错误编号
        /// </summary>
        public int ErrorCode
        {
            get { return errorCode; }
            set
            {
                errorCode = value;
                this.OK = (errorCode == 0);
            }
        }

        /// <summary>
        /// Is OK status
        /// 是否为正常状态
        /// </summary>
        public bool OK { get; private set; }

        /// <summary>
        /// Status field
        /// 状态字段
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Status message
        /// 状态信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Status message id
        /// 错误信息编号
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// Status message id alias
        /// 错误信息编号别名
        /// </summary>
        public string Mid
        {
            get { return this.MessageId; }
            set { this.MessageId = value; }
        }

        /// <summary>
        /// 操作结果关联的数据
        /// </summary>
        /// <value>只读</value>
        public StringKeyDictionary<dynamic> Data { get; private set; }

        public KeySortedList Errors { get; private set; }

        /// <summary>
        /// Constructor
        /// 构造函数
        /// </summary>
        public OperationResult()
        {
            OK = true;
            Data = new StringKeyDictionary<dynamic>();
            Errors = new KeySortedList();
        }

        /// <summary>
        /// Set error
        /// 设置错误
        /// </summary>
        /// <param name="errorCode">Error code</param>
        /// <param name="field">Field</param>
        /// <param name="message">Status message</param>
        /// <param name="messageId">Message id</param>
        public void SetError(int errorCode, string field, string message, string messageId = null)
        {
            if (errorCode == 0)
            {
                // errorCode == 0 will make the status to be OK
                throw new ArgumentException("Invalid to set the status to be OK!", nameof(errorCode));
            }

            ErrorCode = errorCode;
            Field = field;
            Message = message;
            MessageId = messageId;
        }

        private Utf8JsonWriter CreateJsonWriter(Stream stream)
        {
            return new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = false, SkipValidation = true, Encoder = SerializationUtil.CreateJsonSerializerEncoder() });
        }

        private XmlWriter CreateXmlWriter(Stream stream, bool asyncSupport = false)
        {
            return XmlWriter.Create(stream, new XmlWriterSettings() { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = false, Async = asyncSupport });
        }

        private ReadOnlySpan<byte> SerializeJsonObject(object @object)
        {
            return new ReadOnlySpan<byte>(JsonSerializer.SerializeToUtf8Bytes(@object, new JsonSerializerOptions { IgnoreNullValues = true, Encoder = SerializationUtil.CreateJsonSerializerEncoder() }));
        }

        /// <summary>
        /// Serialize to target format to stream
        /// 序列化为目标格式，写入流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="format">Data format</param>
        public void Serialize(Stream stream, DataFormat format)
        {
            if (format == DataFormat.Xml)
            {
                using (var writer = CreateXmlWriter(stream))
                {
                    // Write <root> to start
                    writer.WriteStartElement("root");

                    // Error code
                    writer.WriteElementString("error_code", ErrorCode.ToString());

                    // Field
                    if (!string.IsNullOrEmpty(Field))
                        writer.WriteElementString("field", Field);

                    // Message id
                    if (!string.IsNullOrEmpty(MessageId))
                        writer.WriteElementString("mid", MessageId);

                    // Message
                    if (!string.IsNullOrEmpty(Message))
                    {
                        writer.WriteStartElement("message");
                        writer.WriteCData(Message);
                        writer.WriteEndElement();
                    }

                    // Data
                    if (Data.Count > 0)
                    {
                        writer.WriteStartElement("data");

                        foreach (var item in Data)
                        {
                            if (item.Value == null)
                                continue;

                            writer.WriteStartElement(item.Key);
                            writer.WriteValue(item.Value);
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    // Errors
                    if (Errors.Count > 0)
                    {
                        writer.WriteStartElement("errors");

                        foreach (var item in Errors)
                        {
                            writer.WriteStartElement(item.Key);

                            foreach (var itemValue in item.Value)
                            {
                                writer.WriteElementString("item", itemValue);
                            }

                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }

                    //  Write </root> to end
                    writer.WriteEndElement();

                    // Flush
                    writer.Flush();
                }
            }
            else
            {
                using (var writer = CreateJsonWriter(stream))
                {
                    // Write { to start
                    writer.WriteStartObject();

                    // Error code
                    writer.WriteNumber("error_code", ErrorCode);

                    // Field
                    if (!string.IsNullOrEmpty(Field))
                        writer.WriteString("field", Field);

                    // Message id
                    if (!string.IsNullOrEmpty(MessageId))
                        writer.WriteString("mid", MessageId);

                    // Message
                    if (!string.IsNullOrEmpty(Message))
                    {
                        writer.WriteString("message", Message);
                    }

                    // Data
                    if (Data.Count > 0)
                    {
                        // SkipValidation = true to avoid error, no raw json write method yet
                        // SkipValidation = true 设置避免写入错误，暂时还没有原始JSON写入方式
                        writer.WritePropertyName("data");
                        writer.Flush();
                        stream.Write(SerializeJsonObject(Data));
                    }

                    // Errors
                    if (Errors.Count > 0)
                    {
                        writer.WritePropertyName("errors");
                        writer.Flush();
                        stream.Write(SerializeJsonObject(Errors));
                    }

                    // Write } to end
                    writer.WriteEndObject();

                    // Flush
                    writer.Flush();
                }
            }
        }

        /// <summary>
        /// Async serialize to target format to stream
        /// 异步序列化为目标格式，写入流
        /// </summary>
        /// <param name="stream">Stream to write</param>
        /// <param name="format">Data format</param>
        public async Task SerializeAsync(Stream stream, DataFormat format)
        {
            if (format == DataFormat.Xml)
            {
                using (var writer = CreateXmlWriter(stream, true))
                {
                    // Write <root> to start
                    await writer.WriteStartElementAsync(null, "root", null);

                    // Error code
                    await writer.WriteElementStringAsync(null, "error_code", null, ErrorCode.ToString());

                    // Field
                    if (!string.IsNullOrEmpty(Field))
                        await writer.WriteElementStringAsync(null, "field", null, Field);

                    // Message id
                    if (!string.IsNullOrEmpty(MessageId))
                        await writer.WriteElementStringAsync(null, "mid", null, MessageId);

                    // Message
                    if (!string.IsNullOrEmpty(Message))
                    {
                        await writer.WriteStartElementAsync(null, "message", null);
                        await writer.WriteCDataAsync(Message);
                        await writer.WriteEndElementAsync();
                    }

                    // Data
                    if (Data.Count > 0)
                    {
                        await writer.WriteStartElementAsync(null, "data", null);

                        foreach (var item in Data)
                        {
                            if (item.Value == null)
                                continue;

                            await writer.WriteStartElementAsync(null, item.Key, null);
                            writer.WriteValue(item.Value);
                            await writer.WriteEndElementAsync();
                        }

                        await writer.WriteEndElementAsync();
                    }

                    // Errors
                    if (Errors.Count > 0)
                    {
                        await writer.WriteStartElementAsync(null, "errors", null);

                        foreach (var item in Errors)
                        {
                            await writer.WriteStartElementAsync(null, item.Key, null);

                            foreach (var itemValue in item.Value)
                            {
                                await writer.WriteElementStringAsync(null, "item", null, itemValue);
                            }

                            await writer.WriteEndElementAsync();
                        }

                        await writer.WriteEndElementAsync();
                    }

                    //  Write </root> to end
                    await writer.WriteEndElementAsync();

                    // Flush
                    await writer.FlushAsync();
                }
            }
            else
            {
                using (var writer = CreateJsonWriter(stream))
                {
                    // Write { to start
                    writer.WriteStartObject();

                    // Error code
                    writer.WriteNumber("error_code", ErrorCode);

                    // Field
                    if (!string.IsNullOrEmpty(Field))
                        writer.WriteString("field", Field);

                    // Message id
                    if (!string.IsNullOrEmpty(MessageId))
                        writer.WriteString("mid", MessageId);

                    // Message
                    if (!string.IsNullOrEmpty(Message))
                    {
                        writer.WriteString("message", Message);
                    }

                    // Data
                    if (Data.Count > 0)
                    {
                        // SkipValidation = true to avoid error, no raw json write method yet
                        // SkipValidation = true 设置避免写入错误，暂时还没有原始JSON写入方式
                        writer.WritePropertyName("data");
                        await writer.FlushAsync();
                        stream.Write(SerializeJsonObject(Data));
                    }

                    // Errors
                    if (Errors.Count > 0)
                    {
                        writer.WritePropertyName("errors");
                        await writer.FlushAsync();
                        stream.Write(SerializeJsonObject(Errors));
                    }

                    // Write } to end
                    writer.WriteEndObject();

                    // Flush
                    await writer.FlushAsync();
                }
            }
        }

        /// <summary>
        /// Update operation result with a new one
        /// 用另一个操作结果更新当前结果
        /// </summary>
        /// <param name="newResult">New operation result</param>
        /// <param name="mergeData">Is merge data, default is False to avoid any possible sensitive data leak</param>
        /// <returns>Is OK result</returns>
        public bool Update(OperationResult newResult, bool mergeData = false)
        {
            // Update value
            ErrorCode = newResult.ErrorCode;
            Field = newResult.Field;
            MessageId = newResult.MessageId;
            Message = newResult.Message;

            // Is merge data?
            if (mergeData)
            {
                foreach (var item in newResult.Data)
                {
                    Data[item.Key] = item.Value;
                }

                foreach (var item in newResult.Errors)
                {
                    Errors.Add(item.Key, item.Value);
                }
            }
            else
            {
                Data = newResult.Data;
                Errors = newResult.Errors;
            }

            return newResult.OK;
        }
    }
}