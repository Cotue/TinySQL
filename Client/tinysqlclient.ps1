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
        # Lee la l�nea de respuesta una vez y gu�rdala en una variable
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

    # Crear un EndPoint para conectarse a la API usando los par�metros proporcionados
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
        if ($query -eq "") { continue }  # Ignorar sentencias vac�as

        Write-Host "Ejecutando: $query"

        # Enviar la consulta al servidor y medir el tiempo de ejecuci�n
        $start = Get-Date
        $result = Send-SQLCommand -Query $query -IPEndPoint $ipEndPoint
        $end = Get-Date
        $duration = $end - $start

        # Despu�s de recibir el resultado, imprimir el contenido crudo de ResponseBody
        if ($result.ResponseBody) {
            # Imprimir el contenido crudo para asegurarse de lo que llega
            Write-Host "Contenido crudo de ResponseBody: '$($result.ResponseBody)'"

            # Llamar a la funci�n para formatear y mostrar el cuerpo de la respuesta en formato tabla
            Format-ResponseBody -ResponseBody $result.ResponseBody
        } else {
            Write-Host "Sin resultados."
        }

        # Mostrar el tiempo que tom� ejecutar la consulta
        Write-Host "Tiempo de ejecuci�n: $($duration.TotalMilliseconds) ms"
    }
}

function Format-ResponseBody {
    param (
        [string]$ResponseBody
    )

    # Dividir el ResponseBody en filas (en este caso parece ser una sola fila)
    $rows = $ResponseBody -split '\n'

    # Crear un array para almacenar los objetos que se mostrar�n en la tabla
    $resultados = @()

    # Dividir cada fila por comas (asumimos que los datos est�n separados por comas)
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

# Funci�n para enviar la consulta SQL al servidor
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
            RequestType = 0;  # Por ejemplo, tipo de operaci�n
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
