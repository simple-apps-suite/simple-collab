// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SimpleCollabService.Data;
using SimpleCollabService.Repository.Sqlite;

namespace SimpleCollabService.Endpoints;

static class ExampleEndpoints
{
    const int Ed25519KeyLength = 32;
    const int Ed25519KeyBase64Length = 43; // Base64Url.GetEncodedLength(Ed25519KeyLength);

    const int MaxInt64StringLength = 19; // long.MaxValue.ToString(CultureInfo.InvariantCulture).Length

    public static IResult GetServerInfo()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return Results.Ok<ServerInfoResponse>(new(now));
    }

    public static async Task<IResult> CreateIdentityAsync(
        [FromServices] SqliteConnection db,
        [FromBody] CreateIdentityRequest req,
        CancellationToken cancellationToken
    )
    {
        if (req.PublicKey is null)
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.MissingPublicKey));

        if (req.Timestamp is 0)
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.MissingTimestamp));

        if (req.Pow is null)
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.MissingProofOfWork));

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (req.Timestamp < now - 600 || req.Timestamp > now + 600)
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.InvalidTimestamp));

        byte[] publicKey = new byte[Ed25519KeyBase64Length];
        if (
            !Base64Url.TryDecodeFromChars(req.PublicKey, publicKey, out int publicKeyLength)
            || publicKeyLength != publicKey.Length
        )
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.InvalidPublicKey));

        if (!IsValidPow(req.Pow, publicKey, req.Timestamp))
            return Results.BadRequest<ErrorResponse>(new(ErrorCode.InvalidProofOfWork));

        byte[] hash = SHA256.HashData(publicKey);

        // Add the identity to database, no-op if it already exists.
        await SqliteQueries.InsertIdentityAsync(db, hash, publicKey, cancellationToken);

        string hashBase64 = Base64Url.EncodeToString(hash);
        return Results.Ok<CreateIdentityResponse>(new(hashBase64));
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

        // TODO: validate.

        return true;
    }
}
