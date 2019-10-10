﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// TODO: Use a source package once available (https://github.com/dotnet/sourcelink/issues/443)

namespace Microsoft.SourceLink.Tools
{
    internal sealed class SourceLinkMap
    {
        private readonly List<(FilePathPattern key, UriPattern value)> _entries;

        public SourceLinkMap(List<(FilePathPattern key, UriPattern value)> entries)
        {
            Debug.Assert(entries != null);
            _entries = entries;
        }

        internal struct FilePathPattern
        {
            public readonly string Path;
            public readonly bool IsPrefix;

            public FilePathPattern(string path, bool isPrefix)
            {
                Debug.Assert(path != null);

                Path = path;
                IsPrefix = isPrefix;
            }
        }

        internal struct UriPattern
        {
            public readonly string Prefix;
            public readonly string Suffix;

            public UriPattern(string prefix, string suffix)
            {
                Debug.Assert(prefix != null);
                Debug.Assert(suffix != null);

                Prefix = prefix;
                Suffix = suffix;
            }
        }

        internal static SourceLinkMap Parse(string json, Action<string> reportDiagnostic)
        {
            bool errorReported = false;

            void ReportInvalidJsonDataOnce(string message)
            {
                if (!errorReported)
                {
                    // Bad source link format
                    reportDiagnostic($"The JSON format is invalid: {message}");
                }

                errorReported = true;
            }

            var list = new List<(FilePathPattern key, UriPattern value)>();
            try
            {
                // trim BOM if present:
                var root = JObject.Parse(json.TrimStart('\uFEFF'));
                var documents = root["documents"];

                if (documents.Type != JTokenType.Object)
                {
                    ReportInvalidJsonDataOnce($"expected object: {documents}");
                    return null;
                }

                foreach (var token in documents)
                {
                    if (!(token is JProperty property))
                    {
                        ReportInvalidJsonDataOnce($"expected property: {token}");
                        continue;
                    }

                    string value = (property.Value.Type == JTokenType.String) ?
                        property.Value.Value<string>() : null;

                    if (value == null ||
                        !TryParseEntry(property.Name, value, out var path, out var uri))
                    {
                        ReportInvalidJsonDataOnce($"invalid mapping: '{property.Name}': '{value}'");
                        continue;
                    }

                    list.Add((path, uri));
                }
            }
            catch (JsonReaderException e)
            {
                reportDiagnostic(e.Message);
                return null;
            }

            // Sort the map by decreasing file path length. This ensures that the most specific paths will checked before the least specific
            // and that absolute paths will be checked before a wildcard path with a matching base
            list.Sort((left, right) => -left.key.Path.Length.CompareTo(right.key.Path.Length));

            return new SourceLinkMap(list);
        }

        private static bool TryParseEntry(string key, string value, out FilePathPattern path, out UriPattern uri)
        {
            path = default;
            uri = default;

            // VALIDATION RULES
            // 1. The only acceptable wildcard is one and only one '*', which if present will be replaced by a relative path
            // 2. If the filepath does not contain a *, the uri cannot contain a * and if the filepath contains a * the uri must contain a *
            // 3. If the filepath contains a *, it must be the final character
            // 4. If the uri contains a *, it may be anywhere in the uri

            int filePathStar = key.IndexOf('*');
            if (filePathStar == key.Length - 1)
            {
                key = key.Substring(0, filePathStar);

                if (key.IndexOf('*') >= 0)
                {
                    return false;
                }
            }
            else if (filePathStar >= 0 || key.Length == 0)
            {
                return false;
            }

            string uriPrefix, uriSuffix;
            int uriStar = value.IndexOf('*');
            if (uriStar >= 0)
            {
                if (filePathStar < 0)
                {
                    return false;
                }

                uriPrefix = value.Substring(0, uriStar);
                uriSuffix = value.Substring(uriStar + 1);

                if (uriSuffix.IndexOf('*') >= 0)
                {
                    return false;
                }
            }
            else
            {
                uriPrefix = value;
                uriSuffix = "";
            }

            path = new FilePathPattern(key, isPrefix: filePathStar >= 0);
            uri = new UriPattern(uriPrefix, uriSuffix);
            return true;
        }

        public string GetUri(string path)
        {
            if (path.IndexOf('*') >= 0)
            {
                return null;
            }

            // Note: the mapping function is case-insensitive.

            foreach (var (file, uri) in _entries)
            {
                if (file.IsPrefix)
                {
                    if (path.StartsWith(file.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        var escapedPath = string.Join("/", path.Substring(file.Path.Length).Split(new[] { '/', '\\' }).Select(Uri.EscapeDataString));
                        return uri.Prefix + escapedPath + uri.Suffix;
                    }
                }
                else if (string.Equals(path, file.Path, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Assert(uri.Suffix.Length == 0);
                    return uri.Prefix;
                }
            }

            return null;
        }
    }
}
