FROM ubuntu:22.04

WORKDIR /server

# ca-certificates is needed for the server's own HTTPS calls to the backend
# (LobbyApiController's UnityWebRequest to your Render-hosted Node API).
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY LinuxServer.aarch64 ./
COPY GameAssembly.so ./
COPY LinuxServer_Data ./LinuxServer_Data
COPY UnityPlayer.so ./
COPY libdecor-0.so.0 ./
COPY libdecor-cairo.so ./

RUN chmod +x LinuxServer.aarch64

# On Oracle's Ampere VM there's no platform-injected $PORT like Render's,
# so this defaults straight to 27777 (still overridable via -e PORT=... if needed).
CMD ./LinuxServer.aarch64 -port ${PORT:-27777} -batchmode -nographics
