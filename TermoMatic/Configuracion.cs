using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace TermoMatic
{
    class Configuracion
    {
        public Configuracion() { }

        private string _databaseServer;
        private string _databaseUsuario;
        private string _databaseContrasenia;
        private string _databaseNombre;
        private string _databaseModoConexion;

        public string? DatabaseServer 
        { 
            get { return _databaseServer; } 
            set { _databaseServer = value ?? ""; }
        }

        public string? DatabaseUsuario
        { 
            get { return _databaseUsuario;}
            set { _databaseUsuario = value ?? ""; }
        }

        public string? DatabaseContrasenia
        {
            get { return _databaseContrasenia;}
            set { _databaseContrasenia = value ?? ""; }
        }

        public string? DatabaseNombre
        {
            get { return _databaseNombre;}
            set { _databaseNombre = value ?? ""; }
        }

        public string? DatabaseModoConexion
        {
            get { return _databaseModoConexion; }
            set { _databaseModoConexion = value ?? ""; }
        }

        private static Configuracion? LeerArchivoConfiguracion(string rutaArchivo)
        {
            Configuracion? conf = null;

            try
            {
                using StreamReader sr = new(rutaArchivo);
                string archivoConfiguracion = sr.ReadToEnd();

                conf = JsonSerializer.Deserialize<Configuracion>(archivoConfiguracion);
            }catch (Exception) { }

            return conf;
        }

        public static string CrearCadenaConexionSQL(string rutaArchivo)
        {
            Configuracion? conf = LeerArchivoConfiguracion(rutaArchivo);

            if (conf == null)
                return string.Empty;

            SqlConnectionStringBuilder constructorSQL = new()
            {
                ["data source"] = conf.DatabaseServer,
                ["initial catalog"] = conf.DatabaseNombre
            };

            if (conf.DatabaseModoConexion == "Usuario")
            {
                constructorSQL["user id"] = conf.DatabaseUsuario;
                constructorSQL["password"] = conf.DatabaseContrasenia;
            }
            else
                constructorSQL.IntegratedSecurity = true;

            return constructorSQL.ConnectionString;
        }
    }
}
