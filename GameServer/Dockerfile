FROM ubuntu:22.04

WORKDIR /server

# ca-certificates is needed for the server's own HTTPS calls to the backend
# (LobbyApiController's UnityWebRequest to your Render-hosted Node API).
RUN apt-get update && apt-get install -y --no-install-recommends \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

COPY LinuxBuild.x86_64 ./
COPY LinuxBuild_Data ./LinuxBuild_Data
COPY UnityPlayer.so ./
COPY libdecor-0.so.0 ./
COPY libdecor-cairo.so ./

RUN chmod +x LinuxBuild.x86_64

# Render sets $PORT at runtime; default to 27777 for local `docker run` testing
# where $PORT won't be set. Shell form (not exec/array form) so $PORT expands.
CMD ./LinuxBuild.x86_64 -port ${PORT:-27777} -batchmode -nographics
