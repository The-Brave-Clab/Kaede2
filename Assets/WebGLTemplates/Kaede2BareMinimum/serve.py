from http.server import HTTPServer, SimpleHTTPRequestHandler, ThreadingHTTPServer
import posixpath
import os
import socket
import sys

content_type = {
    ".js": "application/javascript",
    ".wasm": "application/wasm",
    ".json": "application/json",
    ".html": "text/html",
    ".css": "text/css",
    ".xml": "text/xml",
    ".hash": "text/plain",
    ".png": "image/png",
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".ico": "image/x-icon",
    ".data": "binary/octet-stream",
    ".bundle": "binary/octet-stream",
    ".bin": "binary/octet-stream",
}

content_encoding = {
    ".br": "br",
}

class UnityWebHTTPHandler(SimpleHTTPRequestHandler):

    def guess_type(self, path):
        base, ext = posixpath.splitext(path)
        ext = ext.lower()

        if ext in content_encoding:
            base, ext = posixpath.splitext(base)
            ext = ext.lower()

        if ext in content_type:
            return content_type[ext]

        return super().guess_type(path)

    def get_content_encoding(self, path):
        # Determine the file extension
        base, ext = posixpath.splitext(path)
        
        # Get ContentEncoding based on file extension
        return content_encoding.get(ext, None)

    def end_headers(self):
        header_encoding = self.get_content_encoding(self.path)
        if content_encoding:
            self.send_header('Content-Encoding', header_encoding)
        
        super().end_headers()

def _get_best_family(*address):
    infos = socket.getaddrinfo(
        *address,
        type=socket.SOCK_STREAM,
        flags=socket.AI_PASSIVE,
    )
    family, type, proto, canonname, sockaddr = next(iter(infos))
    return family, sockaddr

def test(HandlerClass=UnityWebHTTPHandler,
         ServerClass=ThreadingHTTPServer,
         protocol="HTTP/1.0", port=8000, bind=None):
    """Test the HTTP request handler class.

    This runs an HTTP server on port 8000 (or the port argument).

    """
    ServerClass.address_family, addr = _get_best_family(bind, port)
    HandlerClass.protocol_version = protocol
    with ServerClass(addr, HandlerClass) as httpd:
        host, port = httpd.socket.getsockname()[:2]
        url_host = f'[{host}]' if ':' in host else host
        print(
            f"Serving HTTP on {host} port {port} "
            f"(http://{url_host}:{port}/) ..."
        )
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print("\nKeyboard interrupt received, exiting.")
            sys.exit(0)

if __name__ == '__main__':
    import argparse
    import contextlib

    parser = argparse.ArgumentParser()
    parser.add_argument('--bind', '-b', metavar='ADDRESS',
                        help='specify alternate bind address '
                             '(default: all interfaces)')
    parser.add_argument('--directory', '-d', default=os.getcwd(),
                        help='specify alternate directory '
                             '(default: current directory)')
    parser.add_argument('port', action='store', default=8000, type=int,
                        nargs='?',
                        help='specify alternate port (default: 8000)')
    args = parser.parse_args()
    handler_class = UnityWebHTTPHandler

    # ensure dual-stack is not disabled; ref #38907
    class DualStackServer(ThreadingHTTPServer):

        def server_bind(self):
            # suppress exception when protocol is IPv4
            with contextlib.suppress(Exception):
                self.socket.setsockopt(
                    socket.IPPROTO_IPV6, socket.IPV6_V6ONLY, 0)
            return super().server_bind()

        def finish_request(self, request, client_address):
            self.RequestHandlerClass(request, client_address, self,
                                     directory=args.directory)

    test(
        HandlerClass=handler_class,
        ServerClass=DualStackServer,
        port=args.port,
        bind=args.bind,
    )