using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTPTest
{
    /// <summary>
    /// Esta clase permite la interacción con un servidor SFTP.
    /// Está diseñada como si se trabajara con una consola CLI, en el sentido de que debes navegar hacia el directorio deseado para ejecutar acciones.
    /// También se implementaron métodos para ejecutar acciones sobre un directorio/archivo específico desde el directorio root, pero no han sido validados.
    /// </summary>
    /// <remarks>Esta clase fue desarrollada rápidamente, sin controlar todos los posibles resultados.</remarks>
    public class SFTPService
    {
        /// <summary>
        /// Lleva rastro del directorio actual
        /// </summary>
        public string CurrentDirectory { get; private set; }

        /// <summary>
        /// Representa la Uri de conexión al servidor SFTP.
        /// La información del cliente está guardada en la clase "Config".
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Representa el puerto de conexión al servidor SFTP.
        /// La información del cliente está guardada en la clase "Config".
        /// </summary>
        /// <remarks>No parece ser de utilidad. No se usa en el código.</remarks>
        public int Port { get; private set; }

        /// <summary>
        /// Usuario de autenticación en servidor.
        /// La información del cliente está guardada en la clase "Config".
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// Contraseña de autenticación en servidor.
        /// La información del cliente está guardada en la clase "Config".
        /// </summary>
        public string Pass { get; private set; }

        /// <summary>
        /// Constructor de Servicio de Interacción con servidor SFTP.
        /// </summary>
        /// <param name="host">
        /// Representa la Uri de conexión al servidor SFTP.
        /// La información del cliente está guardada en la clase "Config".
        /// </param>
        /// <param name="port">
        /// Representa el puerto de conexión al servidor SFTP.
        /// La información del cliente está guardada en la clase "Config".
        /// No parece ser de utilidad.
        /// </param>
        /// <param name="user">
        /// Usuario de autenticación en servidor.
        /// La información del cliente está guardada en la clase "Config".
        /// </param>
        /// <param name="pass">
        /// Contraseña de autenticación en servidor.
        /// La información del cliente está guardada en la clase "Config".
        /// </param>
        public SFTPService(string host, int port, string user, string pass)
        {
            Host = host;
            Port = port;
            User = user;
            Pass = pass;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    CurrentDirectory = client.WorkingDirectory;
                    client.Disconnect();
                }
            }
        }

        /// <summary>
        /// Lista todos los archivos/directorios del directorio actual.
        /// </summary>
        /// <returns>Enumerable de archivos/directorios</returns>
        public IEnumerable<SftpFile> ListDirectoryFiles()
        {
            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    return client.ListDirectory(CurrentDirectory)
                        .Where(f=>f.Name!="." && f.Name!="..");
                }
                return null;
            }
        }

        /// <summary>
        /// Devuelve al servicio al directorio de raíz.
        /// </summary>
        /// <remarks>No validado.</remarks>
        public void GoToRoot()
        {
            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    CurrentDirectory = client.WorkingDirectory;
                    client.ChangeDirectory(CurrentDirectory);

                    client.Disconnect();
                }
            }
        }

        /// <summary>
        /// Cambia el directorio actual en base a un directorio del servidor SFTP.
        /// </summary>
        /// <param name="directory">Debe tener configurado "IsDirectory == true"</param>
        /// <exception cref="FormatException">Levanta excepción cuando directory no es un directorio.</exception>
        public void ChangeDirectory(SftpFile directory)
        {
            if (!directory.IsDirectory) throw new FormatException("Param \"directory\" is not a directory");

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    string path = $"{CurrentDirectory}/{directory.Name}";
                    client.ChangeDirectory(path);
                    CurrentDirectory = path;

                    client.Disconnect();
                }
            }
        }

        /// <summary>
        /// Cambia el directorio actual en base a un directorio del servidor SFTP.
        /// </summary>
        /// <param name="directoryPath">El servicio busca el directorio indicado por la ruta y luego navega a  ésta.</param>
        /// <exception cref="FormatException">Si la ruta no es un directorio levanta excepción.</exception>
        /// <remarks>No validado.</remarks>
        public bool ChangeDirectory(string directoryPath)
        {
            bool result = false;
            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    string path = string.Empty;

                    if (directoryPath == "..")
                    {
                        string[] pathArray = directoryPath.Split(new char[] { '/' });
                        for(int i = 0; i < pathArray.Length-1;i++)
                        {
                            path += pathArray[i];
                            if (i < pathArray.Length - 2) path += "/";
                        }
                    }
                    else path = directoryPath;

                    if (path != string.Empty)
                    {
                        SftpFile file = client.Get(path);
                        if(!file.IsDirectory) throw new FormatException("Param \"directory\" is not a directory");
                        client.ChangeDirectory(file.FullName);
                        CurrentDirectory = file.FullName;
                        result = true;
                    }

                    client.Disconnect();
                }
            }
            return result;
        }

        /// <summary>
        /// Elimina un archivo del servidor SFTP.
        /// </summary>
        /// <param name="file">Archivo/Directorio dentro del servidor.</param>
        /// <returns>Logrado o no logrado.</returns>
        public bool Delete(SftpFile file)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    if (file.IsDirectory) client.DeleteDirectory(file.FullName);
                    else client.DeleteFile(file.FullName);
                    result = true;
                    client.Disconnect();
                }

            }

            return result;
        }

        /// <summary>
        /// Elimina un archivo del servidor SFTP.
        /// </summary>
        /// <param name="filePath">Ruta de Archivo/Directorio dentro del servidor.</param>
        /// <returns>Logrado o no logrado.</returns>
        /// <remarks>No validado.</remarks>
        public bool Delete(string filePath)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    SftpFile file = client.Get(filePath);
                    if (file.IsDirectory) client.DeleteDirectory(file.FullName);
                    else client.DeleteFile(file.FullName);
                    result = true;
                    client.Disconnect();
                }
            }

            return result;
        }

        /// <summary>
        /// Descarga archivo de servidor a un directorio local.
        /// </summary>
        /// <param name="file">Archivo en servidor remoto.</param>
        /// <param name="destinationPath">Ruta de descarga local.</param>
        /// <returns>Logrado o no logrado.</returns>
        public bool Download(SftpFile file, string destinationPath)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    string path = $"{destinationPath}/{file.Name}";
                    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                    {
                        client.DownloadFile(file.FullName, fs);
                    }

                    result = true;
                    client.Disconnect();
                }
            }

            return result;
        }

        /// <summary>
        /// Descarga archivo de servidor a un directorio local.
        /// </summary>
        /// <param name="filePath">Ruta de Archivo en servidor remoto.</param>
        /// <param name="destinationPath">Ruta de descarga local.</param>
        /// <returns>Logrado o no logrado.</returns>
        /// <remmarks>No validado.</remmarks>
        public bool Download(string filePath, string destinationPath)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    SftpFile file = client.Get(filePath);
                    if (!file.IsDirectory)
                    {
                        string path = $"{destinationPath}/{file.Name}";
                        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                        {
                            client.DownloadFile(file.FullName, fs);
                        }

                        result = true;
                    }

                    client.Disconnect();
                }
            }

            return result;
        }

        /// <summary>
        /// Carga un archivo al servidor SFTP.
        /// </summary>
        /// <param name="buffer">Array de bytes que conforman el archivo.</param>
        /// <param name="fileName">Nombre/Ruta del archivo.</param>
        /// <param name="canOverride">Se permite sobreescritura o no.</param>
        /// <param name="isFullPath">Se utiliza el parametro fileName como ruta absoluta, o solamente como nombre de archivo.</param>
        /// <returns>Logrado o no logrado.</returns>
        /// <remarks>No validado.</remarks>
        public bool Upload(byte[] buffer, string fileName, bool canOverride = true, bool isFullPath = false)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    using(MemoryStream ms = new MemoryStream(buffer)) 
                    {
                        string destinationPath = isFullPath ? fileName : $"{CurrentDirectory}/{fileName}";
                        client.UploadFile(ms, destinationPath, canOverride);
                        result = true;
                    }

                    client.Disconnect();
                }
            }
            return result;
        }

        /// <summary>
        /// Carga un archivo al servidor SFTP.
        /// </summary>
        /// <param name="base64">String en formato Base64 que conforma el archivo.</param>
        /// <param name="fileName">Nombre/Ruta del archivo.</param>
        /// <param name="canOverride">Se permite sobreescritura o no.</param>
        /// <param name="isFullPath">Se utiliza el parametro fileName como ruta absoluta, o solamente como nombre de archivo.</param>
        /// <returns>Logrado o no logrado.</returns>
        /// <remarks>No validado.</remarks>
        public bool Upload(string base64, string fileName, bool canOverride = true, bool isFullPath = false)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    byte[] buffer = Convert.FromBase64String(base64);
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        string destinationPath = isFullPath ? fileName : $"{CurrentDirectory}/{fileName}";
                        client.UploadFile(ms, destinationPath, canOverride);
                        result = true;
                    }

                    client.Disconnect();
                }
            }
            return result;
        }

        /// <summary>
        /// Carga un archivo al servidor SFTP.
        /// </summary>
        /// <param name="stream">Stream de datos que conforma el archivo.</param>
        /// <param name="fileName">Nombre/Ruta del archivo.</param>
        /// <param name="canOverride">Se permite sobreescritura o no.</param>
        /// <param name="isFullPath">Se utiliza el parametro fileName como ruta absoluta, o solamente como nombre de archivo.</param>
        /// <returns>Logrado o no logrado.</returns>
        /// <remarks>Solamente validado con "isFullPath == false".</remarks>
        public bool Upload(Stream stream, string fileName, bool canOverride = true, bool isFullPath = false)
        {
            bool result = false;

            using (SftpClient client = new SftpClient(Host, User, Pass))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    string destinationPath = isFullPath ? fileName : $"{CurrentDirectory}/{fileName}";
                    client.UploadFile(stream, destinationPath, canOverride);
                    result = true;

                    client.Disconnect();
                }
            }

            return result;
        }

        

    }
}
