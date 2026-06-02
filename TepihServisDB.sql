CREATE DATABASE TepihServisDB;
GO
USE TepihServisDB;
GO

-- KORISNIK
CREATE TABLE Korisnik (
    KorisnikID INT IDENTITY PRIMARY KEY,
    Ime NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Lozinka NVARCHAR(100) NOT NULL,
    Uloga NVARCHAR(20) DEFAULT 'Korisnik'
);

-- ADMIN
CREATE TABLE Admin (
    AdminID INT PRIMARY KEY,
    Kancelarija NVARCHAR(50),
    FOREIGN KEY (AdminID) REFERENCES Korisnik(KorisnikID)
);

-- KLIJENT
CREATE TABLE Klijent (
    KlijentID INT PRIMARY KEY,
    Telefon NVARCHAR(20) NOT NULL,
    Adresa NVARCHAR(100) NOT NULL,
    FOREIGN KEY (KlijentID) REFERENCES Korisnik(KorisnikID)
);

-- STATUS NARUDZBINE
CREATE TABLE StatusNarudzbine (
    StatusID INT IDENTITY PRIMARY KEY,
    Naziv NVARCHAR(50) NOT NULL
);
INSERT INTO StatusNarudzbine (Naziv) VALUES ('Primljeno'), ('Na pranju'), ('Sušenje'), ('Završeno'), ('Isporuèeno');

-- NARUDZBINA
CREATE TABLE Narudzbina (
    NarudzbinaID INT IDENTITY PRIMARY KEY,
    Datum DATETIME DEFAULT GETDATE(),
    KlijentID INT NOT NULL,
    StatusID INT NOT NULL DEFAULT 1,
    FOREIGN KEY (KlijentID) REFERENCES Klijent(KlijentID),
    FOREIGN KEY (StatusID) REFERENCES StatusNarudzbine(StatusID)
);

-- TEPIH
CREATE TABLE Tepih (
    TepihID INT IDENTITY PRIMARY KEY,
    Naziv NVARCHAR(50) NOT NULL,
    Tip NVARCHAR(50),
    Materijal NVARCHAR(50)
);

-- USLUGA
CREATE TABLE Usluga (
    UslugaID INT IDENTITY PRIMARY KEY,
    Naziv NVARCHAR(50) NOT NULL,
    CijenaPoKvadratu DECIMAL(10,2) NOT NULL
);
INSERT INTO Usluga (Naziv, CijenaPoKvadratu) VALUES ('Standardno pranje', 2.50), ('Dubinsko pranje', 4.00), ('Hemijsko èišæenje svile', 6.50);

-- STAVKA NARUDZBINE
CREATE TABLE StavkaNarudzbine (
    StavkaID INT IDENTITY PRIMARY KEY,
    Povrsina DECIMAL(10,2) NOT NULL,
    Cijena DECIMAL(10,2) DEFAULT 0.00,
    NarudzbinaID INT NOT NULL,
    TepihID INT NOT NULL,
    UslugaID INT NOT NULL,
    FOREIGN KEY (NarudzbinaID) REFERENCES Narudzbina(NarudzbinaID),
    FOREIGN KEY (TepihID) REFERENCES Tepih(TepihID),
    FOREIGN KEY (UslugaID) REFERENCES Usluga(UslugaID)
);

-- DOSTAVA
CREATE TABLE Dostava (
    DostavaID INT IDENTITY PRIMARY KEY,
    Adresa NVARCHAR(100) NOT NULL,
    DatumDostave DATETIME,
    NarudzbinaID INT UNIQUE NOT NULL,
    FOREIGN KEY (NarudzbinaID) REFERENCES Narudzbina(NarudzbinaID)
);

-- KOMENTAR
CREATE TABLE Komentar (
    KomentarID INT IDENTITY PRIMARY KEY,
    Tekst NVARCHAR(255) NOT NULL,
    Datum DATETIME DEFAULT GETDATE(),
    KlijentID INT NOT NULL,
    FOREIGN KEY (KlijentID) REFERENCES Klijent(KlijentID)
);

-- OCJENA
CREATE TABLE Ocjena (
    OcjenaID INT IDENTITY PRIMARY KEY,
    Vrijednost INT CHECK (Vrijednost BETWEEN 1 AND 5),
    KlijentID INT NOT NULL,
    FOREIGN KEY (KlijentID) REFERENCES Klijent(KlijentID)
);

-- ZAPOSLENI
CREATE TABLE Zaposleni (
    ZaposleniID INT IDENTITY PRIMARY KEY,
    Ime NVARCHAR(50) NOT NULL,
    Pozicija NVARCHAR(50) NOT NULL,
    Plata DECIMAL(10,2)
);
GO

--TRIGERI, PROCEDURE, FUNKCIJE
GO
CREATE TRIGGER trigger_IzracunajCijenu
ON StavkaNarudzbine
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(Povrsina) OR UPDATE(UslugaID)
    BEGIN
        UPDATE s
        SET s.Cijena = s.Povrsina * u.CijenaPoKvadratu
        FROM StavkaNarudzbine s
        JOIN inserted i ON s.StavkaID = i.StavkaID
        JOIN Usluga u ON s.UslugaID = u.UslugaID;
    END
END;
GO

CREATE TRIGGER trigger_KreirajDostavu
ON Narudzbina
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Dostava (Adresa, DatumDostave, NarudzbinaID)
    SELECT k.Adresa, DATEADD(day, 3, GETDATE()), i.NarudzbinaID
    FROM inserted i
    JOIN Klijent k ON i.KlijentID = k.KlijentID;
END;
GO

CREATE PROCEDURE Sp_FiltrirajNarudzbine
    @StatusID INT = NULL,
    @KlijentID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT n.NarudzbinaID, n.Datum, s.Naziv AS StatusNaziv, ko.Ime AS KlijentIme
    FROM Narudzbina n
    JOIN StatusNarudzbine s ON n.StatusID = s.StatusID
    JOIN Korisnik ko ON n.KlijentID = ko.KorisnikID
    WHERE (@StatusID IS NULL OR n.StatusID = @StatusID)
      AND (@KlijentID IS NULL OR n.KlijentID = @KlijentID);
END;
GO

CREATE FUNCTION UkupnaCijenaNarudzbine(@NarudzbinaID INT)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Total DECIMAL(10,2);
    SELECT @Total = SUM(Cijena)
    FROM StavkaNarudzbine
    WHERE NarudzbinaID = @NarudzbinaID;
    RETURN ISNULL(@Total, 0.00);
END;
GO

CREATE FUNCTION BrojAktivnihNarudzbinaKlijenta(@KlijentID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Broj INT;
    SELECT @Broj = COUNT(*)
    FROM Narudzbina
    WHERE KlijentID = @KlijentID AND StatusID < 5;
    RETURN ISNULL(@Broj, 0);
END;
GO

-- druga stored procedura: izvještaj o zaradi i obimu posla za menadžment
-- mogla bi da se koristi za potrebe administracije i analitike poslovanja u bazi podataka
GO
CREATE PROCEDURE Sp_IzvjestajZarade
    @DatumOd DATETIME,
    @DatumDo DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        COUNT(DISTINCT n.NarudzbinaID) AS UkupnoNarudzbina,
        COUNT(s.StavkaID) AS UkupnoOpranihTepiha,
        SUM(s.Povrsina) AS UkupnaKvadraturaU_m2,
        SUM(s.Cijena) AS UkupnaZarada_EUR
    FROM Narudzbina n
    JOIN StavkaNarudzbine s ON n.NarudzbinaID = s.NarudzbinaID
    WHERE n.Datum BETWEEN @DatumOd AND @DatumDo;
END;
GO