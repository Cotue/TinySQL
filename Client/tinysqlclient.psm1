param (
    [Parameter(Mandatory = $true)]
    [string]$IP,
    [Parameter(Mandatory = $true)]
    [int]$Port
)

$ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse("127.0.0.1"), 1042)

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
        return if ($null -ne $reader.ReadLine()) { $reader.ReadLine() } else { "" }
    }
    finally {
        $reader.Close()
        $stream.Close()
    }
}
function Send-SQLCommand {
    param (
        [string]$command
    )
    $client = New-Object System.Net.Sockets.Socket($ipEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
    $client.Connect($ipEndPoint)
    $requestObject = [PSCustomObject]@{
        RequestType = 0;
        RequestBody = $command
    }
    Write-Host -ForegroundColor Green "Sending command: $command"

    $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress
    Send-Message -client $client -message $jsonMessage
    $response = Receive-Message -client $client

    Write-Host -ForegroundColor Green "Response received: $response"
    
    $responseObject = ConvertFrom-Json -InputObject $response
    Write-Output $responseObject
    $client.Shutdown([System.Net.Sockets.SocketShutdown]::Both)
    $client.Close()
}
function Invoke-MyQuery {
    param (
        [Parameter(Mandatory = $true)]
        [string]$QueryFile,

        [Parameter(Mandatory = $true)]
        [string]$IP,

        [Parameter(Mandatory = $true)]
        [int]$Port
    )

    # Leer el archivo de sentencias SQL
    $queries = Get-Content -Path $QueryFile -Raw
    $queriesList = $queries -split ';'

    # Definir el endpoint del socket
    $ipEndPoint = [System.Net.IPEndPoint]::new([System.Net.IPAddress]::Parse($IP), $Port)

    # Función para enviar la sentencia SQL al servidor
    function Send-SQLCommand {
        param (
            [string]$command
        )

        $client = New-Object System.Net.Sockets.Socket($ipEndPoint.AddressFamily, [System.Net.Sockets.SocketType]::Stream, [System.Net.Sockets.ProtocolType]::Tcp)
        $client.Connect($ipEndPoint)

        $requestObject = [PSCustomObject]@{
            RequestType = 0
            RequestBody = $command
        }

        Write-Host -ForegroundColor Green "Sending command: $command"
        $jsonMessage = ConvertTo-Json -InputObject $requestObject -Compress
        Send-Message -client $client -message $jsonMessage
        $response = Receive-Message -client $client

        $client.Shutdown([System.Net.Sockets.SocketShutdown]::Both)
        $client.Close()

        return $response
    }

    # Procesar cada sentencia SQL
    foreach ($query in $queriesList) {
        if (![string]::IsNullOrWhiteSpace($query)) {
            # Medir el tiempo de ejecución de la sentencia
            $timeTaken = Measure-Command {
                $response = Send-SQLCommand -command $query.Trim()
            }

            # Convertir respuesta JSON a objeto de PowerShell
            $responseObject = ConvertFrom-Json -InputObject $response

            # Mostrar resultado en formato tabla (puedes ajustar las columnas según la respuesta)
            $responseObject | Format-Table -AutoSize

            # Mostrar tiempo de ejecución
            Write-Host -ForegroundColor Yellow ("Execution Time: {0}" -f $timeTaken.TotalMilliseconds) "ms"
        }
    }
}


# This is an example, should not be called here
#Send-SQLCommand -command "CREATE TABLE ESTUDIANTE"
#Send-SQlCommand -command "SELECT * FROM ESTUDIANTE"