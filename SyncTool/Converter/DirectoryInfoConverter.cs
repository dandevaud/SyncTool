using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SyncTool.Converter
{
    class DirectoryInfoConverter: JsonConverter<DirectoryInfo>
    {
        public override DirectoryInfo Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) =>
                new DirectoryInfo(reader.GetString());
    

        public override void Write(
            Utf8JsonWriter writer,
            DirectoryInfo directoryInfo,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(directoryInfo.FullName);

    }
   
}
