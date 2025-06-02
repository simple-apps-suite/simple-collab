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

    [EnumMember(Value = "identity_missing")]
    MissingIdentity,

    [EnumMember(Value = "username_missing")]
    MissingUsername,

    [EnumMember(Value = "signature_missing")]
    MissingSignature,

    [EnumMember(Value = "timestamp_invalid")]
    InvalidTimestamp,

    [EnumMember(Value = "public_key_invalid")]
    InvalidPublicKey,

    [EnumMember(Value = "pow_invalid")]
    InvalidProofOfWork,

    [EnumMember(Value = "hash_invalid")]
    InvalidHash,

    [EnumMember(Value = "identity_invalid")]
    InvalidIdentity,

    [EnumMember(Value = "username_invalid")]
    InvalidUsername,

    [EnumMember(Value = "signature_invalid")]
    InvalidSignature,

    [EnumMember(Value = "unknown_identity")]
    UnknownIdentity,

    [EnumMember(Value = "username_already_taken")]
    UsernameAlreadyTaken,

    [EnumMember(Value = "identity_already_paired")]
    IdentityAlreadyPaired,
}
