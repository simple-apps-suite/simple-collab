// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

#if DEBUG
//#define DEBUG_ACCEPT_ANY_TIMESTAMP
//#define DEBUG_ACCEPT_ANY_SIGNATURE
//#define DEBUG_ACCEPT_ANY_POW
#endif

using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Unicode;
using Chaos.NaCl;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Data;
using SimpleCollabService.Repository.Sqlite;
using SimpleCollabService.Repository.Sqlite.Data;

namespace SimpleCollabService.Endpoints;

static class ExampleEndpoints
{
    const int Ed25519KeyLength = 32;

    const int MaxInt64StringLength = 19; // long.MaxValue.ToString(CultureInfo.InvariantCulture).Length

    public static IResult ServerInfo()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Results.Ok<ServerInfoResponse>(new(now));
    }

    public static async Task<IResult> RegisterIdentityAsync(
        [FromServices] SqliteConnection db,
        [FromBody] RegisterIdentityRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.MissingField("timestamp");

        if (req.PublicKey is null)
            return ErrorResponse.MissingField("public_key");

        if (req.Pow is null)
            return ErrorResponse.MissingField("pow");

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!IsValidTimestamp(req.Timestamp, now))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidTimestamp,
                ErrorMessage.TimestampOutOfRange
            );

        byte[] publicKey = new byte[Ed25519KeyLength];
        if (
            !Base64Url.IsValid(req.PublicKey)
            || !Base64Url.TryDecodeFromChars(req.PublicKey, publicKey, out int publicKeyLength)
            || publicKeyLength != publicKey.Length
        )
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidPublicKey,
                ErrorMessage.PublicKeyNotEd25519
            );

        if (!IsValidPow(req.Pow, publicKey, req.Timestamp))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidProofOfWork,
                ErrorMessage.ProofOfWorkMismatch
            );

        byte[] hash = SHA256.HashData(publicKey);

        // Add the identity to database, this is a no-op if it already exists.
        await SqliteQueries.InsertIdentityAsync(db, hash, publicKey, cancellationToken);

        string hashBase64 = Base64Url.EncodeToString(hash);
        return Results.Ok<RegisterIdentityResponse>(new(hashBase64));
    }

    public static async Task<IResult> IdentityAsync(
        [FromServices] SqliteConnection db,
        [FromRoute] string hash,
        CancellationToken cancellationToken
    )
    {
        byte[] hashBytes = new byte[32];
        if (
            !Base64Url.IsValid(hash)
            || !Base64Url.TryDecodeFromChars(hash, hashBytes, out int hashLen)
            || hashLen is not SHA256.HashSizeInBytes
        )
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.UnknownIdentity,
                ErrorMessage.InvalidIdentity
            );

        byte[]? publicKey = await SqliteQueries.GetPublicKeyByHashAsync(db, hashBytes, cancellationToken);
        if (publicKey is null)
            return ErrorResponse.Result(
                StatusCodes.Status404NotFound,
                ErrorCode.UnknownIdentity,
                ErrorMessage.UnknownIdentity
            );

        string publicKeyBase64 = Base64Url.EncodeToString(publicKey);
        return Results.Ok<IdentityResponse>(new(publicKeyBase64));
    }

    public static async Task<IResult> RegisterUserAsync(
        [FromServices] SqliteConnection db,
        [FromBody] RegisterUserRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.MissingField("timestamp");

        if (string.IsNullOrEmpty(req.Identity))
            return ErrorResponse.MissingField("identity");

        if (string.IsNullOrEmpty(req.Username))
            return ErrorResponse.MissingField("username");

        if (string.IsNullOrEmpty(req.Signature))
            return ErrorResponse.MissingField("signature");

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!IsValidTimestamp(req.Timestamp, now))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidTimestamp,
                ErrorMessage.TimestampOutOfRange
            );

        if (
            req.Username is not { Length: >= 6 and <= 40 }
            || req.Username.Any(c =>
                c is not ((>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_')
            )
        )
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidUsername,
                ErrorMessage.UsernameNotValid
            );

        // Validate signature.
        byte[] usernameHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(req.Username));
        string usernameHashBase64 = Base64Url.EncodeToString(usernameHash);
        (IResult? signatureError, byte[] identityHash) = await ValidateSignatureAsync(
            db,
            "REGISTER_USER",
            req.Timestamp,
            req.Identity,
            req.Signature,
            usernameHashBase64,
            cancellationToken
        );
        if (signatureError is not null)
            return signatureError;

        // Associate identity with new user.
        bool associated = await SqliteQueries.RegisterUserAsync(db, req.Username, identityHash, cancellationToken);
        if (!associated)
        {
            string currentUsername = await SqliteQueries.GetIdentityUsernameAsync(db, identityHash, cancellationToken);
            if (currentUsername is not null)
                return ErrorResponse.Result(
                    StatusCodes.Status409Conflict,
                    ErrorCode.IdentityAlreadyPaired,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessage.IdentityAlreadyPaired, currentUsername)
                );

            return ErrorResponse.Result(
                StatusCodes.Status409Conflict,
                ErrorCode.UsernameAlreadyTaken,
                ErrorMessage.UsernameAlreadyTaken
            );
        }

        return Results.Ok(new RegisterUserResponse());
    }

    public static async Task<IResult> UserInfoAsync(
        [FromServices] SqliteConnection db,
        [FromBody] UserInfoRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.MissingField("timestamp");

        if (string.IsNullOrEmpty(req.Identity))
            return ErrorResponse.MissingField("identity");

        if (string.IsNullOrEmpty(req.Username))
            return ErrorResponse.MissingField("username");

        if (string.IsNullOrEmpty(req.Signature))
            return ErrorResponse.MissingField("signature");

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!IsValidTimestamp(req.Timestamp, now))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidTimestamp,
                ErrorMessage.TimestampOutOfRange
            );

        // Validate signature.
        byte[] usernameHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(req.Username));
        string usernameHashBase64 = Base64Url.EncodeToString(usernameHash);
        (IResult? signatureError, byte[] identityHash) = await ValidateSignatureAsync(
            db,
            "INFO",
            req.Timestamp,
            req.Identity,
            req.Signature,
            usernameHashBase64,
            cancellationToken
        );
        if (signatureError is not null)
            return signatureError;

        UserInfo info = await SqliteQueries.GetUserInfoAsync(db, identityHash, cancellationToken);

        if (string.Compare(info.Username, req.Username, StringComparison.OrdinalIgnoreCase) is not 0)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidIdentity,
                ErrorMessage.MismatchedIdentity
            );

        // TODO: read quota from config.
        return Results.Ok<UserInfoResponse>(new(123456789, info.UsedBytes, info.Expiration));
    }

    public static async Task<IResult> LinkIdentityAsync(
        [FromServices] SqliteConnection db,
        [FromBody] IdentityLinkRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.MissingField("timestamp");

        if (string.IsNullOrEmpty(req.CurrentIdentity))
            return ErrorResponse.MissingField("current_identity");

        if (string.IsNullOrEmpty(req.NewIdentity))
            return ErrorResponse.MissingField("new_identity");

        if (string.IsNullOrEmpty(req.Username))
            return ErrorResponse.MissingField("username");

        if (string.IsNullOrEmpty(req.CurrentSignature))
            return ErrorResponse.MissingField("current_signature");

        if (string.IsNullOrEmpty(req.NewSignature))
            return ErrorResponse.MissingField("new_signature");

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!IsValidTimestamp(req.Timestamp, now))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidTimestamp,
                ErrorMessage.TimestampOutOfRange
            );

        // Validate "current" signature.
        byte[] usernameHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(req.Username));
        string usernameHashBase64 = Base64Url.EncodeToString(usernameHash);
        byte[] currentHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(usernameHashBase64 + req.NewIdentity));
        string currentHashBase64 = Base64Url.EncodeToString(currentHash);
        (IResult? currentSignatureError, byte[] currentIdentityHash) = await ValidateSignatureAsync(
            db,
            "LINK_IDENTITY",
            req.Timestamp,
            req.CurrentIdentity,
            req.CurrentSignature,
            currentHashBase64,
            cancellationToken,
            ErrorCode.InvalidCurrentIdentity,
            ErrorCode.InvalidCurrentSignature
        );
        if (currentSignatureError is not null)
            return currentSignatureError;

        // Validate "new" signature.
        byte[] newHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(usernameHashBase64 + req.CurrentIdentity));
        string newHashBase64 = Base64Url.EncodeToString(newHash);
        (IResult? newSignatureError, byte[] newIdentityHash) = await ValidateSignatureAsync(
            db,
            "LINK_IDENTITY",
            req.Timestamp,
            req.NewIdentity,
            req.NewSignature,
            newHashBase64,
            cancellationToken,
            ErrorCode.UnknownIdentity,
            ErrorCode.InvalidNewSignature
        );
        if (newSignatureError is not null)
            return newSignatureError;

        // Ensure the current identity is linked to the specified user.
        string username = await SqliteQueries.GetIdentityUsernameAsync(db, currentIdentityHash, cancellationToken);
        if (string.Compare(username, req.Username, StringComparison.OrdinalIgnoreCase) is not 0)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidCurrentIdentity,
                ErrorMessage.MismatchedIdentity
            );

        // Associate new identity with user.
        bool assigned = await SqliteQueries.LinkIdentityAsync(
            db,
            req.Username,
            currentIdentityHash,
            newIdentityHash,
            cancellationToken
        );
        if (!assigned)
        {
            string currentUsername = await SqliteQueries.GetIdentityUsernameAsync(
                db,
                newIdentityHash,
                cancellationToken
            );
            if (currentUsername is not null)
                return ErrorResponse.Result(
                    StatusCodes.Status409Conflict,
                    ErrorCode.IdentityAlreadyPaired,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessage.IdentityAlreadyPaired, currentUsername)
                );

            // We might reach here in the rare case the "current identity" was
            // linked to the correct username when we checked, but then the link
            // was severed before we could link the new identity; or in the
            // still rare but slightly more probable case the new identity was
            // garbage-collected before we could link it.
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.UnknownIdentity,
                ErrorMessage.UnknownYourIdentity
            );
        }

        return Results.Ok(new IdentityLinkResponse());
    }

    public static IResult InvalidApiEndpoint()
    {
        return ErrorResponse.Result(
            StatusCodes.Status404NotFound,
            ErrorCode.InvalidEndpoint,
            ErrorMessage.UnknownEndpoint
        );
    }

    public static async Task HandleExceptionAsync(HttpContext context)
    {
        ILogger logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        bool isDevelopment = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
        Exception ex = context.Features.GetRequiredFeature<IExceptionHandlerPathFeature>().Error;
        await HandleExceptionInternal(logger, ex, isDevelopment).ExecuteAsync(context);
    }

    static IResult HandleExceptionInternal(ILogger logger, Exception ex, bool includeDetails)
    {
        if (ex is BadHttpRequestException { InnerException: JsonException jsonException })
        {
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MalformedRequest,
                jsonException.Message
            );
        }

        logger.LogError(ex, "Unhandled exception.");

        return ErrorResponse.Result(
            StatusCodes.Status500InternalServerError,
            ErrorCode.UnexpectedError,
            includeDetails ? ex.Message : ErrorMessage.GenericServerError
        );
    }

    static bool IsValidTimestamp(long timestamp, long now)
    {
#if DEBUG_ACCEPT_ANY_TIMESTAMP
        return true;
#else
        if (timestamp < now - 120 || timestamp > now + 120)
        {
            return false;
        }

        return true;
#endif
    }

    static async Task<(IResult? Error, byte[] IdentityHash)> ValidateSignatureAsync(
        SqliteConnection db,
        string prefix,
        long timestamp,
        string identity,
        string signature,
        string payload,
        CancellationToken cancellationToken,
        ErrorCode unknownIdentity = ErrorCode.UnknownIdentity,
        ErrorCode invalidSignature = ErrorCode.InvalidSignature
    )
    {
        byte[] identityHash = new byte[32];
        if (!Base64Url.TryDecodeFromChars(identity, identityHash, out int hashLen) || hashLen != SHA256.HashSizeInBytes)
            return (
                ErrorResponse.Result(StatusCodes.Status400BadRequest, unknownIdentity, ErrorMessage.InvalidIdentity),
                []
            );

        // Get public key for the identity.
        byte[]? publicKey = await SqliteQueries.GetPublicKeyByHashAsync(db, identityHash, cancellationToken);
        if (publicKey is null)
            return (
                ErrorResponse.Result(
                    StatusCodes.Status400BadRequest,
                    unknownIdentity,
                    ErrorMessage.UnknownYourIdentity
                ),
                []
            );

#if !DEBUG_ACCEPT_ANY_SIGNATURE
        // Compute message: prefix + " " + payload + " " + timestamp
        string message = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", prefix, payload, timestamp);
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

        // Decode signature.
        byte[] signatureBytes = new byte[64];
        if (!Base64Url.TryDecodeFromChars(signature, signatureBytes, out int sigLen) || sigLen != 64)
            return (
                ErrorResponse.Result(
                    StatusCodes.Status400BadRequest,
                    invalidSignature,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessage.SignatureMismatch, message)
                ),
                []
            );

        // Verify signature.
        if (!Ed25519.Verify(signatureBytes, messageBytes, publicKey))
            return (
                ErrorResponse.Result(
                    StatusCodes.Status400BadRequest,
                    invalidSignature,
                    string.Format(CultureInfo.InvariantCulture, ErrorMessage.SignatureMismatch, message)
                ),
                []
            );
#endif

        // No error.
        return (null, identityHash);
    }

    static bool IsValidPow(string pow, ReadOnlySpan<byte> publicKey, long timestamp)
    {
        Span<byte> powSeed = stackalloc byte[SHA256.HashSizeInBytes];

        byte[] buffer = ArrayPool<byte>.Shared.Rent(publicKey.Length + MaxInt64StringLength);
        try
        {
            bool powInputWritten = Utf8.TryWrite(
                buffer,
                CultureInfo.InvariantCulture,
                $"{publicKey}{timestamp}",
                out int powInputLength
            );

            Debug.Assert(powInputWritten);

            bool powSeedWritten = SHA256.TryHashData(buffer.AsSpan(0, powInputLength), powSeed, out int powSeedLength);

            Debug.Assert(powSeedWritten);
            Debug.Assert(powSeedLength is SHA256.HashSizeInBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

#if DEBUG_ACCEPT_ANY_POW
        return true;
#else
        // TODO: validate.

        return true;
#endif
    }
}
