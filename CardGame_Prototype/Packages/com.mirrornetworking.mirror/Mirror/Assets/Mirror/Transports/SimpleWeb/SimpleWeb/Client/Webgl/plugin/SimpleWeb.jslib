// this will create a global object
const SimpleWeb =
{
    webSockets: [],
    next: 1,
    GetWebSocket: function (index)
    {
        return SimpleWeb.webSockets[index]
    },
    AddNextSocket: function (webSocket)
    {
        var index = SimpleWeb.next;
        SimpleWeb.next++;
        SimpleWeb.webSockets[index] = webSocket;
        return index;
    },
    RemoveSocket: function (index)
    {
        SimpleWeb.webSockets[index] = undefined;
    },
};

function IsConnected(index)
{
    var webSocket = SimpleWeb.GetWebSocket(index);
    if (webSocket)
        return webSocket.readyState === webSocket.OPEN;
    else
        return false;
}

function Connect(addressPtr, openCallbackPtr, closeCallBackPtr, messageCallbackPtr, errorCallbackPtr)
{
    // fix for unity 2021 because unity bug in .jslib
    // Modern Emscripten (Unity 6+) removed the global `Runtime` object AND the
    // generic `dynCall(sig, ptr, args)` function it used to provide. Function
    // pointers are now called either via per-signature Module['dynCall_XX']
    // exports, or directly through the wasm table. This shim tries both so it
    // keeps working across Emscripten versions without needing a Unity-version check.
    if (typeof Runtime === "undefined")
    {
        var Runtime = {
            dynCall: function(sig, ptr, args) {
                var fn = (typeof Module !== "undefined") && Module['dynCall_' + sig];
                if (fn) return fn.apply(null, [ptr].concat(args));
                if (typeof dynCall === "function") return dynCall(sig, ptr, args);
                var table = (typeof wasmTable !== "undefined") ? wasmTable
                    : (typeof Module !== "undefined" ? Module['wasmTable'] : undefined);
                if (table) return table.get(ptr).apply(null, args);
                throw new Error("SimpleWeb.jslib: no way to call function pointer (sig=" + sig + ")");
            }
        };
    }

    const address = UTF8ToString(addressPtr);
    console.log("Connecting to " + address);

    // Create webSocket connection.
    var webSocket = new WebSocket(address);
    webSocket.binaryType = 'arraybuffer';

    const index = SimpleWeb.AddNextSocket(webSocket);

    // Connection opened
    webSocket.onopen = function(event) 
    {
        console.log("Connected to " + address);
        Runtime.dynCall('vi', openCallbackPtr, [index]);
    };

    webSocket.onclose = function(event) 
    {
        console.log("Disconnected from " + address);
        Runtime.dynCall('vi', closeCallBackPtr, [index]);
    };

    webSocket.onmessage = function(event) 
    {
        if (event.data instanceof ArrayBuffer) {
            var array = new Uint8Array(event.data);
            var arrayLength = array.length;

            var bufferPtr = _malloc(arrayLength);
            var dataBuffer = new Uint8Array(HEAPU8.buffer, bufferPtr, arrayLength);
            dataBuffer.set(array);

            Runtime.dynCall('viii', messageCallbackPtr, [index, bufferPtr, arrayLength]);
            _free(bufferPtr);
        }
        else
        {
            console.error("message type not supported")
        }
    };

    webSocket.onerror = function(event) 
    {
        console.error('Socket Error', event);
        Runtime.dynCall('vi', errorCallbackPtr, [index]);
    };

    return index;
}

function Disconnect(index) {
    var webSocket = SimpleWeb.GetWebSocket(index);
    if (webSocket)
        webSocket.close(1000, "Disconnect Called by Mirror");

    SimpleWeb.RemoveSocket(index);
}

function Send(index, arrayPtr, offset, length) {
    var webSocket = SimpleWeb.GetWebSocket(index);
    if (webSocket)
    {
        const start = arrayPtr + offset;
        const end = start + length;
        const data = HEAPU8.buffer.slice(start, end);
        webSocket.send(data);
        return true;
    }
    return false;
}

const SimpleWebLib =
{
    $SimpleWeb: SimpleWeb,
    IsConnected,
    Connect,
    Disconnect,
    Send
};

autoAddDeps(SimpleWebLib, '$SimpleWeb');
mergeInto(LibraryManager.library, SimpleWebLib);
