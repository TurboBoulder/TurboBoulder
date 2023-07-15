#!/bin/sh

set -e

# Define the distinguished name variables
C="SE"
ST="Denial"
L="Harnosand"
O="Idania AB"

# Define the output directory
OUTDIR="/certs"
mkdir -p "$OUTDIR"

generate_certs() {
  echo "creating ca"
  # Generate the root CA certificate
  openssl genpkey -algorithm RSA -out "$OUTDIR/rootCA.key" -pkeyopt rsa_keygen_bits:2048
  openssl req -x509 -new -nodes -key "$OUTDIR/rootCA.key" -sha256 -days 1024 -out "$OUTDIR/rootCA.crt" -config rootCA.cnf

  # Generate certificates for the SQL server, client, and web server
  echo "creating certificates"
  for name in sqlserver client webserver
  do
    echo "...${name}"
    openssl genpkey -algorithm RSA -out "$OUTDIR/$name.key" -pkeyopt rsa_keygen_bits:2048
    openssl req -new -key "$OUTDIR/$name.key" -out "$OUTDIR/$name.csr" -reqexts req_ext -extensions req_ext -config ./${name}.cnf
    openssl x509 -req -in "$OUTDIR/$name.csr" -CA "$OUTDIR/rootCA.crt" -CAkey "$OUTDIR/rootCA.key" -CAcreateserial -out "$OUTDIR/$name.crt" -days 500 -sha256 -extensions req_ext -extfile ./${name}.cnf
    openssl pkcs12 -export -out "$OUTDIR/$name.pfx" -inkey "$OUTDIR/$name.key" -in "$OUTDIR/$name.crt" -password pass:3s5Y9gRwZk1P7q
  done
}



# Check if any files already exist
if [ "$(ls -A $OUTDIR)" ]; then
  if [ "$FORCE_RENEW" != "true" ]; then
    echo "Certificates already exist and FORCE_RENEW is not set to true. Skipping certificate generation..."
  else
    echo "Certificates exist, but FORCE_RENEW=true, so they will be regenerated."
    generate_certs
  fi
else
  echo "No certificates found, generating..."
  generate_certs
fi

# Change directory to /certs
cd "$OUTDIR"

# Start a simple web server to serve the certificates
python -m http.server
