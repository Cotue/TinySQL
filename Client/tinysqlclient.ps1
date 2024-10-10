param (
    [Parameter(Mandatory = $true)]
    [string]$IP,              # Dirección IP del servidor
    [Parameter(Mandatory = $true)]
    [int]$Port,               # Puerto donde el servidor está escuchando
    [Parameter(Mandatory = $false)]
    [string]$QueryFile,       # Archivo que contiene las sentencias SQL (opcional)
    [Parameter(Mandatory = $false)]
    [string]$Query            # Consulta SQL directa desde la línea de comandos (opcional)
)

# Crear un EndPoint para conectarse a la API usando los parámetros proporcionados
$ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port)

# Función para enviar la consulta SQL al servidor
function Send-SQLCommand {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Query,
        
        [Parameter(Mandatory=$true)]
        [System.Net.IPEndPoint]$IPEndPoint
    )
    
    # Crear el socket para conectarse a la API
    $client = New-Object System.Net.Sockets.Socket($IPEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
    
    try {
        $client.Connect($IPEndPoint)
        
        # Crear el mensaje de la consulta en formato JSON
        $requestObject = [PSCustomObject]@{
            RequestType = 0;  # Por ejemplo, tipo de operación SQL
            RequestBody = $Query  # Cuerpo de la consulta SQL
        }
        $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress

        # Enviar la consulta al servidor
        $stream = New-Object System.Net.Sockets.NetworkStream($client)
        $writer = New-Object System.IO.StreamWriter($stream)
        $writer.WriteLine($jsonMessage)
        $writer.Flush()

        # Leer la respuesta del servidor
        $reader = New-Object System.IO.StreamReader($stream)
        $response = $reader.ReadLine()

        # Convertir la respuesta JSON en un objeto
        $responseObject = ConvertFrom-Json -InputObject $response
        return $responseObject
    } catch {
        Write-Error "Error al conectar o enviar la consulta: $_"
    } finally {
        $client.Shutdown([System.Net.Sockets.SocketShutdown]::Both)
        $client.Close()
    }
}

# Función para ejecutar las consultas desde un archivo SQL
function Execute-MyQuery {
    param (
        [Parameter(Mandatory=$true)]
        [string]$QueryFile,
        
        [Parameter(Mandatory=$true)]
        [int]$Port,
        
        [Parameter(Mandatory=$true)]
        [string]$IP
    )

    # Crear un EndPoint para conectarse a la API usando los parámetros proporcionados
    $ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port)
    
    # Leer el archivo de consultas SQL
    if (-not (Test-Path $QueryFile)) {
        Write-Error "El archivo $QueryFile no existe."
        return
    }
    
    $queries = Get-Content -Path $QueryFile -Raw
    $queries = $queries -split ';'  # Separar las sentencias por punto y coma

    foreach ($query in $queries) {
        # Limpiar espacios en blanco alrededor de la consulta
        $query = $query.Trim()
        if ($query -eq "") { continue }  # Ignorar sentencias vacías

        Write-Host "Ejecutando: $query"

        # Enviar la consulta al servidor y medir el tiempo de ejecución
        $start = Get-Date
        $result = Send-SQLCommand -Query $query -IPEndPoint $ipEndPoint
        $end = Get-Date
        $duration = $end - $start

<<<<<<< HEAD
        # Mostrar el resultado en formato tabla
        if ($result) {
            $result | Format-Table -Property RequestType, RequestBody, Status, ResponseBody -AutoSize
=======
        # Después de recibir el resultado, imprimir el contenido crudo de ResponseBody
        if ($result.ResponseBody) {
            # Imprimir el contenido crudo para asegurarse de lo que llega
            Write-Host "Contenido crudo de ResponseBody: '$($result.ResponseBody)'"

            # Llamar a la función para formatear y mostrar el cuerpo de la respuesta en formato tabla
            Format-ResponseBody -ResponseBody $result.ResponseBody
>>>>>>> 3ba36bef5c7afe8546bd041a378f2e0fd28e5489
        } else {
            Write-Host "Sin resultados."
        }

        # Mostrar el tiempo que tomó ejecutar la consulta
        Write-Host "Tiempo de ejecución: $($duration.TotalMilliseconds) ms"
    }
}

<<<<<<< HEAD
# Si el parámetro $QueryFile está especificado, ejecutar las consultas desde el archivo
if ($QueryFile) {
    Execute-MyQuery -QueryFile $QueryFile -Port $Port -IP $IP
} elseif ($Query) {
    # Si se proporciona una consulta directa, enviarla al servidor
    Write-Host "Ejecutando consulta SQL directa: $Query"
    $result = Send-SQLCommand -Query $Query -IPEndPoint $ipEndPoint
=======
function Format-ResponseBody {
    param (
        [string]$ResponseBody
    )

    # Dividir el ResponseBody en filas (en este caso parece ser una sola fila)
    $rows = $ResponseBody -split '\n'

    # Crear un array para almacenar los objetos que se mostrarán en la tabla
    $resultados = @()

    # Dividir cada fila por comas (asumimos que los datos están separados por comas)
    foreach ($row in $rows) {
        $columns = $row -split ','

        # Si hay al menos 3 columnas (ID, Nombre, Apellido), creamos un objeto
        if ($columns.Length -ge 3) {
            $obj = [pscustomobject]@{
                ID      = $columns[0].Trim()
                Nombre  = $columns[1].Trim()
                Apellido = $columns[2].Trim()
            }
            $resultados += $obj
        }
    }

    # Mostrar los resultados en formato tabla
    if ($resultados.Count -gt 0) {
        $resultados | Format-Table -AutoSize
    } else {
        Write-Host "No se pudieron formatear los resultados."
    }
}

# Función para enviar la consulta SQL al servidor
function Send-SQLCommand {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Query,
        
        [Parameter(Mandatory=$true)]
        [System.Net.IPEndPoint]$IPEndPoint
    )
>>>>>>> 3ba36bef5c7afe8546bd041a378f2e0fd28e5489
    
    # Mostrar el resultado en formato tabla
    if ($result) {
        $result | Format-Table -Property RequestType, RequestBody, Status, ResponseBody -AutoSize
    } else {
        Write-Host "Sin resultados."
    }
} else {
    Write-Host "Por favor, proporcione un archivo SQL (-QueryFile) o una consulta SQL directa (-Query)."
}

<<<<<<< HEAD



=======
# This is an example, should not be called here
# Ejemplo de prueba
Send-SQLCommand -Query "CREATE TABLE ESTUDIANTE" -IPEndPoint $ipEndPoint
Send-SQLCommand -Query "SELECT * FROM ESTUDIANTE" -IPEndPoint $ipEndPoint
>>>>>>> 3ba36bef5c7afe8546bd041a378f2e0fd28e5489
