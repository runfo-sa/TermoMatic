using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Controls;

namespace TermoMatic
{
    public class TemperaturaObservable : INotifyPropertyChanged
    {
        private string _hora;
        private DateTime _fecha;
        private Dictionary<string, string> _lecturas = [];

        public string Hora
        { 
            get => _hora;
            set 
            {
                _hora = value;
                OnPropertyChanged(nameof(Hora));
            }
        }

        public DateTime Fecha
        {
            get => _fecha;
            set
            {
                _fecha = value;
                OnPropertyChanged(nameof(Fecha));
            }
        }
        
        public Dictionary<string, string> Lecturas
        {
            get => _lecturas;
            set
            {
                _lecturas = value;
                OnPropertyChanged(nameof(Lecturas));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Temperatura
    {
        //Los atributos necesarios para una temperatura son Lector, Lectura y Registro. EJ: (T1, -24.5, 20/05/2024 01:30:00)
        private string _lector;
        private decimal _lectura;
        private DateTime _registro;

        public string Lector 
        { 
            get { return _lector; }
            set { _lector = value; } 
        }

        public decimal Lectura
        {
            get { return _lectura; }
            set { _lectura = value; }
        }

        public DateTime Registro
        {
            get { return _registro; }
            set { _registro = value; }
        }

        //Por ahora el único constructor que necesito es el que recibe dos cadenas y el datetime, pero hice uno default por las dudas.
        public Temperatura() 
        {
            Lector = string.Empty;
            Lectura = 0;
            Registro = DateTime.Now;
        }

        public Temperatura(string lector, string lectura, DateTime registro)
        {
            Lector = lector;
            Registro = registro;

            if (decimal.TryParse(lectura.Replace(',', '.'), CultureInfo.InvariantCulture, out decimal lecturaProcesada))
                Lectura = lecturaProcesada;
            else
                Lectura = 0;
        }

        public Temperatura(string lector, string lectura, string registro)
        {
            Lector = lector;
            if (DateTime.TryParse(registro, out DateTime fecha))
                Registro = fecha;
            else
                Registro = DateTime.MinValue;

            if (decimal.TryParse(lectura.Replace(',', '.'), CultureInfo.InvariantCulture, out decimal lecturaProcesada))
                Lectura = lecturaProcesada;
            else
                Lectura = 0;
        }


        //Nuestros queridos métodos, mal llamados de cariño funciones.

        /// <summary>
        /// Esta función lee las temperaturas desde un archivo dado y las devuelve como una lista de objetos de tipo Temperatura.
        /// </summary>
        /// <param name="rutaArchivo">La ruta del archivo desde donde se leerán las temperaturas.</param>
        /// <returns>Una lista de objetos Temperatura leídos desde el archivo.</returns>
        public static List<Temperatura> LeerTemperaturasDeArchivo(string rutaArchivo)
        {
            // Abrimos el archivo a leer
            List<Temperatura> temps = [];

            try
            {
                // Usamos un StreamReader para leer el archivo línea por línea
                using StreamReader sr = new(rutaArchivo);
                string? linea;
                DateTime dtActual = DateTime.MinValue;

                // Leemos el archivo línea por línea
                while ((linea = sr.ReadLine()) != null)
                {
                    // Acá me parece interesante documentar como se ve una línea del archivo en cuestión,
                    // así voy a documentar las primeras dos líneas de ejemplo:
                    // 24/02/2024 00:00:01;T1:-13,2;T2:3,9;T3:-29;T4:-24,2;T5:-25,4;T6:-12,2;T7:-26,4;T8:-32,4;T9:-30;T10:-29,8;T11:-31;T12:10,4;T13:-22,6;T14:-25,3;T15:-27,6;T16:-28,3;T17:-31,7;T18:-30,6
                    // ; D1S1: 14,1; D1S2: -28,1; D2S1: -18,5; D2S2: -16,9; D3S1: -25,5; D3S2: -24,1; DEF: 0,7; C1: -90; RC: -92; SE3: 7,8; ES: -95; DE: -93; EP: 8,8; SE1: 9,5; SE2: 9,1; R1: 9,9; R2: 9,9; R3: 9,9
                    
                    // De acá obtenemos varias cosas, primero que los campos están separados por punto y coma,
                    // dentro el lector está separado por dos puntos y solo hay valor de Fecha cuando se cambia de lectura.

                    // Dividimos la línea en campos separados por ';'
                    string[] campos = linea.Split(';');

                    // Iteramos sobre los campos
                    foreach (string campo in campos)
                    {
                        // Verificamos si el campo contiene una fecha y hora válida, para actualizar la fecha y hora actual
                        if (campo.Contains('/') && DateTime.TryParseExact(campo, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtLeido))
                            dtActual = dtLeido;

                        // Dividimos el campo en partes separadas por ':'
                        string[] cadTemp = campo.Split(':');

                        // Verificamos si hay partes en el campo, nos interesa que haya 2 (Lector y valor)
                        if (cadTemp.Length == 2)
                        {
                            // Obtenemos el lector y la lectura de la temperatura
                            string lector = cadTemp[0];
                            string lectura = cadTemp[1];

                            // Creamos un objeto Temperatura con los datos obtenidos
                            Temperatura temp = new(lector, lectura, dtActual);

                            // Agregamos la temperatura a la lista
                            temps.Add(temp);
                        }
                    }
                }
            }
            catch (Exception ex) 
            {
                throw new Exception("No se pudo procesar el archivo de temperaturas. \nDetalle: " + ex.Message);
            }

            // Devolvemos la lista de temperaturas
            return temps;
        }


        /// <summary>
        /// Esta función inserta en una base de datos una lista de temperaturas por lotes. Primero intenta convertir la lista en un DataTable.
        /// </summary>
        /// <param name="temps">La lista de temperaturas que se desea insertar en la base de datos.</param>
        /// <param name="cadConexionDb">La cadena de conexión a la base de datos donde se realizarán las inserciones.</param>
        public static void InsertarTemperaturasPorLotes(List<Temperatura> temps, string cadConexionDb)
        {
            // Conectamos con la base de datos
            SqlConnection conexion = new(cadConexionDb);

            // Convertimos la lista de temperaturas en un DataTable
            DataTable dt = ConvertirListaTemperaturaEnDataTable(temps);

            try
            {
                // Abrimos la conexión con la base de datos
                conexion.Open();

                // Creamos un comando SQL para insertar los datos en la base de datos utilizando un procedimiento almacenado
                using SqlCommand comando = new("registro.InsertarTemperaturaDesdeTabla", conexion);

                // Especificamos que el comando es un procedimiento almacenado
                comando.CommandType = CommandType.StoredProcedure;

                // Agregamos el parámetro que contiene las temperaturas a insertar
                comando.Parameters.Add("@TemperaturaSinProcesar", SqlDbType.Structured);
                comando.Parameters["@TemperaturaSinProcesar"].Value = dt;

                // Ejecutamos el comando para insertar las temperaturas en la base de datos
                comando.ExecuteNonQuery();
            }
            catch (Exception ex) 
            {
                throw new Exception("No se pudo guardar las temperaturas en la base de datos. \nDetalle: " + ex.Message);
            }
        }

        public static void ActualizarTemperaturaPorLotes(List<Temperatura> temps,string usuario, string equipo, string cadConexionDb)
        {
            SqlConnection conexion = new(cadConexionDb);

            DataTable dt = ConvertirListaTemperaturaEnDataTable(temps);

            try
            {
                conexion.Open();

                using SqlCommand comando = new("registro.ActualizarTemperaturaDesdeTabla", conexion);

                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.Add("@TemperaturaSinProcesar", SqlDbType.Structured);
                comando.Parameters["@TemperaturaSinProcesar"].Value = dt;

                comando.Parameters.Add("@Usuario", SqlDbType.VarChar);
                comando.Parameters["@Usuario"].Value = usuario;

                comando.Parameters.Add("@Equipo", SqlDbType.VarChar);
                comando.Parameters["@Equipo"].Value = equipo;

                comando.ExecuteNonQuery();

            }
            catch (Exception ex) 
            {
                throw new Exception("No se pudo actualizar las temperaturas en la base de datos. \nDetalle: " + ex.Message);
            }
        }


        /// <summary>
        /// Esta función convierte una lista de temperaturas en un DataTable, que es un formato de datos tabular utilizado para interactuar con bases de datos.
        /// </summary>
        /// <param name="temps">La lista de temperaturas que se desea convertir en un DataTable.</param>
        /// <returns>Un DataTable que contiene la información de las temperaturas.</returns>
        public static DataTable ConvertirListaTemperaturaEnDataTable(List<Temperatura> temps)
        {
            // Creamos un nuevo DataTable para almacenar las temperaturas
            DataTable dt = new();

            // Agregamos las columnas al DataTable
            dt.Columns.Add("LectorDescripcion", typeof(string));
            dt.Columns.Add("Temperatura", typeof(string));
            dt.Columns.Add("FechaRegistro", typeof(string));

            // Iteramos sobre cada temperatura en la lista
            foreach (Temperatura temp in temps)
            {
                // Creamos una nueva fila para el DataTable
                var row = dt.NewRow();

                // Agregamos los valores de la temperatura a la fila
                row["LectorDescripcion"] = temp.Lector;
                row["Temperatura"] = temp.Lectura.ToString(CultureInfo.InvariantCulture);
                row["FechaRegistro"] = temp.Registro.ToString("s", CultureInfo.InvariantCulture);

                // Agregamos la fila al DataTable
                dt.Rows.Add(row);
            }

            // Devolvemos el DataTable con la información de las temperaturas
            return dt;
        }


        /// <summary>
        /// Esta función convierte una lista de temperaturas en un DataTable parecido a lo que vería un usuario en un Excel, donde las columnas son los lectores.
        /// </summary>
        /// <param name="temps">La lista de temperaturas que se desea convertir en un DataTable.</param>
        /// <returns>Un DataTable con la representación visualmente compacta de las temperaturas.</returns>
        public static DataTable ConvertirListaEnDataTableVisual(List<Temperatura> temps)
        {
            // Creamos un nuevo DataTable para almacenar las temperaturas
            DataTable dt = new();

            // Creamos una columna para la fecha de registro y la configuramos como clave primaria
            DataColumn dc = new()
            {
                ColumnName = "HORA",
                DataType = typeof(string)
            };
            dt.Columns.Add(dc);
            dt.PrimaryKey = [dt.Columns["HORA"]];

            // Iteramos sobre cada temperatura en la lista
            foreach (Temperatura temp in temps)
            {
                // Verificamos si ya existe una columna para el lector de la temperatura, si no existe, la creamos
                if (!dt.Columns.Contains(temp.Lector))
                    dt.Columns.Add(temp.Lector);
                

                // Verificamos si ya existe una fila para la fecha de registro de la temperatura
                if (!dt.Rows.Contains(temp.Registro.ToString("HH:mm:ss")))
                {
                    // Si no existe, creamos una nueva fila y agregamos los valores correspondientes
                    DataRow row = dt.NewRow();
                    row["HORA"] = temp.Registro.ToString("HH:mm:ss");
                    row[temp.Lector] = temp.Lectura.ToString("0.0");
                    dt.Rows.Add(row);
                }
                else
                {
                    // Si la fila ya existe, actualizamos el valor de la temperatura en la columna correspondiente al lector
                    DataRow? i = dt.Rows.Find(temp.Registro.ToString("HH:mm:ss"));
                    if (i != null)
                        i[temp.Lector] = temp.Lectura.ToString("0.0");
                }
            }
            // Devolvemos el DataTable con la representación visualmente compacta de las temperaturas
            dt.DefaultView.Sort = "HORA";
            return dt;
        }

        public static ObservableCollection<TemperaturaObservable> ConvertirListaEnObservableCollection(List<Temperatura> lista)
        {
            var collection = new ObservableCollection<TemperaturaObservable>();

            var lookup = new Dictionary<string, TemperaturaObservable>();

            foreach (Temperatura temp in lista)
            {
                var hora = temp.Registro.ToString("HH:mm:ss");
                DateTime fecha = temp.Registro;

                if (!lookup.ContainsKey(hora))
                {
                    var observable = new TemperaturaObservable { Hora = hora, Fecha = fecha };
                    lookup[hora] = observable;
                    collection.Add(observable);
                }

                var existingRow = lookup[hora];

                if(!existingRow.Lecturas.ContainsKey(temp.Lector))
                    existingRow.Lecturas[temp.Lector] = temp.Lectura.ToString("0.0");
                else
                    existingRow.Lecturas[temp.Lector] = temp.Lectura.ToString("0.0");
            }

            return collection;
        }


        public static List<Temperatura> LeerTemperaturasDelDiaSQL(DateTime fecha, string cadConexionDb)
        {
            List<Temperatura> temps = [];

            using SqlConnection sqlCon = new(cadConexionDb);

            sqlCon.Open();

            using SqlCommand comando = new("SELECT t.fecha, t.temperatura, l.descripcion" +
                " FROM registro.Temperatura t INNER JOIN configuracion.lector l on l.id = t.lectorId" +
                " WHERE DATEDIFF(DAYOFYEAR, fecha, '" + fecha.ToString("s", CultureInfo.InvariantCulture) + "') = 0", sqlCon);

            using SqlDataReader sqlDataReader = comando.ExecuteReader();

            while(sqlDataReader.Read())
            {
                Temperatura temp = new()
                {
                    Registro = sqlDataReader.GetDateTime(0),
                    Lectura = sqlDataReader.GetDecimal(1),
                    Lector = sqlDataReader.GetString(2)
                };

                temps.Add(temp);
            }

            return temps;
        }

        public static List<Temperatura> LeerAlgunasTemperaturasDelDiaSQL(List<string> lectores,DateTime fecha, string cadConexionDb)
        {
            List<Temperatura> temps = [];

            using SqlConnection sqlCon = new(cadConexionDb);

            sqlCon.Open();

            string query = "SELECT t.fecha, t.temperatura, l.descripcion" +
            " FROM registro.Temperatura t INNER JOIN configuracion.lector l on l.id = t.lectorId" +
            " WHERE DATEDIFF(DAYOFYEAR, fecha, '" + fecha.ToString("s", CultureInfo.InvariantCulture) + "') = 0";

            if (lectores.Count > 0) 
            {
                query += " AND l.descripcion IN (";

                foreach (string str in lectores)
                {
                    query += $"'{str}',";
                }

                query = query.Remove(query.Length - 1);
                query += ")";
            }

            using SqlCommand comando = new(query, sqlCon);

            using SqlDataReader sqlDataReader = comando.ExecuteReader();

            while (sqlDataReader.Read())
            {
                Temperatura temp = new()
                {
                    Registro = sqlDataReader.GetDateTime(0),
                    Lectura = sqlDataReader.GetDecimal(1),
                    Lector = sqlDataReader.GetString(2)
                };

                temps.Add(temp);
            }

            return temps;
        }

        public static List<string> LeerLectoresActivos(string cadConexionDb)
        {
            List<string> lectores = [];

            using SqlConnection sqlCon = new(cadConexionDb);

            sqlCon.Open();

            using SqlCommand comando = new("SELECT descripcion FROM configuracion.LectoresActivos ORDER BY descripcion", sqlCon);

            using SqlDataReader sqlDataReader = comando.ExecuteReader();

            while(sqlDataReader.Read())
            {
                String lector = new(sqlDataReader.GetString(0));

                lectores.Add(lector);
            }

            return lectores;
        }

        public static List<string> LeerLectoresPorFecha(string cadConexionDb, DateTime FechaRegistro)
        {
            List<string> lectores = [];

            using SqlConnection sqlCon = new(cadConexionDb);

            sqlCon.Open();

            using SqlCommand comando = new($"SELECT descripcion FROM configuracion.Lector WHERE id IN (SELECT DISTINCT lectorId FROM registro.Temperatura WHERE DATEDIFF(DAYOFYEAR, fecha, '{FechaRegistro.ToString("s", CultureInfo.InvariantCulture)}') = 0) ORDER BY descripcion", sqlCon);

            using SqlDataReader sqlDataReader = comando.ExecuteReader();

            while (sqlDataReader.Read())
            {
                String lector = new(sqlDataReader.GetString(0));

                lectores.Add(lector);
            }

            return lectores;
        }

        public static DataTable OrdenarTabla(DataTable dt, string nombreColumna, string dir)
        { 
            dt.DefaultView.Sort = nombreColumna + " " + dir;
            return dt.DefaultView.ToTable();
        }
    }
}
