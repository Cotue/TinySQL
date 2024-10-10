param (
    [Parameter(Mandatory = $true)]
    [string]$IP,
    [Parameter(Mandatory = $true)]
    [int]$Port
)

$ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse("127.0.0.1"), 11000)

function Send-Message {
    param (
        [Parameter(Mandatory=$true)]
        [pscustomobject]$message,
        [Parameter(Mandatory=$true)]
        [System.Net.Sockets.Socket]$client
    )

    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $writer = New-Object System.IO.StreamWriter($stream)
    try {
        $writer.WriteLine($message)
    }
    finally {
        $writer.Close()
        $stream.Close()
    }
}

function Receive-Message {
    param (
        [System.Net.Sockets.Socket]$client
    )
    $stream = New-Object System.Net.Sockets.NetworkStream($client)
    $reader = New-Object System.IO.StreamReader($stream)
    try {
        # Lee la línea de respuesta una vez y guárdala en una variable
        $responseLine = $reader.ReadLine()
        
        if ($null -ne $responseLine) {
            return $responseLine
        } else {
            return ""
        }
    }
    finally {
        $reader.Close()
        $stream.Close()
    }
}

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

        # Mostrar el resultado en formato tabla, asegurándote de que ResponseBody se imprima
        if ($result) {
            $result | Format-Table -Property RequestType, RequestBody, Status, ResponseBody -AutoSize
        } else {
            Write-Host "Sin resultados."
        }

        # Mostrar el tiempo que tomó ejecutar la consulta
        Write-Host "Tiempo de ejecución: $($duration.TotalMilliseconds) ms"
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
    
    # Crear el socket para conectarse a la API
    $client = New-Object System.Net.Sockets.Socket($IPEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
    
    try {
        $client.Connect($IPEndPoint)
        
        # Crear el mensaje de la consulta en formato JSON
        $requestObject = [PSCustomObject]@{
            RequestType = 0;  # Por ejemplo, tipo de operación
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


# This is an example, should not be called here
# Ejemplo de prueba
Send-SQLCommand -Query "CREATE TABLE ESTUDIANTE" -IPEndPoint $ipEndPoint
Send-SQLCommand -Query "SELECT * FROM ESTUDIANTE" -IPEndPoint $ipEndPoint
# Crear una base de datos llamada 'Universidad'
Send-SQLCommand -Query "CREATE DATABASE Universidad" -IPEndPoint $ipEndPoint




