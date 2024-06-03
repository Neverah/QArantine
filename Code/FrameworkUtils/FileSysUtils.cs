using System;
using System.IO;

namespace TestFramework.Code.FrameworkUtils
{
    public static class FileSysUtils
    {
        public static string? FindDirectory(string currentDirectory, string targetDirectoryName)
        {
            try
            {
                // Verifica si el directorio actual es el directorio objetivo
                if (Path.GetFileName(currentDirectory).Equals(targetDirectoryName, StringComparison.OrdinalIgnoreCase))
                {
                    return currentDirectory;
                }

                // Obtén todos los subdirectorios en el directorio actual
                string[] subDirectories = Directory.GetDirectories(currentDirectory);

                foreach (string subDirectory in subDirectories)
                {
                    // Llama recursivamente a la función para buscar en cada subdirectorio
                    string? foundDirectory = FindDirectory(subDirectory, targetDirectoryName);

                    if (foundDirectory != null)
                    {
                        return foundDirectory;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"No se tiene acceso al directorio: {currentDirectory}. Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar en el directorio: {currentDirectory}. Error: {ex.Message}");
            }

            // Retorna null si no se encuentra el directorio objetivo
            return null;
        }
    }
}