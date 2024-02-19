def recv_exact(client_socket):
    # Helper function to receive a fixed number of bytes
    def recv_n_bytes(n):
        received_data = b''
        while len(received_data) < n:
            chunk = client_socket.recv(n - len(received_data))
            if not chunk:
                raise Exception("Connection closed before data was fully received")
            received_data += chunk
        return received_data

    # Read bytes until the underscore is found
    length_buffer = b''
    while True:
        byte = recv_n_bytes(1)
        if byte == b'_':
            break
        length_buffer += byte

    # Convert length_buffer to integer
    data_length = int(length_buffer.decode())

    # Receive the exact amount of data using our helper function
    data = recv_n_bytes(data_length)

    return data
