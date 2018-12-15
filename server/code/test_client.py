import socket
import json
sock = socket.socket()
sock.connect(('127.0.0.1',12080))
senddata = {"action":"SubmitScore","data":{"username":"liusai","score":164}}
sock.send(json.dumps(senddata))
print 'xx'
res = sock.recv(1024)
print 'xxxxxxxxxxxxxxx'
print res
sock.close();