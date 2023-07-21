#
# Define the URL of the certificate
$certUrl = "http://testhost.idania.se:8000/rootCA.crt"

# Define the location where to save the certificate
$certPath = "$env:TEMP\rootCa.crt"

# Download the certificate
Invoke-WebRequest -Uri $certUrl -OutFile $certPath

# Import the certificate temporarily to retrieve its properties
$tempCert = Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\CurrentUser\My

# Get the certificate subject
$certSubject = $tempCert.Subject

# Remove the existing certificates with the same subject from 'Trusted Root Certification Authorities' store
Get-ChildItem -Path Cert:\LocalMachine\Root |
Where-Object { $_.Subject -eq $certSubject } |
ForEach-Object { Remove-Item -Path $_.PSPath }

# Remove the temporary certificate
Remove-Item -Path $tempCert.PSPath

# Import the certificate to the 'Trusted Root Certification Authorities' store
Import-Certificate -FilePath $certPath -CertStoreLocation Cert:\LocalMachine\Root

# Clean up (delete the certificate file)
Remove-Item -Path $certPath

