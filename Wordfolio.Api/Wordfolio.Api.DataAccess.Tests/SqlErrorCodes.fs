namespace Wordfolio.Api.DataAccess.Tests

/// PostgreSQL SQL state codes for common error conditions.
/// See: https://www.postgresql.org/docs/current/errcodes-appendix.html
module SqlErrorCodes =
    /// Foreign key violation error code (23503).
    let ForeignKeyViolation = "23503"

    /// Unique constraint violation error code (23505).
    let UniqueViolation = "23505"

    /// Check constraint violation error code (23514).
    let CheckConstraintViolation = "23514"
