/*
 * Copyright (c) 2024 PlayEveryWare
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace PlayEveryWare.EpicOnlineServices
{
    // This compile conditional is here so that when EOS is disabled, nothing is
    // referenced in the Epic namespace.
#if !EOS_DISABLE
    using Epic.OnlineServices.Auth;
    using Epic.OnlineServices.IntegratedPlatform;
    using Epic.OnlineServices.Platform;
    using Epic.OnlineServices.UI;
    using Extensions;
#endif

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Utility;

    // This compile conditional is here so that when EOS is disabled, nothing is
    // referenced in the Epic namespace.
#if !EOS_DISABLE

    /// <summary>
    /// Used to allow conversion from either a list of strings, or an enum's
    /// "ToString()" function of strings to a single enum value.
    /// </summary>
    internal class
        ListOfStringsToIntegratedPlatformManagementFlags : ListOfStringsToEnumConverter<
            IntegratedPlatformManagementFlags>
    {
        protected override IntegratedPlatformManagementFlags FromStringArray(JArray array)
        {
            return FromStringArrayWithCustomMapping(array, IntegratedPlatformManagementFlagsExtensions.CustomMappings);
        }
    }

    /// <summary>
    /// Used to allow conversion from either a list of strings, or an enum's
    /// "ToString()" function of strings to a single enum value.
    /// </summary>
    internal class ListOfStringsToInputStateButtonFlags : ListOfStringsToEnumConverter<InputStateButtonFlags>
    {
        protected override InputStateButtonFlags FromStringArray(JArray array)
        {
            return FromStringArrayWithCustomMapping(array, null);
        }
    }

    /// <summary>
    /// Used to allow conversion from either a list of strings, or an enum's
    /// "ToString()" function of strings to a single enum value.
    /// </summary>
    internal class ListOfStringsToAuthScopeFlags : ListOfStringsToEnumConverter<AuthScopeFlags>
    {
        protected override AuthScopeFlags FromStringArray(JArray array)
        {
            return FromStringArrayWithCustomMapping(array, AuthScopeFlagsExtensions.CustomMappings);
        }
    }

    /// <summary>
    /// Used to allow conversion from either a list of strings, or an enum's
    /// "ToString()" function of strings to a single enum value.
    /// </summary>
    internal class ListOfStringsToPlatformFlags : ListOfStringsToEnumConverter<WrappedPlatformFlags>
    {
        protected override WrappedPlatformFlags FromStringArray(JArray array)
        {
            Dictionary<string, WrappedPlatformFlags> wrappedCustomMapping = new();
            
            foreach (var customMapping in PlatformFlagsExtensions.CustomMappings)
            {
                string key = customMapping.Key;
                PlatformFlags value = customMapping.Value;
                wrappedCustomMapping[key] = value.Wrap();
            }

            return FromStringArrayWithCustomMapping(array, wrappedCustomMapping);
        }
    }
#endif

    /// <summary>
    /// This class provides base functionality and declares an interface for
    /// converter classes that can accept either a JSON array of strings, a
    /// single value, or a single string into an enum value of the type
    /// specified by the type parameter.
    /// </summary>
    /// <typeparam name="TEnum">
    /// Indicates the enum type that conversion functionality is being
    /// implemented for.
    /// </typeparam>
    internal abstract class ListOfStringsToEnumConverter<TEnum> : JsonConverter where TEnum : struct, Enum
    {
        /// <summary>
        /// Used as a cleaner proxy for typeof(T)
        /// </summary>
        private readonly Type _targetType = typeof(TEnum);

        /// <summary>
        /// Determines whether the type that the implementing class attribute is
        /// applied to can be converted by this JsonConverter.
        /// </summary>
        /// <param name="objectType">
        /// The object to convert _to_.
        /// </param>
        /// <returns>
        /// True if the conversion is supported, false otherwise.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return typeof(TEnum).IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            return token.Type switch
            {
                JTokenType.Array => FromStringArray(token as JArray),

                JTokenType.String => Enum.Parse(
                    _targetType,
                    token.Value<string>()),

                JTokenType.Integer => FromNumberValue(token),

                JTokenType.Null => default(TEnum),

                _ => throw new JsonSerializationException(
                    $"Unexpected token type '{token.Type}' when " +
                    $"parsing to type '{_targetType}'."),
            };
        }

        /// <summary>
        /// Implementing classes should override this function to define the
        /// behavior when an array of strings is being converted into a single
        /// enum value.
        /// </summary>
        /// <param name="array">
        /// A list of string values from which to compose a single enum value.
        /// </param>
        /// <returns>A single enum value of type T.</returns>
        protected abstract TEnum FromStringArray(JArray array);

        /// <summary>
        /// Implementing classes should override this function to define the
        /// behavior when a single numeric value is given for the enum value.
        /// This is important for each implementing class to define because
        /// each enum may have a different underlying numeric type.
        /// </summary>
        /// <param name="token">
        /// The token containing the number value.
        /// </param>
        /// <returns>
        /// A single enum value of type T.
        /// </returns>
        protected TEnum FromNumberValue(JToken token)
        {
            Type underlyingType = Enum.GetUnderlyingType(typeof(TEnum));
            object value = Convert.ChangeType(token, underlyingType);

            // Get the lowest and highest values
            TEnum lowestEnumValue = EnumUtility<TEnum>.GetLowest();
            TEnum highestEnumValue = EnumUtility<TEnum>.GetHighest();

            object lowestUnderlyingValue = Convert.ChangeType(lowestEnumValue, underlyingType);
            object highestUnderlyingValue = Convert.ChangeType(highestEnumValue, underlyingType);

            TEnum finalEnumValue;

            // Ensure value is within range
            if (Comparer<object>.Default.Compare(value, lowestUnderlyingValue) < 0 || Comparer<object>.Default.Compare(value, highestUnderlyingValue) > 0)
            {
                UnityEngine.Debug.LogWarning($"Value {value} is out of range for {nameof(TEnum)}, setting to 0.");
                finalEnumValue = (TEnum)Enum.ToObject(typeof(TEnum), 0);
            }
            else
            {
                finalEnumValue = (TEnum)value;
            }

            return finalEnumValue;
        }

        /// <summary>
        /// Given a JSON array and a custom mapping lookup dictionary, this
        /// function will combose a single enum value based on the values
        /// provided.
        /// </summary>
        /// <param name="array">
        /// A JSON array of string values.
        /// </param>
        /// <param name="customMappings">
        /// Any custom mappings that should be used to parse the enum value. If
        /// there are no such custom mappings, this value can be null.
        /// </param>
        /// <returns>
        /// A single enum value of type T.
        /// </returns>
        protected TEnum FromStringArrayWithCustomMapping(JArray array, IDictionary<string, TEnum> customMappings)
        {
            List<string> elementsAsStrings = new();
            foreach (JToken element in array)
            {
                elementsAsStrings.Add(element.ToString());
            }

            _ = EnumUtility<TEnum>.TryParse(elementsAsStrings, customMappings, out TEnum result);

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Serialize the value using the default serializer
            serializer.Serialize(writer, value);
        }
    }
}