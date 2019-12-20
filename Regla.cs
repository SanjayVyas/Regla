/**
 *-----------------------------------------------------------------------------
 * File:      Regla.cs
 * Project:   Regla
 * Author:    Sanjay Vyas
 *
 * This module contains helper functions for JSon output
 *-----------------------------------------------------------------------------
 * Revision History
 *   [SV] 2019-Dec-19 1.21: Created
 *-----------------------------------------------------------------------------
 */

using System;
using System.Text.Unicode;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;

namespace Regla
{
    static class ReglaHelper
    {
        /**
         * A common Json converter for Regla objects
         */
        public static string ToJson(object o)
        {
            var options = new JsonSerializerOptions();

            // We need pretty print (indented)
            options.WriteIndented = true;

            // Convert lambda names from \u300CBasic\u300E to <Basic>
            options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

            // There seems to be limitation of JsonSerializer, which we need to work around
            options.Converters.Add(new RuleMethodConverter());
            options.Converters.Add(new ExceptionMethodConverter());

            // Return the given object in Json format
            return JsonSerializer.Serialize(o, options);
        }
    }
    /**
     * JsonSerializer throws an exception if it comes across a delegate or a method
     * We just want the name of the method and not the entire method serialized (not possible anyways)
     * So we write a JsonCoverter, which takes the name of the method and writes it in the output
     */
    public class RuleMethodConverter : JsonConverter<Func<object, object, bool>>
    {
        // We are not interested in deserialization
        public override Func<object, object, bool> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => null;

        // Write out just the name of the method
        public override void Write(
                Utf8JsonWriter writer,
                Func<object, object, bool> value,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue(value.Method.Name);
    }

    /**
     * Exception is an object and JsonSerializer should serialize it
     * But it thrown an exception if Exception is a nested property
     * Anyways, we just need the Exception message
     */
    public class ExceptionMethodConverter : JsonConverter<System.Exception>
    {
        // We are not interested in deserialization
        public override System.Exception Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options) => null;

        // Write out just the Message of the exception
        public override void Write(
                Utf8JsonWriter writer,
                System.Exception value,
                JsonSerializerOptions options) =>
                    writer.WriteStringValue(value.Message);
    }

    public class ReglaAttributes { }

}