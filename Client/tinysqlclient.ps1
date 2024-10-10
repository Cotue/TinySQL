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
        
        # Verificar y mostrar el Query antes de enviarlo
        Write-Host "Enviando consulta al servidor: $Query"
        
        # Crear el mensaje de la consulta en formato JSON
        $requestObject = [PSCustomObject]@{
            RequestType = 0;
            RequestBody = $Query  # Aquí se envía la consulta SQL tal como viene
        }
        $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress

        # Verificar el JSON generado
        Write-Host "JSON generado para enviar: $jsonMessage"

        # Enviar la consulta al servidor
        $stream = New-Object System.Net.Sockets.NetworkStream($client)
        $writer = New-Object System.IO.StreamWriter($stream)
        $writer.WriteLine($jsonMessage)
        $writer.Flush()

        # Leer la respuesta del servidor
        $reader = New-Object System.IO.StreamReader($stream)
        $response = $reader.ReadLine()

        if ($null -eq $response) {
            Write-Error "Error: No se recibió ninguna respuesta del servidor."
            return $null
        }

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

        # Mostrar el resultado en formato tabla
        if ($result) {
            $result | Format-Table -Property RequestType, RequestBody, Status, ResponseBody -AutoSize
        } else {
            Write-Host "Sin resultados."
        }

        # Mostrar el tiempo que tomó ejecutar la consulta
        Write-Host "Tiempo de ejecución: $($duration.TotalMilliseconds) ms"
    }
}

# Si el parámetro $QueryFile está especificado, ejecutar las consultas desde el archivo
if ($QueryFile) {
    Execute-MyQuery -QueryFile $QueryFile -Port $Port -IP $IP
} elseif ($Query) {
    # Si se proporciona una consulta directa, enviarla al servidor
    Write-Host "Ejecutando consulta SQL directa: $Query"
    $result = Send-SQLCommand -Query $Query -IPEndPoint $ipEndPoint
    
    # Mostrar el resultado en formato tabla
    if ($result) {
        $result | Format-Table -Property RequestType, RequestBody, Status, ResponseBody -AutoSize
    } else {
        Write-Host "Sin resultados."
    }
} else {
    Write-Host "Por favor, proporcione un archivo SQL (-QueryFile) o una consulta SQL directa (-Query)."
}




