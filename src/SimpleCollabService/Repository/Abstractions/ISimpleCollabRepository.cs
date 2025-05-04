// SPDX-FileCopyrightText: Copyright 2025 Fabio Iotti
// SPDX-License-Identifier: AGPL-3.0-only

namespace SimpleCollabService.Repository.Abstractions;

public interface ISimpleCollabRepository
{
    /// <summary>
    /// Applies any pending migrations to the database.
    /// This method should be called before interacting with the repository,
    /// to ensure that the database schema is up to date.
    /// </summary>
    /// <returns></returns>
    ValueTask ApplyMigrationsAsync(CancellationToken cancellationToken = default);

    #region TODO

    // // User operations
    // Task<User> GetUserByIdAsync(long id);
    // Task<User> GetUserByHashAsync(byte[] hash);
    // Task AddUserAsync(User user);

    // // Document operations
    // Task<Document> GetDocumentByIdAsync(long id);
    // Task<Document> GetDocumentByHashAsync(byte[] hash);
    // Task AddDocumentAsync(Document document);
    // Task UpdateDocumentAsync(Document document);
    // Task<IEnumerable<Document>> GetDocumentsReplacedByAsync(long documentId);

    // // DocumentLink operations
    // Task AddDocumentLinkAsync(DocumentLink documentLink);
    // Task<IEnumerable<DocumentLink>> GetDocumentLinksByParentIdAsync(long parentId);
    // Task<IEnumerable<DocumentLink>> GetDocumentLinksByChildIdAsync(long childId);

    // // Notification operations
    // Task<IEnumerable<Notification>> GetNotificationsForUserAsync(long userId);
    // Task AddNotificationAsync(Notification notification);
    // Task RemoveNotificationAsync(long userId, long documentId);

    // // SharedKey operations
    // Task<SharedKey> GetSharedKeyByIdAsync(long id);
    // Task AddSharedKeyAsync(SharedKey sharedKey);

    // // UserKey operations
    // Task<IEnumerable<UserKey>> GetUserKeysForSharedKeyAsync(long sharedKeyId);
    // Task AddUserKeyAsync(UserKey userKey);
    // Task UpdateUserKeyAsync(UserKey userKey);

    #endregion
}

// // Entity definitions (simplified for repository interface)
// public class User
// {
//     public long Id { get; set; }
//     public byte[] Hash { get; set; }
//     public byte[] PublicKey { get; set; }
// }

// public class Document
// {
//     public long Id { get; set; }
//     public byte[] Hash { get; set; }
//     public Guid Type { get; set; }
//     public long? Replaces { get; set; }
//     public long? WithKey { get; set; }
//     public byte[] Data { get; set; }
//     public byte[] Signature { get; set; }
// }

// public class DocumentLink
// {
//     public long Parent { get; set; }
//     public long Child { get; set; }
// }

// public class Notification
// {
//     public long ForUser { get; set; }
//     public long Document { get; set; }
// }

// public class SharedKey
// {
//     public long Id { get; set; }
//     public long GeneratedBy { get; set; }
// }

// public class UserKey
// {
//     public long Id { get; set; }
//     public long PartOf { get; set; }
//     public long ForUser { get; set; }
//     public char Role { get; set; }
//     public byte[] Key { get; set; }
//     public DateTime? Expiration { get; set; }
//     public bool Acknowledged { get; set; }
// }
