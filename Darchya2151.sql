-- Шаг 3. Создание базы данных
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'Darchya2151')
BEGIN
    ALTER DATABASE Darchya2151 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Darchya2151;
END
GO

CREATE DATABASE Darchya2151;
GO

USE Darchya2151;
GO

PRINT 'База данных Darchya2151 успешно создана!';
GO

-- Шаг 4. Таблица Exercises
IF OBJECT_ID('Exercises', 'U') IS NOT NULL
    DROP TABLE Exercises;
GO

CREATE TABLE Exercises
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    MuscleGroup NVARCHAR(100) NULL
);
GO

-- Шаг 5. Заполнение Exercises (13 записей)
TRUNCATE TABLE Exercises;

INSERT INTO Exercises (Name, Category, MuscleGroup) VALUES
(N'Жим штанги лежа',      N'Грудь', N'Грудные мышцы'),
(N'Отжимания',            N'Грудь', N'Грудные мышцы'),
(N'Разводка гантелей',    N'Грудь', N'Грудные мышцы'),
(N'Подтягивания',         N'Спина', N'Широчайшие мышцы'),
(N'Тяга штанги в наклоне',N'Спина', N'Широчайшие мышцы'),
(N'Тяга верхнего блока',  N'Спина', N'Широчайшие мышцы'),
(N'Приседания со штангой',N'Ноги',  N'Квадрицепсы'),
(N'Становая тяга',        N'Ноги',  N'Бицепсы бедра и спина'),
(N'Жим ногами в тренажере', N'Ноги', N'Квадрицепсы'),
(N'Жим штанги стоя',      N'Плечи', N'Дельтовидные мышцы'),
(N'Жим гантелей сидя',    N'Плечи', N'Дельтовидные мышцы'),
(N'Бицепс со штангой',    N'Руки',  N'Бицепс'),
(N'Планка',               N'Пресс', N'Мышцы кора');

SELECT Id, Name, Category, MuscleGroup FROM Exercises ORDER BY Name;
SELECT COUNT(*) AS TotalExercises FROM Exercises;
GO

-- Шаг 6. Таблица WorkoutLog с внешним ключом
IF OBJECT_ID('WorkoutLog', 'U') IS NOT NULL
    DROP TABLE WorkoutLog;
GO

CREATE TABLE WorkoutLog
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ExerciseId INT NOT NULL,
    LogDate DATE NOT NULL,
    Sets INT NOT NULL,
    Reps INT NOT NULL,
    Weight DECIMAL(5,2) NOT NULL,
    CONSTRAINT FK_WorkoutLog_Exercises FOREIGN KEY (ExerciseId)
        REFERENCES Exercises(Id) ON DELETE CASCADE
);
GO

-- Шаг 7. Тестовые данные журнала (9 записей)
TRUNCATE TABLE WorkoutLog;

INSERT INTO WorkoutLog (ExerciseId, LogDate, Sets, Reps, Weight) VALUES
(1, '2026-11-18', 4, 8,  60.0),
(1, '2026-11-20', 4, 10, 62.5),
(4, '2026-11-19', 4, 10, 0.0),
(5, '2026-11-19', 3, 10, 40.0),
(7, '2026-11-21', 5, 8,  80.0),
(7, '2026-11-23', 5, 6,  90.0),
(10,'2026-11-22', 4, 10, 30.0),
(12,'2026-11-22', 3, 12, 20.0),
(13,'2026-11-24', 3, 60, 0.0);

SELECT wl.Id, e.Name, wl.LogDate, wl.Sets, wl.Reps, wl.Weight,
       (wl.Sets * wl.Reps * wl.Weight) AS Тоннаж
FROM WorkoutLog wl
INNER JOIN Exercises e ON wl.ExerciseId = e.Id
ORDER BY wl.LogDate DESC, e.Name;

SELECT COUNT(*) AS TotalRecords FROM WorkoutLog;
SELECT SUM(Sets * Reps * Weight) AS TotalTonnage FROM WorkoutLog;
GO

-- Шаг 8. Получение строки подключения
SELECT 'Server=' + @@SERVERNAME +
       ';Database=Darchya2151;Trusted_Connection=True;TrustServerCertificate=True;'
       AS ConnectionString;
GO

-- Шаг 9. Проверка целостности данных
BEGIN TRY
    INSERT INTO WorkoutLog (ExerciseId, LogDate, Sets, Reps, Weight)
    VALUES (9999, GETDATE(), 1, 1, 1);
    PRINT 'Ошибка: запись с несуществующим ExerciseId добавилась!';
END TRY
BEGIN CATCH
    PRINT 'Проверка FK сработала: ' + ERROR_MESSAGE();
END CATCH
GO

DELETE FROM Exercises WHERE Id = 1;
SELECT COUNT(*) AS RecordsAfterCascade FROM WorkoutLog;
GO
