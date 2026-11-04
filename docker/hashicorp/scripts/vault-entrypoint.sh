#!/bin/sh

set -u


echo ""
echo "=========================================="
echo "             SNEL VAULT"
echo "=========================================="
echo ""


# =============================================================
# 1. START VAULT SERVER
# =============================================================

echo "[1/10] Starting Vault server..."


vault server \
    -config=/vault/config/vault.hcl &


VAULT_PID=$!


echo "Vault server started."
echo "PID: $VAULT_PID"


# =============================================================
# GRACEFUL SHUTDOWN
# =============================================================

shutdown()
{
    echo ""
    echo "Stopping Vault..."

    kill -TERM "$VAULT_PID" 2>/dev/null || true

    wait "$VAULT_PID"

    exit 0
}


trap shutdown TERM INT


# =============================================================
# 2. WAIT FOR VAULT
# =============================================================

echo ""
echo "[2/10] Waiting for Vault..."


while true
do

    STATUS_JSON=$(vault status -format=json 2>/dev/null || true)


    if [ -n "$STATUS_JSON" ]; then
        break
    fi


    # Detect Vault startup failure.
    if ! kill -0 "$VAULT_PID" 2>/dev/null; then

        echo ""
        echo "ERROR:"
        echo "Vault server stopped unexpectedly."

        wait "$VAULT_PID"

        exit 1

    fi


    sleep 2

done


echo "Vault is reachable."


# =============================================================
# 3. INITIALIZE VAULT
# =============================================================

echo ""
echo "[3/10] Checking Vault initialization..."


STATUS_JSON=$(vault status -format=json 2>/dev/null || true)


if echo "$STATUS_JSON" \
    | grep -q '"initialized":[[:space:]]*false'; then

    echo "Vault is not initialized."


    # A new Vault storage volume must never use keys belonging
    # to an old Vault instance.
    if [ -f /bootstrap/init.json ]; then

        echo "Removing stale bootstrap init.json..."

        rm -f /bootstrap/init.json

    fi


    echo "Initializing Vault..."


    vault operator init \
        -key-shares=1 \
        -key-threshold=1 \
        -format=json \
        > /bootstrap/init.json


    if [ ! -s /bootstrap/init.json ]; then

        echo ""
        echo "ERROR:"
        echo "Vault initialization failed."

        exit 1

    fi


    chmod 600 /bootstrap/init.json


    echo "Vault initialized successfully."


else

    echo "Vault is already initialized."


    if [ ! -f /bootstrap/init.json ]; then

        echo ""
        echo "ERROR:"
        echo "Vault is already initialized but:"
        echo ""
        echo "    /bootstrap/init.json"
        echo ""
        echo "does not exist."
        echo ""
        echo "Automatic unseal cannot continue."

        exit 1

    fi

fi


# =============================================================
# 4. READ BOOTSTRAP CREDENTIALS
# =============================================================

echo ""
echo "[4/10] Loading Vault initialization credentials..."


UNSEAL_KEY=$(awk '
    /"unseal_keys_b64"/ {
        getline;
        gsub(/[",[:space:]]/, "");
        print;
        exit
    }
' /bootstrap/init.json)


ROOT_TOKEN=$(awk -F'"' '
    /"root_token"/ {
        print $4;
        exit
    }
' /bootstrap/init.json)


if [ -z "$UNSEAL_KEY" ]; then

    echo "ERROR: Cannot read Vault unseal key."

    exit 1

fi


if [ -z "$ROOT_TOKEN" ]; then

    echo "ERROR: Cannot read Vault root token."

    exit 1

fi


echo "Vault initialization credentials loaded."


# =============================================================
# 5. UNSEAL VAULT
# =============================================================

echo ""
echo "[5/10] Checking Vault seal status..."


STATUS_JSON=$(vault status -format=json 2>/dev/null || true)


if echo "$STATUS_JSON" \
    | grep -q '"sealed":[[:space:]]*true'; then

    echo "Vault is sealed."

    echo "Automatically unsealing..."


    vault operator unseal \
        "$UNSEAL_KEY" \
        >/dev/null


    sleep 1

fi


STATUS_JSON=$(vault status -format=json 2>/dev/null || true)


if echo "$STATUS_JSON" \
    | grep -q '"sealed":[[:space:]]*true'; then

    echo "ERROR: Vault is still sealed."

    exit 1

fi


echo "Vault is unsealed."


# =============================================================
# 6. ROOT AUTHENTICATION
# =============================================================

echo ""
echo "[6/10] Authenticating bootstrap process..."


export VAULT_TOKEN="$ROOT_TOKEN"


if ! vault token lookup >/dev/null 2>&1; then

    echo "ERROR: Root token authentication failed."

    exit 1

fi


echo "Bootstrap authentication successful."


# =============================================================
# 7. ENABLE KV VERSION 2
# =============================================================

echo ""
echo "[7/10] Checking KV version 2 engine..."


if vault secrets list -format=json \
    | grep -q '"secret/"'; then

    echo "secret/ already exists."


else

    echo "Enabling KV version 2 at secret/..."


    vault secrets enable \
        -path=secret \
        kv-v2


    echo "KV version 2 enabled."

fi


# =============================================================
# 8. CREATE APPLICATION POLICY
# =============================================================

echo ""
echo "[8/10] Configuring SNEL Wallet policy..."


cat > /tmp/snel-wallet-policy.hcl <<'EOF'

# =============================================================
# SNEL Wallet Secret Access
# =============================================================

path "secret/data/snel-wallet/*" {
  capabilities = ["read"]
}


# =============================================================
# SNEL Wallet Metadata
# =============================================================

path "secret/metadata/snel-wallet" {
  capabilities = ["read", "list"]
}


path "secret/metadata/snel-wallet/*" {
  capabilities = ["read", "list"]
}

EOF


vault policy write \
    snel-wallet-read \
    /tmp/snel-wallet-policy.hcl


rm -f /tmp/snel-wallet-policy.hcl


echo "Policy snel-wallet-read configured."


# =============================================================
# 9. CREATE APPLICATION TOKEN
# =============================================================

echo ""
echo "[9/10] Checking application token..."


if vault token lookup "$APP_TOKEN" >/dev/null 2>&1; then

    echo "Application token already exists."


else

    echo "Creating SNEL Wallet application token..."


    vault token create \
        -id="$APP_TOKEN" \
        -policy="snel-wallet-read" \
        -orphan \
        -ttl="720h" \
        -display-name="snel-wallet-application"


    echo "Application token created."

fi


# =============================================================
# 10. CREATE / UPDATE SECRETS
# =============================================================

SECRET_PATH="secret/snel-wallet/$VAULT_ENVIRONMENT"


echo ""
echo "[10/10] Configuring secrets..."
echo "Path: $SECRET_PATH"


if vault kv get "$SECRET_PATH" >/dev/null 2>&1; then

    echo "Secret collection exists."
    echo "Updating configured values..."


    vault kv patch "$SECRET_PATH" \
        ConnectAfrica__DefaultUnit="$ConnectAfrica__DefaultUnit" \
        ConnectAfrica__Password="$ConnectAfrica__Password" \
        ConnectAfrica__Username="$ConnectAfrica__Username" \
        ConnectAfrica__ValidationCode="$ConnectAfrica__ValidationCode" \
        ConnectionStrings__DefaultConnection="$ConnectionStrings__DefaultConnection" \
        ElasticConfiguration__Password="$ElasticConfiguration__Password" \
        ElasticConfiguration__Username="$ElasticConfiguration__Username" \
        Keycloak__Admin__ClientSecret="$Keycloak__Admin__ClientSecret" \
        Keycloak__Admin__Password="$Keycloak__Admin__Password" \
        Keycloak__Admin__Username="$Keycloak__Admin__Username" \
        Keycloak__credentials__secret="$Keycloak__credentials__secret" \
        Webhooks__Interswitch__Secret="$Webhooks__Interswitch__Secret"


else

    echo "Secret collection does not exist."
    echo "Creating $SECRET_PATH..."


    vault kv put "$SECRET_PATH" \
        ConnectAfrica__DefaultUnit="$ConnectAfrica__DefaultUnit" \
        ConnectAfrica__Password="$ConnectAfrica__Password" \
        ConnectAfrica__Username="$ConnectAfrica__Username" \
        ConnectAfrica__ValidationCode="$ConnectAfrica__ValidationCode" \
        ConnectionStrings__DefaultConnection="$ConnectionStrings__DefaultConnection" \
        ElasticConfiguration__Password="$ElasticConfiguration__Password" \
        ElasticConfiguration__Username="$ElasticConfiguration__Username" \
        Keycloak__Admin__ClientSecret="$Keycloak__Admin__ClientSecret" \
        Keycloak__Admin__Password="$Keycloak__Admin__Password" \
        Keycloak__Admin__Username="$Keycloak__Admin__Username" \
        Keycloak__credentials__secret="$Keycloak__credentials__secret" \
        Webhooks__Interswitch__Secret="$Webhooks__Interswitch__Secret"


    echo "Secret collection created."

fi


# =============================================================
# FINAL VALIDATION
# =============================================================

echo ""
echo "Validating Vault configuration..."


if ! vault kv get "$SECRET_PATH" >/dev/null 2>&1; then

    echo ""
    echo "ERROR:"
    echo "Cannot read $SECRET_PATH"

    exit 1

fi


echo ""
echo "=========================================="
echo "           SNEL VAULT READY"
echo "=========================================="
echo ""
echo "Vault URL:"
echo "    http://localhost:8200"
echo ""
echo "Container:"
echo "    snel-vault"
echo ""
echo "Environment:"
echo "    $VAULT_ENVIRONMENT"
echo ""
echo "Secret path:"
echo "    $SECRET_PATH"
echo ""
echo "Application token:"
echo "    $APP_TOKEN"
echo ""
echo "Policies:"
echo "    default"
echo "    snel-wallet-read"
echo ""
echo "=========================================="
echo ""


# =============================================================
# KEEP VAULT RUNNING
# =============================================================
#
# Vault remains the actual server process.
#
# When Docker restarts this container, this script starts again,
# recognizes the existing persistent data, reads init.json,
# automatically unseals Vault, verifies configuration, and
# starts serving requests again.
#
# =============================================================


wait "$VAULT_PID"