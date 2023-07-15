$webRequest = [Net.HttpWebRequest]::Create("https://localhost:7056")
$webRequest.GetResponse() | Out-Null
$cert = $webRequest.ServicePoint.Certificate
$bytes = $cert.Export([Security.Cryptography.X509Certificates.X509ContentType]::Cert)
set-content -value $bytes -encoding byte -path "serverCert.cer"