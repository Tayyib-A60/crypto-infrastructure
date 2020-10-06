USE master
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'WalletsCrypto')
BEGIN
    CREATE DATABASE WalletsCrypto;
END;
GO


USE WalletsCrypto;
GO

IF OBJECT_ID('Users', 'U') IS NULL
BEGIN
    CREATE TABLE Users 
    (
        Id VARCHAR(50) PRIMARY KEY NOT NULL
    )
END;
GO

IF OBJECT_ID('Addresses', 'U') IS NULL
BEGIN
    Create TABLE Addresses 
    (
        Id VARCHAR(50) PRIMARY KEY,
        UserId VARCHAR(50) NOT NULL,
        CryptoCurrencyType TINYINT NOT NULL,
        BlockChainAddress VARCHAR(50) NOT NULL,
        AvailableBalance DECIMAL(28, 14) NOT NULL,
        BookBalance DECIMAL(28, 14) NOT NULL 
    )
END;
GO

IF OBJECT_ID('Transactions', 'U') IS NULL
BEGIN
    Create TABLE Transactions 
    (
        Id VARCHAR(50) PRIMARY KEY,
        UserId VARCHAR(50) NOT NULL,
        AddressId VARCHAR(50) NOT NULL,
        TransactionAmount DECIMAL (28, 14) NOT NULL,
        TransactionFee DECIMAL (28, 14) NOT NULL,
        TransactionAddress VARCHAR(50) NOT NULL,
        TransactionType TINYINT NOT NULL
    )
END;
GO

IF OBJECT_ID('UnspentTransactions', 'U') IS NULL
BEGIN
    Create TABLE UnspentTransactions 
    (
        Id VARCHAR(50) PRIMARY KEY,
        AddressId VARCHAR(50) NOT NULL,
        TxHash VARCHAR(100) NOT NULL,
        Amount DECIMAL (28, 14) NOT NULL,
        IsSpent BIT NOT NULL
    )
END;
GO