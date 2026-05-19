-- Se a versao anterior deste script ja foi aplicada, use:
-- ALTER TABLE server_settings RENAME COLUMN AccountReactivationCodeValidityHours TO AccountReactivationCodeValidityDays;

ALTER TABLE server_settings
    ADD COLUMN AccountReactivationCodeValidityDays INT NOT NULL DEFAULT 30;

CREATE TABLE account_reactivation_codes (
    Id BINARY(16) NOT NULL DEFAULT (UUID_TO_BIN(UUID(), 1)),
    UserUid BINARY(16) NOT NULL,
    Code VARCHAR(128) NOT NULL,
    ExpiresAt DATETIME(6) NOT NULL,
    IsUsed TINYINT(1) NOT NULL DEFAULT 0,
    UsedAt DATETIME(6) NULL,
    CreatedAt DATETIME(6) NOT NULL,
    CONSTRAINT pk_account_reactivation_codes PRIMARY KEY (Id),
    CONSTRAINT fk_account_reactivation_codes_users_user_uid
        FOREIGN KEY (UserUid) REFERENCES users (Uid) ON DELETE CASCADE,
    CONSTRAINT ux_account_reactivation_codes_code UNIQUE (Code),
    INDEX ix_account_reactivation_codes_user_uid_is_used_expires_at (UserUid, IsUsed, ExpiresAt)
);
