// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

//#define DEBUG_ACCEPT_ANY_TIMESTAMP
//#define DEBUG_ACCEPT_ANY_SIGNATURE
//#define DEBUG_ACCEPT_ANY_POW

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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SimpleCollabService.Data;
using SimpleCollabService.Repository.Sqlite;

namespace SimpleCollabService.Endpoints;

static class ExampleEndpoints
{
    const int Ed25519KeyLength = 32;

    const int MaxInt64StringLength = 19; // long.MaxValue.ToString(CultureInfo.InvariantCulture).Length

    public static IResult GetServerInfo()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Results.Ok<ServerInfoResponse>(new(now));
    }

    public static async Task<IResult> RegisterIdentityAsync(
        [FromServices] SqliteConnection db,
        [FromBody] CreateIdentityRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingTimestamp,
                ErrorMessage.MissingTimestamp
            );

        if (req.PublicKey is null)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingPublicKey,
                ErrorMessage.MissingPublicKey
            );

        if (req.Pow is null)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingProofOfWork,
                ErrorMessage.MissingProofOfWork
            );

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
        return Results.Ok<CreateIdentityResponse>(new(hashBase64));
    }

    public static async Task<IResult> ReadIdentityAsync(
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

        byte[]? publicKey = await SqliteQueries.GetPublicKeyByHashAsync(
            db,
            hashBytes,
            cancellationToken
        );
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
        [FromBody] CreateUserRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.Timestamp is 0)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingTimestamp,
                ErrorMessage.MissingTimestamp
            );

        if (string.IsNullOrEmpty(req.Identity))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingIdentity,
                ErrorMessage.MissingIdentity
            );

        if (string.IsNullOrEmpty(req.Username))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingUsername,
                ErrorMessage.MissingUsername
            );

        if (string.IsNullOrEmpty(req.Signature))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.MissingSignature,
                ErrorMessage.MissingSignature
            );

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

        byte[] identityHash = new byte[32];
        if (
            !Base64Url.TryDecodeFromChars(req.Identity, identityHash, out int hashLen)
            || hashLen != SHA256.HashSizeInBytes
        )
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.UnknownIdentity,
                ErrorMessage.InvalidIdentity
            );

        // Get public key for the identity.
        byte[]? publicKey = await SqliteQueries.GetPublicKeyByHashAsync(
            db,
            identityHash,
            cancellationToken
        );
        if (publicKey is null)
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.UnknownIdentity,
                ErrorMessage.UnknownYourIdentity
            );

        // Compute message: "REGISTER_USER " + base64url(sha256(username)) + " " + timestamp
        byte[] usernameHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(req.Username));
        string usernameHashBase64 = Base64Url.EncodeToString(usernameHash);
        string message = $"REGISTER_USER {usernameHashBase64} {req.Timestamp}";
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

        // Decode signature.
        byte[] signatureBytes = new byte[64];
        if (
            !Base64Url.TryDecodeFromChars(req.Signature, signatureBytes, out int sigLen)
            || sigLen != 64
        )
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidSignature,
                string.Format(CultureInfo.InvariantCulture, ErrorMessage.SignatureMismatch, message)
            );

#if !(DEBUG && DEBUG_ACCEPT_ANY_SIGNATURE)
        // Verify signature.
        if (!Ed25519.Verify(signatureBytes, messageBytes, publicKey))
            return ErrorResponse.Result(
                StatusCodes.Status400BadRequest,
                ErrorCode.InvalidSignature,
                string.Format(CultureInfo.InvariantCulture, ErrorMessage.SignatureMismatch, message)
            );
#endif

        // Associate identity with new user.
        bool associated = await SqliteQueries.AssociateIdentityToUserAsync(
            db,
            req.Username,
            identityHash,
            cancellationToken
        );
        if (!associated)
        {
            string currentUsername = await SqliteQueries.GetIdentityUsernameAsync(
                db,
                identityHash,
                cancellationToken
            );
            if (currentUsername is not null)
                return ErrorResponse.Result(
                    StatusCodes.Status409Conflict,
                    ErrorCode.IdentityAlreadyPaired,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        ErrorMessage.IdentityAlreadyPaired,
                        currentUsername
                    )
                );

            return ErrorResponse.Result(
                StatusCodes.Status409Conflict,
                ErrorCode.UsernameAlreadyTaken,
                ErrorMessage.UsernameAlreadyTaken
            );
        }

        return Results.Ok(new CreateUserResponse());
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
        bool isDevelopment = context
            .RequestServices.GetRequiredService<IHostEnvironment>()
            .IsDevelopment();
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
#if DEBUG && DEBUG_ACCEPT_ANY_TIMESTAMP
        return true;
#else
        if (timestamp < now - 120 || timestamp > now + 120)
        {
            return false;
        }

        return true;
#endif
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

            bool powSeedWritten = SHA256.TryHashData(
                buffer.AsSpan(0, powInputLength),
                powSeed,
                out int powSeedLength
            );

            Debug.Assert(powSeedWritten);
            Debug.Assert(powSeedLength is SHA256.HashSizeInBytes);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

#if DEBUG && DEBUG_ACCEPT_ANY_POW
        return true;
#else
        // TODO: validate.

        return true;
#endif
    }
}
