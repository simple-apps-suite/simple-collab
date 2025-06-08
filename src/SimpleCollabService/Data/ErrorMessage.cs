// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Data;

static class ErrorMessage
{
    public const string GenericServerError = "Something went wrong with the server.";

    public const string RequiredFieldMissing = "The required field field '{0}' is missing.";

    public const string UsernameNotValid =
        "Usernames must be between 6 and 40 characters long and may only contain alphanumeric characters or underscores.";

    public const string UsernameAlreadyTaken = "This username is already in use.";

    public const string IdentityAlreadyPaired =
        "This identity is already associated to the username '{0}', you must disassociate it from the current username before you can associate it to a new one.";

    public const string TimestampOutOfRange =
        "The timestamp is too far from current time, read the server clock and try again.";

    public const string PublicKeyNotEd25519 = "The public key is not a valid Ed25519 key encoded in base64url format.";

    public const string ProofOfWorkMismatch = "The proof-of-work does not match.";

    public const string SignatureMismatch = "Expected a signature of '{0}' with your private key.";

    public const string InvalidIdentity =
        "Invalid or unknown identity format, this server expects a base64url SHA256 of the public key bytes.";

    public const string UnknownIdentity = "This identity is not known to this server.";

    public const string UnknownYourIdentity = "Your identity is not known to this server, register it and try again.";

    public const string MismatchedIdentity = "This identity is not associated to the provided username.";

    public const string UnknownEndpoint =
        "This is not a valid API endpoint, you might be using the wrong address or the wrong HTTP verb.";
}
