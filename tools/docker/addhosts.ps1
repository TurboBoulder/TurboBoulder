# Function to add an entry if it doesn't exist
function Add-HostEntry ($ip_address, $hostname) {
    $content = Get-Content -Path "C:\Windows\System32\drivers\etc\hosts"
    if ($content -notcontains "$ip_address`t$hostname") {
        Add-Content -Path "C:\Windows\System32\drivers\etc\hosts" -Value "`n$ip_address`t$hostname"
    }
}

# Function to remove entries based on domain
function Remove-HostEntry ($undesired_domain) {
    $content = Get-Content -Path "C:\Windows\System32\drivers\etc\hosts"
    $content = $content | Where-Object { $_ -notmatch $undesired_domain }
    Set-Content -Path "C:\Windows\System32\drivers\etc\hosts" -Value $content
}

# Usage
Add-HostEntry -ip_address "192.168.1.123" -hostname "testhost.idania.se"
Add-HostEntry -ip_address "192.168.1.123" -hostname "webserver.testhost.idania.se"
Add-HostEntry -ip_address "192.168.1.123" -hostname "sqlserver.testhost.idania.se"
Add-HostEntry -ip_address "192.168.1.123" -hostname "client.testhost.idania.se"
Add-HostEntry -ip_address "192.168.1.123" -hostname "caramba.testhost.idania.se"
Remove-HostEntry -undesired_domain "caramba.testhost.idania.se"
