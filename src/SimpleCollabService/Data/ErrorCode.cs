// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using SimpleCollab.CodeAnalysis.EnumJsonGenerator;

namespace SimpleCollabService.Data;

[EnumMemberValueJsonConverter]
partial class ErrorCodeJsonConverter : JsonConverter<ErrorCode>;

[JsonConverter(typeof(ErrorCodeJsonConverter))]
enum ErrorCode
{
    [EnumMember(Value = "invalid_endpoint")]
    InvalidEndpoint,

    [EnumMember(Value = "malformed_request")]
    MalformedRequest,

    [EnumMember(Value = "unexpected_error")]
    UnexpectedError,

    [EnumMember(Value = "public_key_missing")]
    MissingPublicKey,

    [EnumMember(Value = "timestamp_missing")]
    MissingTimestamp,

    [EnumMember(Value = "pow_missing")]
    MissingProofOfWork,

    [EnumMember(Value = "timestamp_invalid")]
    InvalidTimestamp,

    [EnumMember(Value = "public_key_invalid")]
    InvalidPublicKey,

    [EnumMember(Value = "pow_invalid")]
    InvalidProofOfWork,

    [EnumMember(Value = "hash_invalid")]
    InvalidHash,

    [EnumMember(Value = "unknown_identity")]
    UnknownIdentity,
}
