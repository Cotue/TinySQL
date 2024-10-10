-- Crear una base de datos llamada 'Universidad'
CREATE DATABASE Universidad;

-- Seleccionar la base de datos 'Universidad'
CREATE TABLE Casas (
    ID INTEGER,
    Nombre VARCHAR(30),
    Apellido VARCHAR(50),
    FechaNacimiento DATETIME
);


-- Crear una tabla llamada 'Estudiantes' dentro de la base de datos 'Universidad'
CREATE TABLE Estudiantes (
    ID INTEGER,
    Nombre VARCHAR(30),
    Apellido VARCHAR(50),
    FechaNacimiento DATETIME
);

-- Insertar datos en la tabla 'Estudiantes'
INSERT INTO Estudiantes (ID, Nombre, Apellido, FechaNacimiento) VALUES (1, 'Isaac', 'Ramirez', '2000-01-01 01:02:00');
INSERT INTO Estudiantes (ID, Nombre, Apellido, FechaNacimiento) VALUES (2, 'Juan', 'Perez', '1999-10-10 12:00:00');

-- Seleccionar todos los datos de la tabla 'Estudiantes'
SELECT * FROM Estudiantes;
